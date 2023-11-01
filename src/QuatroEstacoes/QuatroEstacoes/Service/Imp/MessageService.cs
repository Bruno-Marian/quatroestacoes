namespace QuatroEstacoes.Service.Imp
{
    public class MessageService : IMessageService
    {
        public static event Action<string> OnMessage;

        public void SendMessage(string message)
        {
            OnMessage?.Invoke(message);
        }

        public void ClearMessages()
        {
            OnMessage?.Invoke(null);
        }
    }
}
