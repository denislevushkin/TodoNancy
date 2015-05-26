using System;
using Nancy.Hosting.Self;

namespace TodoNancySelfHost
{
    // Command line before console app execution:
    // netsh http add urlacl url=http://+:8080/ user=ARCADIA\DENIS.LYOVUSHKIN

    class Program
    {
        static void Main(string[] args)
        {
            var nancyHost = new NancyHost(new Uri("http://localhost:8080/"));
            nancyHost.Start();
            Console.ReadKey();
            nancyHost.Stop();
        }
    }
}
