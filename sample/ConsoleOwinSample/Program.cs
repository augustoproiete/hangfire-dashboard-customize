using System;
using System.Diagnostics;
using Microsoft.Owin.Hosting;

namespace ConsoleOwinSample
{
    class Program
    {
        static void Main()
        {
            try
            {
                const string baseAddress = "http://localhost:8999";
                Console.WriteLine($"Listening on {baseAddress}...");

                using (WebApp.Start<Startup>(baseAddress))
                {
                    Process.Start(baseAddress);

                    Console.WriteLine("Press any key to continue . . .");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine();
                Console.WriteLine("Press any key to continue . . .");
                Console.ReadKey();
            }
        }
    }
}
