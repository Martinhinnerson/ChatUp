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
    class Client
    {
        private string _name;
        private long _ID;

        public IPEndPoint ServerEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1337);

        private TcpClient _client = new TcpClient();
        private List<Acknowledgement> AckResponces = new List<Acknowledgement>();

        private Thread _threadListen;

        public event EventHandler<string> OnResultsUpdate;
        public event EventHandler<Message> OnMessageReceived;
        public event EventHandler OnServerRequest;
        public event EventHandler OnServerConnect;
        public event EventHandler OnServerDisconnect;
        public event EventHandler<IPEndPoint> OnClientConenction;

        private bool _TCPListen = false;
        public bool TCPListen
        {
            get { return _TCPListen; }
            set
            {
                _TCPListen = value;
                if (value)
                    ListenTCP();
            }
        }

        public Client()
        {
           
            _name = System.Environment.MachineName;
            _ID = DateTime.Now.Ticks;
            
        }

        public void ConnectOrDisconnect()
        {
            if (_client.Connected)
            {
                _client.Client.Disconnect(true);

                TCPListen = false;
                
                if (OnServerDisconnect != null)
                    OnServerDisconnect.Invoke(this, new EventArgs());

              //  if (OnResultsUpdate != null)
               //     OnResultsUpdate.Invoke(this, "Disconnected.");
            }
            else
            {
                try
                {
                   
                    _client = new TcpClient();
                    _client.Client.Connect(ServerEndpoint);
                    
                    TCPListen = true;
                 
                    Thread.Sleep(500);
                    SendMessageTCP(_name);

                    Thread KeepAlive = new Thread(new ThreadStart(delegate
                    {
                        while (_client.Connected)
                        {
                            Thread.Sleep(5000);
                            //SendMessageTCP(new KeepAlive());
                        }
                    }));

                    KeepAlive.IsBackground = true;
                    KeepAlive.Start();

                    if (OnServerConnect != null)
                        OnServerConnect.Invoke(this, new EventArgs());

                }
                catch (Exception ex) //should be specific
                {
                    if (OnResultsUpdate != null)
                        OnResultsUpdate.Invoke(this, "Error when connecting " + ex.Message);
                }
            }
        }

        public void SendMessageTCP(string Item)
        {
            if (_client.Connected)
            {
                byte[] Data = Encoding.ASCII.GetBytes(Item);

                try
                {
                    NetworkStream NetStream = _client.GetStream();
                    NetStream.Write(Data, 0, Data.Length);
                }
                catch (Exception e)
                {
                    if (OnResultsUpdate != null)
                        OnResultsUpdate.Invoke(this, "Error on TCP Send: " + e.Message);
                }
            }
        }

        private void ListenTCP()
        {
            _threadListen = new Thread(new ThreadStart(delegate
            {
                byte[] ReceivedBytes = new byte[4096];
                int BytesRead = 0;

                while (TCPListen)
                {
                    try
                    {
                        BytesRead = _client.GetStream().Read(ReceivedBytes, 0, ReceivedBytes.Length);

                        if (BytesRead == 0)
                            break;
                        else
                        {
                            string Item = Encoding.ASCII.GetString(ReceivedBytes);
                            ProcessItem(Item);
                        }
                    }
                    catch (Exception e) //should be more specific
                    {
                        if (OnResultsUpdate != null)
                            OnResultsUpdate.Invoke(this, "Error on TCP Receive: " + e.Message);
                    }
                }
            }));

            _threadListen.IsBackground = true;

            if (TCPListen)
                _threadListen.Start();
        }

        private void ProcessItem(Message message, IPEndPoint EP = null)
        { 
                    if (OnMessageReceived != null)
                        OnMessageReceived.Invoke(EP, message);
        }

        
        public void ConnectToRemote()
        {
            //Request R = new Request(LocalClientInfo.ID, CI.ID);
            
            if (OnResultsUpdate != null)
                OnResultsUpdate.Invoke(this, "Sent Connection Request");

            Thread Connect = new Thread(new ThreadStart(delegate
            {
                //Connect to remote here

                //Probalby need to catch exceptions here
                    if (OnResultsUpdate != null)
                        OnResultsUpdate.Invoke(this, "Connection Successfull");
                    
            }));

            Connect.IsBackground = true;

            Connect.Start();
        }

        private IPEndPoint FindReachableEndpoint()
        {
            if (OnResultsUpdate != null)
                OnResultsUpdate.Invoke(this, "Attempting to Connect via LAN");

            if (_client.Connected)
            {
                IPAddress IP = IPAddress.Parse("127.0.0.1"); //TODO: shange ip

                IPEndPoint EP = new IPEndPoint(IP, 1337); //TODO: change port

                for (int i = 1; i < 4; i++) // 3 attempts
                {
                    if (!_client.Connected)
                        break;

                    if (OnResultsUpdate != null)
                        OnResultsUpdate.Invoke(this, "Sending Acknowledgement to " + EP.ToString() + ". Attempt " + i + " of 3");
                    
                    Thread.Sleep(200);
                    /*
                    Acknowledgement Responce = AckResponces.FirstOrDefault(a => a.RecipientID == _ID);

                    if (Responce != null)
                    {
                        if (OnResultsUpdate != null)
                            OnResultsUpdate.Invoke(this, "Received Acknowledgement Responce from " + EP.ToString());
                        
                        AckResponces.Remove(Responce);

                        return EP;
                    }*/
                }
            }
            
            return null;
        }

    }
    
}
