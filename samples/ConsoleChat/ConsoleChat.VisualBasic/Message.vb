Imports Newtonsoft.Json

Public Class MessageBase
    Property Author() As String
    Property Content As String
End Class

Public Class OutboundMessage
    Inherits MessageBase

    <JsonProperty("Timestamp")> Property TimestampPlaceholder() As ServerTimeStamp = New ServerTimeStamp()
End Class

Public Class InboundMessage
    Inherits MessageBase

    Property Timestamp() As Long
End Class


Public Class ServerTimeStamp
    <JsonProperty(".sv")> Property TimestampPlaceholder() As String = "timestamp"
End Class
