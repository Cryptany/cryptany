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
    public class TextParam : BaseParam
    {
        public override paramType type { get { return paramType.text; } }

        public TextParam(string ParamName, string ParamValue) 
            : base(ParamName, ParamValue)
        {
        
        }

        public override UIElement DrawParam()
        {
            TextBox tBlock = new TextBox();
            tBlock.Text = value;
            tBlock.Width = 200;
            tBlock.Margin = new Thickness(5.0);
            tBlock.Name = this.paramName;
            return tBlock;
        }
    }
}
