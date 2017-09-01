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
	public class InstanceDescription
	{
		private object _describedInstance;
		private ObjectDescription _objectDescription;
		private bool _isEntity;
		private bool _isEntityBase;
		private List<InstancePropertyDescription> _properties = new List<InstancePropertyDescription>();

		internal InstanceDescription(object instance)
		{
			_describedInstance = instance;
		    _objectDescription = ClassFactory.GetObjectDescription(_describedInstance.GetType(),
		                                                           ClassFactory.CreatePersistentStorage("Default"));
					
			_isEntity = _objectDescription.IsEntity;
			_isEntityBase = _objectDescription.IsDescendentFrom(typeof(EntityBase));
		}

		public object DescribedInstance
		{
			get
			{
				return _describedInstance;
			}
		}

		public ObjectDescription ObjectDescription
		{
			get
			{
				return _objectDescription;
			}
		}

		public bool IsEntity
		{
			get
			{
				return _isEntity;
			}
		}

		public bool IsEntityBase
		{
			get
			{
				return _isEntityBase;
			}
		}

		public List<InstancePropertyDescription> Properties
		{
			get
			{
				return _properties;
			}
		}
		
		//public List<IEntity> GetRelatedOjects(PropertyDescription property)
		//{
		//    if ( !IsEntityBase )
		//        throw new ApplicationException("The DescribedInstance should be an instance of a type derived from Entity");
		//    if ( !property.IsRelation )
		//        throw new ApplicationException("The property should be a relation");
		//    return ( DescribedInstance as EntityBase ).PersistentStorage.GetEntities(property.RelatedType);
		//}
	}
}
