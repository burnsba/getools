using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gebug64.Win.Wpf
{
    public static class Extensions
    {
        public static PointCollection AbsoluteToRelative(this PointCollection pc)
        {
            if (Object.ReferenceEquals(null, pc))
            {
                throw new NullReferenceException();
            }

            if (!pc.Any())
            {
                return pc;
            }

            var first = pc.First();
            var result = new PointCollection();

            foreach (var p in pc)
            {
                result.Add(new System.Windows.Point(p.X - first.X, p.Y - first.Y));
            }

            return result;
        }
    }
}
