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
using Cryptany.Core.Base.MacrosProcessors;
using Cryptany.Core.DB;
using Cryptany.Core;

namespace Cryptany.Core.Base
{
	public static partial class ActionExecuter
	{
	
		private static bool ProcessMacros(IDictionary<string,MacrosProcessor> processors, OutputMessage omsg, ref string errorText)
		{

            List<Macros> macroses = Macros.GetMacroses(((TextContent)omsg.Content).MsgText);
		    for(int i=macroses.Count-1;i>=0;i--)
            {
                Macros m = macroses[i]; 
                MacrosProcessor mp = processors[m.Name];
                if (mp!=null)
                {
                    string replaceString = mp.Execute(omsg,m.Parameters);
                    ((TextContent)omsg.Content).MsgText = ((TextContent)omsg.Content).MsgText.Replace(m.Text, replaceString);
                }
            }
            
            return true;

		}

        public static bool ProcessMacros(OutputMessage outMsg, Guid cr, ref string errorText)
		{

            Dictionary<string, MacrosProcessor> processors = new Dictionary<string, MacrosProcessor>();
           
            ContentProcessor cpn = new ContentProcessor(cr);
             processors.Add(cpn.MacrosName, cpn);
           
		    ServiceNumberProcessor snp = new ServiceNumberProcessor();
            processors.Add(snp.MacrosName, snp);

            HelpDeskProcessor hdp = new HelpDeskProcessor();
            processors.Add(hdp.MacrosName,hdp);
		    
		    return ProcessMacros(processors, outMsg, ref errorText);
            
		}
	}
}
