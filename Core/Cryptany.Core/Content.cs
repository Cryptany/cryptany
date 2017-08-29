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
using System.Diagnostics;

namespace Cryptany.Core
{
	/// <summary>
	/// Абстрактный класс. Определяет интерфейc для подклассов и реализует вспомогательную функциональность для формирования данных для отправки.
	/// </summary>
	[System.Xml.Serialization.XmlInclude(typeof(TextContent))]
	[Serializable]
	public abstract class Content
	{
		
		///// <summary>
		///// Конструктор по-умолчанию
		///// </summary>
		public Content() {}


		/// <summary>
		/// Абстрактное проперти. Возвращает бинарные данные для отправки сообщения.
		/// </summary>
		public abstract byte[] Body
		{
			get;
		}

		/// <summary>
		/// Body закодированное в виде строки в шестнадцатиричном формате.
		/// </summary>
		public string HexEncodedBody
		{
			get
			{
				System.Text.StringBuilder res = new System.Text.StringBuilder();
				byte[] bytes = this.Body;
				if ((bytes == null) || (bytes.Length == 0))
					return "";
				else
				{
					for (int i = 0; i < bytes.Length; i++)
						res.AppendFormat("{0,2:X2}", bytes[i]);
					return res.ToString();
				}
			}
		}


		protected ContentTypes _ContentType;

		/// <summary>
		/// Тип контента
		/// </summary>
		public virtual ContentTypes ContentType
		{
			get
			{
				return _ContentType;
			}
			set
			{
				_ContentType = value;
			}
		}


		protected EncodingTypes _Encoding;
		/// <summary>
		/// Тип кодировки
		/// </summary>
		public EncodingTypes Encoding
		{
			get
			{
				return _Encoding;
			}
			set
			{
				_Encoding = value;
			}
		}

	/// <summary>
	/// Типы контента для отправки.
	/// <remarks>Вводить новые элементы перечисления только если полностью понимаете что и где от этого может развалиться :)</remarks>
	/// </summary>
	public enum ContentTypes
	{
		/// <summary>
		/// Текст
		/// </summary>
		Text = 1,
		/// <summary>
		/// Картинка
		/// </summary>
		Picture,
		/// <summary>
		/// Логотип
		/// </summary>
		Logo,
		/// <summary>
		/// Моно мелодия
		/// </summary>
		MonoMelody,
		/// <summary>
		/// Фиг его знает что.
		/// </summary>
		Vcard
	}

	/// <summary>
	/// Типы кодировок.
	/// <remarks>Вводить новые элементы перечисления только если полностью понимаете что и где от этого может развалиться :)</remarks>
	/// </summary>
	public enum EncodingTypes
	{
		/// <summary>
		/// 
		/// </summary>
		NokiaSMS = 1,
		/// <summary>
		/// EMS сообщение
		/// </summary>
		EMS,
		/// <summary>
		/// wap push сообщение
		/// </summary>
		WAPPush,
		/// <summary>
		/// Простой текст
		/// </summary>
		Text,
		/// <summary>
		/// MMS сообщение
		/// </summary>
		MMS,
		/// <summary>
		/// кодировка Siemens
		/// </summary>
		Siemens
	}
	}

	/// <summary>
	/// Простое текстовое сообщение.
	/// </summary>
	[Serializable]
	public class TextContent : Content
	{
		public bool isUnicode;         // флаг кодировки: isUnicode = false - ASCII, isUnicode = true - BigEndianUnicode

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public TextContent()
		{
			this.isUnicode = false;
		}

		/// <summary>
		/// Конструктор, инициализирующий объект заданной строкой
		/// </summary>
		public TextContent(string msgText)
		{
			_MsgText = msgText;
			_Encoding = EncodingTypes.Text;
			_ContentType = ContentTypes.Text;
			this.isUnicode = false;
		}

		private string _MsgText;

		/// <summary>
		/// Текст сообщения (get/set)
		/// </summary>
		public string MsgText
		{
			get { return _MsgText; }
			set { _MsgText = value; }
		}

		/// <summary>
		/// Возвращает закодированное тело сообщения в ASCII (только английский текст)
		/// или BigEndianUnicode кодировке. (get)
		/// </summary>
		public override byte[] Body
		{
			get
			{
				byte[] result;
				char[] chars = MsgText.ToCharArray();
				bool bASCIIOnly = true;
				for (int i = 0; i < chars.Length; i++)
				{
					if (chars[i] >= 127)
					{
						bASCIIOnly = false;
						break;
					}
				}
				if (bASCIIOnly)
				{
					isUnicode = false;
					result = System.Text.Encoding.ASCII.GetBytes(MsgText);
				}
				else
				{
					isUnicode = true;
					result = System.Text.Encoding.BigEndianUnicode.GetBytes(MsgText);
				}
				return result;
			}
		}

		public override string ToString()
		{
			return _MsgText;
		}
	}


	/// <summary>
	/// Ссылка на контент. Предназначен для формирования ссылки на контент помещённый в базу.
	/// </summary>
	/// <remarks>
	/// Использует формат WAP Push или simple text, в зависимости от настроек.
	/// Для формирования бинарных данных использует класс WapPushContent или TextContent.
	/// </remarks>
    [Serializable]
    public class LinkToContent : Content
    {
        /// <summary>
        /// Конструктор по-умолчанию
        /// </summary>
        public LinkToContent() { }

        /// <summary>
        /// Конструктор инициализирующий объект идентификатором контента в каталоге
        /// </summary>
        /// <param name="contentCatalogID"></param>
        public LinkToContent(int contentCatalogID)
        {
            _ContentCatalogID = contentCatalogID;
            _ContentType = ContentTypes.Text;
            //if ((bool)CoreClassFactory.Settings["ContentLinksAsWapPush"])
            //    _Encoding = EncodingTypes.WAPPush;
            //else
                _Encoding = EncodingTypes.Text;
        }


        private int _ContentCatalogID;

        /// <summary>
        /// ID контента в каталоге контента. (get/set)
        /// </summary>
        public int ContentCatalogID
        {
            get { return _ContentCatalogID; }
            set { _ContentCatalogID = value; }
        }

        /// <summary>
        /// Возвращает бинарные данные для отправки. В зависимости от настройки 'ContentLinksAsWapPush'
        /// использует аналогичный метод класса WapPushContent или TextContent. (get)
        /// </summary>
        public override byte[] Body
        {
            get
            {
                Content content = null;
                //if ((bool)CoreClassFactory.Settings["ContentLinksAsWapPush"])
                //{
                //    content = new WapPushContent(ContentURL, CoreClassFactory.Settings["DefaultWapPushContentTitle"] as string);
                //}
                //else
                //{
                    content = new TextContent(ContentURL);
                //}
                return content.Body;
            }
        }

        private string _Nonce;

        /// <summary>
        /// Возвращает ссылку на контент вида http://BaseContentDownloadURL/Nonce .
        /// Данное свойство следует вызывать только после вызова метода AddToDownloads(...)
        /// <remarks>BaseContentDownloadURL - значение из настроек</remarks>
        /// </summary>
        public string ContentURL
        {
            get
            {
                string s = "http://dl.mobster.ru/" + _Nonce;
                Trace.Write("Returning content URL: " + s);
                return s;
            }
        }
	}
}