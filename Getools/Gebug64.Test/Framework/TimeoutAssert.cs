using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Test.Framework
{
    public class TimeoutAssert
    {
        public static void True(ref bool val, TimeSpan timeout)
        {
            bool fail = false;
            var sw = Stopwatch.StartNew();

            while (val != true)
            {
                System.Threading.Thread.Sleep(1);
                if (sw.Elapsed > timeout)
                {
                    fail = true;
                    break;
                }
            }

            if (fail)
            {
                Assert.True(val);
            }
        }

        public static void False(ref bool val, TimeSpan timeout)
        {
            bool fail = false;
            var sw = Stopwatch.StartNew();

            while (val != false)
            {
                System.Threading.Thread.Sleep(1);
                if (sw.Elapsed > timeout)
                {
                    fail = true;
                    break;
                }
            }

            if (fail)
            {
                Assert.False(val);
            }
        }

        public static void NotEmpty(System.Collections.IEnumerable collection, TimeSpan timeout)
        {
            bool fail = false;
            var enumerator = collection.GetEnumerator();

            var sw = Stopwatch.StartNew();
            try
            {
                while (!enumerator.MoveNext())
                {
                    System.Threading.Thread.Sleep(1);
                    if (sw.Elapsed > timeout)
                    {
                        fail = true;
                        break;
                    }
                }
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
            
            if (fail)
            {
                Assert.NotEmpty(collection);
            }
        }
    }
}
