using System;
using System.IO;

namespace PolloPollo.Web.Logging
{
    public class Logging : ILogging
    {
        private readonly string path = Path.Combine(Directory.GetCurrentDirectory(), "../.pollo_log");
        private FileStream fs;

        public void Log(LogObject obj) 
        {
            if (!File.Exists(path)) {
                fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            } else {
                fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None);
            }

            StreamWriter sw = new StreamWriter(fs);

            sw.WriteLine(
                $"[{DateTime.UtcNow.ToString()} - {obj.EventType.ToString()}] {obj.Message}"
                );
            sw.WriteLine();
            sw.WriteLine();

            sw.Close();
            fs.Close();
        }
    }
}
