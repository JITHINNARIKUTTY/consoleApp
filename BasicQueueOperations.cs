using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Microsoft.WindowsAzure.Storage.Queue.Protocol;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    public class BasicQueueOperations
    {
        public async Task RunQueueStorageOperationsAsync()
        {
            // string queueName = "queue-" + System.Guid.NewGuid().ToString();
            string queueName = "queue";
            Console.WriteLine();
            CloudStorageAccount storageAccount = Common.CreateStorageAccountFromConnectionString(Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(queueName);
            try
            {
                await queue.CreateIfNotExistsAsync();
            }
            catch
            {
                Console.WriteLine("If you are running with the default configuration please make sure you have started the storage emulator.  Press the Windows key and type Azure Storage to select and run it from the list of applications - then restart the sample.");
                Console.ReadLine();
                throw;
            }
            Console.WriteLine("QUEUE created with NAME : " + queue.Name.ToString() + "\n");
            Console.WriteLine("*** MENU ***");
            Console.WriteLine("1 : Insert a message into a queue");
            Console.WriteLine("2 : See messages in a queue");
            Console.WriteLine("3 : De-queue the next message");
            Console.WriteLine("4 : Change the contents of a queued message");
            Console.WriteLine("5 : Enqueue a limited number of messages");
            Console.WriteLine("6 : Get the queue length");
            Console.WriteLine("7 : Delete the Queue");
            Console.WriteLine("8 : listing and comparing queues ");
            Console.WriteLine("9 : Get Service Properties of Queue");
            Console.WriteLine("10 : adding and restoring CORS rules sample");
            Console.WriteLine("11 : QUEUE MetaData");
            Console.WriteLine("12 : exit app");

            var selectedValue = Console.ReadLine();
            Regex regex = new Regex("^[0-9]+$");
            if (regex.IsMatch(selectedValue.ToString()))
            {
                int selection = Convert.ToInt32(selectedValue);
                while (selection != 0)
                {
                    switch (selection)
                    {
                        case 1: //Insert a message into a queue
                            Console.WriteLine("Enter a message");
                            await queue.AddMessageAsync(new CloudQueueMessage(Console.ReadLine()));
                            break;
                        case 2: //Peek messages in a queue
                            CloudQueueMessage peekedMessage = await queue.PeekMessageAsync();
                            

                           
                            if (peekedMessage != null)
                            {
                                Console.WriteLine("The peeked message is: {0}", peekedMessage.AsString);
                            }
                            else
                            {
                                Console.WriteLine("No messages exist");
                            }
                            break;
                        case 3: //De-queue the next message
                            CloudQueueMessage message = await queue.GetMessageAsync();
                            if (message != null)
                            {
                                Console.WriteLine("Processing & deleting message with content: {0}", message.AsString);
                                await queue.DeleteMessageAsync(message);
                            }
                            else
                            {
                                Console.WriteLine("No messages exist");
                            }
                            break;
                        case 4: //Change the contents of a queued message
                            CloudQueueMessage message1 = await queue.GetMessageAsync();
                            if (message1 != null)
                            {
                                Console.WriteLine("Enter content to be updated !");
                                message1.SetMessageContent(Console.ReadLine().ToString());
                                await queue.UpdateMessageAsync(message1, TimeSpan.Zero, MessageUpdateFields.Content | MessageUpdateFields.Visibility);
                                CloudQueueMessage peekedMessage1 = await queue.PeekMessageAsync();
                                if (peekedMessage1 != null)
                                {
                                    Console.WriteLine("The modified message is: {0}", peekedMessage1.AsString);
                                }
                            }
                            else
                            {
                                Console.WriteLine("No messages exist");
                            }
                            break;
                        case 5: //Enqueue a limited number of messages
                            Console.WriteLine("Input the count of the messages : ");
                            var messageCount = Console.ReadLine();
                            if (regex.IsMatch(messageCount.ToString()))
                            {
                                for (int i = 0; i < Convert.ToInt32(messageCount); i++)
                                {
                                    Console.WriteLine("Enter the message no : " + i);
                                    var newMsg = Console.ReadLine();
                                    await queue.AddMessageAsync(new CloudQueueMessage(newMsg));
                                }
                            }
                            else
                            {
                                Console.WriteLine("Please Input a number");
                            }
                            break;
                        case 6: //Get the queue length
                            queue.FetchAttributes();
                            int? count = queue.ApproximateMessageCount;
                            if (count != null && count > 0)
                            {
                                Console.WriteLine("Message count = " + count);
                            }
                            else
                            {
                                Console.WriteLine("No messages exist");
                            }
                            break;
                        case 7: //Delete the Queue
                            await queue.DeleteIfExistsAsync();
                            Console.WriteLine("Queue deleted !!");
                            break;
                        case 8: //listing and comparing queues
                            Console.WriteLine("Enter the no of queues u want to create");
                            var queueCount = Console.ReadLine();
                            List<string> lstQueueNames = new List<string>();
                            if (regex.IsMatch(queueCount.ToString()))
                            {
                                for (int i = 0; i < Convert.ToInt32(queueCount); i++)
                                {
                                    string qName = queueName + "-ls-" + i;
                                    lstQueueNames.Add(qName);

                                    CloudQueue cloudQueue = queueClient.GetQueueReference(queueName);
                                    try
                                    {
                                        await cloudQueue.CreateIfNotExistsAsync();
                                    }
                                    catch (StorageException exStorage)
                                    {
                                        Common.WriteException(exStorage);
                                        Console.WriteLine(
                                            "Please make sure your storage account is specified correctly in the app.config - then restart the sample.");
                                        Console.WriteLine("Press any key to exit");
                                        Console.ReadLine();
                                        throw;
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(" Exception thrown while creating queue.");
                                        Common.WriteException(ex);
                                        throw;
                                    }
                                }
                                QueueContinuationToken queueContinuationToken = null;
                                List<CloudQueue> lstCloudQueues = new List<CloudQueue>();
                                do
                                {
                                    QueueResultSegment queueResultSegment = await queueClient.ListQueuesSegmentedAsync(queueName, queueContinuationToken);
                                    queueContinuationToken = queueResultSegment.ContinuationToken;
                                    lstCloudQueues.AddRange(queueResultSegment.Results);
                                }
                                while (queueContinuationToken != null);

                                foreach (string qNames in lstQueueNames)
                                {
                                    CloudQueue cloudQueue = queueClient.GetQueueReference(qNames);
                                    cloudQueue.DeleteIfExists();
                                }
                            }
                            else
                            {
                                Console.WriteLine("Enter a numeric value");
                            }
                            break;
                        case 9: // get the service properties - sample only
                            ServiceProperties orignalServiceProperties = await queueClient.GetServicePropertiesAsync();
                            Console.WriteLine("Original service properties :");
                            Console.WriteLine("Logging operations : " + orignalServiceProperties.Logging.LoggingOperations);
                            Console.WriteLine("Logging RetentionDays : " + orignalServiceProperties.Logging.RetentionDays);
                            Console.WriteLine("Logging Version : " + orignalServiceProperties.Logging.Version);
                            Console.WriteLine("HourMetrics MetricsLevel : " + orignalServiceProperties.HourMetrics.MetricsLevel);
                            Console.WriteLine("HourMetrics Retention Days : " + orignalServiceProperties.HourMetrics.RetentionDays);
                            Console.WriteLine("HourMetrics Version : " + orignalServiceProperties.HourMetrics.Version);
                            Console.WriteLine("MinuteMetrics MetricsLevel : " + orignalServiceProperties.MinuteMetrics.MetricsLevel);
                            Console.WriteLine("MinuteMetrics Retention Days : " + orignalServiceProperties.MinuteMetrics.RetentionDays);
                            Console.WriteLine("MinuteMetrics Version : " + orignalServiceProperties.MinuteMetrics.Version);
                            try
                            {
                                Console.WriteLine();
                                Console.WriteLine("After changing values");
                                //change the service properties
                                ServiceProperties serviceProperties = await queueClient.GetServicePropertiesAsync();
                                serviceProperties.Logging.LoggingOperations = LoggingOperations.Read | LoggingOperations.Write;
                                serviceProperties.Logging.RetentionDays = 10;
                                serviceProperties.Logging.Version = Constants.AnalyticsConstants.LoggingVersionV1;

                                serviceProperties.HourMetrics.MetricsLevel = MetricsLevel.Service;
                                serviceProperties.HourMetrics.RetentionDays = 10;
                                serviceProperties.HourMetrics.Version = Constants.AnalyticsConstants.MetricsVersionV1;

                                serviceProperties.MinuteMetrics.MetricsLevel = MetricsLevel.Service;
                                serviceProperties.MinuteMetrics.RetentionDays = 10;
                                serviceProperties.MinuteMetrics.Version = Constants.AnalyticsConstants.MetricsVersionV1;

                                await queueClient.SetServicePropertiesAsync(serviceProperties);

                                Console.WriteLine("Logging operations : " + serviceProperties.Logging.LoggingOperations);
                                Console.WriteLine("Logging RetentionDays : " + serviceProperties.Logging.RetentionDays);
                                Console.WriteLine("Logging Version : " + serviceProperties.Logging.Version);
                                Console.WriteLine("HourMetrics MetricsLevel : " + serviceProperties.HourMetrics.MetricsLevel);
                                Console.WriteLine("HourMetrics Retention Days : " + serviceProperties.HourMetrics.RetentionDays);
                                Console.WriteLine("HourMetrics Version : " + serviceProperties.HourMetrics.Version);
                                Console.WriteLine("MinuteMetrics MetricsLevel : " + serviceProperties.MinuteMetrics.MetricsLevel);
                                Console.WriteLine("MinuteMetrics Retention Days : " + serviceProperties.MinuteMetrics.RetentionDays);
                                Console.WriteLine("MinuteMetrics Version : " + serviceProperties.MinuteMetrics.Version);
                            }
                            finally
                            {

                                await queueClient.SetServicePropertiesAsync(orignalServiceProperties);
                            }
                            break;
                        case 10:
                            //adding and restoring CORS rules
                            ServiceProperties orignalServicePropertiesCors = await queueClient.GetServicePropertiesAsync();
                            Console.WriteLine("Original service properties :");
                            Console.WriteLine("existing CORS count :" + orignalServicePropertiesCors.Cors.CorsRules.Count);
                            try
                            {
                                Console.WriteLine();
                                Console.WriteLine("adding CORS Rules");
                                Console.WriteLine("AllowedHeaders = any");
                                Console.WriteLine("AllowedMethods = Get");
                                Console.WriteLine("AllowedOrigins = any");
                                Console.WriteLine("ExposedHeaders = any");
                                Console.WriteLine("MaxAgeInSeconds = 3600");
                                // * for any
                                CorsRule corsRule = new CorsRule
                                {
                                    AllowedHeaders = new List<string> { "*" },
                                    AllowedMethods = CorsHttpMethods.Get,
                                    AllowedOrigins = new List<string> { "*" },
                                    ExposedHeaders = new List<string> { "*" },
                                    MaxAgeInSeconds = 3600
                                };
                                //change the service properties
                                ServiceProperties serviceProperties = await queueClient.GetServicePropertiesAsync();
                                serviceProperties.Cors.CorsRules.Add(corsRule);
                                Console.WriteLine("new CORS count :" + serviceProperties.Cors.CorsRules.Count);
                                await queueClient.SetServicePropertiesAsync(serviceProperties);
                            }
                            finally
                            {
                                await queueClient.SetServicePropertiesAsync(orignalServicePropertiesCors);
                                Console.WriteLine("restoring CORS rules :" + orignalServicePropertiesCors.Cors.CorsRules.Count);
                            }
                            break;
                        case 11: //QUEUE MetaData
                            string quName = "q-" + Guid.NewGuid();

                            Console.WriteLine("Setting queue metadata");
                            queue.Metadata.Add("key1", "value1");
                            queue.Metadata.Add("key2", "value2");

                            Console.WriteLine("Creating queue with name {0}", quName);
                            await queue.CreateIfNotExistsAsync();

                            await queue.FetchAttributesAsync();
                            Console.WriteLine("Get queue metadata:");
                            foreach (var keyValue in queue.Metadata)
                            {
                                Console.WriteLine("  {0}: {1}", keyValue.Key, keyValue.Value);
                            }
                            Console.WriteLine("Deleting queue with name {0}", quName);
                            queue.DeleteIfExists();
                            break;
                        case 12:
                            await queue.DeleteIfExistsAsync();
                            Environment.Exit(0);
                            break;
                        default: Environment.Exit(0); break;
                    }
                    Console.WriteLine();
                    Console.WriteLine("Select another option or press 0 to exit");
                    var newSelection = Console.ReadLine();
                    if (regex.IsMatch(newSelection.ToString()))
                    {
                        selection = Convert.ToInt32(newSelection);
                    }
                    else
                    {
                        Console.WriteLine("Please select a number");
                        Console.Clear();
                        await RunQueueStorageOperationsAsync();
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("Please select a number");
                Console.Clear();
                await RunQueueStorageOperationsAsync();
            }
        }

       //[FunctionName("QueueTrigger")]
       // public static void QueueTrigger([QueueTrigger("queue")] string myQueueItem,TraceWriter log)
       // {
       //     log.Info($"C# function processed: {myQueueItem}");
       // }
    }
}