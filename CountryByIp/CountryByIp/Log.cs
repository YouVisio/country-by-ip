using System;
using System.IO;

namespace CountryByIp
{
    public interface ILog
    {
        void Error(string str);
    }
    internal sealed class Log : ILog
    {
        const string fileName = "errors.log.txt";

        internal Log()
        {
            if (File.Exists(fileName)) File.Delete(fileName);
        }

        private static readonly object _lock = new object();
        void ILog.Error(string str)
        {
            lock (_lock)
            {
                if (!File.Exists(fileName)) File.WriteAllText(fileName, "start: " + DateTime.Now + "\n\n");
                File.AppendAllText(fileName, str + "\n\n"); 
            }
        }
    }
}