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
        public string Image { get; set; }
        
        // =====================================================================
        // Constructors
        // =====================================================================
        public Message(string sender, string text)
        {
            Sender = sender;
            Text = text;
            SendTime = DateTime.Now.ToString();
            Image = null;
        }

        public Message(string sender)
        {
            Sender = sender;
            Text = "";
            SendTime = DateTime.Now.ToString();
            Image = null;
        }

        public Message()
        {
            Sender = "";
            Text = "";
            SendTime = DateTime.Now.ToString();
            Image = null;
        }
        public Message(string sender, string text, string st, string img = null)
        {
            Sender = sender;
            Text = text;
            SendTime = st;
            Image = img;
        }

        // =====================================================================
        // Member functions
        // =====================================================================

        public override string ToString()
        {
            //return Sender + " (" + SendTime.ToString() + ") " + ": " + Text; 
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
            return "I|" + Image;
        }

        public string GetImage()
        {
            return Image;
        }
    }
}
