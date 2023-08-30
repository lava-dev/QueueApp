namespace QueueApp
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            //var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            var emulatorConnectionString = "http://127.0.0.1:10001/";

            QueueClient queueClient = new QueueClient(emulatorConnectionString, "mystoragequeue");
            
            if (args.Length > 0)
            {
                string value = String.Join(" ", args);
                await InsertMessageAsync(queueClient, value);
                Console.WriteLine($"Sent: {value}");
            }
            else
            {
                string value = await RetrieveNextMessageAsync(queueClient);
                Console.WriteLine($"Received: {value}");
            }

            Console.Write("Press Enter...");
            Console.ReadLine();
        }

        static async Task InsertMessageAsync(QueueClient theQueue, string newMessage)
        {
            if (null != await theQueue.CreateIfNotExistsAsync())
            {
                Console.WriteLine("The queue was created.");
            }

            await theQueue.SendMessageAsync(newMessage);

            // this is if you don't wanna generate message with expiration
            // await theQueue.SendMessageAsync(newMessage, default, TimeSpan.FromSeconds(-1), default); 
        }

        static async Task<string> RetrieveNextMessageAsync(QueueClient theQueue)
        {
            if (await theQueue.ExistsAsync())
            {
                QueueProperties properties = await theQueue.GetPropertiesAsync();

                if (properties.ApproximateMessagesCount > 0)
                {
                    QueueMessage[] retrievedMessage = await theQueue.ReceiveMessagesAsync(1);
                    string theMessage = retrievedMessage[0].Body.ToString();
                    await theQueue.DeleteMessageAsync(retrievedMessage[0].MessageId, retrievedMessage[0].PopReceipt);
                    return theMessage;
                }
                else
                {
                    Console.Write("The queue is empty. Attempt to delete it? (Y/N) ");
                    string response = Console.ReadLine();

                    if (response.ToUpper() == "Y")
                    {
                        await theQueue.DeleteIfExistsAsync();
                        return "The queue was deleted.";
                    }
                    else
                    {
                        return "The queue was not deleted.";
                    }
                }
            }

            return "The queue does not exist. Add a message to the command line to create the queue and store the message.";
        }
    }
}
