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
        private Socket _socket { get; set; }
        private EndPoint _epLocal { get; set; }
        private EndPoint _epRemote { get; set; }

        private string _localIP
        {
            get { return _localIP; }
            set
            {
                _localIP = value;
                RaisePropertyChanged("Local IP");
            }
        }
        private string _remoteIP { get; set; }

        byte[] _buffer;

        private string _userName { get; set; }


        private void loadBackend()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

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
            return obj is ChatModel && ((ChatModel)obj)._localIP.Equals(_localIP);
        }

        public override int GetHashCode()
        {
            return _localIP.GetHashCode();
        }

    }
}
