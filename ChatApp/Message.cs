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
            if (sender.Contains("|") || text.Contains("|"))
            {
                throw new MessageException("The sender or text cannot contain a | character");
            }
            Sender = sender;
            Text = text;
            SendTime = DateTime.Now.ToString();
            Image = null;
        }

        public Message(string sender)
        {
            if (sender.Contains("|"))
            {
                throw new MessageException("The sender or text cannot contain a | character");
            }
            Sender = sender;
            Text = "";
            SendTime = DateTime.Now.ToString();
            Image = null;
        }
        
        public Message(string sender, string text, string st, string img = null)
        {
            if (sender.Contains("|") || text.Contains("|") || img.Contains("|"))
            {
                throw new MessageException("The sender, text or image cannot contain a | character");
            }
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

        /// <summary>
        /// Return an image message
        /// </summary>
        /// <returns></returns>
        public string GetImageMessage()
        {
            return "I|" + Image;
        }
    }
}
