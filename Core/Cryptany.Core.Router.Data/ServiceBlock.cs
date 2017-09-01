/*
   Copyright 2006-2017 Cryptany, Inc.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptany.Core.Router.Data;
using System.Diagnostics;
using System.Threading;
using Cryptany.Common.Utils;
using Cryptany.Core.Monitoring;

namespace Cryptany.Core.Router.Data
{
    partial class ServiceBlock
    {
        //public static ServiceBlock CreateTestServiceBlock()
        //{
        //    Block SMS2 = new SendSMSAsync("[SMS2]", null, null);
        //    Block SMS1 = new SendSMSAsync("[SMS1]", SMS2, null);
        //    Block Cond = new ConditionBlock("[Condition]", SMS1, null);
        //    var res = new ServiceBlock();
        //    res.startBlock = Cond;
        //    //res.EnterCondition = new ConditionBlock("[EnterCondition]", SMS1, null);
        //    return res;
        //}



        Dictionary<Guid, Block> _Blocks;
        /// <summary>
        /// Список блоков текущего сервисного блока
        /// </summary>
        public Dictionary<Guid, Block> Blocks
        {
            get
            {
                if (_Blocks == null)
                {
                    _Blocks = (from be in DBCache.BlockEntries.Values
                              where be.ServiceBlockId == this.Id
                              select Block.CreateBlock(be)).ToDictionary(a => a.Id);
                    foreach (Block a in _Blocks.Values)
                        a.ServiceBlock = this;
                }
                return _Blocks;
            }
        }
        
        List<Block> _EnterConditions;
        /// <summary>
        /// Набор проверочных блоков для входа в сервисный блок
        /// </summary>
        public List<Block> EnterConditions
        {
            get
            {
                if (_EnterConditions == null)
                {
                    _EnterConditions = (from be in DBCache.BlockEntries.Values 
                                        where be.ServiceBlockId == Id && be.IsVerification 
                                        select Blocks[be.BlockId]).ToList();
                }
                return _EnterConditions;
            }
        }

        /// <summary>
        /// Проверяет условие входа в сервисный блок.
        /// </summary>
        /// <param name="msg">Проверяемое сообщение</param>
        /// <returns>Возвращает true в случае успешного выполнения всех блоков EnterCondition</returns>
        public bool Check(Message msg)
        {
            foreach (Block b in EnterConditions)
                if (b.Perform(msg).Count() == 0) // Предполагается, что у проверочных блоков пустой OutFalse
                    return false;
            return true;
        }

        Block _StartBlock = null;
        /// <summary>
        /// Входной блок для данной сервисной группы
        /// </summary>
        public Block StartBlock
        {
            get
            {
                if (_StartBlock == null)
                {
                    // Должен быть сиротой и не быть проверочным
                    /*_StartBlock = (from b in Blocks.Values
                                  where b.Parents.Count == 0 && !b.IsVerification
                                  select b).First();
                      */
                    _StartBlock = EnterConditions.First();
                      
                }
                return _StartBlock;
            }
        }

        public TVService TVService
        {
            get
            {
                return DBCache.Services[ServiceId];
            }
        }

        /// <summary>
        /// Используя поиск в ширину выполняет все дочерние блоки. Останавливается на асинхронных блоках
        /// </summary>
        /// <param name="msg">Обрабатываемое сообщение</param>
        /// <param name="src">Блок с которого начинаем выполнять</param>
        /// <returns>Список асинхронных блоков на которых остановилась обработка</returns>
        static AsyncBlock[] PerformBlockRecursively(Message msg, Block src)
        {
            // Очередь в которую попадают блоки которые нужно обработать
            Queue<Block> queue = new Queue<Block>();
            // Возвращаемое значение
            List<AsyncBlock> result = new List<AsyncBlock>();
            // Кладем в очередь на обработку первый блок
            queue.Enqueue((Block)src.Clone());
            // и поехали
            //  Пока очередь обработки не пуста
            while (queue.Count > 0)
            {
                // Достаем блок
                Block cur = queue.Dequeue();

                Block[] tmp;
                try
                {
                    // Выполняем его
                    tmp = cur.Perform(msg);
                }
                catch (Exception e)
                {
                    //Tracer.Write(msg + " Ошибка при обработке блока " + cur.Name + " типа " + cur.BlockType + ". Дальнейшая обработка блока не производится: " + e);
                    Functions.AddEvent("Ошибка обработки входящего сообщения", "Ошибка обработки входящего сообщения " + msg, EventType.Error, null, e);
                    throw;
                    //Console.WriteLine(e.StackTrace);
                }

                // Если блок асинхронный и может повлечь выполнение других блоков, то добавляем его в результат
                if (cur is AsyncBlock && (cur.OutTrue.Count > 0 || cur.OutFalse.Count > 0))
                    result.Add((AsyncBlock)cur);
                // В противном случае всех полученых детей данного блока закидываем в очередь на обработку
                else
                    foreach (var b in tmp)
                        queue.Enqueue((Block)b.Clone());

            }

            return result.ToArray();
        }

        /// <summary>
        /// Выполняет набор блоков и всех их детишек
        /// </summary>
        /// <param name="msg">Обрабатываемое сообщение</param>
        /// <param name="srcs">Набор блоков для выполнения</param>
        /// <returns></returns>
        static AsyncBlock[] PerformBlocksRecursively(Message msg, Block[] srcs)
        {
            List<AsyncBlock> result = new List<AsyncBlock>();
            foreach (var src in srcs)
            {
                // Объеденяем результаты индивидуальной обработки всех блоков
                result.AddRange(PerformBlockRecursively(msg, src));
            }
            return result.ToArray();
        }

        /// <summary>
        /// Обрабатывает внутреннее сообщение роутера, проверяет асинхронные блоки на готовность
        /// </summary>
        /// <param name="msg">Обрабатываемое сообщение</param>
        /// <param name="blocks">Список выполняющихся аснинхронных блоков</param>
        static public void ProcessInnerMessage(Message msg, List<AsyncBlock> blocks)
        {
            // Блоки которые будут удалены из списка
            List<AsyncBlock> toDelete = new List<AsyncBlock>();
            // Блоки, которые будут добавлены в список
            List<AsyncBlock> toAdd = new List<AsyncBlock>();
            foreach (var block in blocks)
            {
                // Проверяем готовность асинхронного блока
                AsyncState state = block.IsReady(msg);
                switch (state.Status)
                {
                    // Если блок завершил выполнение
                    case AsyncStatus.Completed:
                        // То выполняем всех его дочерних
                        AsyncBlock[] addBlocks = PerformBlocksRecursively(msg, state.NextBlocks);
                        // Добавляем результаты выполнения в список добавляемых
                        toAdd.AddRange(addBlocks);
                        // И удаляем завершенный блок из списка
                        toDelete.Add(block);
                        break;

                    case AsyncStatus.Expired:
                        //Tracer.Write("Блок [" + block.Name + "] из сервисного блока [" + block.ServiceBlock.Name + "] протух и будет удален");
                        toDelete.Add(block);
                        break;
                }
            }
            foreach (var b in toDelete)
                blocks.Remove(b);
            blocks.AddRange(toAdd);
        }

        /// <summary>
        /// Первичная обработка собщения в сервисном блоке
        /// </summary>
        /// <param name="msg">Обрабатываемое сообщение</param>
        /// <returns>Список выполняющихся аснинхронных блоков</returns>
        public AsyncBlock[] ProcessInputMessage(Message msg)
        {
            return PerformBlockRecursively(msg, StartBlock);
        }

    }
}
