using System;

namespace avantMobile.Settings
{

	/// <summary>
	/// »сключение. —видетельствует о том, что не удалось преобразовать данные из строкового представление в какое-либо другое.
	/// </summary>
	[global::System.Serializable]
	public class ConvertFromStringException : Exception
	{
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		/// <summary>
		///  онструктор по умолчанию
		/// </summary>
		public ConvertFromStringException() { }

		/// <summary>
		///  онструктор принимающий определ€емое пользователем сообщение
		/// </summary>
		/// <param name="message"></param>
		public ConvertFromStringException(string message) : base(message) { }

		/// <summary>
		///  онструктор принимающий определ€емое пользователем сообщение и внутреннее исключение.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="inner"></param>
		public ConvertFromStringException(string message, Exception inner) : base(message, inner) { }
		/// <summary>
		/// ≈дЄ один унаследованный конструктор. —м. описание System.Exception
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected ConvertFromStringException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

}
