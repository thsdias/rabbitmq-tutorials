using System.Text;
using RabbitMQ.Client;

const string Queue = "task_queue";

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: Queue,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

var message = GetMessage(args);
var body = Encoding.UTF8.GetBytes(message);

// Marca nossas mensagens como persistentes.
var properties = channel.CreateBasicProperties();
properties.Persistent = true;

channel.BasicPublish(exchange: string.Empty,
                     routingKey: Queue,
                     basicProperties: properties,
                     body: body);

Console.WriteLine($" [x] Sent {message}");

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();

static string GetMessage(string[] args)
{
    return ((args.Length > 0) ? string.Join(" ", args) : $"Hello World {Queue}");
}


/*
 *-------------------------------------------------------------------------------------------------------
 * Observacao: 
 * Sobre persistencia de mensagem
 * Marcar mensagens como persistentes nao garante totalmente que uma mensagem nao sera perdida. 
 * Embora diga ao RabbitMQ para salvar a mensagem no disco, ainda ha uma pequena janela de tempo quando o 
 * RabbitMQ aceitou uma mensagem e ainda nao a salvou. Alem disso, o RabbitMQ nao faz fsync(2) para cada 
 * mensagem -- pode ser apenas salvo no cache e nao realmente gravado no disco. 
 * As garantias de persistencia nao sao fortes, mas sao mais do que suficientes para nossa fila de tarefas 
 * simples. Se voce precisar de uma garantia mais forte, podera usar as confirmações do editor
 * https://www.rabbitmq.com/confirms.html
 * -------------------------------------------------------------------------------------------------------
*/
