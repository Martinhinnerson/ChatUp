using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ChatApp
{
    /// <summary>
    /// Message class to structure the messages send between the clients
    /// </summary>
    public class Message
    {
        // =====================================================================
        // Properties
        // =====================================================================
        public string Sender { get; set; }
        public string Text { get; set; }
        public string SendTime { get; set; }
        public byte[] Image { get; set; }
        //public JArray Messages { get; set; }
        
        // =====================================================================
        // Constructors
        // =====================================================================
        public Message(string sender, string text)
        {
            Sender = sender;
            Text = text;
            SendTime = DateTime.Now.ToString();

        }

        public Message(string sender)
        {
            Sender = sender;
            Text = "";
            SendTime = DateTime.Now.ToString();
        }

        public Message()
        {
            Sender = "";
            Text = "";
            SendTime = DateTime.Now.ToString();
        }
        public Message(string sender, string text, string st)
        {
            Sender = sender;
            Text = text;
            SendTime = st;
        }

        public Message(string sender, byte[] img)
        {
            Sender = sender;
            Image = img;
        }

        // =====================================================================
        // Member functions
        // =====================================================================

        public override string ToString()
        {
            return Sender + ": " + Text; 
        }

        /// <summary>
        /// Returns a sendable message
        /// </summary>
        /// <returns></returns>
        public string GetPrintableMessage()
        {
            return "M|" + Text;
        }

        /// <summary>
        /// Returns a Name message
        /// </summary>
        /// <returns></returns>
        public string GetNameMessage()
        {
            return "N|" + Sender;
        }

        /// <summary>
        /// Return an Accept message
        /// </summary>
        /// <returns></returns>
        public string GetAcceptMessage()
        {
            return "A|" + Sender;
        }

        /// <summary>
        /// Returns a decline message
        /// </summary>
        /// <returns></returns>
        public string GetDeclineMessage()
        {
            return "D|" + Sender;
        }

        /// <summary>
        /// Returns a disconnect message
        /// </summary>
        /// <returns></returns>
        public string GetDisconnectMessage()
        {
            return "d|" + Sender;
        }

        //public JObject GetJson()
        //{
        //    return JObject.Parse(json);
        //}

        public string GetImageMessage()
        {
            return "i|" + Sender;
        }

        public byte[] GetImage()
        {
            return Image;
        }
    }
}
