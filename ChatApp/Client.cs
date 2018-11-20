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
                Connect(); //connect everytime the TCPclient is set
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
        /// The name of the client
        /// The name is returned Client.toString()
        /// </summary>
        public string Name;

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
        /// Event for when a message is received
        /// Subscribed to by ChatModel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        public delegate void internalReceivedMessageHandler(object sender, string msg);
        public event internalReceivedMessageHandler internalReceivedMessage; //event used to pass the received data to the server

        // =============================================================================
        // Constructor
        // =============================================================================
        public Client(TcpClient client)
        {
            IsConnected = false;
            TCP_client = client;
            //Connect();
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
            _writer = new StreamWriter(_client.GetStream());
            _reader = new StreamReader(_client.GetStream());
            IsConnected = true;
            ThreadPool.QueueUserWorkItem(o => { Listen(_reader); });//new thread for the reader
        }

        /// <summary>
        /// Disconnect from the client and stop listening
        /// </summary>
        public void Disconnect()
        {
            IsConnected = false;
            Console.WriteLine("Client " + Name + "Disconnected");
            _listenToClient = false;
            _writer.Close();
            _reader.Close();
            //_client.Close();

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

                    internalReceivedMessage(this, input);
                }
                catch (IOException ioe)
                {
                    Console.WriteLine("Can't reach client with id {0}");
                    Disconnect();
                }
                catch (ObjectDisposedException ode)
                {
                    Disconnect();
                    Console.WriteLine("Trying to read from disposed client with id {0}");
                }
                catch (NullReferenceException nre)
                {
                    Console.WriteLine("Error: null in Client.listen");
                    //Console.WriteLine(nre);
                }
                Thread.Sleep(500); //sleep thread for a while
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
            }
        }

    }
}
