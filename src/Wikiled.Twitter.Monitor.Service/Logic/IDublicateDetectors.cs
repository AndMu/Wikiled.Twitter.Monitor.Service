namespace Wikiled.Twitter.Monitor.Service.Logic
{
    public interface IDublicateDetectors
    {
        bool HasReceived(string text);
    }
}