using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Ui
{
    /// <summary>
    /// Helper class to define a group of UI Elements such that checking one element
    /// in the group will uncheck the others.
    /// </summary>
    /// <typeparam name="T">Type of UI element.</typeparam>
    /// <typeparam name="TKey">Type of unique identifier on UI element.</typeparam>
    public class OnlyOneChecked<T, TKey>
        where T : IIsCheckedabled
        where TKey : struct, IEquatable<TKey>
    {
        private Dictionary<TKey, T> _objects = new Dictionary<TKey, T>();

        /// <summary>
        /// Initializes a new instance of the <see cref="OnlyOneChecked{T, TKey}"/> class.
        /// </summary>
        public OnlyOneChecked()
        {
        }

        /// <summary>
        /// Adds an UI element to the group.
        /// </summary>
        /// <param name="obj">UI Element to add.</param>
        /// <param name="key">Unique identifier of UI Element.</param>
        /// <exception cref="NullReferenceException">Throws if <paramref name="obj"/> is null.</exception>
        public void AddItem(T obj, TKey key)
        {
            if (object.ReferenceEquals(null, obj))
            {
                throw new NullReferenceException();
            }

            if (_objects.ContainsKey(key))
            {
                return;
            }

            _objects.Add(key, obj);
        }

        /// <summary>
        /// Removes the UI element with the given key from the group.
        /// </summary>
        /// <param name="key">Unique identifier of element to remove.</param>
        public void RemoveItem(TKey key)
        {
            if (!_objects.ContainsKey(key))
            {
                return;
            }

            _objects.Remove(key);
        }

        /// <summary>
        /// Iterates all the elements in the group, setting the <see cref="IIsCheckedabled.IsChecked"/>
        /// property.
        /// </summary>
        /// <param name="key">Unique identifier of element to check.</param>
        public void CheckOne(TKey key)
        {
            foreach (var kvp in _objects)
            {
                if (kvp.Key.Equals(key))
                {
                    kvp.Value.IsChecked = true;
                }
                else
                {
                    kvp.Value.IsChecked = false;
                }
            }
        }
    }
}
