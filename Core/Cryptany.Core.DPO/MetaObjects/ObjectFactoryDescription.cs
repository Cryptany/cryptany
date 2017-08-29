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

namespace Cryptany.Core.DPO.MetaObjects
{
	public enum ObjectFactoryType
	{
		Class,
		Delegate
	}

	public delegate object DefaultConstruction(Type type);
	public delegate object ConstructionWithArgs(Type type, params object[] args);

	public class ObjectFactoryDescription
	{
		private ObjectFactoryType _factoryType;
		private IObjectFactory _objectFactory = null;
		private DefaultConstruction _default;
		private ConstructionWithArgs _params;

		public ObjectFactoryDescription(IObjectFactory objectFactory)
		{
			_objectFactory = objectFactory;
			_factoryType = ObjectFactoryType.Class;
		}

		public ObjectFactoryDescription(DefaultConstruction defaultConstruction,
			ConstructionWithArgs constructionWithArgs)
		{
			_default = defaultConstruction;
			_params = constructionWithArgs;
			_factoryType = ObjectFactoryType.Delegate;
		}

		public ObjectFactoryType FactoryType
		{
			get
			{
				return _factoryType;
			}
		}

		public object InvokeDefaultConstruction(Type type)
		{
			if ( FactoryType == ObjectFactoryType.Class )
				return _objectFactory.CreateObject(type);
			if ( FactoryType == ObjectFactoryType.Delegate )
				return _default(type);
			return null;
		}

		public object InvokeConstructionWithArgs(Type type, params object[] args)
		{
			if ( FactoryType == ObjectFactoryType.Class )
				return _objectFactory.CreateObject(type, args);
			if ( FactoryType == ObjectFactoryType.Delegate )
				return _params(type, args);
			return null;
		}
	}
}
