using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("AZURE STORAGE CONTAINER : QUEUE OPERATIONS");
            Console.WriteLine("Please check if azure storage emulator is started or not if application stops working");
            BasicQueueOperations basicQueueOperations = new BasicQueueOperations();
            basicQueueOperations.RunQueueStorageOperationsAsync().Wait();
            Environment.Exit(0);
        }
    }
}
