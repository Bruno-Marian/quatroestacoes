using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using System;
using QuatroEstacoes.Service;
using QuatroEstacoes.Models;

namespace QuatroEstacoes.Utils
{
    public class MQTT
    {

        private const string baseTopic = "SENAC";
#if !ATIVO
        static private MQTTnet.Client.MqttClientOptions options;
        static private IMqttClient mqttClient;
#endif
        static private List<string> topicos = new List<string>();
        static private string id_em_id = "";



        static public void TopicosClear()
        {
            topicos.Clear();
        }
        static public void TopicosSet(List<string> Topicos)
        {
            topicos.Clear();
            Topicos.ForEach(top => topicos.Add(($"{baseTopic}/{top}").Replace("//", "/")));
        }
        static public void TopicoSet(string Topico)
        {
            topicos.Add(($"{baseTopic}/{Topico}").Replace("//", "/"));
        }

        static public void TopicosSetClienteX4R()
        {
            var topicos = new List<string>();
            topicos.Clear();
            topicos.Add("Upd");
            topicos.Add("Carga/#");
            topicos.Add("Rem/#");
            topicos.Add("Ping");
            topicos.Add("Dev/#");
            topicos.Add("Msg");
            topicos.Add("PedEv/#");
            TopicosSet(topicos);
        }

        static public void InicializaMQTT(string Id_em_id, string server, string app,
            string AppName, string Version, bool AssinarMensagens, IMessageService messageService)
        {

#if !ATIVO
            try
            {
                if (server == null)
                    server = Constants.UrlServer;

                id_em_id = Id_em_id;
                const int port = Constants.PortServer;
                var user = Constants.UserServer;
                var pass = Constants.PassServer;
                options = new MQTTnet.Client.MqttClientOptionsBuilder()
                        .WithClientId($"BRUNO")
                        .WithTcpServer(server, port)
                        .WithCredentials(user, pass)
                        .WithCleanSession()
                        .Build();

                // Create a new MQTT client.
                var factory = new MqttFactory();
                mqttClient = factory.CreateMqttClient();


                if (!mqttClient.IsConnected)
                {
                    Task.Delay(100);
                    mqttClient.ConnectAsync(options);
                }

                if (mqttClient.IsConnected)
                {

                    foreach (var top in topicos)
                        mqttClient.SubscribeAsync(top);
                }

                mqttClient.ConnectedAsync += async e =>
                {
                    Console.WriteLine("### CONNECTED WITH SERVER ###");

                    await mqttClient.SubscribeAsync("#");
                    foreach (var top in topicos)
                        await mqttClient.SubscribeAsync(top);

                    await MQTT.PublicarStart("", AppName, Version);
                    Console.WriteLine("### SUBSCRIBED ###");
                };



                mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                    Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                    Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                    Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                    Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                    Console.WriteLine();

                   
                    const string med_start = $"{baseTopic}/dados/";

                    var topic = e.ApplicationMessage.Topic;
                    var buffer = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                        messageService.SendMessage(buffer);

                            var mac = topic.Substring(med_start.Length);
                            try
                            {
                        Console.WriteLine("BUFFER");
                        Console.WriteLine(buffer);
                                messageService.SendMessage(buffer);
                            }
                            catch (Exception ex)
                            {
                            }
                                         
                    return Task.CompletedTask;
                };


                Task.Run(async () => await mqttClient.ConnectAsync(options, CancellationToken.None));
                }
            catch (Exception ex)
            {
                Console.WriteLine($"\nMQTT Inicialização: {ex.Message}\n");
            }
#endif
        }

        private static Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            throw new NotImplementedException();
        }

        static public Func<string, string, bool> NovoPacoteRecebido { get; set; }


        static async public Task PublicarStart(string Id_em_id, string AppName, string Version)
        {
#if !ATIVO
            int iem_id = 0;
            int.TryParse(Id_em_id, out iem_id);

            var ipacote = "";
            var cpacote = JsonConvert.SerializeObject(ipacote);
            await Publicar($"{baseTopic}/Start", cpacote, false);
#endif

        }

#if !ATIVO
        static public async Task Publicar(string topico, string msg, bool Zip = false)
        {

            if (mqttClient == null)
                return;

            if (mqttClient.IsConnected)
                try
                {
                    string topic = $"{baseTopic}/{topico}";
                    string payload = msg;

                    var apmsg = new MqttApplicationMessage()
                    {
                        Topic = topic,
                        Payload = Encoding.UTF8.GetBytes(payload),
                        QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
                    };
                    var ret = await mqttClient.PublishAsync(apmsg);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nErro: {ex.Message}\n");
                    Debug.WriteLine($"Erro: {ex.Message}");
                }
        }
#endif 
    }
}
