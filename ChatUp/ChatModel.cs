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
        //public Socket mySocket { get; set; }
        private string _localPort;
        public string LocalPort
        {
            get { return _localPort; }
            set
            {
                _localPort = value;
                RaisePropertyChanged("LocalPort");
            }
        }

        private string _remotePort;
        public string RemotePort
        {
            get { return _remotePort; }
            set
            {
                _remotePort = value;
                RaisePropertyChanged("RemotePort");
            }
        }

        private string _localIP;
        public string LocalIP
        {
            get { return _localIP; }
            set
            {
                _localIP = value;
                RaisePropertyChanged("LocalIP");
            }
        }

        private string _remoteIP;
        public string RemoteIP
        {
            get { return _remoteIP; }
            set
            {
                _remoteIP = value;
                RaisePropertyChanged("RemoteIP");
            }
        }

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                RaisePropertyChanged("UserName");
            }
        }

        public long ID { get; set; }

        public static string response { get; set; }

        public IPEndPoint ServerEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1337);

        private TcpClient _client = new TcpClient();
        
        private Thread _threadListen;
        
        public event EventHandler<Packet> MessageReceived;
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

        // Constructor #################################
        public ChatModel()
        {
            string ip = "127.0.0.1"; 
            LocalIP = ip;
            LocalPort = "1337";
            RemotePort = "1338";
            RemoteIP = ip; 
            UserName = System.Environment.MachineName;
            ID = DateTime.Now.Ticks;
        }
        //##############################################

        public static ManualResetEvent connectDone = new ManualResetEvent(false);
        public static ManualResetEvent sendDone = new ManualResetEvent(false);
        public static ManualResetEvent receiveDone = new ManualResetEvent(false);

        public class StateObject
        {
            //Client Socket
            public Socket workSocket = null;
            //Size of the recieve buffer
            public const int BufferSize = 256;
            //Recieve buffer
            public byte[] buffer = new byte[BufferSize];
            //Recieved data string
            public StringBuilder sb = new StringBuilder();
        }

        private static void StartClient()
        {
            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // The name of the   
                // remote device is "host.contoso.com".  
                IPHostEntry ipHostInfo = Dns.GetHostEntry("host.contoso.com");
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(RemoteIP, RemotePort);

                // Create a TCP/IP socket.  
                Socket client = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();

                // Send test data to the remote device.  
                Send(client, "This is a test<EOF>");
                sendDone.WaitOne();

                // Receive the response from the remote device.  
                Receive(client);
                receiveDone.WaitOne();

                // Write the response to the console.  
                Console.WriteLine("Response received : {0}", response);

                // Release the socket.  
                client.Shutdown(SocketShutdown.Both);
                client.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }



        // handles the completion of the prior asynchronous 
        // connect call. the socket is passed via the objectState 
        // paramater of Connect().
        public static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                //Retrieve the socket from the state object
                Socket s = (Socket)ar.AsyncState;
                //Complete the connection
                s.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}", s.RemoteEndPoint.ToString());

                //Signal that the connection has been made
                connectDone.Set();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString()); //TODO: change exception
            }
            
        }

        // Asynchronous connect using the host name, resolved via 
        // IPAddress
        public static void Connect(EndPoint remoteEP, Socket client)
        {
            connectDone.Reset();

            Console.WriteLine("Establishing connection to {0}", remoteEP.ToString());

            try
            {
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                // wait here until the connect finishes.  
                // The callback sets allDone.
                connectDone.WaitOne();
            }
            catch(ArgumentNullException ae)
            {
                Console.WriteLine("ArgumentNullException : {0}", ae.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

            Console.WriteLine("Connection established");
        }


        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                //Retrieve the socket from the state object
                Socket client = (Socket)ar.AsyncState;

                //Complete sending the data to the remote device
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server", bytesSent);

                //Signal that all bytes have been sent
                sendDone.Set();
            }
            catch(Exception e)//TODO: change the exception
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Send(Socket client, string data)
        {
            //Convert thestring data to byte data using ASCII encoding
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            //Begin sending the data to the remote device
            client.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(SendCallback), client);
        }
        
        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                //Retrieve the state object and the client socket
                //from the asynchronous state object
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                //read data from the remote device
                int bytesRead = client.EndReceive(ar);
                if(bytesRead > 0)
                {
                    //there might be more data, so store the data received so far
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    //get the rest of the data
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    //all the data has arrived, put it in response
                    if(state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }
                    //signal that all bytes have been received
                    receiveDone.Set();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Receive(Socket client)
        {
            try
            {
                //Create the state object
                StateObject state = new StateObject();
                state.workSocket = client;

                //Begin recieving the data from the remote device
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch(Exception e)//TODO: change exception
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}




/*
        public void ConnectDisconnect()
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
                    
                    Acknowledgement Responce = AckResponces.FirstOrDefault(a => a.RecipientID == _ID);

                    if (Responce != null)
                    {
                        if (OnResultsUpdate != null)
                            OnResultsUpdate.Invoke(this, "Received Acknowledgement Responce from " + EP.ToString());
                        
                        AckResponces.Remove(Responce);

                        return EP;
                    }
                }
            }

            return null;
        }*/









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
