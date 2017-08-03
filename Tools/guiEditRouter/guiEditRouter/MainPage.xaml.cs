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
using DragAndDropSimple;
using System.Threading;
using guiEditRouter.RouterDBService;
using System.Collections.ObjectModel;
using Block = guiEditRouter.RouterDBService.Block;
using System.Windows.Controls.Primitives;
using guiEditRouter.UserControlls.PropertiesElements;

namespace guiEditRouter
{
    public partial class MainPage : UserControl
    {
        #region Fields
        Guid ServiceBlockId;
        Page CurrNode;

        List<Page> Nodes;
        Popup ChoseService;

        Dictionary<string, ParamsInBlock> BlockParams;
        List<BlockSettingsParam> BlockSettingsParams;

        RouteDBServiceClient client;
        Dictionary<Guid, string> AllBlockTypes;
        private Guid EmptyBlockGuid = Guid.Empty; 
        #endregion
        #region Основа
        public MainPage()
        {
            InitializeComponent();
            
            client = new RouteDBServiceClient();
            client.GetAllBlockTypesCompleted += new EventHandler<GetAllBlockTypesCompletedEventArgs>(client_GetAllBlockTypesCompleted);
            client.GetAllBlocksCompleted += new EventHandler<GetAllBlocksCompletedEventArgs>(client_GetAllBlocksCompleted);
            client.GetServicesCompleted +=new EventHandler<GetServicesCompletedEventArgs>(client_GetServicesCompleted);
            client.GetServiceBlockCompleted +=new EventHandler<GetServiceBlockCompletedEventArgs>(client_GetServiceBlockCompleted);
            client.GetBlockSettingsParamsCompleted +=new EventHandler<GetBlockSettingsParamsCompletedEventArgs>(client_GetBlockSettingsParamsCompleted);
            client.GetBlockInfoCompleted +=new EventHandler<GetBlockInfoCompletedEventArgs>(client_GetBlockInfoCompleted);
           client.CreateNewBlockCompleted +=new EventHandler<CreateNewBlockCompletedEventArgs>(client_CreateNewBlockCompleted);

            Nodes = new List<Page>();
            
            FillToolBar();

            ChoseService = new Popup();
            //client.GetBlockSettingsParamsAsync();
        }
        #region асинхронные сервисные реакции
        void client_CreateNewBlockCompleted(object sender, CreateNewBlockCompletedEventArgs e)
        {
            MessageBox.Show(e.Result.ToString());

            FillToolBar();
        }
        void client_GetBlockInfoCompleted(object sender, GetBlockInfoCompletedEventArgs e)
        {
            List<BaseParam> bufferParams = e.Result.ToList();
            ParamsInBlock curParams = new ParamsInBlock();
            foreach (var item in bufferParams)
            {
                curParams.Params.Add(item.name, item);
            }
            curParams.BlockTypeId = AllBlockTypes.First(a => a.Value == curParams.Params.First().Value.BlockType).Key;
            curParams.BlockTypeName = AllBlockTypes[curParams.BlockTypeId];

            BlockParams.Add(curParams.BlockTypeName, curParams);
        }
        void client_GetBlockSettingsParamsCompleted(object sender, GetBlockSettingsParamsCompletedEventArgs e)
        {
            BlockSettingsParams = e.Result.ToList();
        }
        void client_GetServiceBlockCompleted(object sender, GetServiceBlockCompletedEventArgs e)
        {
            ServiceBlock SBlock = e.Result;
            TextBlock tblock = new TextBlock();
            tblock.Text = SBlock.name;
            tblock.DataContext = SBlock;


            ((ListBox)((StackPanel)ChoseService.Child).Children.First()).Items.Add(tblock);
        }
        void client_GetServicesCompleted(object sender, GetServicesCompletedEventArgs e)
        {
            ListOfService services = e.Result;
            foreach (var serv in services.LOS)
                foreach (var servBlockId in serv.serviceBlocksId)
                    client.GetServiceBlockAsync(servBlockId.Key, servBlockId.Value);

        }
        void client_GetAllBlocksCompleted(object sender, GetAllBlocksCompletedEventArgs e)
        {
            ObservableCollection<Block> blocks = e.Result;
            foreach (var item in AllBlockTypes)
            {
                TreeViewItem blockTypeItem = new TreeViewItem();
                blockTypeItem.Header = item.Value;

                TextBlock EmptyBlock = new TextBlock();
                EmptyBlock.Text = "Пустой";
                EmptyBlock.DataContext = item;

                blockTypeItem.Items.Add(EmptyBlock);

                var list = blocks.Where(a => a.typeid == item.Key).ToArray();

                foreach (var block in list)
                {
                    TextBlock ToolBoxBlock = new TextBlock();
                    ToolBoxBlock.Text = block.name;
                    ToolBoxBlock.DataContext = block;

                    blockTypeItem.Items.Add(ToolBoxBlock);

                    if (EmptyBlockGuid == block.id)
                    {
                        CurrNode.UpdateData(block);
                        EmptyBlockGuid = Guid.Empty;
                    }
                }
                ToolBoxTree.Items.Add(blockTypeItem);
            }
            //client.CloseAsync();
        }
        void client_GetAllBlockTypesCompleted(object sender, GetAllBlockTypesCompletedEventArgs e)
        {
            AllBlockTypes = e.Result;
            GetBlockParams();
            client.GetAllBlocksAsync();
            #region comments
            //foreach (var item in AllBlockTypes)
            //{
            //    Border nothing = new Border();

            //    nothing.DataContext = item;

            //    nothing.Width = 90;
            //    nothing.Height = 90;
            //    nothing.Background = new SolidColorBrush(Colors.White);
            //    nothing.BorderBrush = new SolidColorBrush(Colors.Black);
            //    nothing.BorderThickness = new Thickness(5);

            //    TextBlock ServTypeName = new TextBlock();
            //    ServTypeName.Text = item.Value;

            //    nothing.Child = ServTypeName;
            //    nothing.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            //    nothing.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            //    ToolBoxItems.Items.Add(nothing); 
            //} 
            #endregion
        } 
        #endregion
        #region загрузка сервисного блока
        private void Open_Click(object sender, RoutedEventArgs e)
        {


            StackPanel p = new StackPanel();
            p.Orientation = Orientation.Vertical;
            ListBox lb = new ListBox();
            p.Children.Add(lb);
            Button b = new Button();
            b.Content = "Choose";
            b.Click += new RoutedEventHandler(b_Click);
            p.Children.Add(b);

            p.Width = 300;

            ChoseService.Child = p;

            client.GetServicesAsync();

            //ChoseService.Width = 800;
            //ChoseService.Height = 600;

            ChoseService.VerticalOffset = 400;
            ChoseService.HorizontalOffset = 400;

            ChoseService.IsOpen = true;
        }
        private void b_Click(object sender, RoutedEventArgs e)
        {
            if(((ListBox)((StackPanel)ChoseService.Child).Children.First()).SelectedItem == null) return;
            WorkPlace.Children.Clear();
            Nodes.Clear();
            ServiceBlock ChosenServiceBlock = ((TextBlock)((ListBox)((StackPanel)ChoseService.Child).Children.First()).SelectedItem).DataContext as ServiceBlock;
            ServiceBlockId = ChosenServiceBlock.id;
            ChoseService.IsOpen = false;
            foreach (var item in ChosenServiceBlock.blocks)
            {

                Page node = new Page(item, 0, 0);

                node.MouseLeftButtonDown += new MouseButtonEventHandler(node_MouseLeftButtonDown);
                node.MouseLeftButtonUp += new MouseButtonEventHandler(node_MouseLeftButtonUp);
                node.MouseMove += new MouseEventHandler(node_MouseMove);
                node.MouseRightButtonDown += new MouseButtonEventHandler(node_MouseRightButtonDown);

                WorkPlace.Children.Add(node);

                if (item.isVerification) node.IsRoot = true;
                Nodes.Add(node);
            }

            foreach (var node in Nodes)
            {
                node.DrawLinks(Nodes);
            }
            DrawTree();
        }  
        #endregion
        #region св-ва блока
        void GetBlockParams()
        {
            BlockParams = new Dictionary<string, ParamsInBlock>();
            foreach (var item in AllBlockTypes)
                client.GetBlockInfoAsync(item.Value);
        }
        private void ShowBlockSettings(Block block)
        {
            PropertiesBar.Children.Clear();
            ParamsInBlock curBlockParams = BlockParams[block.typename];
            if (block.name == "Пустой")
            {
                TextBlock blockType = new TextBlock();
                blockType.Name = "blockType";
                blockType.Text = block.typename;
                blockType.DataContext = block.typeid;
                blockType.FontWeight = FontWeights.Bold;
                PropertiesBar.Children.Add(blockType);

                TextBlock name = new TextBlock();
                name.Text = "Назовите блок";
                name.FontSize = 14;
                name.Margin = new Thickness(1, 5, 1, 5);
                PropertiesBar.Children.Add(name);

                TextBox options = new TextBox();
                options.Text = block.name;
                options.Name = "blockName";
                options.Margin = new Thickness(1, 0, 1, 5);
                options.FontWeight = FontWeights.Bold;
                PropertiesBar.Children.Add(options);

                foreach (var item in curBlockParams.Params)
                {
                    if (item.Value.listParam)
                        PropertiesBar.Children.Add(
                            new MultiValueParam(
                                new _Condition() { Operation = "Не использовать", Property = item.Value.name, Value = "" }
                                , item.Value));
                    else
                        PropertiesBar.Children.Add(
                            new SingleValueParam(
                                new _Condition() { Operation = "Не использовать", Property = item.Value.name, Value = "" }
                                , item.Value));

                }

                Button SaveButton = new Button();
                SaveButton.Content = "Save";
                SaveButton.Height = 70;
                SaveButton.Margin = new Thickness(3);
                SaveButton.Click += new RoutedEventHandler(SaveButton_Click);
                PropertiesBar.Children.Add(SaveButton);
            }
            else
            {
                TextBlock options = new TextBlock();
                options.Text = "Block properties: " + block.name;
                options.FontWeight = FontWeights.Bold;
                PropertiesBar.Children.Add(options);

                if (block.settings.Conditions != null)
                {

                    foreach (var item in block.settings.Conditions)
                    {
                        BaseParam curParam = curBlockParams.Params[item.Property];

                        if (curParam.listParam)
                            PropertiesBar.Children.Add(new MultiValueParam(item, curParam));
                        else
                            PropertiesBar.Children.Add(new SingleValueParam(item, curParam));

                    }
                }
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Block buffBlock = new Block();
            buffBlock.id = Guid.NewGuid();
            this.EmptyBlockGuid = buffBlock.id;
            buffBlock.settings = new _BlockSettings();
            buffBlock.settings.Conditions = new ObservableCollection<_Condition>();

            _Condition buffer;

            foreach (var item in PropertiesBar.Children)
            {
                if (item.GetType() == typeof(MultiValueParam))
                {
                    buffer = ((MultiValueParam)item).GetData();
                    if (buffer != null)
                        buffBlock.settings.Conditions.Add(((MultiValueParam)item).GetData());
                }
                if (item.GetType() == typeof(SingleValueParam))
                {
                    buffer = ((SingleValueParam)item).GetData();
                    if (buffer != null)
                        buffBlock.settings.Conditions.Add(((SingleValueParam)item).GetData());
                }
                if (item.GetType() == typeof(TextBlock) && ((TextBlock)item).Name == "blockType")
                {
                    buffBlock.typeid = (Guid)((TextBlock)item).DataContext;
                    buffBlock.typename = ((TextBlock)item).Text;
                }
                if (item.GetType() == typeof(TextBox) && ((TextBox)item).Name == "blockName")
                {
                    buffBlock.name = ((TextBox)item).Text;
                }
            }

            try
            {
                client.CreateNewBlockAsync(buffBlock);
                PropertiesBar.Children.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion
        #region движение блока
        private void node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (ToolBoxTree.SelectedItem is Canvas)
            {
                if (CurrNode != null && CurrNode != (Page)sender)
                {
                    Page node = (Page)sender;

                    Line link = node.NewLink(CurrNode, ((ToolBoxTree.SelectedItem as Canvas).DataContext.ToString() == "yesLine"));
                    if (link != null)
                    {
                        link.Cursor = Cursors.Hand;
                        link.MouseRightButtonDown += new MouseButtonEventHandler(link_MouseRightButtonDown);
                        Line Arrow = CurrNode.SaveLink(link, node, (((Canvas)ToolBoxTree.SelectedItem).DataContext == "yesLine"));
                        WorkPlace.Children.Add(link);
                        WorkPlace.Children.Add(Arrow);

                        foreach (Link clink in node.links)
                            clink.MoveLinkPoint();
                    }

                    CurrNode = null;
                }
                else
                {
                    CurrNode = (Page)sender;
                }
            }
            else
            {
                Page item = sender as Page;
                item.isMouseCaptured = true;
                item.CaptureMouse();
            }

            #region отображение свойств
            ShowBlockSettings(((Page)sender).Datablock);
            #endregion
        }
        private void node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Page item = sender as Page;
            item.isMouseCaptured = false;
            item.ReleaseMouseCapture();
        }
        public void node_MouseMove(object sender, MouseEventArgs args)
        {
            Page item = sender as Page;
            if (item.isMouseCaptured)
            {
                item.PositionY = (args.GetPosition(WorkPlace).Y - 30 < WorkPlace.ActualHeight) ? args.GetPosition(WorkPlace).Y - 30 : WorkPlace.ActualHeight - 30;
                item.PositionX = (args.GetPosition(WorkPlace).X + item.Width < WorkPlace.ActualWidth) ? args.GetPosition(WorkPlace).X  : WorkPlace.ActualWidth - item.Width;
                foreach (Link link in item.links)
                    link.MoveLinkPoint();
            }

        } 
        #endregion
        #region удаление
        private void link_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Line line = (Line)sender;
            Dictionary<Page, Link> linksToDell = new Dictionary<Page, Link>();


            foreach (var node in Nodes)
            {
                Link buffer = node.links.FirstOrDefault(a => a.LinkToNode == line);
                if (buffer != null)
                {
                    linksToDell.Add(node, buffer);
                }
            }

            foreach (var item in linksToDell)
            {
                ((Page)item.Key).links.Remove(item.Value);
                WorkPlace.Children.Remove(item.Value.Arrow);
                WorkPlace.Children.Remove(item.Value.linkToNode);
            }

        }
        private void node_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Page nodeToDell = (Page)sender;

            foreach (var link in nodeToDell.links)
            {
                WorkPlace.Children.Remove(link.LinkToNode);
                if (link.IsParent)
                    WorkPlace.Children.Remove(link.Arrow);
                else
                    WorkPlace.Children.Remove(link.LinkedNode.links.First(a => a.LinkedNode == nodeToDell).Arrow);
            }
            WorkPlace.Children.Remove(nodeToDell);
            foreach (var node in Nodes.Where(a => a.LinkedWith(nodeToDell)))
            {
                node.links.Remove(node.links.First(a => a.LinkedNode == nodeToDell));
            }

            Nodes.Remove(nodeToDell);

            //node.Delete();
        } 
        #endregion
        #region базовые операции
        public void SituateGraphInCenter()
        {
            double AvgX = 0;
            double AvgY = 0;

            double offsetX = 0;
            double offsetY = 0;

            foreach (var node in Nodes)
            {
                AvgX += node.PositionX;
                AvgY += node.PositionY;
            }

            offsetX = (WorkPlace.ActualWidth / 2) - (AvgX / Nodes.Count);
            offsetY = (WorkPlace.ActualHeight / 2) - (AvgY / Nodes.Count);

            foreach (var node in Nodes)
            {
                node.PositionX += offsetX;
                node.PositionY += offsetY;
            }

            foreach (var node in Nodes)
            {
                foreach (var link in node.links)
                    if (link.IsParent)
                        link.MoveLinkPoint();
            }
        }
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            WorkPlace.Children.Clear();
            Nodes.Clear();
        }
        void FillToolBar()
        {
            ToolBoxTree.Items.Clear();
            TreeViewItem BaseItems = new TreeViewItem();
            BaseItems.Header = "Базовые элементы";

            #region добавление линий
            Canvas yes = new Canvas();
            yes.Width = 90;
            yes.Height = 90;

            Line MyLine = new Line();
            MyLine.X1 = 1;// BeginPoint.X;
            MyLine.Y1 = 1;// BeginPoint.Y;
            MyLine.X2 = 89;// EndPoint.X;
            MyLine.Y2 = 89;// EndPoint.Y;
            MyLine.Stroke = new SolidColorBrush(Colors.Green);
            MyLine.StrokeThickness = 2;
            MyLine.HorizontalAlignment = HorizontalAlignment.Left;
            MyLine.VerticalAlignment = VerticalAlignment.Center;
            yes.Children.Add(MyLine);
            yes.DataContext = "yesLine";

            BaseItems.Items.Add(yes);


            Canvas no = new Canvas();
            no.Width = 90;
            no.Height = 90;

            Line MyLine1 = new Line();
            MyLine1.X1 = 1;// BeginPoint.X;
            MyLine1.Y1 = 1;// BeginPoint.Y;
            MyLine1.X2 = 89;// EndPoint.X;
            MyLine1.Y2 = 89;// EndPoint.Y;
            MyLine1.Stroke = new SolidColorBrush(Colors.Red);
            MyLine1.StrokeThickness = 2;
            MyLine1.HorizontalAlignment = HorizontalAlignment.Left;
            MyLine1.VerticalAlignment = VerticalAlignment.Center;
            no.Children.Add(MyLine1);
            no.DataContext = "noLine";

            BaseItems.Items.Add(no);
            #endregion

            #region добавление курсора
            Border nothing = new Border();
            nothing.Width = 90;
            nothing.Height = 90;
            nothing.Background = new SolidColorBrush(Colors.White);
            nothing.BorderBrush = new SolidColorBrush(Colors.Black);
            nothing.BorderThickness = new Thickness(5);
            BaseItems.Items.Add(nothing);
            #endregion

            ToolBoxTree.Items.Add(BaseItems);

            #region добавление типов блоков
            client.GetAllBlockTypesAsync();
            #endregion

        } 
        #endregion
        /// <summary>
        /// добавление блока
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkPlace_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ToolBoxTree.SelectedItem is TextBlock)
            {
                Block block;
                if (!(((TextBlock)ToolBoxTree.SelectedItem).DataContext is Block))
                {
                    KeyValuePair<Guid, string> blockTypeInfo = (KeyValuePair<Guid, string>)((TextBlock)ToolBoxTree.SelectedItem).DataContext;
                    block = new Block()
                    {
                        name = "Пустой",
                        id = Guid.NewGuid(),
                        blockEntryId = Guid.NewGuid(),
                        links = new ObservableCollection<BlockLink>(),
                        settings = new _BlockSettings(),
                        settingsString = "",
                        typeid = blockTypeInfo.Key,
                        typename = blockTypeInfo.Value
                    };
                }
                else
                {
                    block = (Block)((TextBlock)ToolBoxTree.SelectedItem).DataContext;
                    block.blockEntryId = Guid.NewGuid();
                }



                block.isVerification = (Nodes.Count == 0);

                ShowBlockSettings(block);

                Page node = new Page(block, e.GetPosition(WorkPlace).X, e.GetPosition(WorkPlace).Y);

                node.MouseLeftButtonDown += new MouseButtonEventHandler(node_MouseLeftButtonDown);
                node.MouseLeftButtonUp += new MouseButtonEventHandler(node_MouseLeftButtonUp);
                node.MouseMove += new MouseEventHandler(node_MouseMove);
                node.MouseRightButtonDown += new MouseButtonEventHandler(node_MouseRightButtonDown);

                WorkPlace.Children.Add(node);

                if (node.Datablock.isVerification) node.IsRoot = true;
                Nodes.Add(node);
                CurrNode = node;
            }

            //var client = new RouteDBServiceClient();
        }
        /// <summary>
        /// сохранение сервисного блока
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceBlockId == null)
                MessageBox.Show("Блин, а как же сервисный блок?");
            else
            {
                ServiceBlock SBlockToSave = new ServiceBlock();
                SBlockToSave.id = ServiceBlockId;
                SBlockToSave.blocks = new ObservableCollection<Block>();
                foreach (var node in Nodes)
                {
                    node.Datablock.isVerification = node.IsRoot;
                    node.Datablock.links = node.GetDataLinks();
                    SBlockToSave.blocks.Add(node.Datablock);
                }

                client.SaveServiceBlockAsync(SBlockToSave);
            }
        }
        #endregion
        #region MutationTree
        private void Tree1_Click(object sender, RoutedEventArgs e)
        {
            DrawTree();
        }
        private void DrawTree()
        {
            List<KeyValuePair<Link, Link>> CrossingLinks = new List<KeyValuePair<Link, Link>>();

            #region выделяем все связи
            List<Link> bufferOfLinks = new List<Link>();
            foreach (var node in Nodes)
                bufferOfLinks.AddRange(node.links.Where(a => a.IsParent));
            #endregion

            #region Находим пересекающиеся линии
            for (int i = 0; i < bufferOfLinks.Count; i++)
            {
                for (int j = i + 1; j < bufferOfLinks.Count; j++)
                    if (linesIntersect(bufferOfLinks[i], bufferOfLinks[j]))
                        CrossingLinks.Add(new KeyValuePair<Link, Link>(bufferOfLinks[i], bufferOfLinks[j]));
            }
            #endregion

            foreach (var item in CrossingLinks)
            {
                Swap(item.Key, item.Value);
            }
            #region highlight the crossing lines
            #endregion

            #region стандартное отображение
            #region очистка
            if (Nodes.Count == 0) return;
            WorkPlace.Children.Clear();
            #endregion

            #region отрисовка блоков
            foreach (var item in Nodes)
            {
                WorkPlace.Children.Add(item);
                UpdateLayout();
            }
            #endregion


            Page firstNode = Nodes.First();
            if (firstNode == null || Nodes.Where(a => a.Parents == null).Count() == 0)
            {
                MessageBox.Show("Отсутствует корневой элемент");
                return;
            }
            DrawGraphInfo gi = new DrawGraphInfo();

            foreach (var node in Nodes.Where(a => a.Parents == null))
                gi = SituateOrderTree(node, gi);

            FlipTheGraph();

            #region отрисовка связей
            foreach (var item in Nodes)
            {
                foreach (var link in item.links.Where((a) => { return a.IsParent; }))
                {
                    link.LinkToNode.Cursor = Cursors.Hand;
                    link.LinkToNode.MouseRightButtonDown += new MouseButtonEventHandler(link_MouseRightButtonDown);
                    WorkPlace.Children.Add(link.LinkToNode);
                    WorkPlace.Children.Add(link.Arrow);
                }
                item.State = NodeSate.NotSet;
            }
            #endregion

            SituateGraphInCenter();
            #endregion
        }
        private DrawGraphInfo SituateOrderTree(Page CurNode, DrawGraphInfo info)
        {

            if (CurNode.State == NodeSate.Set)
            {
                info.SetVertical(CurNode.PositionY);
                return info;
            }

            if (CurNode.Children != null)
            {
                DrawGraphInfo buffer = info;
                double maxVertical = 0;//info.Vertical;
                List<Link> childrenLinks = CurNode.links.Where(a => a.IsParent && a.Yes).ToList();
                childrenLinks.AddRange(CurNode.links.Where(a => a.IsParent && !a.Yes));

                foreach (var node in Nodes)
                {
                    if (!childrenLinks.Select(a => a.LinkedNode).Contains(node)) continue;
                    buffer = SituateOrderTree(node, buffer);
                    maxVertical = Math.Max(maxVertical, buffer.Vertical);
                }
                return info.SituateParent(CurNode, buffer.WithVertical(maxVertical));
            }
            else
            {
                return info.SituateGraphSheet(CurNode, info);
            }
        }
        private void Swap(Link link1, Link link2)
        {
            Page parentNode1 = (link1.IsParent) ? link1.myNode : link1.LinkedNode;
            Page parentNode2 = (link2.IsParent) ? link2.myNode : link2.LinkedNode;
            Page childNode1 = (!link1.IsParent) ? link1.myNode : link1.LinkedNode;
            Page childNode2 = (!link2.IsParent) ? link2.myNode : link2.LinkedNode;

            Swap(parentNode1, parentNode1);
            Swap(childNode1, childNode2);
        }
        private void Swap(Page node1, Page node2)
        {
            int nodeNumb = Nodes.IndexOf(node1);
            Nodes[Nodes.IndexOf(node2)] = node1;
            Nodes[nodeNumb] = node2;
        }
        private bool linesIntersect(Link link1, Link link2)
        {
            Line line1 = link1.LinkToNode;
            Line line2 = link2.LinkToNode;
            double v1, v2, v3, v4;

            v1 = (line2.X2 - line2.X1) * (line1.Y1 - line2.Y1) - (line2.Y2 - line2.Y1) * (line1.X1 - line2.X1);
            v2 = (line2.X2 - line2.X1) * (line1.Y2 - line2.Y1) - (line2.Y2 - line2.Y1) * (line1.X2 - line2.X1);
            v3 = (line1.X2 - line1.X1) * (line2.Y1 - line1.Y1) - (line1.Y2 - line1.Y1) * (line2.X1 - line1.X1);
            v4 = (line1.X2 - line1.X1) * (line2.Y2 - line1.Y1) - (line1.Y2 - line1.Y1) * (line2.X2 - line1.X1);

            return (v1 * v2 < 0) && (v3 * v4 < 0);
        }
        private void FlipTheGraph()
        {
            double maxVertical = Nodes.First(a => a.IsRoot).PositionY;
            foreach (var node in Nodes)
                node.PositionY = maxVertical - node.PositionY;
        }
        #endregion
    }
        public class DrawGraphInfo
    {
        public static double HorisontalSpacing { get { return 10; } }
        public static double VerticalSpacing {get {return 10;}}

        Point LastNode;
        public double Vertical { get { return LastNode.Y; } }


        public DrawGraphInfo SituateParent(Page node, DrawGraphInfo info)
        {
            DrawGraphInfo buffer = new DrawGraphInfo();
            node.PositionY = info.LastNode.Y + VerticalSpacing + node.Height;
            if(node.Children.Count > 1)
                node.PositionX = (info.LastNode.X + LastNode.X)/2 - node.Width / 2;
            else
                node.PositionX = this.LastNode.X + HorisontalSpacing;
            node.State = NodeSate.Set;

            buffer.LastNode.Y = node.PositionY + node.Height;
            buffer.LastNode.X = Math.Max(info.LastNode.X, node.PositionX + node.Width + HorisontalSpacing);

            return buffer;
        }

        public DrawGraphInfo SituateGraphSheet(Page node, DrawGraphInfo info)
        {
            DrawGraphInfo buffer = new DrawGraphInfo();

            node.PositionY =  node.Height;
            node.PositionX = info.LastNode.X + HorisontalSpacing;
            
            node.State = NodeSate.Set;

            buffer.LastNode.X = node.PositionX + node.Width;
            buffer.LastNode.Y = VerticalSpacing + node.Height;


            return buffer;
        }

        internal DrawGraphInfo WithVertical(double maxVertical)
        {
            this.LastNode.Y = maxVertical;
            return this;
        }

        internal void SetVertical(double p)
        {
            this.LastNode.Y = p + VerticalSpacing;
        }
    }
        public class ParamsInBlock
        {
            public Guid BlockTypeId;
            public string BlockTypeName;
            public Dictionary<string, BaseParam> Params = new Dictionary<string,BaseParam>();
        }
}
