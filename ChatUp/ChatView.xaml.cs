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

namespace ChatUp
{
    /// <summary>
    /// Interaction logic for ChatView.xaml
    /// </summary>
    public partial class ChatView : Window
    {
        public ChatView()
        {
            InitializeComponent();

            const int contentWidth = 1000;
            const int contentHeight = 600;

            var horisontalBorderHeight = SystemParameters.ResizeFrameHorizontalBorderHeight;
            var verticalBorderWidth = SystemParameters.ResizeFrameVerticalBorderWidth;
            var captionHeight = SystemParameters.CaptionHeight;
            
            Width = contentWidth + 2 * verticalBorderWidth;
            Height = contentHeight + captionHeight + 2 * horisontalBorderHeight;

            this.DataContext = new ChatViewModel(); // Bind the viewmodel to the view
        }

        /*
        //These functions should not be here since its the view and not viewmodel?
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            txtLocalIP.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtLocalPort.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtRemoteIP.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtRemotePort.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }
        private void AddChatButton_Click(object sender, RoutedEventArgs e)
        {
            //add function
        }
        */
    }

}

/*
 * private static Socket ConnectSocket(string server, int port)
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

            using (Socket s = ConnectSocket(server, port))
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

        public static void Main(string[] args)
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
*/