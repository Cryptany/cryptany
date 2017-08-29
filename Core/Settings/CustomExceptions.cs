using System;

namespace Cryptany.Core.Settings
{

	/// <summary>
	/// Exception when it will be some conversion error
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
		/// Default constructor
		/// </summary>
		public ConvertFromStringException() { }

		/// <summary>
		/// User-defined message constructor
		/// </summary>
		/// <param name="message"></param>
		public ConvertFromStringException(string message) : base(message) { }

		/// <summary>
		/// User-define message and inner exception constructor
		/// </summary>
		/// <param name="message"></param>
		/// <param name="inner"></param>
		public ConvertFromStringException(string message, Exception inner) : base(message, inner) { }
		/// <summary>
		/// Serializing constructor, derived from System.Exception
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected ConvertFromStringException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

}
