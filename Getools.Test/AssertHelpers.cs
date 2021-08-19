using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Getools.Test
{
    public static class AssertHelpers
    {
        public static void AssertStringListsEqual(List<string> expected, List<string> actual, int skip = 0, List<Func<string,bool>> assumeMatchWhen = null)
        {
            Assert.Equal(expected.Count, actual.Count);

            for (int i = skip; i < expected.Count; i++)
            {
                if (!object.ReferenceEquals(null, assumeMatchWhen))
                {
                    bool assumeSkip = false;
                    foreach (var func in assumeMatchWhen)
                    {
                        if (func(expected[i]) && func(actual[i]))
                        {
                            assumeSkip = true;
                            break;
                        }
                    }

                    if (assumeSkip)
                    {
                        continue;
                    }
                }

                if (string.Compare(expected[i], actual[i]) != 0)
                {
                    Assert.True(false, $"Expected: \"{expected[i]}\", actual: \"{actual[i]}\"");
                }
            }
        }
    }
}
