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
	/// Структура описывающая формат мелодии.
	/// </summary>
	public struct MelodyFormat
	{
		/// <summary>
		/// Формат MMF
		/// </summary>
		public static readonly MelodyFormat MMF = new MelodyFormat("CCEAD286-874E-4E03-9797-2202F21922FD", "MMF", "application/vnd.smaf", "mmf");
		/// <summary>
		/// Формат IMelody
		/// </summary>
		public static readonly MelodyFormat IMelody = new MelodyFormat("5582315C-7323-4491-B67D-27567185CE32", "IMelody", "IMYaudio/imy", "imy");
		/// <summary>
		/// Формат MMF с виброзвонком
		/// </summary>
		public static readonly MelodyFormat MMFVibro = new MelodyFormat("6389B886-0E91-4B7A-9426-380CF3C1FE94", "MMF+vibro", "application/vnd.smaf", "mmf");
		/// <summary>
		/// Формат WAV
		/// </summary>
		public static readonly MelodyFormat WAV = new MelodyFormat("DDC06B53-E61D-4CD7-AFBB-6784779E9D53", "WAV", "audio/wav", "wav");
		/// <summary>
		/// Формат Nokia RTTTL
		/// </summary>
		public static readonly MelodyFormat RTTTL = new MelodyFormat("230E2DD9-F3B4-4F01-BE23-6F5087DEB875", "Nokia RTTTL", "audio/rtt", "txt");
		/// <summary>
		/// Формат AMR
		/// </summary>
		public static readonly MelodyFormat AMR = new MelodyFormat("32183F86-5D5B-4AB1-A0ED-ABFC049F5B9A", "AMR", "audio/amr", "amr");
		/// <summary>
		/// Формат MP3
		/// </summary>
		public static readonly MelodyFormat MP3 = new MelodyFormat("2FB4F283-9390-4058-9521-DCAFD2D50E21", "MP3", "audio/mp3", "mp3");
		/// <summary>
		/// Формат MMF c голосом
		/// </summary>
		public static readonly MelodyFormat MMFVoice = new MelodyFormat("80B38C75-C30B-47E1-B9E0-F6FFB1D7CAF2", "MMF+voice", "application/vnd.smaf", "mmf");
        /// <summary>
        /// MP3 размером 1600Kb
        /// </summary>
        public static readonly MelodyFormat MP3_1600 = new MelodyFormat("5d54ecc7-2991-4ac4-800d-35dbc2a937e4", "MP3 1600Kb", "audio/mp3", "mp3");
        /// <summary>
        /// MP3 размером 800Kb
        /// </summary>
        public static readonly MelodyFormat MP3_800 = new MelodyFormat("c41785f8-8617-48cf-bf7d-99bf6bce3cc0", "MP3 800Kb", "audio/mp3", "mp3");
        /// <summary>
        /// 40голосная MIDI
        /// </summary>
        public static readonly MelodyFormat MIDI40 = new MelodyFormat("5ec84c89-ec23-4c47-8959-d1a46be3c315", "MIDI 40 voices", "audio/midi", "mid");
        /// <summary>
        /// 64голосная MMF
        /// </summary>
        public static readonly MelodyFormat MMF64 = new MelodyFormat("38ced38a-8944-4f90-ab92-5caba111b198", "MMF 64 voices", "application/vnd.smaf", "mmf");
        /// <summary>
        /// MIDI с голосом
        /// </summary>
        public static readonly MelodyFormat MIDIVoice = new MelodyFormat("b6bd44c4-92b7-48de-8178-dfd8da14c3c2", "MIDI+voice", "audio/midi", "mid");
        /// <summary>
        /// 16-ти голосная MIDI
        /// </summary>
        public static readonly MelodyFormat MIDI16 = new MelodyFormat("1bea3f5e-da5d-464f-80e5-2e363a96bb31", "MIDI 16 voices", "audio/midi", "mid");
        /// <summary>
        /// 4-х голосая MIDI
        /// </summary>
        public static readonly MelodyFormat MIDI4 = new MelodyFormat("46797327-c442-48e8-a5ef-97927d08c727", "MIDI 4 voices", "audio/midi", "mid");
        /// <summary>
        /// Обрезанный MP3
        /// </summary>
        public static readonly MelodyFormat MP3_preview = new MelodyFormat("ab361280-4be0-4fcc-b7c4-d640251de27a", "MP3 preview", "audio/mp3", "mp3");
        /// <summary>
		/// ID в базе данных
		/// </summary>
		public readonly Guid ID;
		/// <summary>
		/// Название формата
		/// </summary>
		public readonly string Name;
		/// <summary>
		/// MIME type формата
		/// </summary>
		public readonly string MIME_Type;
		/// <summary>
		/// Расширение файла
		/// </summary>
		public readonly string Extention;

		/// <summary>
		/// Конструктор он конструктор и есть :)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="name"></param>
		/// <param name="mime"></param>
		/// <param name="extention"></param>
		public MelodyFormat(string id, string name, string mime, string extention)
		{
			ID = new Guid(id);
			Name = name;
			MIME_Type = mime;
			Extention = extention;
		}

		/// <summary>
		/// Возвращает название формата.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Name;
		}


	}

	/// <summary>
	/// Структура описывающая формат картинки
	/// </summary>
	public struct PictureFormat
	{

		/// <summary>
		/// ID в базе данных
		/// </summary>
		public readonly Guid ID;
		/// <summary>
		/// Название формата
		/// </summary>
		public readonly string Name;
		/// <summary>
		/// MIME type формата
		/// </summary>
		public readonly string MIME_Type;
		/// <summary>
		/// Расширение файла
		/// </summary>
		public readonly string Extention;

		/// <summary>
		/// Конструктор он конструктор и есть :)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="name"></param>
		/// <param name="mime"></param>
		/// <param name="extention"></param>
		public PictureFormat(string id, string name, string mime, string extention)
		{
			ID = new Guid(id);
			Name = name;
			MIME_Type = mime;
			Extention = extention;
		}

		/// <summary>
		/// Возвращает название формата.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Name;
		}

        /// <summary>
        /// Тип картинки Siemens
        /// </summary>
		public static readonly PictureFormat Siemens = new PictureFormat("A97C73F2-2E7B-426B-8433-3F4FF839099C", "Siemens", "image/wbmp", "bmp");
		/// <summary>
		/// Тип видео -- MP4
		/// </summary>
        public static readonly PictureFormat VideoMP4 = new PictureFormat("3EC31759-9F84-471E-B89A-440C8F81BCB2", "Video MP4", "video/mpeg4", "mp4");
		/// <summary>
		/// Тип видео 3GPP
		/// </summary>
        public static readonly PictureFormat Video3GPP = new PictureFormat("8CB869CB-2EE6-44A6-B7A4-BDE00F93F77D", "Video 3GPP", "video/3gpp", "3gp");
		/// <summary>
		/// Тип картинки Nokia W-BMP
		/// </summary>
        public static readonly PictureFormat NokiaWbmp = new PictureFormat("A59AF442-6DD6-4533-A8EE-CE8CB3B718FD", "Nokia wbmp", "image/wbmp", "ota");
		/// <summary>
		/// Тип картинки JPEG 174x132
		/// </summary>
        public static readonly PictureFormat ColorJPEG174x132 = new PictureFormat("03CF630A-FF49-4971-B3D6-D48A0C47577A", "Color JPEG 174x132", "image/jpeg", "jpg");
        /// <summary>
        /// Тип картинки JPEG 128x128
        /// </summary>
        public static readonly PictureFormat ColorJPEG128x128 = new PictureFormat("07E1C5C4-30A1-4D8F-B2F0-EA297C7D9F6B", "Color JPEG 128x128", "image/jpeg", "jpg");
        /// <summary>
        /// Тип картинки JPEG 128x160
        /// </summary>
        public static readonly PictureFormat ColorJPEG128x160 = new PictureFormat("703ca01a-4ec2-494c-b57b-370c840348bf", "Color JPEG 128x160", "image/jpeg", "jpg");

	}
}
