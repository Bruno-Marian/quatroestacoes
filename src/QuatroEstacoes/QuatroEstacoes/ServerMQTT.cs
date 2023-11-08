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
            MQTT.InicializaMQTT("0", "blu.sisgel.com", "MudBlazor", "Senac", "1.0", true, messageService);
        }
    }
}
