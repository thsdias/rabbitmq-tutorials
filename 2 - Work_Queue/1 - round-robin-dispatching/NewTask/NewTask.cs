using System.Text;
using RabbitMQ.Client;

const string Queue = "fila";

var factory = new ConnectionFactory { HostName = "localhost" };
var connection = factory.CreateConnection();
var channel = connection.CreateModel();

channel.QueueDeclare(queue: Queue,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

var message = GetMessage(args);
var body = Encoding.UTF8.GetBytes(message);

channel.BasicPublish(exchange: string.Empty,
                     routingKey: Queue,
                     basicProperties: null,
                     body: body);

Console.WriteLine($" [x] Sent {message}");

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();

static string GetMessage(string[] args) 
{
    return ((args.Length > 0) ? string.Join(" ", args) : $"Hello World {Queue}");
}
