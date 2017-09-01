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
	/// <summary>
	/// Вспомогательное перечисление. Допустимые форматы в которых может быть передан ответ от клиента агрегации.
	/// </summary>
	public enum AggregatorMessageFormats
	{
		/// <summary>
		/// Простой текст
		/// </summary>
		SimpleText,
		/// <summary>
		/// Сообщение в формате WAP push
		/// </summary>
		WapPush,
		/// <summary>
		/// Изображение в формате nokia
		/// </summary>
		PictureNokia,
		/// <summary>
		/// Изображение в формате Siemens
		/// </summary>
		PictureSiemens,
		/// <summary>
		/// Изображение в формате EMS
		/// </summary>
		PictureEMS,
		/// <summary>
		/// Монофония в формате RTTTL
		/// </summary>
		SoundRTTTL,
		/// <summary>
		/// Монофония в формате IMelody
		/// </summary>
		SoundIMelody
	}

	/// <summary>
	/// Результат перенаправления сообщения клиенту агрегации
	/// </summary>
	public enum SendMessageResult
	{
		/// <summary>
		/// Сообщение успешно принято
		/// </summary>
		OK,
		/// <summary>
		/// Сообщение не принято по неизвестной причине
		/// </summary>
		FailedUnknownReason,
		/// <summary>
		/// Ответное сообщение не принято по причине неверного идентификатора сессии
		/// </summary>
		FailedUnknownSession,
		/// <summary>
		/// Ответное сообщение не принято по причине превышения максимального количества ответных сообщений в сессии
		/// </summary>
		FailedSessionMessageLimit,
		/// <summary>
		/// Неверный идентификатор сообщения
		/// </summary>
		FailedBadMessageID,
		/// <summary>
		/// Передан некорректный параметр
		/// </summary>
		FailedInvalidArgument
	}

	/// <summary>
	/// Данный класс описывает ответное сообщение клиента системы агрегации
	/// </summary>
	public class AggregatorOutputMessage
	{
		private Guid _SessionID;

		/// <summary>
		/// SessionID => AggregatorInbox.id или AggregatorOutbox.sessionid
		/// </summary>
		public Guid SessionID
		{
			get { return _SessionID; }
			set { _SessionID = value; }
		}


		private Guid _MessageID;
		/// <summary>
		/// Message id => AggregatorOutbox.id. Генерируется клиентом системы агрегации
		/// </summary>
		public Guid MessageID
		{
			get { return _MessageID; }
			set { _MessageID = value; }
		}



		private string _MsgText;

		/// <summary>
		/// Message text. Valid only if Format in ( AggregatorMessageFormats.SimpleText, AggregatorMessageFormats.WapPush )
		/// </summary>
		public string MsgText
		{
			get { return _MsgText; }
			set { _MsgText = value; }
		}

		private string _MsgTitle;

		/// <summary>
		/// Заголовок сообщения Wap push. Имеет смысл только для сообщений формата AgregatorMessageFormats.WapPush
		/// </summary>
		public string MsgTitle
		{
			get { return _MsgTitle; }
			set { _MsgTitle = value; }
		}

		private byte[] _Body;

		/// <summary>
		/// Бинарное тело сообщения. Не имеет смысла для сообщений в форматах
		/// AggregatorMessageFormats.SimpleText и AggregatorMessageFormats.WapPush 
		/// </summary>
		public byte[] Body
		{
			get { return _Body; }
			set { _Body = value; }
		}



		private AggregatorMessageFormats _Format;

		/// <summary>
		/// Формат сообщения
		/// </summary>
		public AggregatorMessageFormats Format
		{
			get { return _Format; }
			set { _Format = value; }
		}


		/// <summary>
		/// Возвращает описание сообщения.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			System.Text.StringBuilder sb = new StringBuilder();
			sb.AppendFormat("Outgoing aggregation message.\r\nID: {0}\r\nText:{1}\r\nFormat:{2}",
				_MessageID, (_MsgText != null ? _MsgText : "NULL"), _Format);
			return sb.ToString();
		}
	}
}
