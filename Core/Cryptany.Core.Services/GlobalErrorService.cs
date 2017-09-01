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
using Cryptany.Core.ConfigOM;
using Cryptany.Core.Services;

namespace Cryptany.Core
{
    public class GlobalErrorService : TVADService
    {
        public GlobalErrorService(IRouter router, string serviceName) : base(router, serviceName)
        {
        }
        public GlobalErrorService(IRouter router) : base(router)
        {
        }
    }
}