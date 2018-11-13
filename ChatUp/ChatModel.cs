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
        private string _localPort;
        public string LocalPort
        {
            get { return _localPort; }
            set
            {
                _localPort = value;
                NotifyPropertyChanged(m => m.LocalPort);
            }
        }
        private string _remotePort;
        public string RemotePort
        {
            get { return _remotePort; }
            set
            {
                _remotePort = value;
                NotifyPropertyChanged(m => m.RemotePort);
            }
        }
        private string _localIP;
        public string LocalIP
        {
            get { return _localIP; }
            set
            {
                _localIP = value;
                NotifyPropertyChanged(m => m.LocalIP);
            }
        }
        private string _remoteIp;
        public string RemoteIP
        {
            get { return _remoteIP; }
            set
            {
                _remoteIP = value;
                NotifyPropertyChanged(m => m.RemoteIP);
            }
        }

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                _usetName = value;
                NotifyPropertyChanged(m => m.UserName);
            }
        }
        public ChatModel()
        {   
            string ip = "127.0.0.1"; //localhost
            LocalIP = ip;//GetLocalIP();
            LocalPort = "1337";
            RemotePort = "1338";
            RemoteIP = ip; //GetLocalIP(); 
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
