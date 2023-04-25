using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

/*
 * -------------------------------------------------------------------------------------------------------
 * Apenas alterar a configuracao 'durable: true' nao funcionará em nossa configuracao atual. 
 * Isso porque ja definimos uma fila chamada 'fila' que nao é duravel. 
 * RabbitMQ nao permite que voce redefina uma fila existente com parametros diferentes e retornara um 
 * erro para qualquer programa que tente fazer isso. 
 * Solucao rapida - declarar uma fila com nome diferente
 * -------------------------------------------------------------------------------------------------------
*/

const string Queue = "task_queue";

var factory = new ConnectionFactory { HostName = "localhost" };

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: Queue,
                     durable: true, // Garante quando o RabbitMQ for encerrado ou travado, ele nao esquecera as filas e mensagens. A fila sobrevivera a uma reinicializacao do nó RabbitMQ.
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
