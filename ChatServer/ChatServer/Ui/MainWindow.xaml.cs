namespace ChatServer
{
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using ChatSharedRessource;
    using System.Windows;
    using System.Collections.Generic;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ServerStateTextBlock.Text = Constants.ServerOn;
            new Thread(new ThreadStart(StartServer)).Start();
        }

        public void StartServer()
        {
            
            TcpListener server = new TcpListener(IPAddress.Any, Constants.ServerListenerPort);
            server.Start();
            new Thread(new ThreadStart(CommunicationLoop)).Start();
            while (true)
            {
                ClientWorking client = new ClientWorking(server.AcceptTcpClient());
                new Thread(new ThreadStart(client.DoSomethingWithClient)).Start();
            }
            server.Stop();


        }

        public void ListenTcpQueue()
        {
            if (ListenQueue.MyInstance().Count() != 0)
            {
                Thread test = new Thread(new ThreadStart(ChangeText));
                test.Start();
            }

        }

        public void ChangeText()
        {
            this.Dispatcher.Invoke(() => {
                if (ListenQueue.MyInstance().Count() != 0)
                    ServerStateTextBlock.Text = ServerStateTextBlock.Text + ListenQueue.MyInstance().PullMessage() + Constants.Return;
            });
            
        }

        public static void GetClientMessage(string message)
        {
            ListenQueue.MyInstance().AddMessage(message);
        }

        public void CommunicationLoop()
        {
            while (true)
            {
                ListenTcpQueue();
                //SendMessage();
            }
        }
    }
}
