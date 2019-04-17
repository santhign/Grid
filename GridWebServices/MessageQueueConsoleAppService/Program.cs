using System;
using System.Threading;

namespace MessageQueueConsoleAppService
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Timer t = new Timer(TimerCallback, null, 0, 900000);
            Timer t = new Timer(TimerCallback, null, 0, 60000); 
            // Wait for the user to hit <Enter>
            Console.ReadLine();
        }

        private static void TimerCallback(Object o)
        {
            // Display the date/time when this method got called.
            Console.WriteLine("In TimerCallback: " + DateTime.Now);
            // Force a garbage collection to occur for this demo.
            GC.Collect();
        }
    }
}
