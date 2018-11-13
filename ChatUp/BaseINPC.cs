using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ChatUp
{
    public class BaseINPC<TResult> : INotifyPropertyChanged
    {
        // Allows you to specify a lambda for notify property changed
        public event PropertyChangedEventHandler PropertyChanged;

        // Defined as virtual so you can override if you wish
        protected virtual void NotifyPropertyChanged<TResult>(Expression<Func<TModel, TResult>> property)
        {
            // Convert expression to a property name
            string propertyName = ((MemberExpression)property.
                Body).Member.Name;

            // Fire notify property changed event
            InternalNotifyPropertyChanged(propertyName);
        }

        protected void InternalNotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
