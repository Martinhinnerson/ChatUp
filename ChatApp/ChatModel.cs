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
using System.Windows.Threading;

namespace ChatApp
{
    public class ChatModel : BaseViewModel
    {

        private int _localPort;
        public int LocalPort
        {
            get { return _localPort; }
            set
            {
                _localPort = value;
                RaisePropertyChanged("LocalPort");
            }
        }

        private int _remotePort;
        public int RemotePort
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
        
        private int _numClients; // Store how many clients is connected

        public bool IsListening { get; set; } 
        public bool IsConnected { get; set; } 

        private List<Client> _clients;//Store all connected clients
        public List<Client> Clients
        {
            get { return _clients; }
            set
            {
                _clients = value;
                RaisePropertyChanged("Clients");
            }
        }

        private int _selectedClient; //Which client are we looking at in the chat window
        public int SelectedClient
        {
            get { return _selectedClient; }
            set
            {
                _selectedClient = value;
                RaisePropertyChanged("SelectedClient");
            }
        }

        private string _lastReceivedMessage;
        public string LastReceivedMessage
        {
            get { return _lastReceivedMessage; }
            set
            {
                _lastReceivedMessage = value;
                RaisePropertyChanged("LastReceivedMessage");
            }
        }

        public delegate void ReceivedMessageHandler(object sender, string str);
        public event ReceivedMessageHandler ReceivedMessageFromClient;

        public bool Accept { get; set; }

        private bool _showPopup;
        public bool ShowPopup
        {
            get { return _showPopup; }
            set
            {
                _showPopup = value;
                RaisePropertyChanged("ShowPopup");
            }
        }

        private string _popupMessage;
        public string PopupMessage
        {
            get { return _popupMessage; }
            set
            {
                _popupMessage = value;
                RaisePropertyChanged("PopupMessage");
            }
        }

        private string _newConnectionName;
        public string NewConnectionName
        {
            get { return _newConnectionName; }
            set
            {
                _newConnectionName = value;
                RaisePropertyChanged("NewConnectionName");
            }
        }

        /*
         * Constructor
         */
        public ChatModel()
        {
            ShowPopup = false;
            IsListening = false;
            IsConnected = false;

            LocalIP = "127.0.0.1";
            RemoteIP = "127.0.0.1";
            LocalPort = 8888;
            RemotePort = 8888;

            UserName = System.Environment.MachineName;

            Clients = new List<Client>();
            
        }

        /*
         * Start the chat connection 
         */
        public void StartListening()
        {
            IsListening = true;
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    //Listener that listens for new incoming connections
                    TcpListener listener = new TcpListener(IPAddress.Parse(_localIP), _localPort);

                    listener.Start();

                    Console.WriteLine("Started listening for clients.");

                    while (IsListening)
                    {
                        if (listener.Pending())
                        {
                            Client client = new Client(listener.AcceptTcpClient(), _numClients);
                            client.internalReceivedMessage += ReceivedMessage; //subscribe to event

                            client.ClientRemoved += client_ClientRemoved; //Add clientRemoved event

                            Console.WriteLine("Wait for popup...");
                            
                            PopupMessage = "New incoming connection on " + LocalIP + ":" + LocalPort;

                            ShowPopup = true;
                            while (ShowPopup) //wait for user to accept/decline new connection
                            {
                                Thread.Sleep(100);
                            }

                            if (Accept)
                            {
                                //Accept incoming connection and create new client instance

                                AddClient(client);
                            }
                            else
                            {
                                //Accept and immediately close
                                client.RemoveClient();
                                Console.WriteLine("Client declined.");
                            }
                        }
                        else
                        {
                            Thread.Sleep(500); 
                        }
                    }

                    //Stop chat if we are here

                    Clients.ForEach(client => client.RemoveClient()); // Disconnect all clients

                    Clients.Clear(); //Clear the clients list

                    return;//Exit the thread

                }
                catch (SocketException se)
                {
                    Console.WriteLine("This socket adress is already in use.");
                    return; //exit thread
                }

            });//.Start();

        }

        public void Invite()
        {
            ThreadPool.QueueUserWorkItem(o =>
             {
                 try
                 {
                     Message msg = new Message(UserName);
                     TcpClient tcpclient = new TcpClient(RemoteIP, RemotePort);
                     Client client = new Client(tcpclient, _numClients);
                     client.SendString(msg.GetNameMessage());

                     AddClient(client);
                 }
                 catch (SocketException se)
                 {
                     Console.WriteLine("There is nobody listening on the remote address.");
                     return;//exit thread
                 }
             });
        }

        public void AddClient(Client client)
        {
            client.Name = NewConnectionName;

            Clients.Add(client); //add client to client list

            SelectedClient = _numClients;

            Message msg = new Message(UserName);
            client.SendString(msg.GetNameMessage());

            Console.WriteLine("New client with id " + _numClients + " connected.");

            _numClients++; //increment the number of clients
        }


        /*
         * Close the chat and remove all clients
         */
        public void StopListening()
        {
            Console.WriteLine("Chat stopped.");
            IsListening = false;
            Clients.ForEach(client => client.RemoveClient());//Disconnect all clients int Clients list
            Clients.Clear();//empty the client list
            
        }

        //Event to remove client
        public void client_ClientRemoved(object sender, EventArgs e)
        {
            RemoveClientFromList((sender as Client).ID);
        }

        /*
         * Send string to cliend with id
         */
        public void SendToClient(string str, int id)
        {
            //COMMENT - Should i check with if statement or with exceptions?

            if (id < _numClients) //if the client exists
            {
                Clients[id].SendMessage(str, UserName);
            }
            else
            {
                Console.WriteLine("Trying to send string to client ID " + id + " which does not exist");
            }
        }

        /*
         * Remove client from the client list
         */
        public void RemoveClientFromList(int id)
        {
            try
            {
                var clientToRemove = Clients.Single(r => r.ID == id);
                Clients.Remove(clientToRemove);
                Console.WriteLine("The client with id {0} was removed from the client list.", id);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Error, tried to remove a client that does not exist");
            }
        }
        
        /*
         * Received message event
         */
        private void ReceivedMessage(object sender, string str)
        {
            ReceivedMessageFromClient(sender, str);
            
            string[] splitStr = str.Split('|');

            if (splitStr.Length > 1)
            {
                if (splitStr[0].Equals("N"))
                {
                    NewConnectionName = splitStr[1];
                }
                else if(splitStr[0].Equals("M"))
                {
                    LastReceivedMessage = (sender as Client).Name + ": " + splitStr[1];
                }
            }
            else
            {
                LastReceivedMessage = str;
            }
            
            
        }
        
    }
}

