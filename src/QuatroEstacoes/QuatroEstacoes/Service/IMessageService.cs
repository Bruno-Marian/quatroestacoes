namespace QuatroEstacoes.Service
{
    public interface IMessageService
    {
        event Action<string> OnMessage;
        void SendMessage(string message);
        void ClearMessages();
    }
}
