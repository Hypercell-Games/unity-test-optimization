namespace Core.Messenger
{
    public interface IMessageListener<in T>
    {
        void OnMessage(T message);
    }
}
