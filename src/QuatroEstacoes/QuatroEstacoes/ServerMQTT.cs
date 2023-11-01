using QuatroEstacoes.Service;
using QuatroEstacoes.Utils;

namespace QuatroEstacoes
{
    public class ServerMQTT
    {
        private static IMessageService messageService;
        public static void InicializaMQTT(IMessageService imessageService)
        {
            messageService = imessageService;
            MQTT.TopicosClear();
            var topicos = new List<string>
            {
                "/#"
            };
            MQTT.TopicosSet(topicos);
            MQTT.InicializaMQTT("0", "mqtt.sisgel.com", "MAUI", "Senac", "1.0", true, messageService);
        }
    }
}
