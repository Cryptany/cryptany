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
    public class BinaryParam : BaseParam
    {
        public override paramType type { get { return paramType.binary; } }

        new public bool value;

        public BinaryParam(string ParamName, bool ParamValue) 
        {
            paramName = ParamName;
            value = ParamValue;
        }

        public override UIElement DrawParam()
        {
            CheckBox chBlock = new CheckBox();
            chBlock.IsChecked = value;
            chBlock.Name = this.paramName;
            chBlock.Margin = new Thickness(5.0);
            chBlock.Content = "";
            chBlock.FlowDirection = FlowDirection.RightToLeft;
            return chBlock;
        }
    }
}
