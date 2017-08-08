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
            : base("Неизвестный тип SMPP-команды: " + commandCode)
        {
            _commandCode = commandCode;
            
        }
    }
}
