using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Ui
{
    public class OnlyOneChecked<T, TKey>
        where T : IIsCheckedabled
        where TKey : struct, IEquatable<TKey>
    {
        private Dictionary<TKey, T> _objects = new Dictionary<TKey, T>();

        public OnlyOneChecked()
        {
        }

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

        public void RemoveItem(TKey key)
        {
            if (!_objects.ContainsKey(key))
            {
                return;
            }

            _objects.Remove(key);
        }

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
