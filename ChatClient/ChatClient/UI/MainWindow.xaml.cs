


namespace ChatClient
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Input;
    using System.Windows.Threading;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using ChatSharedRessource.Models;
    using ChatSharedRessource.Assets;
    using System.Windows;
    using System;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       

        public  MyObserveBoxContainer ComboList { get; set; }
        MainWindow  MyMainWindow { get; set; }
        Process CurrentProcess { get; set; }
        Client CommunicatorClient { get; set; }


        public MainWindow()
        {
            InitializeComponent();
            ListenQueues.MyInstance().PropertyChanged += new PropertyChangedEventHandler(ListeningQueueChanged);
            Clients.MyStaticClients = new List<Client>();
            Clients.ClientsChanged = false;      
            Client.CurrentConnection = new Client(GetLocalIpAddress(), "", "", "", 0, false);            
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ClientIpLabel.Content = GetLocalIpAddress();
            new Thread(new ThreadStart(ClientAsynchListener.StartServer)).Start();
            Closing += this.OnWindowClosing;
            MyMainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            CurrentProcess = Process.GetCurrentProcess();
            ComboList = new MyObserveBoxContainer();
            this.DataContext = this;
        }

        private void AskForConnection()
        {
            Message connectMessage = new Message(IpTextBox.Text, Constants.ServerListenerPort.ToString(), GetLocalIpAddress(),
                (CurrentProcess.Id / 7).ToString(), Constants.AddMePlease, CommunicatorClient.GetClientStr(), new Connect());
            CommunicatorClient.SendMessage(connectMessage);
        }

        private void AskForDisconnection()
        {
            Message disconnectMessage = new Message(IpTextBox.Text, Constants.ServerListenerPort.ToString(),
                GetLocalIpAddress(),(CurrentProcess.Id / 7).ToString(), Constants.Disconnect, CommunicatorClient.GetClientStr(), new Quit());
            Client.CurrentConnection.IsConnected = false;
            CommunicatorClient.SendMessage(disconnectMessage);
            Clients.MyStaticClients.Clear();
            Clients.ClientsChanged = true;
            GuiConnectStatusEvent();
            RefreshUiList();          
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

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            CommunicatorClient = new Client(GetLocalIpAddress(), (CurrentProcess.Id / 7).ToString(),
                NameTextBox.Text, IpTextBox.Text, 0, false);
            
            if (!Client.CurrentConnection.IsConnected)
            {
                Message connectMessage = new Message(IpTextBox.Text, Constants.ServerListenerPort.ToString(),
                    GetLocalIpAddress(),
                    (CurrentProcess.Id / 7).ToString(), Constants.AddMePlease, CommunicatorClient.GetClientStr(), new Connect());
                CommunicatorClient.SendMessage(connectMessage);
            }
            else
            {
                AskForDisconnection();
            }
        }
        public static void ExecuteMessage(Message currentMessage)
        {
            currentMessage?.ControlCommand.ClientTreatment(currentMessage);
            GuiConnectStatusEvent();
            RefreshClientList();
        }

        public static void GuiConnectStatusEvent()
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

                if (Client.CurrentConnection.IsConnected)
                {
                    mainWindow.ConnectButton.Content = Constants.Disconnect;
                    mainWindow.StatusLabel.Content = Constants.Connect;
                    mainWindow.NameTextBox.IsReadOnly = true;
                }
                else
                {
                    mainWindow.ConnectButton.Content = Constants.Connect;
                    mainWindow.StatusLabel.Content = Constants.Disconnect;
                    mainWindow.NameTextBox.IsReadOnly = false;
                }
            }));
        }
        static void ListeningQueueChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "newMessage")
            {
                    ExecuteMessage(ListenQueues.MyInstance().PullMessage());
            }
            else if (e.PropertyName == "newTextMessage")
            {
                WriteToMainBox(ListenQueues.MyInstance().PullTextMessage());
            }
        }
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {       
            AskForDisconnection();
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

        public static void WriteToMainBox(string message)
        {
            // Error connecting
            if(message== Constants.ConnectErrorDuplicated)
            {
                MessageBoxResult result = MessageBox.Show(Constants.ConnectErrorDuplicated);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    mainWindow.ClientMessageTextBlock.Text =
                        mainWindow.ClientMessageTextBlock.Text + message + Constants.Return;
                }));

            }
           
        }


        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SendButton_Click(sender, e);
            }
        }
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsListView.SelectedItems.Count == 0)
            {
                MessageBox.Show(Constants.YouMustSelectClient);
            }
            else
            {
                SendMessageToClients();
            }
            
        }

        

        public void SendMessageToClients()
        {
            Clients multicastSend = new Clients();
            // Get all selected Client ... to send to them
            foreach (Client client in ClientsListView.SelectedItems)
            {
                multicastSend.MyClients.Add(client);
            }
            //Create Client Sender
            CommunicatorClient = new Client(GetLocalIpAddress(), (CurrentProcess.Id / 7).ToString(),
                NameTextBox.Text, IpTextBox.Text, 0, false);
            // create message
            Message multiSendMessage = new Message(IpTextBox.Text, Constants.ServerListenerPort.ToString(),
                GetLocalIpAddress(),
                (CurrentProcess.Id / 7).ToString(), Constants.Received + NameTextBox.Text + Constants.ReturnDash + MessageTextBox.Text + Constants.Return, multicastSend.GetListString(), new Send());
            // Send Message to server with clients  to send list
            CommunicatorClient.SendMessage(multiSendMessage);
            // refresh Msg box
            WriteToMainBox(NameTextBox.Text + Constants.SendMessag + MessageTextBox.Text);
            MessageTextBox.Text = string.Empty;
        }

        private void RefreshListBt_Click(object sender, RoutedEventArgs e)
        {
            Message refreshListMessage = new Message(IpTextBox.Text, Constants.ServerListenerPort.ToString(),
                GetLocalIpAddress(), (CurrentProcess.Id / 7).ToString(),NameTextBox.Text, CommunicatorClient.GetClientStr(), new ClientList());
            CommunicatorClient.SendMessage(refreshListMessage);
            ListenQueues.MyInstance().AddTextMessage(Constants.RefreshClients);
        }
    }
}