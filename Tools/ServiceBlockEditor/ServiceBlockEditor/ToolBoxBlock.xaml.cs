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

namespace ServiceBlockEditor
{
    public partial class ToolBoxBlock : UserControl
    {
        public ToolBoxBlock()
        {
            InitializeComponent();
        }

        private void CheckedMode_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckedMode.BorderThickness = new System.Windows.Thickness(1);
            CheckedMode.Background = new SolidColorBrush(Colors.Blue);
        }

        private void CheckedMode_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckedMode.BorderThickness = new System.Windows.Thickness(0);
            CheckedMode.Background = new SolidColorBrush(Colors.Transparent);
        }
    }
}
