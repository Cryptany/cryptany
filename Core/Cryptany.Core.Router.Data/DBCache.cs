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
using System.Diagnostics;
using Cryptany.Common.Utils;
using Cryptany.Core.Monitoring;

namespace Cryptany.Core.Router.Data
{

    public struct TariffKey
    {
        public Guid OperatorId;
        public Guid ServiceNumberId;
    }

    /// <summary>
    /// Локальное хранилище необходимого куска базы данных
    /// </summary>
    public static class DBCache
    {

        static Entities data;

        /// <summary>
        /// Получает набор блоков
        /// </summary>
        public static Dictionary<Guid, Block> Blocks { get; private set; }

        /// <summary>
        /// Получает набор связей между блоками
        /// </summary>
        public static Dictionary<Guid, BlockLink> BlockLinks { get; private set; }

        /// <summary>
        /// Получает набор типов блоков
        /// </summary>
        public static Dictionary<Guid, BlockType> BlockTypes { get; private set; }

        /// <summary>
        /// Получает набор токенов
        /// </summary>
        public static Dictionary<Guid, Token> Tokens { get; private set; }

        /// <summary>
        /// Получает набор регулярных выражений
        /// </summary>
        public static Dictionary<Guid, TokenRegex> RegularExpressions { get; private set; }

        /// <summary>
        /// Получает набор ТВ и ошибочных сервисов
        /// </summary>
        public static Dictionary<Guid, TVService> Services { get; private set; }

        /// <summary>
        /// Получает набор сервмсных блоков
        /// </summary>
        public static Dictionary<Guid, ServiceBlock> ServiceBlocks { get; private set; }

        /// <summary>
        /// Получает набор типов сервисов
        /// </summary>
        public static Dictionary<Guid, ServiceType> ServiceTypes { get; private set; }

        /// <summary>
        /// Получает связи блоков с сервисными блоками
        /// </summary>
        public static Dictionary<Guid, BlockEntry> BlockEntries { get; private set; }

        /// <summary>
        /// Получает список тарифов
        /// </summary>
        public static Dictionary<Guid, Tariff> Tariffs { get; private set; }

        /// <summary>
        /// Получает список сервисных номеров
        /// </summary>
        public static Dictionary<Guid, ServiceNumber> ServiceNumbers { get; private set; }

        /// <summary>
        /// Принудительно ререзагружает наборы из базы данных
        /// </summary>
        static public void Refresh()
        {
            ServiceTypes = data.ServiceType.ToDictionary(a => a.Id);
            BlockTypes = data.BlockType.ToDictionary(a => a.Id);
            RegularExpressions = data.TokenRegex.ToDictionary(a => a.Id);
            Tokens = data.Token.ToDictionary(a => a.Id);
            Services = data.Service.ToDictionary(a => a.Id);
            Blocks = data.Block.ToDictionary(a => a.Id);
            BlockLinks = data.BlockLink.ToDictionary(a => a.Id);
            ServiceBlocks = data.ServiceBlock.ToDictionary(a => a.Id);
            BlockEntries = data.BlockEntry.ToDictionary(a => a.Id);
            ServiceNumbers = data.ServiceNumbers.ToDictionary(a => a.Id);
            Tariffs = data.Tariff.Where(t => t.IsActive.Value).ToDictionary(a => a.Id); 
            //if (_MaterializeBlock != null)
            //    foreach (var k in Blocks.Keys)
            //        Blocks[k] = _MaterializeBlock(Blocks[k]);

        }

        static DBCache()
        {
            try
            {
                data = Entities.Data;
                Refresh();
            }
            catch (Exception e)
            {
                //Tracer.Write("Ошибка загрузки энтитей из БД. "+ e);
                Functions.AddEvent("Ошибка загрузки из БД", "Ошибка загрузки настроек роутера из базы данных", EventType.Error, null, e);
            }

        }
    }
}
