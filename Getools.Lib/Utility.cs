﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib
{
    /// <summary>
    /// General utility methods.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Gets standard string giving the assembly name and version.
        /// </summary>
        /// <returns>String.</returns>
        public static string GetAutoGeneratedAssemblyVersion()
        {
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(FindMe));
            string version = assembly.GetName().Version.ToString();
            string assemblyName = assembly.GetName().Name;

            return $"{assemblyName}: {version}";
        }

        public static void ApplyCommaList<T>(Action<string> writepart, List<T> collection, Func<T, string> makepart)
        {
            int index = 0;
            for (int i = 0; i < collection.Count - 1; i++, index++)
            {
                writepart(makepart(collection[i]) + ",");
            }

            if (collection.Count > 0)
            {
                writepart(makepart(collection[collection.Count - 1]));
            }
        }

        public static void ApplyCommaList<T>(Action<string> writepart, List<T> collection, Func<T, int, string> makepart)
        {
            int index = 0;
            for (int i = 0; i < collection.Count - 1; i++)
            {
                writepart(makepart(collection[i], index) + ",");
                index++;
            }

            if (collection.Count > 0)
            {
                writepart(makepart(collection[collection.Count - 1], index));
            }
        }

        public static void AllButLast<T>(List<T> collection, Action<T> allButLastAction, Action<T> lastAction)
        {
            int index = 0;
            for (int i = 0; i < collection.Count - 1; i++, index++)
            {
                allButLastAction(collection[i]);
            }

            if (collection.Count > 0)
            {
                lastAction(collection[collection.Count - 1]);
            }
        }
    }
}
