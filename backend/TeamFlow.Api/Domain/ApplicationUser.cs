
using Microsoft.AspNetCore.Identity;
namespace TeamFlow.Api.Domain;
public class ApplicationUser : IdentityUser<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/*
Şu satır önemli:

public class ApplicationUser : IdentityUser<Guid>

Anlamı: ApplicationUser, IdentityUser’ın özelliklerini miras alır ve kullanıcı kimliği için Guid kullanır.

Bu yüzden aşağıdaki alanları tekrar yazmıyoruz:

| Identity’den gelen alan | Görevi |
|---|---|
| Id | Kullanıcı kimliği |
| UserName | Kullanıcı adı |
| Email | E-posta |
| PasswordHash | Parolanın hash değeri |
| EmailConfirmed | E-posta doğrulama durumu |

*/