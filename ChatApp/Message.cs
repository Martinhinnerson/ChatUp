using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public DateTime SendTime { get; set; }

        // =====================================================================
        // Constructors
        // =====================================================================
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

        // =====================================================================
        // Member functions
        // =====================================================================

        /// <summary>
        /// Returns a sendable message
        /// </summary>
        /// <returns></returns>
        public string GetPrintableMessage()
        {
            return "M|" + Text + "              (" + SendTime.ToString() + ")";
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
    }
}
