using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace WpfScratch.Mvvm
{
    /// <summary>
    /// ViewModel base class.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Property changed event.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Property changed notifier.
        /// </summary>
        /// <param name="property">Name of property that changed.</param>
        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        /// Property changed notifier.
        /// </summary>
        /// <typeparam name="T">Type of object containing property.</typeparam>
        /// <param name="prop">Property that changed.</param>
        protected void OnPropertyChanged<T>(Expression<Func<T>> prop)
        {
            var mem = (MemberExpression)prop.Body;
            OnPropertyChanged(mem.Member.Name);
        }
    }
}
