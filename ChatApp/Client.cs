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

        private ObservableCollection<object> _visibleConversation;
        public ObservableCollection<Object> VisibleConversation
        {
            get { return _visibleConversation; }
            set
            {
                _visibleConversation = value;
            }
        }

        public Message newMessage { get; set; }

        public AutoResetEvent InviteAnswer = new AutoResetEvent(false);
        public AutoResetEvent NameReceived = new AutoResetEvent(false);
        public delegate void MessageAddedEvent();
        public event MessageAddedEvent MessageAdded;
        public delegate void DisconnectEvent(string name);
        public event DisconnectEvent ClientDisconnected;

        // =============================================================================
        // Constructor
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
            Conversation = null;
            TCP_client = null;
        }

        // =============================================================================
        // Member functions
        // =============================================================================

        /// <summary>
        /// Ovveride so that toStrinf() returns the name of the client
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
                Console.WriteLine("Client " + Name + "Disconnected");
                _listenToClient = false;
                _writer.Close();
                _reader.Close();
                //_client.Close();
                //StoreConversation();
            } //TODO: add socket exception
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
                catch (IOException ioe)
                {
                    //Console.WriteLine(ioe);
                    Disconnect();
                }
                catch (ObjectDisposedException ode)
                {
                    Disconnect();
                    //Console.WriteLine(ode);
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
                newMessage = new Message(name, str);
                AddToConversation();
                
                _writer.WriteLine(newMessage.GetPrintableMessage());
                _writer.Flush();
            }
        }

        public void SendImage(string name, string img)
        {
            if (IsConnected)
            {
                int size = img.Length;
                
                newMessage = new Message(name);
                newMessage.Image = img;

                AddToConversation();

                _writer.WriteLine(newMessage.GetImageMessage());
                _writer.Flush();



                //Thread.Sleep(100);
                //_writer.Write(newMessage.GetImage());
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
                    if (splitStr[0].Equals("N"))
                    {
                        //Console.WriteLine("Received name");
                        Name = splitStr[1];
                        NameReceived.Set();
                    }
                    else if (splitStr[0].Equals("M"))
                    {
                        AddMessage(splitStr[1]);
                        StoreConversation();
                    }
                    else if (splitStr[0].Equals("A"))
                    {
                       // Console.WriteLine("Received accept");
                        InviteAccepted = true;
                        InviteAnswer.Set();
                    }
                    else if (splitStr[0].Equals("D"))
                    {
                        //Console.WriteLine("Received decline");
                        InviteAccepted = false;
                        InviteAnswer.Set();
                    }
                    else if (splitStr[0].Equals("d"))
                    {
                       // Console.WriteLine("Received disconnect");
                        ClientDisconnected(splitStr[1]);
                        Disconnect(); //look for exception here             
                    }
                    else if (splitStr[0].Equals("I"))
                    {
                        Console.WriteLine(str);
                        AddImage(splitStr[1]);
                    }
                }
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

        public void AddMessage(string msg)
        {
            newMessage = new Message(Name, msg);
            Application.Current.Dispatcher.Invoke(new Action(() => AddToConversation()));
        }

        public void AddImage(string img)
        {
            newMessage = new Message(Name);
            newMessage.Image = img;
            Application.Current.Dispatcher.Invoke(new Action(() => AddToConversation()));
        }

        public void AddToConversation()
        {
            Conversation.Add(newMessage);
            LastReceivedMessageTime = DateTime.Now;
            fixVisibleCollection();
            MessageAdded.Invoke();
        }


        public void StoreConversation()
        {
            string filename = Name + ".JSON";
            using (StreamWriter file = File.CreateText(Directory.GetCurrentDirectory() + @"\Conversations\" + _username + ','+ filename))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, Conversation);
                writer.Close();
                file.Close();
            }
        }

        public void fixVisibleCollection()
        {
            VisibleConversation.Clear();
            foreach(Message m in Conversation)
            {
                string name = m.Sender;
                if(m.Image == null || m.Image == "")
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
            }//TODO: Add exception
        }

        public Image Base64ToImage(string str)
        {
            byte[] byteImg = Convert.FromBase64String(str);
            
            Image img = new Image();
            using (MemoryStream ms = new MemoryStream(byteImg, 0, byteImg.Length))
            {
                img.Source = BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
            return img;
        }
    }
}
