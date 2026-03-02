namespace NexAlytics.Application.DTOs;

public class PowerBiEmbedDto
{
    public string EmbedToken { get; set; } = string.Empty;
    public string EmbedUrl { get; set; } = string.Empty;
    public string ReportId { get; set; } = string.Empty;
    public string? Error { get; set; }
}
