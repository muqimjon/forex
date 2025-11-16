namespace Forex.Domain.Entities;

using Forex.Domain.Commons;
using Forex.Domain.Enums;

public class UserNotification : Auditable
{
    public long UserId { get; set; }
    public User User { get; set; } = default!;

    public string Title { get; set; } = string.Empty;       // qisqa sarlavha
    public string Message { get; set; } = string.Empty;     // to‘liq matn
    public string? Link { get; set; }                       // agar action bo‘lsa (masalan, Invoice sahifasiga o‘tish)
    public bool IsRead { get; set; } = false;               // o‘qilgan/o‘qilmagan holati
    public NotificationType Type { get; set; }              // enum: Info, Warning, Error, Success
    public DateTime SentAt { get; set; } = DateTime.UtcNow; // yuborilgan vaqt
}
