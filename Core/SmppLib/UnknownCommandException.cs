using System;

namespace avantMobile.SmppLib
{
    public class UnknownCommandException : Exception
    {
        private readonly uint _commandCode;
        public uint CommandCode
        {
            get
            {
                return _commandCode;
            }
        }

        public UnknownCommandException(uint commandCode)
            : base("����������� ��� SMPP-�������: " + commandCode)
        {
            _commandCode = commandCode;
            
        }
    }
}
