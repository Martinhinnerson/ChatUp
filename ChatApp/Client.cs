using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace ChatApp
{

    /*
     * This class handles all connected client
     * When a client connects a new Client instance is created
     */
    public class Client
    {
        private TcpClient _client;

        private StreamWriter _writer; //writer is used to write to the client

        private StreamReader _reader; //reader is used to read from the client

        private bool _listenToClient = true;

        public string Name;

        public int ID { get; set; } //client id used if we have multiple clients

        public delegate void internalReceivedMessageHandler(object sender, string msg);
        public event internalReceivedMessageHandler internalReceivedMessage; //event used to pass the received data to the server

        protected virtual void OnClientRemoved(EventArgs e)
        {
            EventHandler handler = ClientRemoved;
            if(handler != null)
            {
                handler(this, e);
            }
        }
        public event EventHandler ClientRemoved;

        /*
         * Constructor
         */
        public Client(TcpClient client, int id)
        {
            _client = client;
            ID = id;

            _writer = new StreamWriter(_client.GetStream());
            _reader = new StreamReader(_client.GetStream());

            ThreadPool.QueueUserWorkItem(o => { Listen(_reader); });//.Start(); //new thread for the reader
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
                        Console.WriteLine("Client " + ID + " disconnected.");
                        RemoveClient();
                        return; //exit from the thread
                    }

                    internalReceivedMessage(this, input);
                }
                catch (IOException ioe)
                {
                    Console.WriteLine("Can't reach client with id {0}", ID);
                    OnClientRemoved(EventArgs.Empty);
                    RemoveClient();
                }
                catch(ObjectDisposedException ode)
                {
                    Console.WriteLine("Trying to read from disposed client with id {0}", ID);
                }
                Thread.Sleep(500); //sleep thread for a while
            }
            return;
        }

        /*
         * Remove the connection to the client
         */
        public void RemoveClient()
        {
            Console.WriteLine("Removing client with id " + ID);
            _listenToClient = false;
            _writer.Close();
            _client.Close();

        }

        /*
         * Send string to client
         */
        public void SendString(string str)
        {
            Console.WriteLine("Sending string to client with id " + ID);
            _writer.WriteLine(str);//send the string
            _writer.Flush(); //Clear the buffer
        }
    }
}
