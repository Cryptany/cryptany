using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using guiEditRouter.RouterDBService;

namespace guiEditRouter.UserControlls.PropertiesElements
{
    public partial class SingleValueParam : UserControl
    {
        private _Condition Data;
        private bool textValue;
        private List<string> OperationList
        {
            get
            {
                List<string> buff = new List<string>();
                buff.Add("Не использовать");
                buff.Add("Default");
                buff.Add("Equals");
                buff.Add("NotEquals");
                buff.Add("Empty");
                buff.Add("NotIn");
                buff.Add("NotEmpty");
                return buff;
            }
        }


        public SingleValueParam(_Condition Data, BaseParam Info)
        {
            InitializeComponent();
            this.Data = Data;
            this.Block.BorderBrush = (Info.required) ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);

            ParamName.Text = Data.Property;
            if (Data.Operation == "Default")
                OperationType.Visibility = System.Windows.Visibility.Collapsed;
            else
            {
                foreach (var item in OperationList)
                {
                    OperationType.Items.Add(item);
                }
                OperationType.SelectedIndex = OperationList.IndexOf(Data.Operation); 
            }
                
            textValue = (Info.paramTemplates == null || Info.paramTemplates.Count == 0);

            if (textValue)
            {
                this.Value.Visibility = System.Windows.Visibility.Visible;
                this.ValueList.Visibility = System.Windows.Visibility.Collapsed;
                this.Value.Text = Data.Value;
            }
            else
            {
                this.Value.Visibility = System.Windows.Visibility.Collapsed;
                this.ValueList.Visibility = System.Windows.Visibility.Visible;
                
                Guid curValueId;
                bool PossibleValuesIsGuid = Guid.TryParse(Data.Value, out curValueId);
                foreach(var item in Info.paramTemplates)
                {
                    ComboBoxItem cbItem = new ComboBoxItem();
                    cbItem.Content = item.Value; 
                    cbItem.DataContext = item;

                    if (PossibleValuesIsGuid && item.Key == curValueId)
                        cbItem.IsSelected = true;
                    if (!PossibleValuesIsGuid && item.Value == Data.Value) 
                        cbItem.IsSelected = true;
                    this.ValueList.Items.Add(cbItem);
                }
               
            }
        }
        public _Condition GetData()
        {
            if (OperationType.SelectedIndex == 0) return null;
            string valuesBuff = (textValue) ? this.Value.Text : 
                ((KeyValuePair<Guid, string>)((ComboBoxItem)this.ValueList.SelectedItem).DataContext).Key.ToString();

            return new _Condition() { Property = Data.Property, Operation = (string)this.OperationType.SelectedValue, Value = valuesBuff };

        }
    }
}
