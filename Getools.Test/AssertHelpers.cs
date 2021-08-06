using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Getools.Test
{
    public static class AssertHelpers
    {
        public static void AssertStringListsEqual(List<string> expected, List<string> actual, int skip = 0)
        {
            Assert.Equal(expected.Count, actual.Count);

            for (int i = skip; i < expected.Count; i++)
            {
                if (string.Compare(expected[i], actual[i]) != 0)
                {
                    Assert.True(false, $"Expected: \"{expected[i]}\", actual: \"{actual[i]}\"");
                }
            }
        }
    }
}
