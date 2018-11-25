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
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChatApp
{
    /// <summary>
    /// Chat model containing most of the important functions and properties of the chat
    /// </summary>
    public class ChatModel : BaseViewModel
    {
        // =====================================================================
        // Properties
        // =====================================================================
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

        /// <summary>
        /// User name bound to the GUI
        /// Currently you should not change name while having connections in your connections list
        /// This can result in clients adding another instance of you which will cause weird things to happen...
        /// </summary>
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

        /// <summary>
        /// If the client has been accepted or not
        /// </summary>
        /// <value></value>
        public bool Accept { get; set; }

        /// <summary>
        /// if we are not listening for new connections
        /// </summary>
        private bool _isNotListening;
        public bool IsNotListening
        {
            get { return _isNotListening; }
            set
            {
                _isNotListening = value;
                RaisePropertyChanged("IsNotListening");
            }
        }

        /// <summary>
        /// List with all clients
        /// This collection is bound to the GUI
        /// </summary>
        private ObservableCollection<Client> _clients;//Store all connected clients
        public ObservableCollection<Client> Clients
        {
            get { return _clients; }
            set
            {
                _clients = value;
                RaisePropertyChanged("Clients");
                FilterConnections();
            }
        }

        /// <summary>
        /// This collections shows the filtered clients list if we are using the search function
        /// </summary>
        private ObservableCollection<Client> _filteredCliens;
        public ObservableCollection<Client> FilteredClients
        {
            get { return _filteredCliens; }
            set
            {
                _filteredCliens = value;
                RaisePropertyChanged("FilteredClients");
            }
        }

        /// <summary>
        /// The client currently selected in the GUI
        /// TODO: this should probaly be a pointer instead.
        /// </summary>
        private Client _selectedClient;
        public Client SelectedClient
        {
            get { return _selectedClient; }
            set
            {
                _selectedClient = value;
                RaisePropertyChanged("SelectedClient");
            }
        }

        /// <summary>
        /// When a new client is connected it is instansiated here
        /// If the connection is accepted this will be added to the clients list, otherwise ignored
        /// </summary>
        private Client _newClient;
        public Client NewClient
        {
            get { return _newClient; }
            set
            {
                _newClient = value;
                //RaisePropertyChanged("NewClient");
            }
        }

        /// <summary>
        /// The last message received
        /// TODO: change this!
        /// </summary>
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
        /// <summary>
        /// Event used by the view model
        /// </summary>
        public delegate void AddresAlreadyInUse();
        public event AddresAlreadyInUse AddressBusy;

        /// <summary>
        /// If we have to wait for an accept/decline message
        /// </summary>
        /// <returns></returns>
        public AutoResetEvent WaitForUser = new AutoResetEvent(false);

        //TODO: Change this name?
        public AutoResetEvent WaitForName = new AutoResetEvent(false);

        /// <summary>
        /// Is true if the invite is accepted
        /// </summary>
        /// <value></value>
        //public bool InviteAccepted { get; set; }

        /// <summary>
        /// If the popup window is shown or not
        /// </summary>
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
        /// <summary>
        /// The message shown in the popup window
        /// </summary>
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
        /// <summary>
        /// The name of a new incoming connection
        /// </summary>
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
        /// <summary>
        /// A status message bound to the GUI
        /// </summary>
        private string _status;
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                RaisePropertyChanged("Status");
            }
        }

        /// <summary>
        /// The search text field is bount to this
        /// </summary>
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                RaisePropertyChanged("SearchText");
                FilterConnections();
            }
        }

        private bool _isNotLoggedIn;
        public bool IsNotLoggedIn
        {
            get { return _isNotLoggedIn; }
            set
            {
                _isNotLoggedIn = value;
                RaisePropertyChanged("IsNotLoggedIn");
            }
        }

        // =============================================================================
        // Constructors
        // =============================================================================
        public ChatModel()
        {
            ShowPopup = false;
            IsNotListening = true;
            IsNotLoggedIn = true;

            LocalIP = "127.0.0.1";
            RemoteIP = "127.0.0.1";
            LocalPort = 8888;
            RemotePort = 8888;

            UserName = System.Environment.MachineName;

            Clients = new ObservableCollection<Client>();
        }

        // =====================================================================
        // Member functions
        // =====================================================================

        /// <summary>
        /// Listen for incoming connection invites
        /// This function is run in its own thread
        /// If a new connection is received, a popup will let you accept or deny
        /// </summary>
        public void StartListening()
        {
            IsNotListening = false;
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    //Listener that listens for new incoming connections
                    TcpListener listener = new TcpListener(IPAddress.Parse(_localIP), _localPort);

                    listener.Start();

                    Status = "Started listening for clients.";
                    Console.WriteLine("Started listening for clients.");

                    while (!IsNotListening)
                    {
                        if (listener.Pending())
                        {
                            NewClient = new Client(UserName);
                            NewClient.MessageAdded += FilterConnections; //Subscribe to message added event
                            NewClient.TCP_client = listener.AcceptTcpClient();
                            NewClient.Connect();
                            Message msg = new Message(UserName);

                            NewClient.SendString(msg.GetNameMessage());//send name message to new client

                            Console.WriteLine("Waiting for name in listen");
                            NewClient.NameReceived.WaitOne();
                            PopupMessage = "New incoming connection from: " + NewClient.Name;
                            ShowPopup = true;

                            while (ShowPopup) //wait for user to accept/decline new connection
                            {
                                Thread.Sleep(100);
                            }

                            if (!Accept)   //Accept and immediately close
                            {
                                NewClient.SendString(msg.GetDeclineMessage());
                                NewClient.Disconnect();
                                Status = "Client declined.";
                                Console.WriteLine("Client declined.");
                            }
                            else //Accept incoming connection and create new client instance
                            {
                                Status = "Client accepted";
                                NewClient.SendString(msg.GetAcceptMessage());
                                Application.Current.Dispatcher.Invoke(new Action(() => AddClientToList()));
                            }

                        }
                        else
                        {
                            Thread.Sleep(500);
                        }
                    }

                    return;//Exit the thread

                }
                catch (SocketException se)
                {
                    Status = "This socket is already in use";
                    Console.WriteLine("This socket adress is already in use.");
                    AddressBusy();
                    return; //exit thread
                }

            });

        }

        /// <summary>
        /// Close the chat and stop listening
        /// </summary>
        public void StopListening()
        {
            Status = "Search stopped";
            Console.WriteLine("Chat stopped.");
            IsNotListening = true;
        }

        /// <summary>
        /// Invite the specified remote connection
        /// If the invite is accepted, add it to Clients
        /// </summary>
        public void Invite()
        {
            ThreadPool.QueueUserWorkItem(o =>
             {
                 try
                 {
                     Message msg = new Message(UserName);
                     NewClient = new Client(UserName);
                     NewClient.MessageAdded += FilterConnections; //Subscribe to message added event
                     NewClient.TCP_client = new TcpClient(RemoteIP, RemotePort);
                     NewClient.Connect();

                     Console.WriteLine("Waiting for name...");
                     NewClient.NameReceived.WaitOne();
                     
                     NewClient.SendString(msg.GetNameMessage());

                     Console.WriteLine("Waiting for accept...");
                     NewClient.InviteAnswer.WaitOne();
                     
                     if (NewClient.InviteAccepted)
                     {
                         Status = "Invite accepted.";
                         Application.Current.Dispatcher.Invoke(new Action(() => AddClientToList()));
                     }
                     else
                     {
                         Status = "Invite declined";
                         NewClient.Disconnect();
                         NewClient = null;
                     }
                     
                     NewClient.InviteAccepted = false;
                     
                     return;
                 }
                 catch (SocketException se)
                 {
                     Status = "There is nobody listening on the remote address.";
                     return;//exit thread
                 }
             });
        }

        /// <summary>
        /// Add the NewClient client to the client list
        /// If a client with the same name already exists, connect to it instead with the new tcpClient
        /// </summary>
        public void AddClientToList()
        {
            var client = Clients.FirstOrDefault(x => x.Name == NewClient.Name);

            if (client == null) //there exists no client with the new name, add a new one
            {
                Status = "New client " + NewClient.Name + " connected";
                Clients.Add(NewClient);
                NewClient.fixVisibleCollection();
                SelectedClient = NewClient;
                NewClient.ClientDisconnected += ClientDisconnected;
            }
            else //Client with that name already exists, update the TCP listener and connect
            {
                try
                {
                    Status = "Client already exist in client list, connecting.";
                    NewClient.Conversation = Clients.Single(x => x.Name == NewClient.Name).Conversation;
                    Clients.Remove(Clients.Single(x => x.Name == NewClient.Name));
                    Clients.Add(NewClient);
                    NewClient.fixVisibleCollection();
                    SelectedClient = NewClient;
                    SelectedClient.Connect();
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("Error: There is multiple clients with the same name in the clients list.");
                    return;
                }
            }
            
            FilterConnections();
        }

        /// <summary>
        /// Remove the selected client from the client list
        /// This will also disconnect it 
        /// </summary>
        public void RemoveSelectedClient()
        {
            if (Clients.Any())
            {
                Application.Current.Dispatcher.Invoke(new Action(() => RemoveClientFromList()));
            }
            else
            {
                Status = "There are no clients connected";
            }
        }

        /// <summary>
        /// Helper function run by RemoveSelectedClient
        /// This function is run from the main thread since it has to modify Clients
        /// </summary>
        public void RemoveClientFromList()
        {
            Clients.Remove(SelectedClient);
        }

        /// <summary>
        /// Disconnect from the selected client
        /// </summary>
        public void DisconnectSelectedClient()
        {
            if (Clients.Any())
            {
                try
                {
                    Clients.Single(i => i.Name == SelectedClient.Name).Disconnect(); //look for exception here
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("Error: There is multiple clients with the same name in the clients list.");
                    return;
                }
            }
            else
            {
                Status = "There are no clients to disconnect";
            }
        }

        public void ClientDisconnected(string name)
        {
            Status = "Client " + name + " disconnected";
        }

        /// <summary>
        /// Connect to the selected client
        /// </summary>
        public void ConnectSelectedClient()
        {
            if (Clients.Any())
            {
                try
                {
                    Clients.Single(i => i.Name == SelectedClient.Name).Connect(); //look for exception here
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("Error: There is multiple clients with the same name in the clients list.");
                    return;
                }
            }
            else
            {
                Status = "There are no clients to connect";
            }
        }

        /// <summary>
        /// Send a string to the selected client
        /// </summary>
        /// <param name="msg">string to send</param>
        public void SendToSelectedClient(string msg)
        {
            if (Clients.Any())
            {
                try
                {
                    if (Clients.Single(i => i.Name == SelectedClient.Name).IsConnected)
                    {
                        Status = "Message sent to " + SelectedClient.Name;
                        Clients.Single(i => i.Name == SelectedClient.Name).SendMessage(msg, UserName);
                    }
                    else
                    {
                        Status = "Can't send, the client is not connected";
                    }
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("Error: There is multiple clients with the same name in the clients list.");
                    return;
                }
            }
        }

        /// <summary>
        /// Send an image to the selected client
        /// </summary>
        /// <param name="msg">string to send</param>
        public void SendImageToSelectedClient(string img)
        {
            if (Clients.Any())
            {
                try
                {
                    if (Clients.Single(i => i.Name == SelectedClient.Name).IsConnected)
                    {
                        Status = "Image sent to " + SelectedClient.Name;
                        Clients.Single(i => i.Name == SelectedClient.Name).SendImage(UserName, img);
                    }
                    else
                    {
                        Status = "Can't send, the client is not connected";
                    }
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("Error: There is multiple clients with the same name in the clients list.");
                    return;
                }

            }
        }

        public void FilterConnections()
        {
            if (!string.IsNullOrEmpty(SearchText)) //if the searchfield is not empty
            {
               IEnumerable<Client> filtered = from client in Clients
                   where client.Name.Contains(SearchText)
                   orderby client.LastReceivedMessageTime descending
                   select client;
                
                FilteredClients = new ObservableCollection<Client>(filtered);
            }
            else
            {
                IEnumerable<Client> filtered = from client in Clients
                                               orderby client.LastReceivedMessageTime descending
                                               select client;

                FilteredClients = new ObservableCollection<Client>(filtered);
            }
        }

        /// <summary>
        /// Load the conversations from the Json files stored in the Conversations directory
        /// </summary>
        public void LoadConversations()
        {
            string folderpath = Directory.GetCurrentDirectory() + @"\Conversations\";
            DirectoryInfo dir = new DirectoryInfo(Directory.GetCurrentDirectory() + @"\Conversations\");
            //Console.WriteLine(folderpath);
            FileInfo[] files = dir.GetFiles("*.JSON");
            foreach (FileInfo file in files)
            {
                
                StreamReader reader = new StreamReader(folderpath + file.Name);
                try
                {
                    string input = reader.ReadToEnd();

                    JArray output = JsonConvert.DeserializeObject<JArray>(input);


                    string[] splitStr = file.Name.Split(',');

                    if (splitStr[0] == UserName)
                    {

                        string clientName = splitStr[1].Substring(0, splitStr[1].LastIndexOf(".")); //get name without extension

                        Client client = new Client(clientName, UserName);

                        foreach (var item in output.Children())
                        {
                            string s = item["Sender"].ToString();
                            string st = item["SendTime"].ToString();
                            string t = item["Text"].ToString();
                            string img = item["Image"].ToString();
                            Message m = new Message(s, t, st, img);
                            client.Conversation.Add(m);
                        }
                        client.fixVisibleCollection();
                        Clients.Add(client);
                    }
                    
                }
                catch (IndexOutOfRangeException ior)
                {
                    Console.WriteLine("The read file {0} does not have correct naming format.", file.Name);
                    continue;
                }
                catch (JsonReaderException jre)
                {
                    Console.WriteLine("The read file {0} does not have correct JSON formating", file.Name);
                    continue;
                }
                finally
                {
                    reader.Close();
                }
            }

            FilterConnections();
        }
        
        public void StoreConversations()
        {
            foreach(Client client in Clients)
            {
                client.StoreConversation();
            }
        }
    }
}

