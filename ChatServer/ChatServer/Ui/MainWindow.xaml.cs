

using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;

namespace ChatServer
{
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using ChatSharedRessource.Models;
    using ChatSharedRessource.Assets;
    using System.Windows;
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static MyObserveBoxContainer ComboList { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ListenQueues.MyInstance().PropertyChanged += new PropertyChangedEventHandler(ListeningQueueChanged);

            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ServerStateTextBlock.Text = Constants.ServerOn;
            ServerIpLabel.Content = GetLocalIpAddress();
            Clients.MyStaticClients = new List<Client>();
            new Thread(new ThreadStart(AsynchServer.StartServer)).Start();
            Closing += this.OnWindowClosing;
            ComboList = new MyObserveBoxContainer();            
            this.DataContext = this;

        }

        static void ListeningQueueChanged(object sender, PropertyChangedEventArgs e)
        {

            if (e.PropertyName == "newMessage")
            {
                Message currentMessage = ListenQueues.MyInstance().PullMessage(); // removed
                currentMessage?.ControlCommand.ServerTreatment(currentMessage);             
                currentMessage = null;
                RefreshClientList();
            }
            else if (e.PropertyName == "newTextMessage")
            {
                WriteToMainBox(ListenQueues.MyInstance().PullTextMessage());
            }

        }

        public static void RefreshClientList()
        {
            if (Clients.ClientsChanged)
            {
                RefreshUiList();
                Clients.ClientsChanged = false;
            }
        }
        public static void RefreshUiList()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            {
                MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                mainWindow.ClientsListView.Items.Clear();
                Clients allClients = new Clients(Clients.GetStaticListString());
                allClients.MyClients.ForEach(x => mainWindow.ClientsListView.Items.Add(x));

            }));
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        public static void GetClientMessage(Message message)
        {
            ListenQueues.MyInstance().AddMessage(message);
        }
        public static void WriteToMainBox(string message)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                mainWindow.ServerStateTextBlock.Text = mainWindow.ServerStateTextBlock.Text + message + Constants.Return;
            }));
        }

        public void TextBoxListenLoop()
        {
            while (true)
            {
                if (ListenQueues.MyInstance().CountText() > 0)
                {
                    WriteToMainBox(ListenQueues.MyInstance().PullTextMessage());
                }

            }
        }


        public static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception(Constants.IpNotFound);
        }
    }
}
