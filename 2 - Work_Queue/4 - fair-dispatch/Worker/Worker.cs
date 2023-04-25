using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

/*
 * -------------------------------------------------------------------------------------------------------
 * O RabbitMQ apenas despacha uma mensagem quando a mensagem entra na fila. 
 * Ele nao olha para o numero de mensagens nao confirmadas para um consumidor. 
 * Ele simplesmente despacha cegamente cada n-esima mensagem para o n-esimo consumidor.
 * 
 * Para alterar esse comportamento, podemos usar o metodo BasicQos com a configuracao prefetchCount = 1
 * Isso diz ao RabbitMQ para nao enviar mais de uma mensagem para um trabalhador por vez. 
 * Ou, em outras palavras, nao despache uma nova mensagem para um worker ate que ele tenha processado 
 * e reconhecido a anterior. 
 * Em vez disso, ele o despachara para o proximo worker que ainda nao estiver ocupado.
 * -------------------------------------------------------------------------------------------------------
*/

const string Queue = "task_queue";

var factory = new ConnectionFactory { HostName = "localhost" };

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: Queue,
                     durable: true,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

// Garante que o RabbitMQ para nao envie mais de uma mensagem para um mesmo worker por vez.
channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

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
                    autoAck: false,
                    consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();
