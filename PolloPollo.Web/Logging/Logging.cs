using System.IO;

namespace PolloPollo.Web.Logging
{
    public class Logging : ILogging
    {
        private readonly string path = "../.pollo_log";
        private FileStream fs;

        public void Log(string message) 
        {
            if (!File.Exists(path)) {
                fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            } else {
                fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None);
            }
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(message);
            sw.Close();
            fs.Close();
        }
    }
}
