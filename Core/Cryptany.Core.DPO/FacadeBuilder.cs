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
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

namespace Cryptany.Core.DPO
{
    public class FacadeBuilder
    {
        private const string selectName = "select";
        private const string facadeClassNamePostfix = "Facade";
        private const string uniqueFacadeNamePrefix = "Facade";
        private Type _facadeType = null;
        private Type _constructedFacadeType = null;
        private PersistentStorage _ps = null;
        private static int builderCounter = 0;
		private static Dictionary<string, PersistentStorage> _psFacades = new Dictionary<string, PersistentStorage>();

        public static PersistentStorage GetPersistentStorageByFacadeBuilderUniqueName(string name)
        {
            return _psFacades[name];
        }

        public FacadeBuilder(Type entityType, PersistentStorage ps)
        {
            _ps = ps;
            _facadeType = entityType;
            Build(null);
        }

        public FacadeBuilder(Type entityType, PersistentStorage ps, string outputFileName)
        {
            _ps = ps;
            _facadeType = entityType;
            Build(outputFileName);
        }

        private void Build(string fileName)
        {
            string uniqueName = GetUniqueFacadeName();
            string output = "using System;\r\n";
            output += "using Cryptany.Core.DPO;\r\n";
            output += "using " + _facadeType.Namespace + ";\r\n";
            output += "using System.Collections.Generic;\r\n";
            output += "\r\n";
            output += "namespace " + _facadeType.Namespace + " {\r\n";
            output += "public class " + FacadeClassShortName + " {\r\n";
            output += "private string uniqueName = \"" + uniqueName + "\";\r\n";
            output += "private PersistentStorage _ps = null;\r\n";
            output += "public " + FacadeClassShortName + "(){\r\n";
            output += "_ps = FacadeBuilder.GetPersistentStorageByFacadeBuilderUniqueName(uniqueName);\r\n";
            output += "}\r\n";
            output += "public EntityCollection<" + _facadeType.FullName + "> " + SelectMethodName + "(){\r\n";
            output += "return new EntityCollection<" + _facadeType.FullName + ">(_ps.GetEntities(typeof(" + _facadeType.FullName + ")));}\r\n";
            output += "}\r\n";//class
            output += "}\r\n";//namespace
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CompilerParameters param = null;
            if ( fileName == null )
                param = new CompilerParameters(new string[] { Assembly.GetExecutingAssembly().Location, _facadeType.Assembly.Location });
            else
                param = new CompilerParameters(new string[] { Assembly.GetExecutingAssembly().Location, _facadeType.Assembly.Location }, fileName);
            CompilerResults res = codeProvider.CompileAssemblyFromSource(param, new string[] { output });
            _constructedFacadeType = res.CompiledAssembly.GetType(FacadeClassName);
            _psFacades.Add(uniqueName, Ps);
            Activator.CreateInstance(_constructedFacadeType, new object[] { });
        }

        private string GetUniqueFacadeName()
        {
            builderCounter++;
            return uniqueFacadeNamePrefix + builderCounter.ToString();
        }

        public PersistentStorage Ps
        {
            get
            {
                return _ps;
            }
        }

        public Type ConstructedFacadeType
        {
            get
            {
                return _constructedFacadeType;
            }
        }

        public string FacadeClassName
        {
            get
            {
                return FacadeType.FullName + facadeClassNamePostfix;
            }
        }

        public string FacadeClassShortName
        {
            get
            {
                return FacadeType.Name + facadeClassNamePostfix;
            }
        }


        public string SelectMethodName
        {
            get
            {
                return selectName;
            }
        }

        public Type FacadeType
        {
            get
            {
                return _facadeType;
            }
        }
    }
}
