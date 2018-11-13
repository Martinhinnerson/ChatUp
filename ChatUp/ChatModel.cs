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
    public class ChatModel : BaseINPC
    {
        public Socket Socket { get; set; }
        private string _epLocal;
        public string epLocal
        {
            get { return _epLocal; }
            set
            {
                _epLocal = value;
                RaisePropertyChanged("epLocal");
            }
        }
        public int epRemote { get; set; } //This should be EndPoint instead
        public string localIP { get; set; }
        public string remoteIP { get; set; }

        byte[] _buffer; //this should maybe be in viewModel?

        public string UserName { get; set; }

        public ChatModel()
        {
            string ip = "127.0.0.1";
            localIP = ip;//GetLocalIP();
            epLocal = "1337";
            epRemote = 1338;
            remoteIP = ip; //GetLocalIP(); //This should be user input instead
        }


        private void loadBackend()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            //get user ip
            //localIpText.Text = GetLocalIP();
            //remoteIpText.Text = GetLocalIP();
        }
        
        private string GetLocalIP()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }



        public override bool Equals(object obj)
        {
            return obj is ChatModel && ((ChatModel)obj).localIP.Equals(localIP);
        }

        public override int GetHashCode()
        {
            return localIP.GetHashCode();
        }

    }
}
