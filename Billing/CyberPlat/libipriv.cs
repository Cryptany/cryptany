/*
   CopyRight (C) 1998-2005 CyberPlat.Com. All Rights Reserved.
   e-mail: support@cyberplat.com
*/

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace org.CyberPlat
{
	public class IPrivException : System.Exception
	{
		private int _code;
		public IPrivException(int c)
		{
			_code=c;
		}
		public override string ToString()
		{
			switch(_code)
			{
				case -1: return "������ � ����������";
				case -2: return "������ ��������� ������";
				case -3: return "�������� ������ ���������";
				case -4: return "�������� �������� �� �� �����";
				case -5: return "������ �� ���������� ��������� ���������";
				case -6: return "����������� �������� ����������";
				case -7: return "����� ����� �� ������������� ����� �������";
				case -8: return "�������� ������� ����� ��������� �����";
				case -9: return "�������� ��� ���������";
				case -10: return "������ ASCII ����������� ���������";
				case -11: return "������ ASCII ������������� ���������";
				case -12: return "����������� ��� ��������������";
				case -13: return "�������������� �� ������";
				case -14: return "����� �� �������������� ���������������";
				case -15: return "���� �� ������";
				case -16: return "������ ������ �����";
				case -17: return "���� �� ����� ���� �����������";
				case -18: return "������ ������������ �������";
				case -19: return "�������� ���� � ����� �������� ������� �����������";
				case -20: return "������� �� ������������� ����������� ���������";
				case -21: return "������ �������� �����";
				case -22: return "������ ������ � ����";
				case -23: return "�������� ������ �������� �����";
				case -24: return "������ ��������� ������";
			}
			return "����� ������";
		}
		public int code
		{
			get {return _code; }
		}
	};

	public class IPrivKey
	{
		private byte[] pkey;

		public IPrivKey()
		{
			pkey=new byte[36];
		}
		public string signText(string src)
		{
			return IPriv.signText(src,this);
		}
		public string verifyText(string src)
		{
			return IPriv.verifyText(src,this);
		}		
		public void closeKey()
		{
			IPriv.closeKey(this);
		}

		public byte[] getKey()
		{
			return pkey;
		}
	};

	public class IPriv
	{
		// for internal usage only
        [DllImport("libipriv/libipriv.dll")]
		internal static extern int Crypt_Initialize();

        [DllImport("libipriv/libipriv.dll")]
		internal static extern int Crypt_Done();

        [DllImport("libipriv/libipriv.dll")]
		internal static extern int Crypt_OpenSecretKeyFromFile(int eng,
			[MarshalAs(UnmanagedType.LPStr)]string path,
			[MarshalAs(UnmanagedType.LPStr)]string passwd,
			[MarshalAs(UnmanagedType.LPArray)]byte[] pkey);

        [DllImport("libipriv/libipriv.dll")]
		internal static extern int Crypt_OpenPublicKeyFromFile(int eng,
			[MarshalAs(UnmanagedType.LPStr)]string path,
			uint keyserial,
			[MarshalAs(UnmanagedType.LPArray)]byte[] pkey,
			[MarshalAs(UnmanagedType.LPArray)]byte[] �akey);

        [DllImport("libipriv/libipriv.dll")]
		internal static extern int Crypt_Sign([MarshalAs(UnmanagedType.LPStr)]string src,
			int nsrc,[MarshalAs(UnmanagedType.LPStr)]StringBuilder dst,
			int ndst,
			[MarshalAs(UnmanagedType.LPArray)]byte[] pkey);

        [DllImport("libipriv/libipriv.dll")]
		internal static extern int Crypt_Verify([MarshalAs(UnmanagedType.LPStr)]string src,
			int nsrc,[MarshalAs(UnmanagedType.LPArray)]byte[] pdst,
			[MarshalAs(UnmanagedType.LPArray)]byte[] pndst,[MarshalAs(UnmanagedType.LPArray)]byte[] pkey);

        [DllImport("libipriv/libipriv.dll")]
		internal static extern int Crypt_CloseKey([MarshalAs(UnmanagedType.LPArray)]byte[] pkey);


		public static void Initialize()
		{
			Crypt_Initialize();
		}
		public static void Done()
		{
			Crypt_Done();
		}
		public static IPrivKey openSecretKey(string path,string passwd)
		{
			IPrivKey k=new IPrivKey();
			int rc=Crypt_OpenSecretKeyFromFile(0,path,passwd,k.getKey());
			if(rc!=0)
				throw(new IPrivException(rc));
			return k;
		}
		public static IPrivKey openPublicKey(string path,uint keyserial)
		{
			IPrivKey k=new IPrivKey();
			int rc=Crypt_OpenPublicKeyFromFile(0,path,keyserial,k.getKey(),null);
			if(rc!=0)
				throw(new IPrivException(rc));
			return k;
		}
		public static string signText(string src,IPrivKey key)
		{
			string dst;
			StringBuilder tmp=new StringBuilder(2048);
			int rc=Crypt_Sign(src,src.Length,tmp,tmp.Capacity,key.getKey());
			if(rc<0)
				throw(new IPrivException(rc));
			dst=tmp.ToString(0,rc);
			return dst;
		}
		public static string verifyText(string src,IPrivKey key)
		{
			int rc=Crypt_Verify(src,-1,null,null,key.getKey());
			if(rc!=0)
				throw(new IPrivException(rc));
			return "";
		}
		public static void closeKey(IPrivKey key)
		{
			Crypt_CloseKey(key.getKey());
		}
	};

}
