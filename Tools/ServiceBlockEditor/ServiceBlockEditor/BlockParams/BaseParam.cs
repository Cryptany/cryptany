using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ServiceBlockEditor.BlockParams
{
    public class BaseParam
    {
        public enum paramType
        { 
            text,
            listOfValues,
            binary,
            unkwnown
        }

        public virtual paramType type { get { return paramType.unkwnown; } }
        public string paramName {get;set;}
        public string value;

        public BaseParam(string ParamName, string ParamValue)
        {
            paramName = ParamName;
            value = ParamValue;
        }

        public BaseParam()
        { }

        public virtual UIElement DrawParam()
        {
            return null;
        }

        public virtual void Commit()
        { }
        public virtual void Update()
        { }
    }
}
