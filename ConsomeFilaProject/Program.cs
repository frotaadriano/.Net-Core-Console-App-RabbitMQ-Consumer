using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;

namespace ConsomeFilaProject
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var tarefa = new Tarefa();
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "minhafila1",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        tarefa = JsonSerializer.Deserialize<Tarefa>(message);

                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                        Console.WriteLine("========================================================");
                        Console.WriteLine($"Tarefa id:{tarefa.Id}, descriçao:{tarefa.Descricao}, ");
                        Console.WriteLine("========================================================\n");
                    }
                    catch (Exception e)
                    {
                        channel.BasicNack(deliveryTag: ea.DeliveryTag, false, true);
                        throw e;
                    }
                };

                channel.BasicConsume(queue: "minhafila1",
                                     autoAck: false,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}