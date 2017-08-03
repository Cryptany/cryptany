using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using CyberPlat;

namespace UWPFClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly object _lockDone = new object();
        static bool _done;

        private readonly object _lockThreads = new object();
        private readonly List<Thread> _threads = new List<Thread>();

        const string PASSWORD = "123";

        private bool Done
        {
            set
            {
                lock (_lockDone)
                {
                    _done = value;
                }
            }
            get
            {
                lock (_lockDone)
                {
                    return _done;
                }
            }
        }

        public MainWindow()
        {
            //9104501874
            //9854764020
            InitializeComponent();
            try
            {
                var a = new CyberPlatClient();
                //Debug.Print(a.PayRequest(null, null, 0, null));
                //Debug.Print(a.GetStatus());
                a.Done();
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        private void SendRequest(string phoneNumber, int moneyCount)
        {
            try
            {
                Debug.Print("{0}: {1} Start", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
                var client = new UniplatService.UniplatServiceClient();
                var req = new UniplatService.ReqDebit
                {
                    Amount = moneyCount,
                    Comment = string.Format("Ваше такси: {0}", Thread.CurrentThread.ManagedThreadId),
                    PartnerCode = "tst",
                    ServiceId = "6889",
                    MSISDN = phoneNumber,
                    PaymentNumber = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture).Substring(1, 10),
                    PaymentId = Guid.NewGuid().ToString()
                };
                req.Hash = GetHash(req.PaymentId + req.PartnerCode + req.ServiceId + req.MSISDN + req.PaymentNumber +
                                   req.Amount + PASSWORD);
                var resp = client.DebitMoney(req);
                Debug.Print("{0}: {1} Finish", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
                RemoveThread();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("UWPFClient", string.Format("Ошибка в SendRequest: {0}", ex.Message));
            }
        }

        private void RemoveThread()
        {
            lock (_lockThreads)
            {
                _threads.Remove(Thread.CurrentThread);
                if (_threads.Count == 0)
                {
                    Done = true;
                    Debug.Print("Работа закончена!");
                }
                Debug.Print("count={0}", _threads.Count);
            }
        }

        private void SingleRequest(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                double moneyCount;
                if (double.TryParse(MoneyCount.Text, out moneyCount))
                {
                    //переводим рубли в копейки + комис
                    moneyCount = moneyCount * 107.45;

                    var phoneNumber = PhoneNumber.Text;
                    var thread = new Thread(() => SendRequest(phoneNumber, (int)moneyCount));
                    thread.Start();
                    _threads.Add(thread);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("UWPFClient", string.Format("Ошибка в SingleRequest: {0}", ex.Message));
            }
        }

        private void CycleRequest(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                int it;
                int.TryParse(Iteration.Text, out it);
                int th;
                int.TryParse(Threads.Text, out th);
                for (var i = 0; i < it; i++)
                {
                    Debug.Print("Цикл {0}", i);
                    Done = false;
                    for (var j = 0; j < th; j++)
                    {
                        SingleRequest(null, null);
                    }
                    while (!Done)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("UWPFClient", string.Format("Ошибка в CycleRequest: {0}", ex.Message));
            }
        }

        static private string GetHash(string sStr)
        {
            // сначала конвертирую строку в байты
            //byte[] data = Encoding.Unicode.GetBytes(sStr);
            var data = Encoding.ASCII.GetBytes(sStr);

            // экземпляр MD5
            var md5 = MD5.Create();

            // получаю хеш
            var result = md5.ComputeHash(data);

            // конвертирую хеш из byte[16] в строку шестнадцатиричного формата
            // (вида «3C842B246BC74D28E59CCD92AF46F5DA»)
            // это опциональный этап, если вам хеш нужен в строковом виде
            //sResult = BitConverter.ToString(result).Replace("-", string.Empty);

            return Convert.ToBase64String(result);
        }
    }
}
