using System.Text;
using NapPlana.Core.Data;
using NapPlana.Core.Data.Message;

namespace NapArona.Controllers.Routing;

public static class MessageTextExtractor
{
    public static string ExtractText(List<MessageBase>? messages)
    {
        if (messages is null || messages.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        foreach (var message in messages)
        {
            if (message.MessageType != MessageDataType.Text)
            {
                continue;
            }

            if (message.MessageData is TextMessageData textData)
            {
                builder.Append(textData.Text);
            }
        }

        return builder.ToString().Trim();
    }
}
