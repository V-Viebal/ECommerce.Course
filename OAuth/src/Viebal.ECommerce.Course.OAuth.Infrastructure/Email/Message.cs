using MimeKit;
using System.Diagnostics.CodeAnalysis;

namespace Viebal.ECommerce.Course.OAuth.Infrastructure.Email;

class Message
{
    public required List<MailboxAddress> To { get; set; }

    public required string Subject { get; set; }

    public string? Content { get; set; }

    [SetsRequiredMembers]
    public Message(IEnumerable<string> to, string subject, string content = "")
    {
        To = new List<MailboxAddress>();
        // Assume that the input is just email addresses without display names
        To.AddRange(to.Select(x => new MailboxAddress(string.Empty, x)));

        Subject = subject;
        Content = content;
    }
}
