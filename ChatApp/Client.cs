using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ChatApp
{

    /// <summary>
    /// Class that represents each connected client
    /// </summary>
    public class Client : BaseViewModel
    {
        // =====================================================================
        // Properties
        // =====================================================================

        /// <summary>
        /// Stores the current TCPclient for the client
        /// This is set everytime a new client is created and updated when we connect to the same client again
        /// Runs the Connect function in the setter
        /// </summary>
        private TcpClient _client;
        public TcpClient TCP_client
        {
            get { return _client; }
            set
            {
                _client = value;
                //Connect(); //connect everytime the TCPclient is set
                RaisePropertyChanged("NewClient");
            }
        }

        /// <summary>
        /// The stream writer of the client
        /// </summary>
        private StreamWriter _writer;

        /// <summary>
        /// The stream reader of the client
        /// </summary>
        private StreamReader _reader;

        /// <summary>
        /// If we are listening on the tcpclient or not
        /// </summary>
        private bool _listenToClient = true;

        /// <summary>
        /// If the tcpclient is connected
        /// </summary>
        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                RaisePropertyChanged("IsConnected");
            }
        }

        /// <summary>
        /// This boolean is set when an invite is accepted and is false otherwise
        /// </summary>
        /// <value></value>
        public bool InviteAccepted { get; set; }

        /// <summary>
        /// The name of the client
        /// The name is returned Client.toString()
        /// </summary>
        public string Name = "";

        /// <summary>
        /// Collects received messages
        /// </summary>
        private ObservableCollection<Message> _conversation;
        public ObservableCollection<Message> Conversation
        {
            get { return _conversation; }
            set
            {
                _conversation = value;
            }
        }

        /// <summary>
        /// Temporary message where new messages are stored
        /// </summary>
        /// <value></value>
        public Message newMessage { get; set; }

        /// <summary>
        /// Event that is activated when an answer is received
        /// </summary>
        /// <returns></returns>
        public AutoResetEvent InviteAnswer = new AutoResetEvent(false);
        
        /// <summary>
        /// Event that is set when we have received the new clients name in a NameMessage
        /// </summary>
        /// <returns></returns>
        public AutoResetEvent NameReceived = new AutoResetEvent(false);
        
        /// <summary>
        /// Event for when a client is disconnected
        /// Is used by the ChatModel to know when a client has disconnected
        /// </summary>
        /// <param name="name"></param>
        public delegate void DisconnectEvent(string name);
        public event DisconnectEvent ClientDisconnected;

        // =============================================================================
        // Constructors
        // =============================================================================
        public Client(TcpClient client)
        {
            Console.WriteLine("client created");
            InviteAccepted = false;
            IsConnected = false;
            TCP_client = client;
            Conversation = new ObservableCollection<Message>();
        }

        public Client()
        {
            Console.WriteLine("client created");
            InviteAccepted = false;
            IsConnected = false;
            TCP_client = null;
            Conversation = new ObservableCollection<Message>();
            //StoreConversation();
        }
        public Client(string name)
        {
            Console.WriteLine("client created");
            InviteAccepted = false;
            IsConnected = false;
            TCP_client = null;
            Name = name;
            Conversation = new ObservableCollection<Message>();
            //StoreConversation();
        }

        // =============================================================================
        // Member functions
        // =============================================================================

        /// <summary>
        /// Ovveride so that toString() returns the name of the client
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Connect the client and start listening
        /// </summary>
        public void Connect()
        {
            if (!IsConnected)
            {
                _writer = new StreamWriter(_client.GetStream());
                _reader = new StreamReader(_client.GetStream());
                IsConnected = true;
                ThreadPool.QueueUserWorkItem(o => { Listen(_reader); });//new thread for the reader
            }
        }

        /// <summary>
        /// Disconnect from the client and stop listening
        /// </summary>
        public void Disconnect()
        {
            if (IsConnected)
            {
                IsConnected = false;
                Console.WriteLine("Client " + Name + " disconnected");
                _listenToClient = false;
                _writer.Close();
                _reader.Close();
                //_client.Close();
                //StoreConversation();
            }
        }

        /// <summary>
        /// Listen for incoming data from the client
        /// When data is received an event is fired to tell the main thread that we have received something
        /// </summary>
        /// <param name="reader"></param>
        public void Listen(StreamReader reader)
        {
            while (_listenToClient)
            {
                try
                {
                    string input = reader.ReadLine();

                    if (input == null)
                    {
                        Console.WriteLine("Client " + Name + " disconnected.");
                        Disconnect();
                    }

                    Console.WriteLine("message reveived");
                    Console.WriteLine(input);
                  
                    ReceivedMessage(input);

                }
                catch (IOException ioe)
                {
                    Console.WriteLine(ioe);
                    Disconnect();
                }
                catch (ObjectDisposedException ode)
                {
                    Disconnect();
                    Console.WriteLine(ode);
                }
                catch (NullReferenceException nre)
                {
                    Console.WriteLine(nre);
                }
                Thread.Sleep(100); //sleep thread for a while
            }
            return;
        }

        /// <summary>
        /// Send a string to the client if we are connected
        /// </summary>
        /// <param name="str"></param>
        public void SendString(string str)
        {
            if (IsConnected)
            {
                //Console.WriteLine("Sending string to client with id " + ID);
                _writer.WriteLine(str);//send the string
                _writer.Flush(); //Clear the buffer
            }
        }

        /// <summary>
        /// Send a message to the client if we are connected
        /// </summary>
        /// <param name="str"></param>
        /// <param name="name"></param>
        public void SendMessage(string str, string name)
        {
            if (IsConnected)
            {
                //Console.WriteLine("Sending message to client with id " + ID);
                Message msg = new Message(name, str);
                _writer.WriteLine(msg.GetPrintableMessage());
                _writer.Flush();
                Console.WriteLine("In client send");
            }
        }

        /// <summary>
        /// Send an image to the client if we are connected
        /// </summary>
        /// <param name="img"></param>
        /// <param name="name"></param>
        public void SendImage(byte[] img, string name)
        {
            if (IsConnected)
            {
                int size = img.Length;
                
                Message msg = new Message(name, img);
            
                _writer.WriteLine(msg.GetImageMessage());
                _writer.Flush();

                Thread.Sleep(100);
                _writer.Write(msg.GetImage());
            }
        }


        /// <summary>
        /// Add a message to the clients list
        /// This function schedules AddToConversation to the main thread
        /// </summary>
        /// <param name="msg"></param>
        public void AddMessage(string msg)
        {
            newMessage = new Message(Name, msg);
            Application.Current.Dispatcher.Invoke(new Action(() => AddToConversation()));
        }

        /// <summary>
        /// Add a message to the conversation list.
        /// This function is usually called with dispatcher to the main thread because it has to change observable collection
        /// </summary>
        public void AddToConversation()
        {
            Conversation.Add(newMessage);
        }


        /// <summary>
        /// Function is called when a message has been received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="str">The received string</param>
        private void ReceivedMessage(string str)
        {
            try
            {
                string[] splitStr = str.Split('|');

                if (splitStr.Length > 1)
                {
                    //Name message
                    if (splitStr[0].Equals("N"))
                    {
                        Console.WriteLine("Received name");
                        Name = splitStr[1];
                        NameReceived.Set();
                    }
                    //Normal message
                    else if (splitStr[0].Equals("M"))
                    {
                        AddMessage(splitStr[1]);
                        StoreConversation();
                    }
                    //Accept message
                    else if (splitStr[0].Equals("A"))
                    {
                        Console.WriteLine("Received accept");
                        InviteAccepted = true;
                        InviteAnswer.Set();
                    }
                    //Decline message
                    else if (splitStr[0].Equals("D"))
                    {
                        Console.WriteLine("Received decline");
                        InviteAccepted = false;
                        InviteAnswer.Set();
                    }
                    //Disconnect message telling the clinet we are disconnecting
                    else if (splitStr[0].Equals("d"))
                    {
                        Console.WriteLine("Received disconnect");
                        ClientDisconnected(splitStr[1]);
                        Disconnect(); //look for exception here             
                    }
                }
                //All other messages are invalid
                else
                {
                    Console.WriteLine(str);
                }
            }
            catch (ArgumentOutOfRangeException aoe)
            {
                Console.WriteLine("Tried to remove item in list that does not exist");
                Console.WriteLine(aoe);
            }

        }

        /// <summary>
        /// Store the Conversation list to a .JSON file with JSON formating
        /// </summary>
        public void StoreConversation()
        {
            try{
                string filename = Name + ".JSON";

                using (StreamWriter file = File.CreateText(Directory.GetCurrentDirectory() + @"\Conversations\" + filename))
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    //string output = JsonConvert.SerializeObject(Conversation);

                    serializer.Serialize(writer, Conversation);

                    //writer.Write(output);

                    //Conversation.WriteTo(writer);
                    writer.Close();
                    file.Close();
                }
            }
            catch(Exception e) 
            {
                //TODO: change this exception
            }
        }
    }
}
