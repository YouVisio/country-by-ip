using System;
using System.Diagnostics;

namespace CountryByIp
{
    class Program
    {
        static void Main()
        {
            var u = Factory.Get.Updater;

            var sw = Stopwatch.StartNew();

            u.OnCountryComplete += c => Console.WriteLine("Country complete "+c);
            u.OnEnd += () =>
                       {
                           sw.Stop();

                           Console.WriteLine("Execution complete. Time Taken:" + sw.Elapsed);
                       };

            u.Execute();

            Console.ReadLine();
        }
    }
}
