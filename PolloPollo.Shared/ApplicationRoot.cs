using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PolloPollo.Shared
{
    public class ApplicationRoot
    {
        public static string get()
        {
            var exePath = Path.GetDirectoryName(System.Reflection
                              .Assembly.GetExecutingAssembly().CodeBase);
            Regex appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var appRoot = appPathMatcher.Match(exePath).Value;
            var root = appRoot.Split(Path.DirectorySeparatorChar);
            Array.Resize(ref root, root.Length - 1);

            return string.Join(Path.DirectorySeparatorChar + "", root);
        }

        public static string getWebRoot()
        {
            var appRoot = get();

            return Path.Combine(appRoot, "PolloPollo.Web");
        }
    }
}
