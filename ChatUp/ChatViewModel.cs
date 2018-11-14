using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.ObjectModel;

namespace ChatUp
{
    public class ChatViewModel : BaseINPC
    {
        public ChatModel Chat { get; set; }
        public ObservableCollection<ChatModel> Messages { get; set; }
        public ObservableCollection<string> Users { get; set; }
        
        public ChatViewModel()
        {
            Chat = new ChatModel();
            ConnectButtonCommand = new RelayCommand(new Action<object>(ConnectButtonClick));
            SendButtonCommand = new RelayCommand(new Action<object>(SendButtonClick));
        }
        public ICommand ConnectButtonCommand { get; set; }
        public ICommand SendButtonCommand { get; set; }

        private void ConnectButtonClick(object sender)
        {
            //implement button click here
            MessageBox.Show(sender.ToString());
        }

        private void SendButtonClick(object sender)
        {
            //implement send button click here
            MessageBox.Show(sender.ToString());
        }

        private static Socket SocketConnect(string server, int port)
        {
            Socket s = null;
            IPHostEntry hostEntry = null;

            hostEntry = Dns.GetHostEntry(server);

            // Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
            // an exception that occurs when the host IP Address is not compatible with the address family
            // (typical in the IPv6 case).
            foreach (IPAddress address in hostEntry.AddressList)
            {
                IPEndPoint ipe = new IPEndPoint(address, port);
                Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                tempSocket.Connect(ipe);

                if (tempSocket.Connected)
                {
                    s = tempSocket;
                    break;
                }
                else
                {
                    continue;
                }
            }
            return s;
        }

        private static string SocketSendRecieve(string server, int port)
        {
            string request = "GET / HTTP/1.1\r\nHost: " + server +
            "\r\nConnection: Close\r\n\r\n";
            Byte[] bytesSent = Encoding.ASCII.GetBytes(request);
            Byte[] bytesReceived = new Byte[256];

            using (Socket s = SocketConnect(server, port))
            {
                if (s == null)
                    return ("Connection failed.");

                //Send request to the server
                s.Send(bytesSent, bytesSent.Length, 0);

                int bytes = 0;
                string page = "Default HTML page on " + server + ":/r/n";

                //loop until the page is transmitted
                do
                {
                    bytes = s.Receive(bytesReceived, bytesReceived.Length, 0);
                    page = page + Encoding.ASCII.GetString(bytesReceived, 0, bytes);
                }
                while (bytes > 0);


                return page;
            }
        }

        public static void loop(string[] args)
        {
            string host;
            int port = 80;

            if (args.Length == 0)
            {
                //If no server name is passed as argument to this program,
                //use the current host name as the default.
                host = Dns.GetHostName();
            }
            else
            {
                host = args[0];
            }

            string result = SocketSendRecieve(host, port);
            Console.WriteLine(result);

        }

    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute) : this(execute, null) { }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException("execute");

            _canExecute = canExecute;
            _execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((object)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute((object)parameter);
        }

    }
}
