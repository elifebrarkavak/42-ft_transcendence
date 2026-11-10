# 01 — Backend öğrenme notları

Bu not, birlikte uyguladığımız ilk adımları ve mevcut kodun durumunu özetler.

## 1. Backend ve HTTP

Backend, istemciden gelen isteği işler ve cevap üretir.

```text
Tarayıcı / Postman → HTTP isteği → API → HTTP cevabı
```

Bir isteğin temel parçaları:

- **Metot:** GET, POST gibi yapılacak işlemi belirtir.
- **Adres:** `/api/health` gibi hangi endpoint'e ulaşılacağını belirtir.
- **Query parametreleri:** URL'de `?name=Samet` gibi veri taşır.
- **Body:** POST örneğimizde JSON verisini taşır.
- **Header:** `Content-Type: application/json` gibi ek bilgiler taşır.

Bir cevapta HTTP durum kodu ve gerektiğinde JSON gövdesi bulunur.

| Durum kodu | Anlamı |
|------------|--------|
| 200 OK | İstek başarılı. |
| 201 Created | Yeni kaynak oluşturuldu; henüz kullanmadık. |
| 400 Bad Request | Gönderilen istek/veri geçersiz. |
| 404 Not Found | Bu çalışmada istenen endpoint bulunamadığında gördük; kaynak bulunamadığında da kullanılabilir. |

Tarayıcı adres çubuğundan bir adres açmak GET isteği gönderir. POST isteğini
Postman ile denedik. Durum kodu tarayıcının Network sekmesinde de görülebilir.

## 2. Uygulamayı çalıştırma

API klasöründe:

```bash
cd backend/TeamFlow.Api
dotnet run
```

Terminaldeki `Now listening on:` adresini kullan. Port her ortamda aynı olmak
zorunda değildir. Örnek: `http://localhost:5120`.

`dotnet run` ile çalışırken yaptığımız değişikliklerden sonra Ctrl+C ile
durdurup tekrar başlattık.

Derleme kontrolü:

```bash
dotnet build
```

## 3. Program.cs ne yapıyor?

`Program.cs`, uygulamanın başlangıç noktasıdır.

| Kod | Görevi |
|-----|--------|
| `WebApplication.CreateBuilder(args)` | Yapılandırma ve servis kayıtları için hazırlık yapar. |
| `builder.Services.AddControllers()` | Controller servislerini kaydeder. |
| `builder.Services.AddOpenApi()` | OpenAPI dokümanı üretimi için servisleri ekler. |
| `builder.Build()` | Uygulamayı oluşturur. |
| `app.MapOpenApi()` | Mevcut kodda geliştirme ortamında OpenAPI adresini açar. |
| `app.UseHttpsRedirection()` | HTTPS yönlendirme middleware'ini ekler; HTTPS adresi ayrıca yapılandırılmalıdır. |
| `app.UseAuthorization()` | Yetkilendirme middleware'ini ekler; tek başına login veya endpoint koruması oluşturmaz. |
| `app.MapControllers()` | Controller endpoint'lerini uygulamaya bağlar. |
| `app.Run()` | Uygulamayı başlatır. |

## 4. Controller ve route

Controller HTTP isteklerini karşılayan sınıftır.

```csharp
[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
```

- `[ApiController]`: API davranışlarını, örneğin otomatik model doğrulama cevaplarını etkinleştirir.
- `[Route("api/health")]`: Controller'ın temel adresidir.
- `ControllerBase`: `Ok()` ve `BadRequest()` gibi HTTP cevap yardımcılarını sağlar.
- `IActionResult`: Metodun farklı türde HTTP cevapları döndürmesine olanak tanır.

```csharp
[HttpGet("greet")]
public IActionResult Greet([FromQuery] string? name)
```

Temel adres ile metottaki adres birleşir:

```text
api/health + greet → GET /api/health/greet
```

Burada metot adının `Greet` olması adresi belirlemez; attribute'lar belirler.

## 5. Health endpoint'i

`GET /api/health` cevabına uygulama adını ekledik:

```csharp
return Ok(new
{
    status = "ok",
    application = "TeamFlow",
    time = DateTime.UtcNow
});
```

- `new { ... }`: İsimsiz (anonymous) bir nesne oluşturur.
- `Ok(...)`: 200 durum koduyla nesneyi JSON olarak döndürür.
- `DateTime.UtcNow`: UTC zamanını alır.

Bu endpoint'in çalışması API'nin cevap verdiğini gösterir. Veritabanına
bağlanıldığını göstermez.

Mevcut ek endpoint: `GET /api/health/ping` → `{ "message": "pong" }`.

## 6. Query parametresi ve validasyon

İstek:

```http
GET /api/health/greet?name=Samet
```

`[FromQuery] string? name`, URL'deki `name` değerini alır.

```csharp
if (string.IsNullOrWhiteSpace(name))
{
    return BadRequest(new
    {
        code = "NAME_REQUIRED",
        message = "Name must."
    });
}

return Ok(new
{
    message = $"Merhaba, {name}!"
});
```

- `string?`: Değer null olabilir.
- `IsNullOrWhiteSpace`: Null, boş veya yalnızca boşluk içeren metni kontrol eder.
- `BadRequest(...)`: 400 cevabı üretir.
- `return`: Metodun çalışmasını o noktada bitirir.
- `$"...{name}..."`: Değişkeni metne yerleştirir.

| Deneme | Beklenen cevap |
|--------|----------------|
| `/api/health/greet?name=Samet` | 200, `Merhaba, Samet!` |
| `/api/health/greet` | 400, `NAME_REQUIRED` |

Nullable olmayan `string name` kullanılırsa `[ApiController]`, eksik değere
bizim if bloğumuzdan önce otomatik 400 döndürebilir. Bu yüzden `string?` kullandık.

## 7. DTO: CreateTaskRequest

Dosya: `backend/TeamFlow.Api/Dtos/CreateTaskRequest.cs`.

DTO, istemcinin göndereceği veya alacağı verinin şeklini tanımlar. Biz burada
istek DTO'su oluşturduk:

```csharp
namespace TeamFlow.Api.Dtos;

public class CreateTaskRequest
{
    public string? Title { get; set; }
}
```

- `namespace`: Sınıfın ait olduğu isim grubunu tanımlar.
- `using TeamFlow.Api.Dtos;`: Başka dosyada bu gruptaki tipi kısa adıyla kullanmayı sağlar; namespace tanımlamaz.
- `get; set;`: Property'nin okunmasını ve değiştirilmesini sağlar.
- DTO oluşturmak veritabanı tablosu oluşturmaz.

## 8. İlk POST endpoint'i

Mevcut dosya adı: `backend/TeamFlow.Api/Controllers/TaskController.cs`.

```csharp
[Route("api/tasks")]
// Controller sınıfı...

[HttpPost]
public IActionResult Create([FromBody] CreateTaskRequest request)
```

`[FromBody]`, istek gövdesindeki JSON'un DTO'ya dönüştürülmesini sağlar:

```text
JSON: { "title": "Backend öğren" }
    ↓
CreateTaskRequest nesnesi
    ↓
request.Title → "Backend öğren"
```

### Postman ile deneme

1. Metot: POST.
2. Adres: `http://localhost:5120/api/tasks` (kendi portunu kullan).
3. Body → raw → JSON.
4. Gönder:

```json
{
  "title": "Backend öğren"
}
```

Mevcut endpoint 200 ile başlığı geri gönderir:

```json
{
  "title": "Backend öğren"
}
```

Boş başlık gönderildiğinde 400 döner:

```json
{
  "code": "VALIDATION_ERROR",
  "errors": {
    "title": ["REQUIRED"]
  }
}
```

Bu kontrolde `new[] { "REQUIRED" }`, string dizisi oluşturur.

Bu ilk örnekte yalnızca başlığı geri döndürmüştük. Sonraki adımda endpoint'i
`TaskItem` oluşturup bellek listesine ekleyecek şekilde geliştirdik (bölüm 12).
Mevcut kod hâlâ 200 döndürüyor; kaynak oluşturma cevabını 201 yapmak ilerideki
iyileştirmelerden biri.

## 9. Entity: TaskItem

Dosya: `backend/TeamFlow.Api/Domain/TaskItem.cs`.

Entity, uygulamada yönettiğimiz varlığın modelidir. Görevde başlığın yanında
kimlik, durum ve oluşturulma tarihi de bulunur.

| Alan | Anlamı |
|------|--------|
| `Guid Id` | Görevin kimliği. `Guid.NewGuid()` yeni bir kimlik üretir. |
| `string Title` | Görevin başlığı. `string.Empty` boş metindir. |
| `Status` | Görevin durumu; başlangıçta `TODO` olması planlandı. |
| `DateTime CreatedAt` | Görevin UTC oluşturulma zamanı. |

`Task`, C# ekosisteminde zaten kullanılan bir tip olduğu için modele
`TaskItem` adını verdik.

### DTO ile entity farkı

```text
CreateTaskRequest → İstemci hangi veriyi gönderir?
TaskItem          → Uygulamada bir görev hangi bilgileri taşır?
```

Kullanıcı yalnızca başlık gönderir. Kimliği, başlangıç durumunu ve oluşturulma
zamanını backend belirler.

Bu düzenlemeler tamamlandı: `TaskItem.cs` dosyasına
`namespace TeamFlow.Api.Domain;` eklendi ve property adı `Status` oldu.
`TaskController.cs` içinde de `namespace TeamFlow.Api.Controllers;` bulunuyor.

## 10. Şu an nerede kaldık?

Tamamladıklarımız:

- API'yi çalıştırma ve GET isteği gönderme.
- JSON cevabını değiştirme.
- Query parametresi alma.
- Boş veri için 400 döndürme.
- DTO oluşturma.
- POST isteğinde JSON alma ve doğrulama.
- İlk görev entity'sini yazma.
- DTO'dan görev nesnesi oluşturma ve başlığı `Trim()` ile temizleme.
- Görevleri geçici bellek listesinde saklama.
- Bütün görevleri ve ID ile tek bir görevi getirme.
- Route parametresi, GUID kısıtı ve kayıt bulunamadığında 404 döndürme.
- PATCH ile görev başlığını güncelleme.
- Docker'daki `teamflow` veritabanına psql ile bağlanma.
- Bağlantı bilgisini User Secrets'a kaydetme.
- AppDbContext oluşturma ve PostgreSQL sağlayıcısını kaydetme (derlendi).
- InitialTasks migration'ını oluşturma ve veritabanına uygulama.
- Oluşturma, listeleme, tek görev getirme, güncelleme ve silmeyi PostgreSQL'e taşıma.
- 201 Created, Location başlığı ve 204 No Content kullanma.

Henüz yapmadıklarımız:

- Kayıt, giriş ve yetkilendirme.
- Ortak hata yönetimi ve frontend bağlantısı.
- Organizasyon/proje ilişkileri ve concurrency kontrolü.

Identity kullanıcı modeli de eklendi (bölüm 25). Sonraki adım, Identity
migration'ının uygulanmasını doğrulamak ve servisleri kaydetmek.

## 11. API sözleşmesi

Önceden `docs/api-contract.md` dosyasında login için bir sözleşme taslağı
hazırladık: adres, istek alanları, başarılı cevap ve hatalar.

Bu belge frontend ile backend'in aynı veri biçimini kullanmasına yardımcı olur.
Belgenin varlığı login endpoint'inin uygulandığı anlamına gelmez.

## 12. DTO'dan entity oluşturma

`Create` metodunda, başlık kontrolünden sonra:

```csharp
var task = new TaskItem
{
    Title = request.Title.Trim()
};

Tasks.Add(task);
```

`new TaskItem` her istekte yeni bir nesne oluşturur. Kimlik, durum ve zaman
entity'nin başlangıç değerlerinden gelir. `Trim()`, başlığın başındaki ve
sonundaki boşlukları kaldırır. Kontrolü önce yaptığımız için null veya yalnızca
boşluk içeren başlıklar bu satıra ulaşmaz.

```text
JSON → CreateTaskRequest → Validasyon → TaskItem → Liste → JSON cevap
```

Mevcut POST cevabında `id`, `title`, `status` ve `createdAt` bulunur.

## 13. Bellekte saklama ve listeleme

Controller sınıfının içinde, metotların dışında:

```csharp
private static readonly List<TaskItem> Tasks = new();
```

- `private`: Alana yalnızca bu sınıftan erişilir.
- `static`: Liste controller nesneleri arasında paylaşılır; her istekte sıfırlanmaz.
- `readonly`: Alana başka bir liste atanamaz; listedeki elemanlar değiştirilebilir.
- `List<TaskItem>`: Görev nesnelerini tutan liste.
- `new()`: Boş liste oluşturur.
- `Tasks.Add(task)`: Görevi listeye ekler.

Listeleme metodu:

```csharp
[HttpGet]
public IActionResult GetAll()
{
    return Ok(Tasks);
}
```

`GET /api/tasks` JSON dizisi döndürür. Liste boşsa cevap `[]` olur.
`POST /api/tasks` ile görev ekledikten sonra GET ile listede görülebilir.

**Bu, geçici bir öğrenme çözümü:** Uygulama yeniden başlayınca veriler silinir.
`List<T>` eşzamanlı okuma/yazma için güvenli bir veri deposu değildir. Gerçek
çok kullanıcılı uygulamada bu saklama işini veritabanına taşıyacağız.

## 14. Route parametresi ile tek görev getirme

```csharp
[HttpGet("{id:guid}")]
public IActionResult GetById([FromRoute] Guid id)
{
    var task = Tasks.FirstOrDefault(item => item.Id == id);

    if (task is null)
    {
        return NotFound(new { code = "TASK_NOT_FOUND" });
    }

    return Ok(task);
}
```

- `{id:guid}`: URL'nin bu bölümü GUID biçiminde olmalı.
- `[FromRoute]`: Değeri URL yolundan alır.
- `FirstOrDefault`: Koşula uyan ilk elemanı bulur; burada bulunamazsa null döner.
- `item => item.Id == id`: Her görev için kimlik eşitliğini sınayan lambda ifadesi.
- `NotFound(...)`: 404 durum koduyla cevap üretir.

| İstek | Sonuç |
|-------|-------|
| `GET /api/tasks` | 200, görev dizisi |
| `GET /api/tasks/VAR-OLAN-GUID` | 200, tek görev nesnesi |
| `GET /api/tasks/00000000-0000-0000-0000-000000000000` | Bu kimlik listede yoksa 404, `TASK_NOT_FOUND` |
| `GET /api/tasks/abc` | GUID kısıtına uymadığı için bu route eşleşmez; 404 |

Query ve route farkı:

```text
/api/health/greet?name=Samet → [FromQuery]
/api/tasks/{id}             → [FromRoute]
JSON istek gövdesi          → [FromBody]
```

## 15. Görev silme — ilk bellek örneği

Bu bölüm bellekte çalışan ilk silme örneğini gösterir. Güncel controller'da
silme işlemi PostgreSQL kullanıyor; bölüm 23'e bak.

Controller sınıfının içine, diğer metotların dışına eklenecek:

```csharp
[HttpDelete("{id:guid}")]
public IActionResult Delete([FromRoute] Guid id)
{
    var task = Tasks.FirstOrDefault(item => item.Id == id);

    if (task is null)
    {
        return NotFound(new { code = "TASK_NOT_FOUND" });
    }

    Tasks.Remove(task);
    return NoContent();
}
```

`Remove`, bulunan görevi listeden çıkarır. `NoContent()`, gövdesi boş olan
204 cevabını üretir; frontend bu cevapta JSON okumaya çalışmamalı.

Deneme sırası:

1. Uygulamayı yeniden başlat ve POST ile görev oluştur.
2. Cevaptaki ID ile DELETE gönder; 204 beklenir.
3. Aynı ID ile GET gönder; 404 beklenir.
4. Aynı ID ile tekrar DELETE gönder; yazdığımız kurala göre 404 beklenir.

## 16. Güncel endpoint özeti

| Metot | Adres | İşlem | Kodda mevcut mu? |
|-------|-------|-------|-----------------|
| GET | `/api/health` | Uygulama durumu | Evet |
| GET | `/api/health/ping` | Basit pong cevabı | Evet |
| GET | `/api/health/greet?name=Samet` | Parametreli selamlama | Evet |
| POST | `/api/tasks` | Görev oluşturup belleğe ekleme | Evet |
| GET | `/api/tasks` | Görevleri listeleme | Evet |
| GET | `/api/tasks/{id}` | Tek görev getirme | Evet |
| PATCH | `/api/tasks/{id}` | Görev başlığını güncelleme | Evet |
| DELETE | `/api/tasks/{id}` | Görev silme | Evet |

## 17. PATCH ile güncelleme

`Dtos/UpdateTaskRequest.cs`, güncellemede alınan `Title` alanını tanımlar.
`TaskController.Update` metodu route'tan ID'yi, body'den DTO'yu alır:

```csharp
[HttpPatch("{id:guid}")]
public IActionResult Update(
    [FromRoute] Guid id,
    [FromBody] UpdateTaskRequest request)
```

Başlık boşsa 400, görev bulunamazsa 404 döner. Geçerli istekte:

```csharp
task.Title = request.Title.Trim();
return Ok(task);
```

Bulunan `task`, listedeki nesneye referanstır. Başlığı değiştirmek listedeki
nesneyi de değiştirir; tekrar `Tasks.Add(task)` çağrılmaz. ID ve oluşturulma
zamanı aynı kalır. Kullanıcı Postman denemelerini başarılı olarak bildirdi.

## 18. PostgreSQL'i çalıştırma ve psql

Repo kökünde:

```bash
docker compose up -d postgres
docker compose ps
docker compose exec postgres sh -c 'psql -U "$POSTGRES_USER" -d "$POSTGRES_DB"'
```

- `up -d`: Servisi arka planda başlatır.
- `ps`: Container durumunu gösterir.
- `exec`: Çalışan container içinde komut çalıştırır.
- `psql`: PostgreSQL'in terminal istemcisidir.

Bağlantı içinde şu sorguyu çalıştırdık:

```sql
SELECT current_database();
```

Sonuç `teamflow` oldu. PostgreSQL Docker'da çalışıyor ve 5432 portu bilgisayara
açık. psql'den çıkmak için `/q` değil, ters eğik çizgiyle `\q` yazılır.
`teamflow-#`, tamamlanmamış bir komut beklendiğini gösterir; hata sonrası bunu gördük.

Bu kontrol PostgreSQL'in çalıştığını doğruladı; API bağlantısını henüz doğrulamadı.

## 19. Connection string ve User Secrets

Connection string, sunucu, port, veritabanı ve giriş bilgilerini taşır.
API klasöründe çalıştırdık:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=teamflow;Username=KULLANICI;Password=PAROLA"
```

Buradaki kullanıcı ve parola yer tutucudur; gerçek değerler nota eklenmez.
Kendi değerleriyle çalıştırılan komutun başarılı kaydetme mesajı alındı.

- API bilgisayarda çalıştığı için `Host=localhost` kullanıyoruz.
- API ileride aynı Compose ağına alındığında servis adı olan `postgres` kullanılacak.
- Compose'un `.env` okuması, `dotnet run` uygulamasının da otomatik okuması anlamına gelmez.
- User Secrets, yerel geliştirme ayarlarını repo dışında tutar; şifrelenmiş kasa değildir.
- Kaydetme işlemi veritabanına bağlantı testi yapmaz.

## 20. AppDbContext ve servis kaydı

`Data/AppDbContext.cs` dosyasına eklendi:

```csharp
using Microsoft.EntityFrameworkCore;
using TeamFlow.Api.Domain;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
}
```

- `DbContext`: EF Core ile sorgulama, değişiklik takibi ve kayıt işlemlerinin merkezi.
- `DbContextOptions`: Sağlayıcı ve bağlantı gibi ayarları taşır.
- `base(options)`: Ayarları üst sınıfa iletir.
- `DbSet<TaskItem>`: Görevleri sorgulamak ve kaydetmek için erişim noktası.
- `=> Set<TaskItem>()`: Property okunduğunda ilgili entity kümesini getirir.

Mevcut dosyada namespace yok; bu yüzden sınıf global namespace'te ve kod derleniyor.
Klasör adının `Data` olması kendiliğinden namespace oluşturmaz. Tutarlılık için
sonradan `namespace TeamFlow.Api.Data;` eklenirse `Program.cs` içine de
`using TeamFlow.Api.Data;` eklenmeli.

`Program.cs` içinde `builder.Build()` öncesinde:

```csharp
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Database info not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
```

- `GetConnectionString`: `ConnectionStrings:DefaultConnection` ayarını okur.
- `?? throw`: Ayar null ise uygulamanın anlaşılır bir hatayla durmasını sağlar.
- `AddDbContext`: Context'i dependency injection sistemine kaydeder; varsayılan yaşam süresi scoped'dur.
- `UseNpgsql`: PostgreSQL sağlayıcısını ve bağlantı bilgisini yapılandırır.

Dependency injection, sınıfın ihtiyaç duyduğu nesnenin uygulama tarafından
sağlanmasıdır. Artık controller'a constructor üzerinden `AppDbContext` veriliyor.

Kullanıcı `dotnet build` işleminin başarılı olduğunu bildirdi. Bu, kodun
derlendiğini gösterir; bağlantı ayarının doğru olduğunu veya tablo oluştuğunu
kanıtlamaz. O aşamada henüz migration uygulanmamıştı. Sonraki adımlarda migration
uygulandı ve controller veritabanına taşındı.

## 21. Migration uygulama ve doğrulama

API klasöründe:

```bash
dotnet ef --version
dotnet ef migrations add InitialTasks --output-dir Data/Migrations
dotnet ef database update
```

- `migrations add`: Modelden şema değişikliklerini içeren dosyalar üretir; tabloyu henüz oluşturmaz.
- `Up`: Migration uygulanırken yapılacak işlemler; burada `Tasks` tablosunu oluşturur.
- `Down`: Migration geri alınırken yapılacak ters işlemler; burada tabloyu siler.
- `database update`: Bekleyen migration'ları bağlantıdaki veritabanına uygular.
- Model snapshot: EF Core'un sonraki migration'da değişiklikleri karşılaştırmak için kullandığı model kaydıdır.

İlk denemede `28P01` hatası alındı: bağlantıda gerçek kullanıcı yerine
`KULLANICI` yer tutucusu kalmıştı. User Secrets doğru bilgilerle güncellendi;
aynı migration tekrar oluşturulmadan uygulandı. Parola bu nota yazılmaz.

Doğrulama sırasında `teamflow` veritabanında `Tasks` ve `__EFMigrationsHistory`
tabloları, geçmişte de `20261109202519_InitialTasks` kaydı görüldü.
Compose'u engelleyen `.env` sonundaki tek başına `\` satırı kaldırıldı.

psql içinde inceleme:

```sql
SELECT * FROM "Tasks";
```

```text
\dt
\d "Tasks"
```

## 22. Controller'da DbContext ve kalıcı oluşturma

```csharp
private readonly AppDbContext _db;

public TaskController(AppDbContext db)
{
    _db = db;
}
```

Constructor, controller oluşturulurken çalışır. ASP.NET Core kayıtlı context'i
sağlar. `_db` alanı, metotların bu context'e erişmesini sağlar.

Veritabanı işlemlerini bekleyen metotlar `async Task<IActionResult>` döndürür.
`await`, asenkron işlemin sonucunu beklerken iş parçacığını bloke etmez.

Görev oluştururken:

```csharp
_db.Tasks.Add(task);
await _db.SaveChangesAsync();

return CreatedAtAction(
    nameof(GetById),
    new { id = task.Id },
    new
    {
        id = task.Id,
        title = task.Title,
        status = task.Status,
        createdAt = task.CreatedAt
    });
```

`Add`, entity'yi eklenmek üzere işaretler; `SaveChangesAsync` veritabanına yazar.
`CreatedAtAction`, 201 ve yeni kaydın adresini içeren `Location` başlığı üretir.
`nameof(GetById)`, metodun adını string olarak verir.

## 23. Veritabanından okuma, güncelleme ve silme

EF Core asenkron sorgu metotları için:

```csharp
using Microsoft.EntityFrameworkCore;
```

Listeleme:

```csharp
var tasks = await _db.Tasks.AsNoTracking().ToListAsync();
return Ok(tasks);
```

`ToListAsync` sorguyu çalıştırır. `AsNoTracking`, salt okuma sorgusunda değişiklik
takibini kapatır. Burada eski büyük harfli `Tasks` yerine sorgunun sonucu olan
küçük harfli `tasks` döndürülmeli; geliştirme sırasında bu hata düzeltildi.

Tek görev okuma:

```csharp
var task = await _db.Tasks
    .AsNoTracking()
    .FirstOrDefaultAsync(item => item.Id == id);
```

Sonuç null ise 404, bulunursa 200 döndürülür.

Güncellemede sorguyu `AsNoTracking` olmadan çalıştırıyoruz:

```csharp
var task = await _db.Tasks.FirstOrDefaultAsync(item => item.Id == id);
// Null kontrolünden sonra:
task.Title = request.Title.Trim();
await _db.SaveChangesAsync();
```

Context nesneyi takip ettiği için ayrıca `_db.Tasks.Update(task)` gerekmez.
Başlık değişir; ID ve oluşturulma zamanı aynı kalır.

Silmede görev bulunup null kontrolü yapıldıktan sonra:

```csharp
_db.Tasks.Remove(task);
await _db.SaveChangesAsync();
return NoContent();
```

`Remove`, silme işlemini işaretler; kayıt çağrısı işlemi uygular. Başarı cevabı
204'tür ve gövde içermez. Bulunamayan görev için 404 döner.

Silme metodunda `[HttpDelete("{id:guid}")]` eksik olduğu için 405 görüldü.
Attribute eklendi, derleme hatasız geçti ve kullanıcı doğru cevabı doğruladı.
Kod değişikliğinden sonra çalışan API'nin yeniden başlatılması gerekir.

Eski `static List<TaskItem>` artık kullanılmıyor; dosyada yalnızca yorum olarak
kalıyor. Oluşturma, okuma ve güncelleme için kullanıcı başarılı denemeleri ve
yeniden başlatmadan sonra verinin korunduğunu bildirdi.

## 24. Roadmap'te durumumuz — 10 Kasım 2026

| Aşama | Durum |
|-------|-------|
| 1. API sözleşmesi | Login taslağı var; diğer sözleşmeler eksik. |
| 2. Veritabanı ve API temeli | PostgreSQL, görev migration'ı, Identity kullanıcı modeli ve IdentityDbContext hazır. AddIdentity migration'ı üretildi ve incelendi; uygulanması henüz doğrulanmadı. Ortak hata yönetimi ve frontend bağlantısı eksik. |
| 3. Kayıt ve oturum | Henüz başlanmadı. |
| 4–5. Organizasyon, üyelik ve projeler | Henüz başlanmadı. |
| 6. Görevler | Temel CRUD PostgreSQL'de çalışıyor; proje ilişkileri, yetkiler ve concurrency eksik. |
| 7–10. Arama, profil/arkadaşlık, admin/GDPR ve teslim | Henüz başlanmadı. |

İkinci aşamanın veritabanı kısmı tamamlandı; aşamanın tamamı henüz bitmedi.
Görev CRUD'u öğrenme amacıyla erkenden yapıldı. Mevcut endpoint'lerde oturum ve
yetki kontrolü yok. Sırada Identity migration'ının uygulanmasını doğrulamak,
Identity servislerini kaydetmek, ardından kayıt ve giriş var.

## 25. Identity kullanıcı modeli

Identity, kullanıcı ve rol yönetimi için ASP.NET Core altyapısıdır. Parola
hashleme ve doğrulama işlemlerini kendi algoritmamızı yazmadan kullanacağız.
Identity eklemek tek başına endpoint'leri korumaz veya login oluşturmaz.

API klasöründe paket eklendi:

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version "10.0.*"
```

Projede çözümlenen sürüm `10.0.14`. Paket Identity'nin EF Core üzerinden
veritabanında saklanabilmesi için gereken entegrasyonu sağlar.

`Domain/ApplicationUser.cs`:

```csharp
using Microsoft.AspNetCore.Identity;

namespace TeamFlow.Api.Domain;

public class ApplicationUser : IdentityUser<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

`IdentityUser<Guid>` sınıfından miras alıyoruz. Kullanıcının kimlik tipi GUID.
`Id`, `UserName`, `Email`, `PasswordHash` ve `EmailConfirmed` gibi alanlar hazır
gelir; tekrar yazılmaz. `Name`, `AvatarUrl`, `CreatedAt` uygulamaya özel alanlar.

Parolayı düz metin saklayan bir entity alanı eklemiyoruz. Kayıt sırasında
parolayı Identity'nin kullanıcı yönetim servisine vereceğiz. Kullanıcı entity'sini
API cevabı olarak doğrudan döndürmek yerine ileride response DTO kullanacağız.

## 26. DbContext'i Identity ile genişletme

`Data/AppDbContext.cs` artık şu yapıda:

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TeamFlow.Api.Domain;

namespace TeamFlow.Api.Data;

public class AppDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
}
```

`IdentityDbContext`, normal DbContext özelliklerini korur; Identity tablolarını
ve ilişkilerini de modele ekler.

| Tür parametresi | Görevi |
|----------------|--------|
| `ApplicationUser` | Kullanıcı modeli |
| `IdentityRole<Guid>` | GUID kimlikli rol modeli |
| `Guid` | Identity ilişkilerinde kullanılan kimlik tipi |

`Tasks` korunur; görevler ve kullanıcılar aynı veritabanında saklanabilir.
Bu değişiklik henüz görevlere kullanıcı ilişkisi eklemez.

Context'e `TeamFlow.Api.Data` namespace'i eklendi. Context'i kullanan dosyalar
gerektiğinde `using TeamFlow.Api.Data;` ile bu tipe erişir.

Geliştirme sırasında `Tasks` yanlışlıkla `Task` yazıldığı için controller'daki
`_db.Tasks` erişimleri derlenmedi. Property tekrar `Tasks` yapıldı. Eksik kullanıcı
namespace'i de eklendi. Son kontrolde derleme 0 hata, 0 uyarıyla geçti.

## 27. AddIdentity migration'ı

Çalıştırılan komut:

```bash
dotnet ef migrations add AddIdentity --output-dir Data/Migrations
```

`20261110182254_AddIdentity.cs` oluştu. İncelemede yeni Identity tablolarını
oluşturduğu ve mevcut `Tasks` tablosunu değiştirmediği görüldü.

| Tablo | Saklanan bilgi |
|-------|----------------|
| `AspNetUsers` | Kullanıcılar ve özel profil alanları |
| `AspNetRoles` | Roller |
| `AspNetUserRoles` | Kullanıcı-rol eşleşmeleri |
| `AspNetUserClaims` | Kullanıcı claim kayıtları |
| `AspNetRoleClaims` | Rol claim kayıtları |
| `AspNetUserLogins` | Harici giriş sağlayıcısı eşleşmeleri |
| `AspNetUserTokens` | Identity kullanıcı token kayıtları |

Tablonun oluşturulması ilgili özelliğin tamamen geliştirildiği anlamına gelmez.
Örneğin harici giriş tablosunun bulunması OAuth akışını otomatik oluşturmaz.

Migration'ın `Down` bölümünde bu Identity tablolarını kaldıran işlemler bulunur;
normal ileri uygulamada çalışan bölüm `Up`'tır. `Down` çalıştırılmadı.

Uygulama için verilen sonraki komut:

```bash
dotnet ef database update
```

**Güncel durum:** AddIdentity dosyaları ve başarılı derleme kontrol edildi.
Sonrasında Identity servisleri ve register endpoint'i eklendi; kullanıcı kayıt
denemesinin başarılı olduğunu bildirdi. Bu son aşamada veritabanı ayrıca
sorgulanmadı. Login endpoint'i henüz yazılmadı.

## 28. Identity servislerini kaydetme

`Program.cs` içinde, DbContext kaydından sonra ve `builder.Build()` öncesinde:

```csharp
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>();
```

- `AddIdentityCore`: Kullanıcı yönetim servislerini, örneğin UserManager'ı kaydeder.
- `RequireUniqueEmail`: Identity kullanıcı doğrulamasında e-postanın benzersizliğini kontrol eder; tek başına veritabanında unique constraint oluşturmaz.
- Parola: En az 8 karakter, rakam, küçük ve büyük harf gerekir. Özel karakter zorunlu değildir.
- `AddRoles`: Rol yönetim servislerini ekler; henüz kullanıcıya rol atamadık.
- `AddEntityFrameworkStores`: Kullanıcı/rol saklama işlemlerini AppDbContext'e bağlar.

Bu servis kaydı şema değişikliği değildir; kendisi için migration gerekmez.
Derlemenin başarılı olduğu kullanıcı tarafından bildirildi. Bu kayıt henüz
cookie oturumunu veya endpoint korumasını yapılandırmaz.

## 29. RegisterRequest DTO'su

`Dtos/RegisterRequest.cs` dosyasında nullable string alanlar tanımlandı:

```csharp
public class RegisterRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
}
```

DTO istekteki parolayı geçici olarak taşır. Entity'de ise Identity'nin
ürettiği `PasswordHash` saklanır. Parola loglanmaz, cevapta döndürülmez ve
düz metin olarak veritabanına yazılmaz.

Mevcut DTO global namespace'te. Tutarlılık için ileride
`namespace TeamFlow.Api.Dtos;` eklenirse AuthController'a ilgili using de eklenmeli.

## 30. İlk kayıt endpoint'i

`Controllers/AuthController.cs` içinde `POST /api/auth/register` oluşturuldu.
Controller, constructor üzerinden `UserManager<ApplicationUser>` alır:

```csharp
private readonly UserManager<ApplicationUser> _userManager;

public AuthController(UserManager<ApplicationUser> userManager)
{
    _userManager = userManager;
}
```

İşlem sırası:

1. Ad, e-posta ve parola null/boş/yalnızca boşluk ise 400 `VALIDATION_ERROR` döner.
2. `ApplicationUser` oluşturulur; ID üretilir, ad ve e-postanın kenar boşlukları temizlenir.
3. Kullanıcı adı olarak da e-posta kullanılır.
4. Kullanıcı ve parola UserManager'a verilir.
5. Başarısızlıkta Identity hata kodları döndürülür.
6. Başarıda 201 ve yalnızca `id`, `name`, `email` döndürülür.

Temel kayıt kodu:

```csharp
var user = new ApplicationUser
{
    Id = Guid.NewGuid(),
    Name = request.Name.Trim(),
    UserName = request.Email.Trim(),
    Email = request.Email.Trim()
};

var result = await _userManager.CreateAsync(user, request.Password);
```

UserManager, kullanıcı/parola kurallarını kontrol eder ve başarılıysa parola
hash'iyle birlikte kullanıcıyı kaydeder. Ayrıca `_db.Users.Add` veya
`SaveChangesAsync` çağırmıyoruz. Parolaya Trim uygulanmaz.

`result.Succeeded` başarı durumudur. `result.Errors.Select(...)`, her hatadan
yalnızca kodunu içeren bir nesne üretir.

Postman örneği (gerçek hesap bilgileri değildir):

```http
POST /api/auth/register
Content-Type: application/json
```

```json
{
  "name": "Samet",
  "email": "samet@example.com",
  "password": "Deneme123"
}
```

Başarı: 201 ve kullanıcı bilgileri. **Kayıt işlemi henüz oturum açmaz.**
Kullanıcı denemenin başarılı olduğunu bildirdi.

Ek denemeler: Aynı e-postayla tekrar kayıt ve farklı e-postayla zayıf parola
gönderildiğinde 400 beklenir. Bu denemelerin sonuçları ayrıca paylaşılmadı.

### Mevcut kodda kalan küçük düzenlemeler

- Kayıt hatası kodu `REGISTRATION_FAILED` olarak düzeltildi.
- AuthController'a `namespace TeamFlow.Api.Controllers;` eklendi.
- RegisterRequest'e DTO namespace'i ve controller'a ilgili using eklendi.
- Alan bazlı doğrulama, uzunluk sınırları ve ortak hata formatı henüz tamamlanmadı.

Bu üç düzenleme sonraki kullanıcı isteğiyle uygulama koduna da eklendi.

## 31. Güncel durum — register sonrası

- Kullanıcı modeli ve IdentityDbContext hazır.
- Identity servisleri kayıtlı.
- Register endpoint'i yazıldı; kullanıcı başarılı kayıt bildirdi.
- Login, logout, me, cookie/JWT oturumu, CSRF, rate limit ve endpoint yetkilendirmesi henüz yok.
- Ortak hata yönetimi ve frontend entegrasyonu eksik.

Sıradaki ders: Kayıt endpoint'indeki küçük düzenlemeler ve validasyon;
ardından login ve oturum yönetimi. Bölüm 24 önceki kontrol noktasını gösterir;
en son durum bu bölümdür.
