namespace Firebase.ConsoleChat
{
    using Newtonsoft.Json;

    public class MessageBase
    {
        public string Author { get; set; }

        public string Content { get; set; }
    }

    /// <summary>
    /// Outbound message has a <see cref="ServerTimeStamp"/> to indicated that this is a server set property.
    /// </summary>
    public class OutboundMessage : MessageBase
    {
        [JsonProperty("Timestamp")]
        public ServerTimeStamp TimestampPlaceholder { get; } = new ServerTimeStamp();
    }

    /// <summary>
    /// Inbound message has <see cref="Timestamp"/> in a form of a UNIX timestamp.
    /// </summary>
    public class InboundMessage : MessageBase
    {
        public long Timestamp { get; set; }
    }

    public class ServerTimeStamp
    {
        [JsonProperty(".sv")]
        public string TimestampPlaceholder { get; } = "timestamp";
    }
}
