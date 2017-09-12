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

namespace Cryptany.Core
{
	/// <summary>
	/// Error during class creation
	/// </summary>
	[global::System.Serializable]
	public class ClassCreationException : Exception
	{
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public ClassCreationException() { }
		public ClassCreationException(string message) : base(message) { }
		public ClassCreationException(string message, Exception inner) : base(message, inner) { }
		protected ClassCreationException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

	
	/// <summary>
	/// Error during token processing
	/// </summary>
	[global::System.Serializable]
	public class MatchTokenException : Exception
	{
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public MatchTokenException() { }
		public MatchTokenException(string message) : base(message) { }
		public MatchTokenException(string message, Exception inner) : base(message, inner) { }
		protected MatchTokenException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

	/// <summary>
	/// Проблема при обработке униварсального токена
	/// </summary>
	[global::System.Serializable]
	public class MatchUniversalTokenException : MatchTokenException
	{
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public MatchUniversalTokenException() { }
		public MatchUniversalTokenException(string message) : base(message) { }
		public MatchUniversalTokenException(string message, Exception inner) : base(message, inner) { }
		protected MatchUniversalTokenException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

	/// <summary>
	/// Проблема при обработке сообщения
	/// </summary>
	[global::System.Serializable]
	public class MessageProcessingException : Exception
	{
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public MessageProcessingException() { }
		public MessageProcessingException(string message) : base(message) { }
		public MessageProcessingException(string message, Exception inner) : base(message, inner) { }
		protected MessageProcessingException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
	
	/// <summary>
	/// Проблема при отправке сообщения
	/// </summary>
	[global::System.Serializable]
	public class MessageSendingException : Exception
	{
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public MessageSendingException() { }
		public MessageSendingException(string message) : base(message) { }
		public MessageSendingException(string message, Exception inner) : base(message, inner) { }
		protected MessageSendingException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}


}
