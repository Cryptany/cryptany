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
using System.Text;

namespace Cryptany.Common.Constants
{
/*	public enum Product
	{
		/// <summary>
		/// Цветные картинки
		/// </summary>
		ColorPicture = 1,
		/// <summary>
		/// Логотипы
		/// </summary>
		Logo,
		/// <summary>
		/// Полифонические мелодии
		/// </summary>
		PolySound,
		/// <summary>
		/// Реалтоны MP3
		/// </summary>
		Realton,
		/// <summary>
		/// Видео
		/// </summary>
		Video,
		/// <summary>
		/// Темы
		/// </summary>
		Theme,
		/// <summary>
		/// Игры
		/// </summary>
		Game,
		/// <summary>
		/// Приложения
		/// </summary>
		Application,
		/// <summary>
		/// Монофонические мелодии
		/// </summary>
		MonoSound
	}
*/
	/// <summary>
	/// Типы продуктов. По мере добавления в базу необходимо обновлять перечисление.
	/// </summary>
	public enum ProductType
	{
		/// <summary>
		/// Мелодии
		/// </summary>
		Melody = 1,
		/// <summary>
		/// Картинки
		/// </summary>
		Picture,
		/// <summary>
		/// Приложения
		/// </summary>
		Application,
		/// <summary>
		/// Пакет контента
		/// </summary>
		ContentPack,
        /// <summary>
        /// Текстовый кусок
        /// </summary>
        Text
	}


	/// <summary>
	/// Части java приложения
	/// </summary>
	public enum ApplicationPart
	{
		/// <summary>
		/// Java Archive
		/// </summary>
		JAR,
		/// <summary>
		/// Java Archive Description
		/// </summary>
		JAD
	}

	/// <summary>
	/// Типы цифровых контента
	/// </summary>
	public enum ContentCodeType
	{ 
		/// <summary>
		/// Коды контента для печатной ректамы
		/// </summary>
		Printable = 1,
		/// <summary>
		/// Коды контента для партнёрской программы
		/// </summary>
		B2B
	}

	/// <summary>
	/// Источники поступления денег
	/// </summary>
	public enum MoneyIncomeType
	{ 
		/// <summary>
		/// Оплата через WAP CPA
		/// </summary>
		WAPCPAPayment = 1,
		/// <summary>
		/// Оплата через отправку сообщения на сервисный номер (абонент отправил нам сообщение)
		/// </summary>
		IncomingMessagePayment = 2,
		/// <summary>
		/// Оплата через получение сообщения с зарезервированного номера (мы отправили сообщение абоненту)
		/// </summary>
		OutgoingMessagePayment = 3
	}


	/// <summary>
	/// Типы ответных сообщений
	/// </summary>
	public enum AnswerMessageType
	{
		/// <summary>
		/// Simple text message
		/// </summary>
		Text = 1,
		/// <summary>
		/// Answer text will be sent as wap push message
		/// </summary>
		WapPush,
		/// <summary>
		/// Binary content
		/// </summary>
		Content,
		/// <summary>
		/// Link to existing content
		/// </summary>
		LinkToContent
	}

	/// <summary>
	/// Тибы блоков ответов. По мере обновления базы (services.AnswerBlockTypes), необходимо обновлять перечисление.
	/// </summary>
	public enum AnswerBlockType
	{
		/// <summary>
		/// Вырожденный блок содержащий всего обно сообщение
		/// </summary>
		SingleMessageBlock,
		/// <summary>
		/// Циклический блок. По мере обращения абонента и попадания его на соответствующий блок, ему отсылается одно из сообщений по очереди.
		/// </summary>
		CycleBlock,
		/// <summary>
		/// Блок с неопределённым назначением, т.е. логика обработки блока заранее не определена и полностью опрелеляется логикой работы сервиса.
		/// </summary>
		VariantBlock
	}



	/// <summary>
	/// Продукты. См. таблицу products.
	/// </summary>
	public enum Products
	{
		/// <summary>
		/// Цветные картинки
		/// </summary>
		ColorPicture = 1,
		/// <summary>
		/// Логотипы
		/// </summary>
		Logo,
		/// <summary>
		/// Полифонические мелодии
		/// </summary>
		PolySound,
		/// <summary>
		/// Реалтоны MP3
		/// </summary>
		Realton,
		/// <summary>
		/// Видео
		/// </summary>
		Video,
		/// <summary>
		/// Темы
		/// </summary>
		Theme,
		/// <summary>
		/// Игры
		/// </summary>
		Game,
		/// <summary>
		/// Приложения
		/// </summary>
		Application,
		/// <summary>
		/// Монофонические мелодии
		/// </summary>
		MonoSound,
        /// <summary>
        /// Контент-паки
        /// </summary>
        ContentPacks,
        /// <summary>
        /// Полные MP3
        /// </summary>
        FullMP3,
        /// <summary>
        /// Суперзвуки
        /// </summary>
        Supersounds,
        /// <summary>
        /// Тексты
        /// </summary>
        Text,
        /// <summary>
        /// Демоигры
        /// </summary>
        Demogames,
        /// <summary>
        /// Галереи
        /// </summary>
       Galleries = 17
	}

    /// <summary>
    /// Типы шаблонов для перенаправления. По мере обновления таблицы kernel.DownloadTemplateTarget в БД, необходимо обновлять перечисление.
    /// </summary>
    public enum DownloadTemplateTargetEnum
    {
        Unknown = 0,
        /// <summary>
        /// Шаблон указывает на страницу уведомления
        /// </summary>
        Notification = 1,
        /// <summary>
        /// Шаблон указывает на корзину клиента
        /// </summary>
        ShoppingCart = 2
    }
}
