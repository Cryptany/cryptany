using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TestMSMQ;
using System.Messaging;
using System.IO;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using Avant.Core.Interaction;
using System.Xml;
using System.Reflection;
using System.Collections;
using Microsoft.Win32;

namespace MSMQEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string MashineName 
        {
            get
            {
                if (CurrentMashine.IsChecked == true)
                    return Environment.MachineName;
                return txtMashineName.Text;
            }
        }
        BackgroundWorker bWorker; 
        List<string> Queues = new List<string>();
        Popup bWorkerController;

        List<MessageWithText> CurMessages;
        string CurQueueName
        {
            get
            {
                return "FormatName:Direct=OS:" + MashineName + "\\" + (string)QueuesList.SelectedItem;
            }
        }
        Message CurMessage;
        string TextToCopy;

        public MainWindow()
        {
            InitializeComponent();
            
        }
        private MessageType GetMessageType(Message message)
        {
            try
            {
                message.BodyStream.Seek(0,SeekOrigin.Begin);
                message.Formatter = new System.Messaging.BinaryMessageFormatter();
                if (message.Body.GetType() == typeof(avantMobile.avantCore.OutputMessage))
                    return MessageType.OutputMessage;
                if (message.Body.GetType() == typeof(Avant.Core.MsmqLog.MSMQLogEntry))
                    return MessageType.MSMQLogEntry;
                if (message.Body.GetType() == typeof(Avant.Core.Management.WMI.MessageState))
                    return MessageType.MessageState;
                if (message.Body.GetType() == typeof(avantMobile.avantCore.Message))
                    return MessageType.Message;
                if (message.Body.GetType() == typeof(SubscriptionMessage))
                    return MessageType.SubscriptionMessage;
            }
            catch (Exception)
            {
                try
                {
                    message.Formatter = new System.Messaging.XmlMessageFormatter();
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(message.BodyStream);
                    return MessageType.XML;
                }
                catch (Exception)
                {
                    return MessageType.Unknown;
                }
            }
            return MessageType.Unknown;
        }
        #region блокировка на время обработки
        private void DrawBWorkerController(string message)
        {
            bWorkerController = new Popup();
            bWorkerController.AllowsTransparency = true;
            bWorkerController.Visibility = System.Windows.Visibility.Visible;
            bWorkerController.HorizontalOffset = 500;
            bWorkerController.VerticalOffset = 500;
            //bWorkerController.Width = 200;
            //bWorkerController.Height = 150;

            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(2);
            border.Background = new SolidColorBrush(Colors.White);
            

            StackPanel panel = new StackPanel();
            panel.Margin = new Thickness(5);
            panel.Orientation = Orientation.Vertical;
            TextBlock Info = new TextBlock();
            Info.Text = message;
            Info.FontSize = 30;
            //Button StopButton = new Button();
            //StopButton.Content = "Оcтановить";
            //StopButton.Click += new RoutedEventHandler(StopBWorker_Click);
            panel.Children.Add(Info);
            //panel.Children.Add(StopButton);
            border.Child = panel;
            bWorkerController.Child = border;
            bWorkerController.IsOpen = true;
            this.IsEnabled = false;
        }
        private void StopBWorker_Click(object sender, RoutedEventArgs e)
        {
            bWorker.CancelAsync();
            bWorkerController.IsOpen = false;
            this.IsEnabled = false;
        } 
        #endregion
        #region четние очередей
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bWorker  = new BackgroundWorker();
            bWorker.DoWork += new DoWorkEventHandler(GetAllQueues);
            bWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GetAllQueues_RunWorkerCompleted);
            bWorker.WorkerSupportsCancellation = true;
            //bWorker.WorkerReportsProgress = true;
            bWorker.RunWorkerAsync(new object[]{MashineName,CurrentUser.IsChecked});
            DrawBWorkerController("Выполняется поиск очередей");
        }
        private void GetAllQueues(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            object[] par = e.Argument as object[];
            string _mashineName = par[0] as string;
            bool useCurUser = (bool)par[1];
            List<string> result = new List<string>();
            if (useCurUser)
            {
                MessageQueue[] QueueList = MessageQueue.GetPrivateQueuesByMachine(_mashineName);
                foreach (MessageQueue q in QueueList)
                {
                    result.Add(q.QueueName);
                }
                result.Sort();
            }
            else
            {
                string username = "safari2pool";
                string domain = "avant";
                using (Impersonator imp = new Impersonator(username, domain, "Daw18!otM"))
                {
                    MessageQueue[] QueueList = MessageQueue.GetPrivateQueuesByMachine(_mashineName);
                    foreach (MessageQueue q in QueueList)
                    {
                        result.Add(q.QueueName);
                    }
                    result.Sort();
                }
            }
            e.Result = result;
        }
        private void GetAllQueues_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            bWorkerController.IsOpen = false;
            this.IsEnabled = true;
            if (!(e.Error == null))
                MessageBox.Show("Ошибка: " + e.Error.Message);
            else
                this.QueuesList.DataContext = e.Result;
        }
        #endregion
        #region чтение сообщений
        private void QueuesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageInfo.Items.Clear();
            bWorker = new BackgroundWorker();
            bWorker.DoWork += new DoWorkEventHandler(GetMessages);
            bWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GetMessages_RunWorkerCompleted);
            bWorker.WorkerSupportsCancellation = true;
            //bWorker.WorkerReportsProgress = true;
            bWorker.RunWorkerAsync(new object[]{CurQueueName, CurrentUser.IsChecked});
            DrawBWorkerController("Выполняется чтение сообщений");

        }
        private void GetMessages(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            object[] par = e.Argument as object[];
            string _curQueueName = par[0] as string;
            bool useCurUser = (bool)par[1];
            Message[] messages;

            if (useCurUser)
            {
                var queue = new MessageQueue(_curQueueName)
                {
                    MessageReadPropertyFilter = new MessagePropertyFilter
                    {
                        Body = true,
                        Id = true,
                    }   
                };

               messages = queue.GetAllMessages();
                #region comments
                //using (System.Messaging.Cursor c = queue.CreateCursor())
                //{
                //    System.Messaging.Message m = null;
                //    try
                //    {
                //        m = queue.Peek(new TimeSpan(0, 1, 0), c, PeekAction.Current);
                //        messages.Add(m);
                //    }
                //    catch (MessageQueueException mex)
                //    {
                //        m = null;
                //    }

                //    while (m != null)
                //    {
                //        try
                //        {
                //            m = queue.Peek(new TimeSpan(0, 0, 1), c, PeekAction.Next);
                //            messages.Add(m);
                //        }
                //        catch (Exception ex)
                //        {
                //            m = null;
                //        } 
                //    }
                //} 
                #endregion

            }
            else
            {
                string username = "safari2pool";
                string domain = "avant";
                using (Impersonator imp = new Impersonator(username, domain, "Daw18!otM"))
                {
                    var queue = new MessageQueue(_curQueueName)
                        {
                            MessageReadPropertyFilter = new MessagePropertyFilter
                            {
                                Body = true,
                                Id = true,
                            }
                        };

                    messages = queue.GetAllMessages();
                    #region comments
                    //using (System.Messaging.Cursor c = queue.CreateCursor())
                    //{
                    //    System.Messaging.Message m = null;
                    //    try
                    //    {
                    //        m = queue.Peek(new TimeSpan(0, 0, 30), c, PeekAction.Current);
                    //        messages.Add(m);
                    //    }
                    //    catch (MessageQueueException mex)
                    //    {
                    //        m = null;
                    //    }

                    //    while (m != null)
                    //    {
                    //        try
                    //        {
                    //            m = queue.Peek(new TimeSpan(0, 0, 1), c, PeekAction.Next);
                    //            messages.Add(m);
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            m = null;
                    //        }
                    //    }
                    //} 
                    #endregion
                }
            }
            
            e.Result = ParseMessages(messages);
        }
        private void GetMessages_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            bWorkerController.IsOpen = false;
            this.IsEnabled = true;
            if (!(e.Error == null))
                MessageBox.Show("Ошибка: " + e.Error.Message);
            else
            {
                CurMessages = (List<MessageWithText>)e.Result;
                MessageList.DataContext = CurMessages;
            }
            
        } 
        #endregion
        #region Отображение сообщения
        private void MessageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if((MessageWithText)MessageList.SelectedItem == null) return;
            MessageWithText selMess = (MessageWithText)MessageList.SelectedItem;
            CurMessage = selMess.Message;
            Message message = selMess.Message;
            MessageInfo.Items.Clear();
            switch (GetMessageType(message))
            {
                case MessageType.Message:
                    message.Formatter = new System.Messaging.BinaryMessageFormatter();
                    avantMobile.avantCore.Message mes = (avantMobile.avantCore.Message)message.Body;
                    DrawMessage(mes);
                    break;
                case MessageType.MessageState:
                    message.Formatter = new System.Messaging.BinaryMessageFormatter();
                    Avant.Core.Management.WMI.MessageState mes1 = (Avant.Core.Management.WMI.MessageState)message.Body;
                    DrawMessage(mes1);
                    break;
                case MessageType.MSMQLogEntry:
                    message.Formatter = new System.Messaging.BinaryMessageFormatter();
                    Avant.Core.MsmqLog.MSMQLogEntry mes2 = (Avant.Core.MsmqLog.MSMQLogEntry)message.Body;
                    DrawMessage(mes2);
                    break;
                case MessageType.OutputMessage:
                    message.Formatter = new System.Messaging.BinaryMessageFormatter();
                    avantMobile.avantCore.OutputMessage mes3 = (avantMobile.avantCore.OutputMessage)message.Body;
                    DrawMessage(mes3);
                    break;
                case MessageType.SubscriptionMessage:
                    message.Formatter = new System.Messaging.BinaryMessageFormatter();
                    SubscriptionMessage mes4 = (SubscriptionMessage)message.Body;
                    DrawMessage(mes4);
                    break;
                case MessageType.XML:
                    DrawSimpleMessage(selMess.Text);
                    break;
                case MessageType.Unknown:
                    DrawSimpleMessage(selMess.Text);
                    break;
            }
        }
        private void DrawSimpleMessage(string p)
        {
            TextBlock info = new TextBlock();
            info.TextWrapping = TextWrapping.WrapWithOverflow;
            info.Text = p;
            MessageInfo.Items.Add(info);
            TextToCopy = p;
        }
        private void DrawMessage(object mes4)
        {
            TextToCopy = "";
            string buffer = "";
            foreach (FieldInfo property in mes4.GetType().GetFields())
            {
                if (property.FieldType == typeof(DateTime))
                {
                    buffer = property.Name + " : " + ((DateTime)property.GetValue(mes4)).ToShortDateString();
                    TextToCopy += buffer + ";" + Environment.NewLine;
                    MessageInfo.Items.Add(buffer);
                }
                if (property.FieldType == typeof(List<Guid>))
                {
                    buffer = property.Name + " : ";
                    foreach (var param in (List<Guid>)property.GetValue(mes4))
                        buffer += param.ToString() + ";";
                    TextToCopy += buffer + Environment.NewLine;
                    MessageInfo.Items.Add(buffer);
                }
                if (property.FieldType == typeof(Hashtable))
                {
                    buffer = property.Name + " : ";
                    foreach (KeyValuePair<string, string> param in (Hashtable)property.GetValue(mes4))
                        buffer += param.Key + "=" + param.Value + Environment.NewLine;
                    TextToCopy += buffer + ";";
                    MessageInfo.Items.Add(buffer);
                }
                if (property.FieldType != typeof(Hashtable) 
                    && property.FieldType != typeof(List<Guid>)
                    && property.FieldType != typeof(DateTime))
                {
                    buffer = property.Name + " : " + property.GetValue(mes4).ToString();
                    TextToCopy += buffer + ";" + Environment.NewLine;
                    MessageInfo.Items.Add(buffer);
                }
            }
            foreach (PropertyInfo property in mes4.GetType().GetProperties())
            {
                if (property.PropertyType == typeof(DateTime))
                {
                    buffer = property.Name + " : " + ((DateTime)property.GetValue(mes4,null)).ToShortDateString();
                    TextToCopy += buffer + ";" + Environment.NewLine;
                    MessageInfo.Items.Add(buffer);
                }
                if (property.PropertyType == typeof(List<Guid>))
                {
                    buffer = property.Name + " : ";
                    foreach (var param in (List<Guid>)property.GetValue(mes4,null))
                        buffer += param.ToString() + ";";
                    TextToCopy += buffer + Environment.NewLine;
                    MessageInfo.Items.Add(buffer);
                }
                if (property.PropertyType == typeof(Hashtable))
                {
                    buffer = property.Name + " : ";
                    foreach (KeyValuePair<string, string> param in (Hashtable)property.GetValue(mes4,null))
                        buffer += param.Key + "=" + param.Value + Environment.NewLine;
                    TextToCopy += buffer + ";";
                    MessageInfo.Items.Add(buffer);
                }
                if (property.PropertyType != typeof(Hashtable)
                    && property.PropertyType != typeof(List<Guid>)
                    && property.PropertyType != typeof(DateTime))
                {
                    if (property.GetValue(mes4, null) != null)
                    {
                        buffer = property.Name + " : " + property.GetValue(mes4, null).ToString();
                        TextToCopy += buffer + ";" + Environment.NewLine;
                        MessageInfo.Items.Add(buffer);
                    }
                }
            }
        }
        #endregion
        #region удаление сообщения
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            bWorker = new BackgroundWorker();
            bWorker.DoWork += new DoWorkEventHandler(DellMessage);
            bWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DellMessage_RunWorkerCompleted);
            bWorker.RunWorkerAsync(new object[]{CurQueueName, CurMessage, CurrentUser.IsChecked});
            DrawBWorkerController("Выполняется удаление сообщения");
        }
        private void DellMessage(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            
            object[] arg = e.Argument as object[];
            string _curQueueName = arg[0] as string;
            Message _curMessage = arg[1] as Message;
            bool useCurUser = (bool)arg[2];

            if (useCurUser)
            {
                 MessageQueue _MainInputSMSQueue = new MessageQueue(_curQueueName, false);
                    _MainInputSMSQueue.ReceiveById(_curMessage.Id);
            }
            else
            {
                string username = "safari2pool";
                string domain = "avant";
                using (Impersonator imp = new Impersonator(username, domain, "Daw18!otM"))
                {
                    MessageQueue _MainInputSMSQueue = new MessageQueue(_curQueueName, false);
                    _MainInputSMSQueue.ReceiveById(_curMessage.Id);
                }
            }
        }
        private void DellMessage_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            bWorkerController.IsOpen = false;
            this.IsEnabled = true;

            if (!(e.Error == null))
                MessageBox.Show("Ошибка: " + e.Error.Message);
            else
            {
                var DellItem = CurMessages.Where(a=>a.Message == CurMessage).FirstOrDefault();
                if (DellItem != null)
                {
                    CurMessages.Remove(DellItem);
                    MessageList.DataContext = null;
                    MessageList.DataContext = CurMessages;
                }
            }
        }
        #endregion
        /// <summary>
        /// Сохр сообщ в буфер обмена
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TextToCopy);
        }
        /// <summary>
        /// Обновление сообщений
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MessageInfo.Items.Clear();
            bWorker = new BackgroundWorker();
            bWorker.DoWork += new DoWorkEventHandler(GetMessages);
            bWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GetMessages_RunWorkerCompleted);
            bWorker.WorkerSupportsCancellation = true;
            //bWorker.WorkerReportsProgress = true;
            bWorker.RunWorkerAsync(new object[] { CurQueueName, CurrentUser.IsChecked });
            DrawBWorkerController("Выполняется чтение сообщений");
        }
        /// <summary>
        /// сохранение в файл
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Stream myStream;
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt";
            if (dlg.ShowDialog() == true)
            {
                if ((myStream = dlg.OpenFile()) != null)
                {
                    StreamWriter writer = new StreamWriter(myStream);
                    writer.WriteLine(TextToCopy);
                    writer.Close();
                }
            }
        }
        /// <summary>
        /// разбор сообщений
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        private List<MessageWithText> ParseMessages(Message[] messages)
        {
            MessageType MType = MessageType.Unknown;
            if (messages.Count() > 0)
                MType = GetMessageType(messages[0]);
            List<MessageWithText> MessagesWithId = new List<MessageWithText>();
            foreach (var message in messages)
            {
                string ms = "";
                switch (MType)
                {
                    case MessageType.Message:
                        message.Formatter = new System.Messaging.BinaryMessageFormatter();
                        avantMobile.avantCore.Message mes = (avantMobile.avantCore.Message)message.Body;
                        ms = "MSISDN=" + mes.MSISDN + "; Текст=" + mes.Text;
                        break;
                    case MessageType.MessageState:
                        message.Formatter = new System.Messaging.BinaryMessageFormatter();
                        Avant.Core.Management.WMI.MessageState mes1 = (Avant.Core.Management.WMI.MessageState)message.Body;
                        ms = "Статус=" + mes1.Status + "; Дата=" + mes1.StatusTime + "; Описание" + mes1.StatusDescription;
                        break;
                    case MessageType.MSMQLogEntry:
                        message.Formatter = new System.Messaging.BinaryMessageFormatter();
                        Avant.Core.MsmqLog.MSMQLogEntry mes2 = (Avant.Core.MsmqLog.MSMQLogEntry)message.Body;
                        ms = "Комманда=" + mes2.CommandText + "; БД=" + mes2.DatabaseName;
                        break;
                    case MessageType.OutputMessage:
                        message.Formatter = new System.Messaging.BinaryMessageFormatter();
                        avantMobile.avantCore.OutputMessage mes3 = (avantMobile.avantCore.OutputMessage)message.Body;
                        ms = "ID=" + mes3.InboxMsgID.ToString() + "; Текст=" + mes3.Content;
                        break;
                    case MessageType.SubscriptionMessage:
                        message.Formatter = new System.Messaging.BinaryMessageFormatter();
                        SubscriptionMessage mes4 = (SubscriptionMessage)message.Body;
                        ms = "MSISDN=" + mes4.MSISDN + "; Действие=" + mes4.actionType.ToString();
                        break;
                    case MessageType.XML:
                        message.Formatter = new System.Messaging.BinaryMessageFormatter();
                        StreamReader sr = new StreamReader(message.BodyStream);
                        while (sr.Peek() >= 0)
                        {
                            ms += sr.ReadLine();
                        }
                        ms = Regex.Replace(ms, @"[^a-zA-Zа-яА-Я0-9=\.,\-:]", " ");
                        ms = Regex.Replace(ms, @"[ ]+", " ");
                        break;
                    case MessageType.Unknown:
                        message.Formatter = new System.Messaging.BinaryMessageFormatter();
                        StreamReader sr1 = new StreamReader(message.BodyStream);
                        message.BodyStream.Seek(0, SeekOrigin.Begin);
                        while (sr1.Peek() >= 0)
                        {
                            ms += sr1.ReadLine();
                        }
                        ms = Regex.Replace(ms, @"[^a-zA-Zа-яА-Я0-9=\.,\-:]", " ");
                        ms = Regex.Replace(ms, @"[ ]+", " ");
                        break;
                }
                MessagesWithId.Add(new MessageWithText() { Message = message, Text = ms });
            }
            return MessagesWithId;
        }
        #region Ожидание сообщений
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            int waitTime;
            if (!int.TryParse(PeriodOfWaiting.Text, out waitTime))
            {
                MessageBox.Show("Введите период ожидания");
                return;
            }
            MessageInfo.Items.Clear();
            bWorker = new BackgroundWorker();
            bWorker.DoWork += new DoWorkEventHandler(WaitMessages);
            bWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GetMessages_RunWorkerCompleted);
            bWorker.WorkerSupportsCancellation = true;
            //bWorker.WorkerReportsProgress = true;
            bWorker.RunWorkerAsync(new object[] { CurQueueName, CurrentUser.IsChecked, waitTime });
            DrawBWorkerController("Выполняется чтение сообщений");
        }

        private void WaitMessages(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            object[] par = e.Argument as object[];
            string _curQueueName = par[0] as string;
            bool useCurUser = (bool)par[1];
            int waitPeriod = (int)par[2];
            List<Message> messages = new List<Message>();

            if (useCurUser)
            {
                var queue = new MessageQueue(_curQueueName)
                {
                    MessageReadPropertyFilter = new MessagePropertyFilter
                    {
                        Body = true,
                        Id = true,
                    }
                };

                using (System.Messaging.Cursor c = queue.CreateCursor())
                {
                    System.Messaging.Message m = null;
                    try
                    {
                        m = queue.Peek(new TimeSpan(0, 0, waitPeriod), c, PeekAction.Current);
                        messages.Add(m);
                    }
                    catch (MessageQueueException mex)
                    {
                        m = null;
                    }

                    while (m != null)
                    {
                        try
                        {
                            m = queue.Peek(new TimeSpan(0, 0, 1), c, PeekAction.Next);
                            messages.Add(m);
                        }
                        catch (Exception ex)
                        {
                            m = null;
                        }
                    }
                } 


            }
            else
            {
                string username = "safari2pool";
                string domain = "avant";
                using (Impersonator imp = new Impersonator(username, domain, "Daw18!otM"))
                {
                    var queue = new MessageQueue(_curQueueName)
                    {
                        MessageReadPropertyFilter = new MessagePropertyFilter
                        {
                            Body = true,
                            Id = true,
                        }
                    };

                    using (System.Messaging.Cursor c = queue.CreateCursor())
                    {
                        System.Messaging.Message m = null;
                        try
                        {
                            m = queue.Peek(new TimeSpan(0, 0, waitPeriod), c, PeekAction.Current);
                            messages.Add(m);
                        }
                        catch (MessageQueueException mex)
                        {
                            m = null;
                        }

                        while (m != null)
                        {
                            try
                            {
                                m = queue.Peek(new TimeSpan(0, 0, 1), c, PeekAction.Next);
                                messages.Add(m);
                            }
                            catch (Exception ex)
                            {
                                m = null;
                            }
                        }
                    } 

                }
            }

            e.Result = ParseMessages(messages.ToArray());
        }

        #endregion
    }

    public class MessageWithText
    {
        public string Text { get; set; }
        public Message Message { get; set; }
    }
    enum MessageType
    { 
        XML,
        Unknown,
        OutputMessage,
        MSMQLogEntry,
        MessageState,
        Message,
        SubscriptionMessage
    }
}
