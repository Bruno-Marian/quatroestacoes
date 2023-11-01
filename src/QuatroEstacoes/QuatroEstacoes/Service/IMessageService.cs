namespace QuatroEstacoes.Service
{
    public interface IMessageService
    {
        static event Action<string> OnMessage;
        void SendMessage(string message);
        void ClearMessages();
    }
}
