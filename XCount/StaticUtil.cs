using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCount
{
    static class StaticUtil
    {
        public static string ReplaceStrings(string inputString, string[] originalStrings, string[] replacementStrings)
        {
            if (originalStrings.Length != replacementStrings.Length)
            {
                throw new ArgumentException("The number of original strings must match the number of replacement strings.");
            }

            for (int i = 0; i < originalStrings.Length; i++)
            {
                inputString = inputString.Replace(originalStrings[i], replacementStrings[i]);
            }

            return inputString;
        }
    }
}
