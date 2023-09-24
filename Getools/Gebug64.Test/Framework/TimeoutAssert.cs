using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Gebug64.Test.Framework
{
    public class TimeoutAssert
    {
        private const int SpinSleepMs = 1;

        public static void True(ref bool val, TimeSpan timeout, string? errorMessage = null)
        {
            bool fail = false;
            var sw = Stopwatch.StartNew();

            while (val != true)
            {
                System.Threading.Thread.Sleep(SpinSleepMs);
                if (sw.Elapsed > timeout)
                {
                    fail = true;
                    break;
                }
            }

            if (fail)
            {
                Assert.True(val, errorMessage);
            }
        }

        public static void False(ref bool val, TimeSpan timeout, string? errorMessage = null)
        {
            bool fail = false;
            var sw = Stopwatch.StartNew();

            while (val != false)
            {
                System.Threading.Thread.Sleep(SpinSleepMs);
                if (sw.Elapsed > timeout)
                {
                    fail = true;
                    break;
                }
            }

            if (fail)
            {
                Assert.False(val, errorMessage);
            }
        }

        public static void NotEmpty(System.Collections.IEnumerable collection, TimeSpan timeout, string? errorMessage = null)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                errorMessage = "Expected non empty collection";
            }

            bool fail = false;
            var sw = Stopwatch.StartNew();

            while (true)
            {
                // I don't know if it's a threading issue or what, but the enumerator
                // reference needs to be updated every loop iteration.
                var enumerator = collection.GetEnumerator();

                try
                {
                    if (enumerator.MoveNext())
                    {
                        break;
                    }
                }
                finally
                {
                    (enumerator as IDisposable)?.Dispose();
                }

                System.Threading.Thread.Sleep(SpinSleepMs);
                if (sw.Elapsed > timeout)
                {
                    fail = true;
                    break;
                }
            }

            
            if (fail)
            {
                Assert.True(false, errorMessage);
            }
        }
    }
}
