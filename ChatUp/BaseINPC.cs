using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ChatUp
{
    public class BaseINPC : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public int ID { get; set; }

        private string name = String.Empty;

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if(value != this.name)
                {
                    this.name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }
    }
}
