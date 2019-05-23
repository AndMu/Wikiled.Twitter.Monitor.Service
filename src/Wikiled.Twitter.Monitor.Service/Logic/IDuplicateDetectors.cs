namespace Wikiled.Twitter.Monitor.Service.Logic
{
    public interface IDuplicateDetectors
    {
        bool HasReceived(string text);
    }
}