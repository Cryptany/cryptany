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
using System.Collections;

namespace Cryptany.Core.DPO.Collections
{
    public class TreeListNode<T>
    {
        private static int _count = 0;
        private string _name;
        private T _value;
        private TreeListNodeCollection<T> _childNodes;
        private TreeListNode<T> _parent;

        internal TreeListNode()
        {
            Init(null, default(T));
        }

        internal TreeListNode(T value)
        {
            Init(null, value);
        }

        public TreeListNode(string name)
        {
            Init(name, default(T));
        }

        public TreeListNode(string name, T value)
        {
            Init(name, value);
        }

        private void Init(string name, T value)
        {
            if ( name == null )
            {
                int count = ++_count;
                _name = "TreeListNode" + count.ToString();

            }
            else
                _name = name;

            _value = value;
            _childNodes = new TreeListNodeCollection<T>(this);
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        public TreeListNodeCollection<T> ChildNodes
        {
            get
            {
                return _childNodes;
            }
        }

        public TreeListNode<T> Parent
        {
            get
            {
                return _parent;
            }
            internal set
            {
                _parent = value;
            }
        }

    }

    public class TreeListNodeCollection<T> : DictionaryBase
    {
        private TreeListNode<T> _parentNode;

        internal TreeListNodeCollection(TreeListNode<T> node)
        {
            _parentNode = node;
        }

        public TreeListNode<T> ParentNode
        {
            get
            {
                return _parentNode;
            }
        }

        public TreeListNode<T> Add(string name, T value)
        {
            TreeListNode<T> node = new TreeListNode<T>(name, value);
            node.Parent = _parentNode;
            this.Dictionary.Add(name, node);
            return node;
        }

        public void Add(TreeListNode<T> node)
        {
            node.Parent = _parentNode;
            Dictionary.Add(node.Name, node);
        }

        public void Remove(string name)
        {
            Dictionary.Remove(name);
        }

        public List<T> Values
        {
            get
            {
                List<T> list = new List<T>();
                foreach ( T item in Dictionary.Values )
                {
                    list.Add(item);
                }
                return list;
            }
        }

        public List<T> Keys
        {
            get
            {
                List<T> list = new List<T>();
                foreach ( T item in Dictionary.Keys )
                {
                    list.Add(item);
                }
                return list;
            }
        }

        public int Count
        {
            get
            {
                return Dictionary.Count;
            }
        }
    }

    public class TreeList<T>
    {
        private TreeListNodeCollection<T> _nodes;

        public TreeList()
        {
            _nodes = new TreeListNodeCollection<T>(null);
        }

        public TreeListNodeCollection<T> Nodes
        {
            get
            {
                return _nodes;
            }
        }
    }
}
