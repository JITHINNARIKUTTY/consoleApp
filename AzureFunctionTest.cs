using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs;

namespace ConsoleApp2
{
    public static class AzureFunctionTest
    {
        [FunctionName("QueueTrigger")]
        public static void QueueTrigger(
        [QueueTrigger("queue")] string myQueueItem,
        TraceWriter log)
        {
            log.Info($"C# function processed: {myQueueItem}");
        }
    }
}
