using Microsoft.Identity.Client;
using Microsoft.PowerBI.Api;
using Microsoft.Rest;
using NexAlytics.Application.DTOs;

namespace NexAlytics.Application.Services;

public class PowerBiService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PowerBiService> _logger;

    private const string PowerBiApiUrl = "https://api.powerbi.com";
    private const string PowerBiScopes = "https://analysis.windows.net/powerbi/api/.default";

    public PowerBiService(IConfiguration configuration, ILogger<PowerBiService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<PowerBiEmbedDto> GetEmbedTokenAsync()
    {
        var dto = new PowerBiEmbedDto();

        try
        {
            var tenantId = _configuration["PowerBI:TenantId"];
            var clientId = _configuration["PowerBI:ClientId"];
            var clientSecret = _configuration["PowerBI:ClientSecret"];
            var workspaceId = _configuration["PowerBI:WorkspaceId"];
            var reportId = _configuration["PowerBI:ReportId"];

            if (string.IsNullOrEmpty(tenantId) || tenantId == "<azure-tenant-id>")
            {
                dto.Error = "Power BI no configurado. Configure las credenciales en appsettings.json";
                return dto;
            }

            var authority = $"https://login.microsoftonline.com/{tenantId}";
            var app = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(authority)
                .Build();

            var result = await app.AcquireTokenForClient(new[] { PowerBiScopes }).ExecuteAsync();

            var tokenCredentials = new TokenCredentials(result.AccessToken, "Bearer");
            using var client = new PowerBIClient(new Uri(PowerBiApiUrl), tokenCredentials);

            var workspaceGuid = Guid.Parse(workspaceId!);
            var reportGuid = Guid.Parse(reportId!);

            var report = await client.Reports.GetReportInGroupAsync(workspaceGuid, reportGuid);

            var generateTokenRequest = new Microsoft.PowerBI.Api.Models.GenerateTokenRequest
            {
                AccessLevel = "view"
            };

            var tokenResponse = await client.Reports.GenerateTokenInGroupAsync(
                workspaceGuid, reportGuid, generateTokenRequest);

            dto.EmbedToken = tokenResponse.Token;
            dto.EmbedUrl = report.EmbedUrl;
            dto.ReportId = reportId!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Power BI embed token");
            dto.Error = $"Error al obtener token de Power BI: {ex.Message}";
        }

        return dto;
    }
}
