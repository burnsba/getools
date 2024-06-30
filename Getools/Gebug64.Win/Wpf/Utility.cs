using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Gebug64.Win.Wpf
{
    public static class Utility
    {
        // https://stackoverflow.com/a/41985834/1462295
        public static T? FindParentOfType<T>(DependencyObject? child)
            where T : DependencyObject
        {
            if (object.ReferenceEquals(null, child))
            {
                return null;
            }

            DependencyObject parentDepObj = child;
            do
            {
                parentDepObj = VisualTreeHelper.GetParent(parentDepObj);
                T? parent = parentDepObj as T;
                if (parent != null)
                {
                    return parent;
                }
            }
            while (parentDepObj != null);

            return null;
        }

        public static T? FindParentByRef<T>(DependencyObject? child, T obj)
            where T : DependencyObject
        {
            if (object.ReferenceEquals(null, child))
            {
                return null;
            }

            DependencyObject parentDepObj = child;
            do
            {
                parentDepObj = VisualTreeHelper.GetParent(parentDepObj);
                T? parent = parentDepObj as T;
                if (parent != null && object.ReferenceEquals(parent, obj))
                {
                    return parent;
                }
            }
            while (parentDepObj != null);

            return null;
        }
    }
}
