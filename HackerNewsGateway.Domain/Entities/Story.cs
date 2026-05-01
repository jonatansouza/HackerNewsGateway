namespace HackerNewsGateway.Domain.Entities
{
    /// <summary>
    /// Represents a core Story within the domain.
    /// </summary>
    public record Story(
        string Title,           // Maps from "title"[cite: 1]
        string Uri,             // Maps from "url"[cite: 1]
        string PostedBy,        // Maps from "by"[cite: 1]
        DateTimeOffset Time,    // Converted from Unix Time[cite: 1]
        int Score,              // Maps from "score"[cite: 1]
        int CommentCount        // Maps from "descendants"[cite: 1]
    );
}
