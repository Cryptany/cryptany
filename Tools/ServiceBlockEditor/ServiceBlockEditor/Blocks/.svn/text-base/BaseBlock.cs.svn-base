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
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ServiceBlockEditor.BlockParams;
using DragAndDropSimple;
using System.Windows.Media.Imaging;

namespace ServiceBlockEditor.Blocks
{
    public class BaseBlock 
    {

        #region создание нового блока
        public BaseBlock(Guid BlockTypeID, string Name)
        {
            this.blockTypeID = BlockTypeID;
            this.blockName = Name;
        } 
        #endregion

        #region загрузка из БД
        public BaseBlock(Guid ID)
        {
            id = ID;
        } 
        #endregion

        public BaseBlock(string Name, BitmapImage Preview)
        {
            this.blockName = Name;
            this.preview = Preview;
        } 

        Guid id;
        public string blockName { get; set; }
        Guid blockTypeID;
        string blockType;
        //BaseBlock settings;
        bool isVerification;
        public BitmapImage preview { get; set; }

    }
}
