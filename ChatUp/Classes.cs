using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatUp
{
    public class Packet
    {
        public string Sender { get; set; }
        public string Message { get; set; }
        public long Message_ID { get; set; }
        
    }
    /*
    public class Request
    {
        public long ID { get; set; }

        public Request(long Sender_ID)
        {
            ID = Sender_ID;
        }
    }

    public class Acknowledgement
    {
        public long ID { get; set; }
        public bool Responce { get; set; }

        public Acknowledgement(long Sender_ID)
        {
            ID = Sender_ID;
        }
    }

    public class KeepAlive
    {
        public long ID { get; set; }
    }*/
}
