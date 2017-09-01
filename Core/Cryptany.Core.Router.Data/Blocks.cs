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
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Xml;
using System.Reflection;
using Cryptany.Common.Utils;
using Cryptany.Core.Monitoring;

namespace Cryptany.Core.Router.Data
{
    /// <summary>
    /// Родительский класс для синхронных блоков
    /// </summary>
    public abstract class SyncBlock : Block
    {
        #region Constructors
        /// <summary>
        /// Инициализирует блок с заданым именем
        /// </summary>
        /// <param name="Name">Имя нового блока</param>
        public SyncBlock(string Name) : base(Name) { }

        /// <summary>
        /// Инициализирует блок по шаблону. Используется при десериализации и клонировании
        /// </summary>
        /// <param name="other"></param>
        public SyncBlock(Block other) : base(other) { }
        #endregion

        /// <summary>
        /// Клонирует данный блок.
        /// </summary>
        /// <returns> Т.к. синхронный блок не предполагает изменения внутреннего состояния, то возвращается ссылка на исходный объект</returns>
        public override object Clone()
        {
            return this;
        }
    }

    /// <summary>
    /// Родительский класс для асинхронных блоков
    /// </summary>
    public abstract class AsyncBlock : Block, ISerializable
    {
        #region Constructors
        /// <summary>
        /// Инициализирует блок с заданым именем
        /// </summary>
        /// <param name="Name">Имя нового блока</param>
        public AsyncBlock(string Name) : base(Name) { }

        /// <summary>
        /// Инициализирует блок по шаблону. Используется при десериализации и клонировании
        /// </summary>
        /// <param name="other"></param>
        public AsyncBlock(SerializationInfo info, StreamingContext context)
            : base(DBCache.ServiceBlocks[(Guid)info.GetValue("_serviceBlockId", typeof(Guid))].Blocks[(Guid)info.GetValue("_id", typeof(Guid))]) 
        {
            PerformTime = info.GetDateTime("_performTime");
            NextCheckTime = info.GetDateTime("_nextCheckTime");
        }


        public AsyncBlock(Block other)
            : base(other) { }

        #endregion

        private DateTime? _performTime = null;

        protected DateTime PerformTime
        {
            get
            {
                return _performTime.HasValue ? _performTime.Value : DateTime.Now;
            }
            set
            {
                if (_performTime > value || !_performTime.HasValue)
                    _performTime = value;
            }
        }

        private DateTime? _nextCheckTime = null;

        public DateTime NextCheckTime
        {
            get
            {
                return _nextCheckTime.HasValue ? _nextCheckTime.Value : DateTime.MinValue;
            }
            set
            {
                _nextCheckTime = value;
            }
        }



        /// <summary>
        /// Запрос состояния асинхронного блока
        /// </summary>
        /// <param name="msg"></param>
        /// <returns>Состояние асинхронного блока</returns>
        public abstract AsyncState IsReady(Message msg);

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_id", Id);
            info.AddValue("_serviceBlockId", ServiceBlock.Id);
            info.AddValue("_performTime", PerformTime);
            info.AddValue("_nextCheckTime", NextCheckTime);
        }
    }

    /// <summary>
    /// Базовый класс для всех видов блоков
    /// </summary>
    partial class Block// : IBlock
    {
        #region Factory
        static Dictionary<BlockTypeValue, ConstructorInfo> ctors = new Dictionary<BlockTypeValue,ConstructorInfo>();
        public static Block CreateBlock(BlockEntry entry)
        {
            BlockTypeValue type = DBCache.Blocks[entry.BlockId].BlockType;
            if (!ctors.ContainsKey(type))
            {
                Type t = Type.GetType("Cryptany.Core.Router." + type + ", Router");
                ConstructorInfo ci;
                try
                {
                    ci = t.GetConstructor(new Type[] { typeof(Block) });
                }
                catch(TypeInitializationException e)
                {
                    Functions.AddEvent("Ошибка инициализации", type + " ошибка инициализации типа", EventType.Critical, null, e.InnerException);
                    throw e.InnerException;
                }
                ctors.Add(type, ci);
            }
            ConstructorInfo c = ctors[type];
            try
            {
                Block result = (Block)c.Invoke(new object[] { DBCache.Blocks[entry.BlockId] });
                result.ServiceBlock = DBCache.ServiceBlocks[entry.ServiceBlockId];
                result.IsVerification = entry.IsVerification;
                return result;
            }
            catch (Exception e)
            {
                Functions.AddEvent("Ошибка создания блока",
                                   "Ошибка создания блока " + entry.BlockId + " в сервисе " +
                                   DBCache.ServiceBlocks[entry.ServiceBlockId].Name, EventType.Critical, null, e);
                return null;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initialized empty block
        /// </summary>
        public Block(){}

        /// <summary>
        /// Инициализирует блок с заданым именем
        /// </summary>
        /// <param name="Name">Имя нового блока</param>
        public Block(string Name)
        {
            _Name = Name;
        }

        /// <summary>
        /// Инициализирует блок по шаблону. Используется при десериализации и клонировании
        /// </summary>
        /// <param name="other"></param>
        public Block(Block other)
        {
            this._BlockTypeId = other._BlockTypeId;
            this._Id = other._Id;
            this._Name = other._Name;
            this._Settings = other._Settings;
            if (other._OutFalse != null)
                this._OutFalse = other.OutFalse;
            if (other._OutTrue != null)
                this._OutTrue = other.OutTrue;
            if (other.settingsDictionary != null)
                this.settingsDictionary = other.settingsDictionary;
            this.IsVerification = other.IsVerification;
            this.ServiceBlock = other.ServiceBlock;
        }
        #endregion

        #region Interface Implementation
        protected Dictionary<string, string> settingsDictionary;

        /// <summary>
        /// Осуществляет доступ к свойствам блока
        /// </summary>
        /// <param name="s">Имя свойства</param>
        /// <returns></returns>
        public string this[string s]
        {
            get
            {
                if (settingsDictionary.ContainsKey(s))
                    return settingsDictionary[s];
                throw new InvalidOperationException("Key is not presented in collection");
            }
            set
            {
                if (settingsDictionary.ContainsKey(s))
                    settingsDictionary[s] = value;
                throw new InvalidOperationException("Key is not presented in collection");
            }
        }

        /// <summary>
        /// Клонирует данный блок
        /// </summary>
        /// <returns>Клон</returns>
        public virtual object Clone()
        {
            throw new Exception("Clone method not overrided for block type " + BlockType);
        }

        List<Block> _OutFalse = null;
        /// <summary>
        /// Блоки которые необходимо обработать в случае неудачного выполнения текущего блока
        /// </summary>
        public List<Block> OutFalse
        {
            get
            {
                if (_OutFalse == null)
                {
                    //throw new Exception("переделай нах");
                    //Tracer.Write("\t\tCalculating Block Children " + Name);
                    Guid parentId = BlockEntry.Id;

                    var list = from l in DBCache.BlockLinks.Values
                               where l.ParentId == parentId && !l.Kind
                               select ServiceBlock.Blocks[DBCache.BlockEntries[l.ChildId].BlockId];

                    _OutFalse = list.ToList();
                }
                return _OutFalse;
            }
            set { throw new NotImplementedException(); }
        }

        List<Block> _OutTrue = null;
        /// <summary>
        /// Блоки которые необходимо обработать в случае успешного выполнения текущего блока
        /// </summary>
        public List<Block> OutTrue
        {
            get
            {
                if (_OutTrue == null)
                {
                    //throw new Exception("переделай нах");
                    //Tracer.Write("\t\tCalculating Block Children " + Name);
                    Guid parentId = BlockEntry.Id;

                    var list = from l in DBCache.BlockLinks.Values
                               where l.ParentId == parentId && l.Kind
                               select ServiceBlock.Blocks[DBCache.BlockEntries[l.ChildId].BlockId];

                    _OutTrue = list.ToList();

                }
                return _OutTrue;
            }
            set { throw new NotImplementedException(); }
        }

        List<Block> _Parents = null;
        /// <summary>
        /// Блоки ссылающиеся на текущий блок
        /// </summary>
        public List<Block> Parents
        {
            get
            {
                if (_Parents == null)
                {
                    //Tracer.Write("\t\tCalculating Block Parents " + Name);

                    Guid childId = BlockEntry.Id;

                    var list = from l in DBCache.BlockLinks.Values
                               where l.ChildId == childId && !DBCache.BlockEntries[l.ParentId].IsVerification
                               select ServiceBlock.Blocks[DBCache.BlockEntries[l.ParentId].BlockId];
                    _Parents = list.ToList();
                }
                
                return _Parents;
            }
        }

        /// <summary>
        /// Тип блока
        /// </summary>
        public BlockTypeValue BlockType
        {
            get
            {
                return DBCache.BlockTypes[this.BlockTypeId].Value;
            }
            set { }
        }

        /// <summary>
        /// Родительский сервисный блок
        /// </summary>
        public ServiceBlock ServiceBlock
        {
            get;
            set;
        }

        /// <summary>
        /// Является ли блок проверочным
        /// </summary>
        public bool IsVerification
        {
            get;
            set;
        }

        /// <summary>
        /// Получает вхождение блока в сервисный блок
        /// </summary>
        BlockEntry BlockEntry
        {
            get
            {
                return (from be in DBCache.BlockEntries.Values
                 where be.BlockId == Id && be.ServiceBlockId == ServiceBlock.Id
                 select be).Single();
            }
        }

        #endregion

        #region Others
        /// <summary>
        /// Принудительное вычисление всех дочерих и родительских блоков
        /// </summary>
        public void RecalculateChild()
        {
            List<Block> tmp = null;
            _OutFalse = null;
            tmp = OutFalse;

            _OutTrue = null;
            tmp = OutTrue;

            _Parents = null;
            tmp = Parents;
        }

        /// <summary>
        /// Обработка сообщения блоком
        /// </summary>
        /// <param name="msg">Обрабатываемое сообщение</param>
        /// <returns>Список блоков которые необходимо обработать после текущего</returns>
        public virtual Block[] Perform(Message msg)
        {
            throw new Exception("Perform method not overrided for block type " + BlockType);
        }

        partial void OnSettingsChanging(string value)
        {
            var xs = new XmlDocument();
            xs.LoadXml(value);
            settingsDictionary = new Dictionary<string, string>();
            foreach (XmlNode node in xs["BlockSettings"].ChildNodes)
                if (node.Name != "Condition")
                    break;
                else
                    settingsDictionary[node.Attributes["Property"].Value] = node.Attributes["Value"].Value;
        }

        bool TryGetSetting(string name, out string result)
        {
            if (settingsDictionary.ContainsKey(name))
            {
                result = settingsDictionary[name];
                return true;
            }
            result = null;
            return false;
        }

        public string GetRequiredParameter(string Name)
        {
            string res;
            if (TryGetSetting(Name, out res))
                return res;
            throw new InvalidOperationException("Required parameter " + Name + " is not presented in collection");
        }

        public string GetOptionalParameter(string Name, string Default)
        {
            string res;
            if (TryGetSetting(Name, out res))
                return res;
            return Default;
        }

        #endregion
    }
}
