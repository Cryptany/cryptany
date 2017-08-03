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

namespace ServiceBlockEditor.Manager
{
    public static class BlockManager
    {
        static string connectionString = "";

        static Dictionary<Guid, string> blockTypes;
        public static Dictionary<Guid, string> BlockTypes
        {
            get 
            {
                if (blockTypes == null)
                {
                    //загрузка из БД через wcf
                }
                return blockTypes;
            }
        }
    }
}
