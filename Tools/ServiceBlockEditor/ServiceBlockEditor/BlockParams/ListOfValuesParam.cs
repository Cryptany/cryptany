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
using System.Collections.Generic;

namespace ServiceBlockEditor.BlockParams
{
    public class ListOfValuesParam : BaseParam
    {
        public override paramType type { get { return paramType.listOfValues; } }

        List<string> values = new List<string>();

        public ListOfValuesParam(string ParamName, string ParamValue, List<string> Values) 
            : base(ParamName, ParamValue)
        {
            values = Values;
        }


        public override UIElement DrawParam()
        {
            ComboBox cbBlock = new ComboBox();
            cbBlock.ItemsSource = values;
            cbBlock.SelectedItem = value;
            cbBlock.Width = 200;
            cbBlock.Margin = new Thickness(5.0);
            cbBlock.Name = this.paramName;
            return cbBlock;
        }
    }
}
