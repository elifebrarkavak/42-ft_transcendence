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

Henüz yapmadıklarımız:

- Silme metodunu mevcut controller'a ekleme (örneği anlatıldı; dosyada henüz yok).
- Migration oluşturma/uygulama ve API üzerinden veritabanı bağlantısını sınama.
- Controller'daki bellek listesini veritabanıyla değiştirme.
- Kayıt, giriş ve yetkilendirme.

Sonraki adım: Migration ile görev tablosunu oluşturmak. Silme örneği anlatıldı
ancak son dosya kontrolünde DELETE metodu henüz controller'da bulunmuyordu.

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

## 15. Görev silme — anlatılan sonraki adım

**Durum:** Not güncellenirken `TaskController.cs` içinde bu metot henüz yoktu.
Aşağıdaki örnek derste anlatıldı; uygulanıp denenmesi gerekiyor.

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
| DELETE | `/api/tasks/{id}` | Görev silme | Henüz değil; örneği anlatıldı |

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
sağlanmasıdır. Sonraki adımlarda controller'a `AppDbContext` vereceğiz.

Kullanıcı `dotnet build` işleminin başarılı olduğunu bildirdi. Bu, kodun
derlendiğini gösterir; bağlantı ayarının doğru olduğunu veya tablo oluştuğunu
kanıtlamaz. Henüz migration uygulanmadı; controller hâlâ `static List<TaskItem>`
kullanıyor ve görevler uygulama yeniden başlayınca siliniyor.

Sıradaki işlem: EF CLI ile migration oluşturup `teamflow` veritabanına uygulamak.
