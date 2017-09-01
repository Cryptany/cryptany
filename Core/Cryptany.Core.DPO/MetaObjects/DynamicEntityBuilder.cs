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
using Cryptany.Core.DPO.MetaObjects.DynamicEntityBuilding;
using System.Xml;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using Cryptany.Core.DPO.MetaObjects;

namespace Cryptany.Core.DPO
{
	public class DynamicEntityBuilder: EntityBase, IObjectFactory
	{
		private string _name;
		private string _xmlSource;
		private bool _sourceAssigned = false;
		private Type _type;

		public DynamicEntityBuilder()
		{
		}

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		public string XmlSource
		{
			get
			{
				return _xmlSource;
			}
			set
			{
				if ( _sourceAssigned )
					throw new InvalidOperationException("The value for the property XmlSource have already been set");
				_xmlSource = value;
				if ( Build() )
					_sourceAssigned = true;
				else
					throw new System.IO.FileLoadException("The entity description cannot be parsed");
			}
		}

		public Type Type
		{
			get
			{
				return _type;
			}
		}

		public object CreateObject(Type type)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public object CreateObject(Type type, params object[] args)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		private bool Build()
		{
			StringReader sr = new StringReader(_xmlSource);
			XmlReader reader = XmlReader.Create(sr);
			DynamicEntityDescription ded = DynamicEntityDescription.Create(reader);
			string code = ded.Compile();
			CSharpCodeProvider codeProvider = new CSharpCodeProvider();
			CompilerParameters param = new CompilerParameters(new string[] { Assembly.GetExecutingAssembly().Location });
			CompilerResults res = codeProvider.CompileAssemblyFromSource(param, new string[] { code });
			_type = res.CompiledAssembly.GetType(ded.FullName);
			RegisterFactory();
			return true;
		}

		private void RegisterFactory()
		{
			if ( _type == null )
				throw new System.IO.FileLoadException("The entity description cannot be parsed");
			ClassFactory.SetObjectFactory(_type, this);
		}
	}
}
