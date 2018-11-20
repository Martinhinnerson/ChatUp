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

    /*
     * This class handles all connected client
     * When a client connects a new Client instance is created
     */
    public class Client : BaseViewModel
    {
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

        private StreamWriter _writer; //writer is used to write to the client

        private StreamReader _reader; //reader is used to read from the client

        private bool _listenToClient = true;

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

        public string Name;
        

        private ObservableCollection<Message> _conversation;
        public ObservableCollection<Message> Conversation
        {
            get { return _conversation; }
            set
            {
                _conversation = value;
            }
        }
        

        public delegate void internalReceivedMessageHandler(object sender, string msg);
        public event internalReceivedMessageHandler internalReceivedMessage; //event used to pass the received data to the server


        //Override so that the name will show in the GUI list
        public override string ToString()
        {
            return Name;
        }

        /*
         * Constructor
         */
        public Client(TcpClient client)
        {
            IsConnected = false;
            TCP_client = client;
            //Connect();
        }

        public void Connect()
        {
            _writer = new StreamWriter(_client.GetStream());
            _reader = new StreamReader(_client.GetStream());
            IsConnected = true;
            ThreadPool.QueueUserWorkItem(o => { Listen(_reader); });//new thread for the reader
        }

        /*
         * Remove the connection to the client
         */
        public void Disconnect()
        {
            IsConnected = false;
            Console.WriteLine("Client " + Name + "Disconnected");
            _listenToClient = false;
            _writer.Close();
            _reader.Close();
            //_client.Close();

        }

        /*
         * Listen for incoming data from the client
         * When data is received an event is fired to tell the main thread that we have received something
         */
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
                catch(ObjectDisposedException ode)
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
        
        /*
         * Send string to client
         */
        public void SendString(string str)
        {
            if (IsConnected)
            {
                //Console.WriteLine("Sending string to client with id " + ID);
                _writer.WriteLine(str);//send the string
                _writer.Flush(); //Clear the buffer
            }
        }


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
