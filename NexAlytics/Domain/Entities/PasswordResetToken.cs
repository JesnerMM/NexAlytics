namespace NexAlytics.Domain.Entities;

public class PasswordResetToken
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool Used { get; set; }
    public DateTime CreatedAt { get; set; }
}
