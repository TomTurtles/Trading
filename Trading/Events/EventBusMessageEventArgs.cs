namespace Trading;
internal class EventBusMessageEventArgs(string msg) : EventArgs
{
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public string Message { get; set; } = msg;
}
