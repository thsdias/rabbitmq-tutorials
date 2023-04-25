using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

const string Queue = "fila";

var factory = new ConnectionFactory { HostName = "localhost" };

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: Queue,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

Console.WriteLine(" [*] Waiting for messages.");

var consumer = new EventingBasicConsumer(channel);

consumer.Received += (model, ea) => {
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($" [x] Received {message}");

    int dots = message.Split('.').Length - 1;
    Thread.Sleep(dots * 1000);

    Console.WriteLine(" [x] Done");

    // Aqui o canal tambem pode ser acessado como ((EventingBasicConsumer)sender).Model
    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
};

channel.BasicConsume(queue: Queue,
                    autoAck: false, // garante que nada sera perdido, mesmo que haja problemas durante processamento do worker. Apos ser encerrado, todas as mensagens nao confirmadas serao reenviadas.
                    consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();
