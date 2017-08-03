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
using ServiceBlockEditor.Blocks;
using DragAndDropSimple;
using ServiceBlockEditor.BlockParams;
using System.Windows.Controls.Primitives;
using System.Data.Services.Client;
using System.Windows.Media.Imaging;
using System.IO;




namespace ServiceBlockEditor
{
    public partial class MainPage : UserControl
    {
        List<BaseBlock> BlockElements = new List<BaseBlock>();
        BaseBlock SelectedBlock, BlockToLink;
        Popup AskForA;
        //RouteDBServiceClient client;

        double mouseX;
        double mouseY;

        //void Work(object sender, GetServicesCompletedEventArgs e)
        //{
        //    GetServicesResponse serviceResponse = e.Result;
        //    client.CloseAsync();
        //}


        //void client_SayHelloCompleted(object sender, SayHelloCompletedEventArgs e)
        //{
            
           
        //}

        public MainPage()
        {
            InitializeComponent();

            
            //client = new RouteDBServiceClient();
            //ListOfService listOfSer = new ListOfService();

            //client.SayHelloCompleted += new EventHandler<SayHelloCompletedEventArgs>(client_SayHelloCompleted);
            //SayHelloRequest request1 = new SayHelloRequest();
            //client.SayHelloAsync(request1);


            //client.GetServicesCompleted += new EventHandler<GetServicesCompletedEventArgs>(Work); // ClientAddCategoryCompleted;
            //GetServicesRequest request2 = new GetServicesRequest();
            //client.GetServicesAsync(request2);





            List<BaseBlock> BaseBlockList = CreateBlocsToToolBox();
            ToolBoxItemsList.ItemsSource = BaseBlockList;

            //#region DrawLine
            ////Line MyLine = new Line();
            ////MyLine.X1 = 1;// BeginPoint.X;
            ////MyLine.Y1 = 1;// BeginPoint.Y;
            ////MyLine.X2 = 50;// EndPoint.X;
            ////MyLine.Y2 = 50;// EndPoint.Y;
            ////MyLine.Stroke = new SolidColorBrush(Colors.Black);
            ////MyLine.StrokeThickness = 2;
            ////MyLine.HorizontalAlignment = HorizontalAlignment.Left;
            ////MyLine.VerticalAlignment = VerticalAlignment.Center;
            ////Condition.Children.Add(MyLine); 
            //#endregion
        }




        private List<BaseBlock> CreateBlocsToToolBox()
        {
            //List<string> listParams = new List<string>();
            //listParams.Add("val3");
            //listParams.Add("val2");
            //listParams.Add("val1");

            //List<BaseParam> blockParams = new List<BaseParam>();
            //blockParams.Add(new TextParam("Text Param", "Lala"));
            //blockParams.Add(new BinaryParam("Binary Param", true));
            //blockParams.Add(new ListOfValuesParam("List Param", "val1", listParams));

            //Stream imgStream = File.OpenRead(Directory.GetCurrentDirectory() + "/img/images.jpg");// @"C:\Users\samusenko\documents\visual studio 2010\Projects\ServiceBlockEditor\ServiceBlockEditor\img\images.jpg");
            //BitmapImage bitmap1 = new BitmapImage();
            //bitmap1.SetSource(imgStream);
            //this.testImg.Source = bitmap1;


            BitmapImage bi1 = new BitmapImage();
            bi1.UriSource = new Uri("img/images.jpg", UriKind.Relative);


            BitmapImage bi2 = new BitmapImage();
            bi2.UriSource = new Uri("img/no.jpg", UriKind.Relative);
            BitmapImage bi3 = new BitmapImage();
            bi3.UriSource = new Uri("img/yes.jpg", UriKind.Relative);

            List<BaseBlock> buffer = new List<BaseBlock>();
            buffer.Add(new BaseBlock("блок", bi1));
            buffer.Add(new BaseBlock("да", bi2));
            buffer.Add(new BaseBlock("нет", bi3));
            //buffer.Add(new BaseBlock("Condition", blockParams));
            //buffer.Add(new BaseBlock("SMS_Send", blockParams));
            //buffer.Add(new BaseBlock("Subscription", blockParams));
            //buffer.Add(new BaseBlock("Check_Subscription", blockParams));

            return buffer;
        }

        private void Action_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Hellow");
            if (ToolBoxItemsList.SelectedItem != null && ToolBoxItemsList.SelectedIndex == 0)
                Action.Children.Add(new DragableBox());

            ToolBoxItemsList.SelectedItem = null;
        }

        //#region запрос имени блока
        //public void AskForABlockName(double X, double Y)
        //{
        //    AskForA = new Popup();
        //    Border border = new Border();
        //    border.BorderBrush = new SolidColorBrush(Colors.Black);
        //    border.BorderThickness = new Thickness(5.0);

        //    StackPanel panel1 = new StackPanel();
        //    panel1.Background = new SolidColorBrush(Colors.White);

        //    TextBlock tb = new TextBlock();
        //    tb.Text = "Name the block";

        //    TextBox tbx = new TextBox();
        //    tbx.Name = "blockName";

        //    panel1.Children.Add(tb);
        //    panel1.Children.Add(tbx);

        //    Button Ok = new Button();
        //    Ok.Content = "Ok";
        //    Ok.Click += new RoutedEventHandler(SaveBlockName);

        //    panel1.Children.Add(Ok);

        //    border.Child = panel1;
        //    AskForA.Child = border;
        //    AskForA.HorizontalOffset = X;
        //    AskForA.VerticalOffset = Y;
        //    AskForA.IsOpen = true;
        //}

        //private void SaveBlockName(object sender, RoutedEventArgs e)
        //{
        //    AskForA.IsOpen = false;
        //    string contextName = ((TextBox)((StackPanel)((Border)AskForA.Child).Child).Children[1]).Text;
        //    BaseBlock newBlock = new BaseBlock(contextName, (BaseBlock)ToolBoxItemsList.SelectedItem);
        //    DragableBlock newUIBlock = new DragableBlock(newBlock, mouseX, mouseY);
        //    newBlock.UI = newUIBlock;
        //    ((Canvas)Action).Children.Add(newUIBlock);
        //    newUIBlock.MouseLeftButtonDown += new MouseButtonEventHandler(Block_Click);
        //    BlockElements.Add(newBlock);

        //    ToolBoxItemsList.SelectedItem = null;
        //} 
        //#endregion

        //#region создание нового блока
        //private void Action_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    foreach (DragableBlock block in BlockElements.Select((a) => { return a.UI; }))
        //        block.SetUnCuptured();
        //    if (SelectedBlock != null) SelectedBlock.UI.SetCuptured();


        //    if (ToolBoxItemsList.SelectedItem != null)
        //    {
        //        mouseX = e.GetPosition(Action).X;
        //        mouseY = e.GetPosition(Action).Y;
        //        AskForABlockName(e.GetPosition(this).X, e.GetPosition(this).Y);
        //    }

        //} 
        //#endregion

        //#region запрос типа связи
        //public void AskForARelationType(double X, double Y)
        //{
        //    AskForA = new Popup();
        //    Border border = new Border();
        //    border.BorderBrush = new SolidColorBrush(Colors.Black);
        //    border.BorderThickness = new Thickness(5.0);

        //    StackPanel panel1 = new StackPanel();
        //    panel1.Background = new SolidColorBrush(Colors.White);

        //    TextBlock tb = new TextBlock();
        //    tb.Text = "Type the relation";

        //    StackPanel panel2 = new StackPanel();
        //    panel2.Orientation = Orientation.Horizontal;

        //    Button Yes = new Button();
        //    Yes.Content = "yes";
        //    Yes.Click += new RoutedEventHandler(SaveRelationTypeYes);
        //    Button No = new Button();
        //    Yes.Content = "no";
        //    Yes.Click += new RoutedEventHandler(SaveRelationTypeNo);

        //    panel2.Children.Add(Yes);
        //    panel2.Children.Add(No);

        //    panel1.Children.Add(tb);
        //    panel1.Children.Add(panel2);

        //    border.Child = panel1;
        //    AskForA.Child = border;
        //    AskForA.HorizontalOffset = X;
        //    AskForA.VerticalOffset = Y;
        //    AskForA.IsOpen = true;
        //}

        //private void SaveRelationTypeYes(object sender, RoutedEventArgs e)
        //{
        //    AskForA.IsOpen = false;
        //    SelectedBlock.YesLink.Add(BlockToLink);
        //}
        //private void SaveRelationTypeNo(object sender, RoutedEventArgs e)
        //{
        //    AskForA.IsOpen = false;
        //    SelectedBlock.NoLink.Add(BlockToLink);
        //}
        //#endregion

        //#region добавление связи между блоками
        //void Block_Click(object sender, RoutedEventArgs e)
        //{
           

        //    if (SelectedBlock != null)
        //    {
        //        BlockToLink = ((DragableBlock)sender).data;
        //        AskForARelationType(300, 300);
        //    }
        //    SelectedBlock = ((DragableBlock)sender).data;
        //} 
        //#endregion

        //private void File_Click(object sender, RoutedEventArgs e)
        //{
           
        //}

        //public void ConnectBLocks(DragableBlock block)
        //{ 
            
        //}


        
    }
}
