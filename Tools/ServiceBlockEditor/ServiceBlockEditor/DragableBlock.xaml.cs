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
using System.Windows.Controls.Primitives;
using ServiceBlockEditor.Blocks;
using ServiceBlockEditor.BlockParams;

namespace DragAndDropSimple
{
    public partial class DragableBlock : UserControl
    {
        public DragableBlock(BaseBlock Data, double X, double Y)
        {
            //InitializeComponent();
            //data = Data;

            //Run r = new Run();
            //r.Text = data.BlockName;
            //MyCaption.Inlines.Add(r);
            //MyCaption.Inlines.Add(new LineBreak());
            //r = new Run();
            //r.Text = data.contextName;
            //MyCaption.Inlines.Add(r); 

         
            //{
            //    DBlock.SetValue(Canvas.TopProperty, Y);
            //    mouseVerticalPosition = Y;
            //    MyCaption.SetValue(Canvas.TopProperty, Y);
            //}
          
            //{
            //    DBlock.SetValue(Canvas.LeftProperty, X);
            //    mouseHorizontalPosition = X;
            //    MyCaption.SetValue(Canvas.LeftProperty,X);
            //}


        }
        public DragableBlock()
        {
            //InitializeComponent();
            //data = new BaseBlock("Empty", new List<BaseParam>());

            //Run r = new Run();
            //r.Text = data.BlockName;
            //MyCaption.Inlines.Add(r);
            //MyCaption.Inlines.Add(new LineBreak());
            //r = new Run();
            //r.Text = data.contextName;
            //MyCaption.Inlines.Add(r); 
        }
        public BaseBlock data;
       
        bool isMouseCaptured;
        double mouseVerticalPosition;
        double mouseHorizontalPosition;

        #region Для редактора

        Popup p = new Popup();

        bool isMouseCapturedEditor;
        double mouseVerticalPositionEditor;
        double mouseHorizontalPositionEditor;

        public void p_MouseLeftButtonDown(object sender, MouseEventArgs args)
        {
            Border item = sender as Border;
            mouseVerticalPositionEditor = args.GetPosition(null).Y;
            mouseHorizontalPositionEditor = args.GetPosition(null).X;
            isMouseCapturedEditor = true;
            item.CaptureMouse();
        }
        public void p_MouseMove(object sender, MouseEventArgs args)
        {
            Border item = sender as Border;

            if (isMouseCapturedEditor)
            {
                double deltaV = args.GetPosition(null).Y - mouseVerticalPositionEditor;
                double deltaH = args.GetPosition(null).X - mouseHorizontalPositionEditor;
                double newTop = deltaV + (double)p.VerticalOffset;
                double newLeft = deltaH + (double)p.HorizontalOffset;

                p.VerticalOffset = newTop;
                mouseVerticalPositionEditor = args.GetPosition(null).Y;

                p.HorizontalOffset = newLeft;
                mouseHorizontalPositionEditor = args.GetPosition(null).X;
            }
        }
        public void p_MouseLeftButtonUp(object sender, MouseEventArgs args)
        {
            Border item = sender as Border;
            isMouseCapturedEditor = false;
            item.ReleaseMouseCapture();
            mouseVerticalPositionEditor = -1;
            mouseHorizontalPositionEditor = -1;
        }
        //public void CreatePopup()
        //{

        //    Border border = new Border();
        //    border.BorderBrush = new SolidColorBrush(Colors.Black);
        //    border.BorderThickness = new Thickness(5.0);

        //    StackPanel panel1 = new StackPanel();
        //    panel1.Background = new SolidColorBrush(Colors.White);

        //    foreach (BaseParam item in data.Properties)
        //    {
        //        StackPanel panel2 = new StackPanel();
        //        panel2.Orientation = Orientation.Horizontal;
        //        TextBlock paramName = new TextBlock();
        //        paramName.Text = item.paramName;
        //        paramName.Margin = new Thickness(5.0);
        //        paramName.Width = 200;
        //        panel2.Children.Add(paramName);

        //        panel2.Children.Add(item.DrawParam());

        //        panel1.Children.Add(panel2);
        //    }

        //    StackPanel panel3 = new StackPanel();
        //    panel3.Orientation = Orientation.Horizontal;

        //    Button Cancel = new Button();
        //    Cancel.Content = "Cancel";
        //    Cancel.Margin = new Thickness(5.0);
        //    //button1.Click += new RoutedEventHandler(button1_Click);
        //    panel3.Children.Add(Cancel);

        //    Button Ok = new Button();
        //    Ok.Content = "Ok";
        //    Ok.Margin = new Thickness(5.0);
        //    Ok.Click += new RoutedEventHandler(button1_Click);

        //    panel3.Children.Add(Ok);
        //    panel1.Children.Add(panel3);
        //    border.Child = panel1;

        //    border.MouseLeftButtonDown += new MouseButtonEventHandler(p_MouseLeftButtonDown);
        //    border.MouseMove += new MouseEventHandler(p_MouseMove);
        //    border.MouseLeftButtonUp += new MouseButtonEventHandler(p_MouseLeftButtonUp);
            
        //    p.Child = border;
        //    p.Width = 600;


        //}
        #endregion
        
        

        public void Handle_MouseDown(object sender, MouseEventArgs args)
        {
            Border item = sender as Border;
            mouseVerticalPosition = args.GetPosition(null).Y;
            mouseHorizontalPosition = args.GetPosition(null).X;
            isMouseCaptured = true;
            item.CaptureMouse();
        }

        

        public void SetCuptured()
        {
            DBlock.Background = new SolidColorBrush(Colors.LightGray);
        }
        public void SetUnCuptured()
        {
            DBlock.Background = new SolidColorBrush(Colors.White);
        }
       

        public void Handle_MouseMove(object sender, MouseEventArgs args)
        {
            Border item = sender as Border;

            if (isMouseCaptured)
            {
                    // Calculate the current position of the object.
                double deltaV = args.GetPosition(null).Y - mouseVerticalPosition;
                double deltaH = args.GetPosition(null).X - mouseHorizontalPosition;
                double newTop = deltaV + (double)item.GetValue(Canvas.TopProperty);
                double newLeft = deltaH + (double)item.GetValue(Canvas.LeftProperty);

                if (newTop > 0 && newTop < ((Canvas)this.Parent).ActualHeight - 50)
                {
                    item.SetValue(Canvas.TopProperty, newTop);
                    mouseVerticalPosition = args.GetPosition(null).Y;
                    this.MyCaption.SetValue(Canvas.TopProperty, newTop);
                }
                if (newLeft > 0 && newLeft < ((Canvas)this.Parent).ActualWidth - 50)
                {
                    item.SetValue(Canvas.LeftProperty, newLeft);
                    mouseHorizontalPosition = args.GetPosition(null).X;
                    this.MyCaption.SetValue(Canvas.LeftProperty, newLeft);
                }
            }
        }

        public void Handle_MouseUp(object sender, MouseEventArgs args)
        {
            Border item = sender as Border;
            isMouseCaptured = false;
            item.ReleaseMouseCapture();
            mouseVerticalPosition = -1;
            mouseHorizontalPosition = -1;
        }


     

        //private void Border_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    //скрываем контекстное меню с silverlight
        //    e.Handled = true;

        //    CreatePopup();
        //    // Set where the popup will show up on the screen.
        //    p.VerticalOffset = e.GetPosition(null).Y;
        //    p.HorizontalOffset = e.GetPosition(null).X;

        //    // Open the popup.
        //    p.IsOpen = true;
        //}
        void button1_Click(object sender, RoutedEventArgs e)
        {
            // Close the popup.
            p.IsOpen = false;

        }

    }
}
