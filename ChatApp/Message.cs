using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp
{
    public class Message
    {
        public string Sender { get; set; }
        public string Text { get; set; }
        public DateTime SendTime { get; set; }

        public Message(string sender, string text)
        {
            Sender = sender;
            Text = text;
            SendTime = DateTime.Now;
        }

        public Message(string sender)
        {
            Sender = sender;
            Text = "";
            SendTime = DateTime.Now;
        }

        public string GetPrintableMessage()
        {
            return "M|" + Text + "              (" + SendTime.ToString() + ")";
        } 

        public string GetNameMessage()
        {
            return "N|" + Sender;
        }

        public string GetAcceptMessage()
        {
            return "A|" + Sender;
        }
        public string GetDeclineMessage()
        {
            return "D|" + Sender;
        }
        public string GetDisconnectMessage()
        {
            return "d|" + Sender;
        }
    }
}
