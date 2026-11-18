using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;          // unused on purpose (IDE0005)
using System.Text;          // unused on purpose (IDE0005)

namespace OrderKit.Analysis
{
    /// <summary>
    /// Lint fixture: style and maintainability violations only, every one
    /// intentional. Do not "fix" it -- a clean run here means the linter did not look.
    /// </summary>
    public static class LintViolations
    {
        public static int unused_constant = 42;                    // naming (IDE1006), mutable public static field (CA2211)

        public static int BadlyNamedMethod(int InputValue, List<int> other_arg)   // parameter naming (IDE1006)
        {
            int unusedLocal = InputValue * 2;                      // unused variable (CS0219 / IDE0059)
            other_arg.Add(InputValue);
            int result = 0;
            for (int i = 0; i < other_arg.Count; i++)              // index loop over a list (could be foreach)
            {
                result += other_arg[i];
            }
            return result;
        }

        public static string SwallowEverything(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception)                                      // empty catch hides the failure (CA1031)
            {
            }
            return null;
        }

        public static string CompareBooleans(bool? value)
        {
            if (value == null) return "none";
            if (value == true) return "true";                      // redundant comparison
            return "false";
        }

        public static string Concatenate(string[] parts)
        {
            string text = "";
            foreach (string part in parts)
            {
                text = text + part;                                // string concatenation in a loop
            }
            return text;
        }
    }
}
