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

namespace Cryptany.Core.DPO.Configuration
{
    public enum ErrorType
    {
        Unknown,
        InvalidXmlDocument,
        UnexpectedNode,
        InvalidStructural
    }

    public class EntityConfigurationError
    {
        private ErrorType _errorType = ErrorType.Unknown;
        private string _message;

        public EntityConfigurationError()
        {
        }

        public EntityConfigurationError(string message)
        {
            _message = message;
        }

        public EntityConfigurationError(ErrorType errorType, string message)
        {
            _errorType = errorType;
            _message = message;
        }

        public string Message
        {
            get
            {
                return _message;
            }
        }

        public ErrorType ErrorType
        {
            get
            {
                return _errorType;
            }
        }

        public override string ToString()
        {
            string res = string.Format("Error type: '{0}', Message: '{1}'", _errorType.ToString(), _message);
            return res;
        }
    }
}
