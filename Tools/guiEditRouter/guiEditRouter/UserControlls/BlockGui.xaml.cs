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
using Block = guiEditRouter.RouterDBService.Block;
using guiEditRouter.RouterDBService;
using System.Collections.ObjectModel;

namespace DragAndDropSimple
{
    public partial class Page : UserControl
    {
        public bool LinkedWith(Page node)
        {
            return (links.Count((a) => { return a.ContainsNode(node); }) > 0);
        }

        public  double Height 
        { 
            get 
            { 
                if(this.Rect.ActualHeight == 0)
                    this.UpdateLayout();
                return this.Rect.ActualHeight;
            } 
        }
        public  double Width 
        { 
            get 
            {
                if (this.Rect.ActualWidth == 0)
                    this.UpdateLayout();
                return this.Rect.ActualWidth; 
            } 
        }

        public NodeSate State = NodeSate.NotSet;

        public Page()
        {
            InitializeComponent();
            links = new List<Link>();
        }

        public Page(Block block, double X, double Y)
        {
            InitializeComponent();
            links = new List<Link>();
            PositionX = X;
            PositionY = Y;
            Datablock = block;
            Name = Datablock.name;
            Type = Datablock.typename;
        }

        public void UpdateData(Block Datablock)
        {
            this.Datablock = Datablock;
            Name = Datablock.name;
            Type = Datablock.typename;
        }

        public Block Datablock;

        public Guid id { get { return Datablock.id; } }

        public List<Page> Children
        {
            get
            {
                if (links.Count(a => a.IsParent) > 0)
                {
                    return links.Where(a => a.IsParent).Select(a => a.LinkedNode).ToList(); 
                }
                else return null;
            }
        }

        public List<Page> Parents
        {
            get
            {
                if (links.Count(a => !a.IsParent) > 0)
                {
                    return links.Where(a => !a.IsParent).Select(a => a.LinkedNode).ToList();
                }
                else return null;
            }
        }

        
        public string Name
        {
            get { return BlockName.Text; }
            set {BlockName.Text = value;}
        }
        public string Type
        {
            get { return BlockType.Text; }
            set { BlockType.Text = value; }
        }
        
        public List<Link> links;
        public double PositionX
        {
            get { return (double)this.GetValue(Canvas.LeftProperty) ; }
            set { this.SetValue(Canvas.LeftProperty,((( value ) >= 0)?  value  : 0)); }
        }
        public double PositionY
        {
            get { return (double)this.GetValue(Canvas.TopProperty) ; }
            set { this.SetValue(Canvas.TopProperty, (((value ) >= 0) ? value  : 0)); }
        }
        public bool isMouseCaptured;

        bool isRoot = false;

        public bool IsRoot
        {
            get
            {
                return isRoot;
            }
            set
            {
                isRoot = value;
                if (isRoot)
                    Rect.Background = new SolidColorBrush(Colors.Green);
                else
                    Rect.Background = new SolidColorBrush(Colors.Red);
            }
        }

        public Line NewLink(Page Node, bool Yes)
        {
            if (links.Count((a) => { return a.ContainsNode(Node); }) > 0) //не добавляем повторные связи
                return null;
            Link Created = new Link(this, Node, Yes);
            links.Add(Created);
            return Created.LinkToNode;
        }

        public Line SaveLink(Line link, Page Node, bool Yes)
        {
            Link Added = new Link(link, Node, this, Yes);
            links.Add(Added);
            return Added.Arrow;
        }

        internal void MoveLinkPoint(Page myNode)
        {
            Link linkToMove = links.First((a) => { return a.ContainsNode(myNode); });
            linkToMove.MoveCurrLinkPoint();
        }

        #region пружинки
        //public void ApproachingToNodes(double distance)
        //{
        //    double linkLingthSum = 0;
        //    //считаем суммарную длинну связей
        //    foreach (var link in links)
        //        linkLingthSum += link.LinkLength;

        //    foreach (var link in links)
        //    {
        //        if (link.LinkLength > distance)
        //        {
        //            //двигаем узел вдоль линии
        //            MoveNodeAlongTheLine(link, (10 * link.LinkLength / linkLingthSum));
        //        }
        //        else
        //        {
        //            //двигаем узел вдоль линии
        //            MoveNodeAlongTheLine(link, -(10 * (linkLingthSum - link.LinkLength) / linkLingthSum));
        //        }
        //    }
        //    //перерисовываем связи
        //    Line a;
        //    foreach (var link in links)
        //        a = link.LinkToNode;
        //}

        //private void MoveNodeAlongTheLine(Link link, double distance)
        //{
        //    double K = distance / link.LinkLength;

        //    double devX = link.ParentLinePoint.X - link.ChieldLinePoint.X; //если папа справа, то > 0
        //    double devY = link.ParentLinePoint.Y - link.ChieldLinePoint.Y; //если папа снизу, то > 0

        //    if (link.IsParent)
        //    {
        //        PositionX -= devX * K;
        //        PositionY -= devY * K;
        //    }
        //    else
        //    {
        //        PositionX += devX * K;
        //        PositionY += devY * K;
        //    }
        //} 
        #endregion

        #region comments
        //public double mouseVerticalPosition;
        //public double mouseHorizontalPosition;

        //public void Handle_MouseDown(object sender, MouseEventArgs args)
        //{
        //    Rectangle item = sender as Rectangle;
        //    mouseVerticalPosition = args.GetPosition(null).Y;
        //    mouseHorizontalPosition = args.GetPosition(null).X;
        //    isMouseCaptured = true;
        //    item.CaptureMouse();
        //}

        //public void Handle_MouseMove(object sender, MouseEventArgs args)
        //{
        //    Rectangle item = sender as Rectangle;
        //    if (isMouseCaptured)
        //    {

        //        // Calculate the current position of the object.
        //        double deltaV = args.GetPosition(null).Y - mouseVerticalPosition;
        //        double deltaH = args.GetPosition(null).X - mouseHorizontalPosition;
        //        double newTop = deltaV + (double)item.GetValue(Canvas.TopProperty);
        //        double newLeft = deltaH + (double)item.GetValue(Canvas.LeftProperty);

        //        // Set new position of object.
        //        item.SetValue(Canvas.TopProperty, newTop);
        //        item.SetValue(Canvas.LeftProperty, newLeft);

        //        // Update position global variables.
        //        mouseVerticalPosition = args.GetPosition(null).Y;
        //        mouseHorizontalPosition = args.GetPosition(null).X;
        //    }
        //}

        //public void Handle_MouseUp(object sender, MouseEventArgs args)
        //{
        //    Rectangle item = sender as Rectangle;
        //    isMouseCaptured = false;
        //    item.ReleaseMouseCapture();
        //    mouseVerticalPosition = -1;
        //    mouseHorizontalPosition = -1;
        //} 
        #endregion

        internal void DrawLinks( List<Page> GraficElements)
        {
            foreach (var link in Datablock.links.Where(a => !a.output))
            {
                Link ParentLink = new Link(this, GraficElements.First(a => a.id == link.linkedBlockId), link.yes);
                links.Add(ParentLink);
                Page LinkedBlock = GraficElements.First(a => a.id == link.linkedBlockId);
                LinkedBlock.links.Add(new Link(ParentLink.LinkToNode, this, LinkedBlock, link.yes));

            }
        }


        public ObservableCollection<BlockLink> GetDataLinks()
        {
            ObservableCollection<BlockLink> result = new ObservableCollection<BlockLink>();
            foreach(var link in links)
               result.Add(new BlockLink(){ id = Guid.NewGuid(), linkedBlockId = link.LinkedNode.Datablock.blockEntryId, output = link.IsParent, yes = link.Yes});
            
            return result;
        }

    }


    public class Link
    {
        public Line linkToNode;
        public Line LinkToNode 
        { 
            get 
            {
                if (isParent)
                    MoveLinkPoint();
                return linkToNode; 
            } 
        }
        public Line Arrow;

        public Point ParentLinePoint { get { return new Point(linkToNode.X1, linkToNode.Y1); } }
        public Point ChieldLinePoint { get { return new Point(linkToNode.X2, linkToNode.Y2); } }

        public double LinkLength
        { get { return Math.Sqrt(Math.Pow((ParentLinePoint.X - ChieldLinePoint.X), 2) + Math.Pow((ParentLinePoint.Y - ChieldLinePoint.Y), 2)); } }

        bool isParent;
        public bool IsParent { get { return isParent; } }

        Page linkedNode;
        public Page LinkedNode { get { return linkedNode; } }

        public Page myNode;
        bool yes;
        public bool Yes  {get {return yes;}}
       

        public Link(Line LinkToNode, Page LinkedNode, Page MyNode, bool Yes)
        {
            linkToNode = LinkToNode;
            isParent = true;
            linkedNode = LinkedNode;
            yes = Yes;
            myNode = MyNode;

            Arrow = new Line();
            Arrow.StrokeEndLineCap = PenLineCap.Triangle;
            Arrow.StrokeThickness = 8;
            Arrow.Stroke = new SolidColorBrush(Colors.Black);

        }

        public Link(Page MyNode, Page LinkedNode,  bool Yes)
        {
            Line link = new Line();
            myNode = MyNode;

            link.Stroke = new SolidColorBrush(((Yes) ? Colors.Green : Colors.Red));
            link.StrokeThickness = 2;
            
            Arrow = new Line();

            linkToNode = link;
            isParent = false;
            linkedNode = LinkedNode;
            yes = Yes;

        }

        public Link()
        {
            // TODO: Complete member initialization
        }

     

        public Line GetArrow()
        {

            Arrow.X2 = linkToNode.X2;
            Arrow.Y2 = linkToNode.Y2;

            double lineLingth = Math.Sqrt(Math.Pow(linkToNode.X2 - linkToNode.X1, 2) + Math.Pow(linkToNode.Y2 - linkToNode.Y1, 2));

            if (lineLingth > 0)
            {
                Arrow.X1 = Arrow.X2 + (7 / lineLingth) * (linkToNode.X1 - linkToNode.X2);
                Arrow.Y1 = Arrow.Y2 + (7 / lineLingth) * (linkToNode.Y1 - linkToNode.Y2);
            }
            return Arrow;
        }

        void Move(double X, double Y)
        {
            if (!isParent)
            {
                linkToNode.X2 = X;
                linkToNode.Y2 = Y;
            }
            else
            {
                linkToNode.X1 = X;
                linkToNode.Y1 = Y;
            } 
        }

        public bool ContainsNode(Page Node)
        {
            return (linkedNode == Node);
        }

        public void MoveLinkPoint()
        {
            //double devX = linkedNode.PositionX - myNode.PositionX;
            double devY = linkedNode.PositionY - myNode.PositionY;

            //if (Math.Abs(devX) >= Math.Abs(devY))
            //{
            //    if (devX >= 0)
            //    {
            //        //правая точка
            //        Move(myNode.PositionX + 50, myNode.PositionY + 25);
            //        linkedNode.MoveLinkPoint(myNode);
            //    }
            //    else
            //    { 
            //        //левая точка
            //        Move(myNode.PositionX, myNode.PositionY + 25);
            //        linkedNode.MoveLinkPoint(myNode);
            //    }
            //}
            //else 
            //{
            //double width = (myNode.Rect.ActualWidth == 0) ? 30 : myNode.Rect.ActualWidth/2;
            if (devY >= 0)
            {
                //нижняя точка
                Move(myNode.PositionX + myNode.Width / 2, myNode.PositionY + myNode.Rect.Height);
                linkedNode.MoveLinkPoint(myNode);
            }
            else
            {
                //верхняя точка
                Move(myNode.PositionX + myNode.Width/2, myNode.PositionY);
                linkedNode.MoveLinkPoint(myNode);
            }
            //}
            GetArrow();
        }

        internal void MoveCurrLinkPoint()
        {
            //double devX = linkedNode.PositionX - myNode.PositionX;
            double devY = linkedNode.PositionY - myNode.PositionY;

            //if (Math.Abs(devX) >= Math.Abs(devY))
            //{
            //    if (devX >= 0)
            //    {
            //        //правая точка
            //        Move(myNode.PositionX + 50, myNode.PositionY + 25);
                   
            //    }
            //    else
            //    {
            //        //левая точка
            //        Move(myNode.PositionX, myNode.PositionY + 25);
            //    }
            //}
            //else
            //{
                if (devY >= 0)
                {
                    //нижняя точка
                    //Move(myNode.PositionX + myNode.Rect.ActualWidth / 2, myNode.PositionY + myNode.Rect.ActualHeight);
                    Move(myNode.PositionX + myNode.Width / 2, myNode.PositionY + myNode.Rect.Height);
                }
                else
                {
                    //верхняя точка
                    //Move(myNode.PositionX + myNode.Rect.ActualWidth / 2, myNode.PositionY);
                    Move(myNode.PositionX + myNode.Width / 2, myNode.PositionY);
                }
            //}
            GetArrow();
        }
    }

    public enum NodeSate
    { 
        Set,
        NotSet,
    }
}
