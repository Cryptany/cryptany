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
using guiEditRouter;
using System.Collections.ObjectModel;

namespace guiEditRouter.UserControlls.PropertiesElements
{
    public partial class MultiValueParam : UserControl
    {
        private MultiValueParamData dataContext;
        private _Condition Data;

        public MultiValueParam(_Condition Data, BaseParam Info)
        {
            InitializeComponent();
            this.Data = Data;
            this.Block.BorderBrush = (Info.required) ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);

            Dictionary<Guid, string> ValueList = new Dictionary<Guid, string>();
            var buffer = Data.Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in buffer)
                ValueList.Add(new Guid(item), Info.paramTemplates[new Guid(item)]);

            dataContext = new MultiValueParamData(Data.Property, Data.Operation, ValueList, Info.paramTemplates);
            this.DataContext = dataContext;
        }
        private void AddValue_Click(object sender, RoutedEventArgs e)
        {
            dataContext.Values.Add((KeyValuePair<Guid, string>)(PossibleValues.SelectedItem));
        }
        private void DeleteValue(object sender, RoutedEventArgs e)
        {
            dataContext.Values.Remove(dataContext.Values.First(a => a.Key == (Guid)((Button)sender).DataContext));
        }
        public _Condition GetData()
        {
            if (OperationType.SelectedIndex == 0) return null;

            string valuesBuff = "";
            foreach (var item in dataContext.Values)
                valuesBuff += item.Key.ToString() + ";";
            return new _Condition() { Property = Data.Property, Operation = (string)this.OperationType.SelectedValue, Value = valuesBuff };
        }
    }

    public class MultiValueParamData
    {
        public string Name { get; set; }
        public int Operation { get; set; }
        public List<string> OperationList 
        { 
            get
            {
                List<string> buff = new List<string>();
                buff.Add("Не использовать");
                buff.Add("Equals");
                buff.Add("NotEquals");
                buff.Add("Default");
                buff.Add("In");
                buff.Add("NotIn");
                buff.Add("Contains");
                buff.Add("NotContains");
                return buff;
            } 
        }
        public ObservableCollection<KeyValuePair<Guid, string>> Values { get; set; }
        public ObservableCollection<KeyValuePair<Guid, string>> Templates { get; set; }

        public MultiValueParamData() { }
        public MultiValueParamData(string Name, string Operation, Dictionary<Guid, string> ValueList, Dictionary<Guid, string> Templates)
        {
            this.Name = Name;
            this.Operation = OperationList.IndexOf(Operation);
            this.Values = new ObservableCollection<KeyValuePair<Guid, string>>(ValueList);
            this.Templates = new ObservableCollection<KeyValuePair<Guid, string>>(Templates);
        }
    }
}




