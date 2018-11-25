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
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
        public DateTime LastReceivedMessageTime;

        private string _username;

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
        /// The conversation that is shown in the UI.
        /// This collection is of type object so that it can hold both strings for the messages and images for the image messages
        /// </summary>
        private ObservableCollection<object> _visibleConversation;
        public ObservableCollection<object> VisibleConversation
        {
            get { return _visibleConversation; }
            set
            {
                _visibleConversation = value;
            }
        }

        /// <summary>
        /// newMessage is used when sending and adding messages tot the Conversation
        /// </summary>
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
        public delegate void MessageAddedEvent();
        public event MessageAddedEvent MessageAdded;
        public delegate void DisconnectEvent(string name);
        public event DisconnectEvent ClientDisconnected;

        // =============================================================================
        // Constructors
        // =============================================================================
        public Client(TcpClient client, string user)
        {
            InviteAccepted = false;
            IsConnected = false;
            TCP_client = client;
            _username = user;
            Conversation = new ObservableCollection<Message>();
            VisibleConversation = new ObservableCollection<object>();
            LastReceivedMessageTime = DateTime.Now;
        }

        public Client(string user)
        {
            InviteAccepted = false;
            IsConnected = false;
            TCP_client = null;
            _username = user;
            Conversation = new ObservableCollection<Message>();
            VisibleConversation = new ObservableCollection<object>();
            LastReceivedMessageTime = DateTime.Now;
        }
        public Client(string name, string user)
        {
            InviteAccepted = false;
            IsConnected = false;
            TCP_client = null;
            Name = name;
            _username = user;
            Conversation = new ObservableCollection<Message>();
            VisibleConversation = new ObservableCollection<object>();
            LastReceivedMessageTime = DateTime.Now;
        }

        /// <summary>
        /// Destructor (Finalizer)
        /// </summary>
        ~Client()
        {
            Disconnect();
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
            } 
        }

        /// <summary>
        /// Listen for incoming data from the client
        /// When data is received an event is fired to tell the main thread that we have received something
        /// </summary>
        /// <param name="reader"></param>
        public void Listen(StreamReader reader)
        {
            while (_listenToClient && IsConnected)
            {
                try
                {
                    string input = reader.ReadLine();

                    if (input == null)
                    {
                        Console.WriteLine("Client " + Name + " disconnected.");
                        Disconnect();
                    }

                    // Console.WriteLine("message reveived");
                    // Console.WriteLine(input);

                    ReceivedMessage(input);

                }
                catch (IOException)
                {
                    //Console.WriteLine(ioe);
                    Disconnect();
                }
                catch (ObjectDisposedException)
                {
                    Disconnect();
                    //Console.WriteLine(ode);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine("Error in listen: the client has most likely disconnected");
                    return;
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
            try
            {
                if (IsConnected)
                {
                    //Console.WriteLine("Sending string to client with id " + ID);
                    _writer.WriteLine(str);//send the string
                    _writer.Flush(); //Clear the buffer
                }
            }
            catch (MessageException)
            {
                Console.WriteLine("Error, the message cannot contain the | characted");
            }
            catch (IOException)
            {
                Console.WriteLine("Something went wrong when string to the client");

            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("_writer is no more, reconnecting");
                Disconnect();
                Connect();
            }
        }

        /// <summary>
        /// Send a message to the client if we are connected
        /// </summary>
        /// <param name="str"></param>
        /// <param name="name"></param>
        public void SendMessage(string str, string name)
        {
            try
            {
                if (IsConnected)
                {
                    //Console.WriteLine("Sending message to client with id " + ID);
                    newMessage = new Message(name, str);
                    AddToConversation();

                    _writer.WriteLine(newMessage.GetPrintableMessage());
                    _writer.Flush();
                }
            }
            catch (MessageException)
            {
                Console.WriteLine("Error, the message cannot contain the | characted");
            }
            catch (IOException)
            {
                Console.WriteLine("Something went wrong when sending an image to the client");

            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("_writer is no more, reconnecting");
                Disconnect();
                Connect();
            }
        }

        /// <summary>
        /// Send an image to the client if we are connected
        /// </summary>
        /// <param name="name"></param>
        /// <param name="img"></param>
        public void SendImage(string name, string img)
        {
            try
            {
                if (IsConnected)
                {
                    int size = img.Length;

                    newMessage = new Message(name);
                    newMessage.Image = img;

                    AddToConversation();

                    _writer.WriteLine(newMessage.GetImageMessage());
                    _writer.Flush();
                }
            }
            catch (MessageException)
            {
                Console.WriteLine("Error, the message cannot contain the | characted");
            }
            catch (IOException)
            {
                Console.WriteLine("Something went wrong when sending an image to the client");
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("_writer is no more, reconnecting");
                Disconnect();
                Connect();
            }
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
                        //Console.WriteLine("Received name");
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
                        // Console.WriteLine("Received accept");
                        InviteAccepted = true;
                        InviteAnswer.Set();
                    }
                    //Decline message
                    else if (splitStr[0].Equals("D"))
                    {
                        //Console.WriteLine("Received decline");
                        InviteAccepted = false;
                        InviteAnswer.Set();
                    }
                    //Disconnect message telling the clinet we are disconnecting
                    else if (splitStr[0].Equals("d"))
                    {
                        // Console.WriteLine("Received disconnect");
                        ClientDisconnected(splitStr[1]);
                        Disconnect(); //look for exception here             
                    }
                    else if (splitStr[0].Equals("I"))
                    {
                        AddImage(splitStr[1]);
                    }
                }
                //All other messages are invalid
                else
                {
                    Console.WriteLine("Invalid message received: {0}", str);
                }
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Error in received message: the client has most likely disconnected");
                return;
            }
        }

        /// <summary>
        /// Calls AddToConversation with the message from the UI thread
        /// </summary>
        /// <param name="msg"></param>
        public void AddMessage(string msg)
        {
            newMessage = new Message(Name, msg);
            Application.Current.Dispatcher.Invoke(new Action(() => AddToConversation()));
        }

        /// <summary>
        /// Calls AddToConversation with the image from the UI thres
        /// </summary>
        /// <param name="img"></param>
        public void AddImage(string img)
        {
            newMessage = new Message(Name);
            newMessage.Image = img;
            Application.Current.Dispatcher.Invoke(new Action(() => AddToConversation()));
        }

        /// <summary>
        /// Add the message or image stored in newMessage to the conversations list
        /// </summary>
        public void AddToConversation()
        {
            Conversation.Add(newMessage);
            LastReceivedMessageTime = DateTime.Now;
            fixVisibleCollection();
            MessageAdded.Invoke();
        }

        /// <summary>
        /// Convert the conversation list to JSON format and store it in file
        /// </summary>
        public void StoreConversation()
        {
            string filename = Name + ".JSON";
            using (StreamWriter file = File.CreateText(Directory.GetCurrentDirectory() + @"\Conversations\" + _username + ',' + filename))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, Conversation);
                writer.Close();
                file.Close();
            }
        }

        /// <summary>
        /// Fixes the visible collection so that it shows whats in the conversations list.
        /// For example it adds image objects if the message is an image and otherwire it adds a string
        /// </summary>
        public void fixVisibleCollection()
        {
            VisibleConversation.Clear();
            foreach (Message m in Conversation)
            {
                string name = m.Sender;
                if (m.Image == null || m.Image == "")
                {
                    VisibleConversation.Add(m.ToString());
                }
                else
                {
                    Image img = Base64ToImage(m.Image);
                    img.Width = 200;
                    VisibleConversation.Add(name + ": ");
                    VisibleConversation.Add(img);
                }
            }
        }

        /// <summary>
        /// Help function to convert the base64 string stored in the Message is converted into an image object
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public Image Base64ToImage(string str)
        {
            try
            {
                byte[] byteImg = Convert.FromBase64String(str);

                Image img = new Image();
                using (MemoryStream ms = new MemoryStream(byteImg, 0, byteImg.Length))
                {
                    img.Source = BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
                return img;
            }
            catch (FormatException)
            {
                Console.WriteLine("Error, a base64 image could not be converted to a byte[]. Wrong base64 string format");
                return null;
            }
        }
    }
}
