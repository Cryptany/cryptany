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
using System.Collections.Generic;
using System.Text;
using Cryptany.Core.ConfigOM.Interfaces;
using Cryptany.Core.DPO;
using Cryptany.Core.DPO.MetaObjects.Attributes;
using Cryptany.Core.DPO.MetaObjects;

namespace Cryptany.Core.ConfigOM
{
	[Serializable]
	[DbSchema("services")]
	[Table("Channels", "ServiceID", ConditionOperation.Equals, "3864a580-3f65-4343-a5c4-a942c4492b30")]
	public class GlobalErrorChannel : TvadChannel
	{
	}
}
