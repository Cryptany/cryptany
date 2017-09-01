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

namespace Cryptany.Core.Router.Data
{
    /// <summary>
    /// Статус обрабтки асинхронного сообщения
    /// </summary>
    public enum AsyncStatus
    {
        Completed,
        Incompleted,
        Expired
    }

    /// <summary>
    /// Состояние обработки асинхронного блока
    /// </summary>
    public class AsyncState
    {
        /// <summary>
        /// Текущий статус
        /// </summary>
        public AsyncStatus Status = AsyncStatus.Incompleted;

        /// <summary>
        /// При статусе Completed Набор ожидающих выполнения блоков
        /// </summary>
        public Block[] NextBlocks = new Block[0];

        /// <summary>
        /// Инициализирует объект, со статусом Completed и заданным NextBlocks
        /// </summary>
        /// <param name="nextBlock"></param>
        public AsyncState(Block[] nextBlock)
        {
            Status = AsyncStatus.Completed;
            if (nextBlock != null)
                NextBlocks = nextBlock;
        }

        /// <summary>
        /// Инициализирует объект, со статусом Incompleted
        /// </summary>
        public AsyncState() { }

        public AsyncState(AsyncStatus status) : this()
        {
            Status = status;
        }
    }
}
