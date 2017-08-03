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
				case -1: return "Ошибка в аргументах";
				case -2: return "Ошибка выделения памяти";
				case -3: return "Неверный формат документа";
				case -4: return "Документ прочитан не до конца";
				case -5: return "Ошибка во внутренней структуре документа";
				case -6: return "Неизвестный алгоритм шифрования";
				case -7: return "Длина ключа не соответствует длине подписи";
				case -8: return "Неверная кодовая фраза закрытого ключа";
				case -9: return "Неверный тип документа";
				case -10: return "Ошибка ASCII кодирования документа";
				case -11: return "Ошибка ASCII декодирования документа";
				case -12: return "Неизвестный тип криптосредства";
				case -13: return "Криптосредство не готово";
				case -14: return "Вызов не поддерживается криптосредством";
				case -15: return "Файл не найден";
				case -16: return "Ошибка чтения файла";
				case -17: return "Ключ не может быть использован";
				case -18: return "Ошибка формирования подписи";
				case -19: return "Открытый ключ с таким серийным номером отсутствует";
				case -20: return "Подпись не соответствует содержимому документа";
				case -21: return "Ошибка создания файла";
				case -22: return "Ошибка записи в файл";
				case -23: return "Неверный формат карточки ключа";
				case -24: return "Ошибка генерации ключей";
			}
			return "Общая ошибка";
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
			[MarshalAs(UnmanagedType.LPArray)]byte[] сakey);

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
