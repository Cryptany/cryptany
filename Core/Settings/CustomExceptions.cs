using System;

namespace avantMobile.Settings
{

	/// <summary>
	/// ����������. ��������������� � ���, ��� �� ������� ������������� ������ �� ���������� ������������� � �����-���� ������.
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
		/// ����������� �� ���������
		/// </summary>
		public ConvertFromStringException() { }

		/// <summary>
		/// ����������� ����������� ������������ ������������� ���������
		/// </summary>
		/// <param name="message"></param>
		public ConvertFromStringException(string message) : base(message) { }

		/// <summary>
		/// ����������� ����������� ������������ ������������� ��������� � ���������� ����������.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="inner"></param>
		public ConvertFromStringException(string message, Exception inner) : base(message, inner) { }
		/// <summary>
		/// �� ���� �������������� �����������. ��. �������� System.Exception
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected ConvertFromStringException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

}
