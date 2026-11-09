# TeamFlow — API sözleşmesi

Bu doküman frontend ile backend'in ortak anlaşmasıdır. Backend hazır olmadan
frontend aşağıdaki örnek cevapları mock veri olarak kullanabilir.

**Durum:** İlk taslak. Login endpoint'i henüz uygulanmadı; bu dosya mevcut
API davranışını değil, geliştirilecek davranışı tanımlar.

## Ortak kurallar

- API adresleri `/api` ile başlar.
- JSON alanları `camelCase` yazılır.
- ID değerleri UUID biçiminde string'dir.
- JSON istek ve cevaplarında `Content-Type: application/json` kullanılır.
- Frontend, kullanıcıya gösterilecek hata metnini `code` üzerinden çevirir.
- Parola hiçbir cevapta döndürülmez ve loglanmaz.
- Örnek kullanıcı ve parola yalnızca dokümantasyon içindir; hazır hesap değildir.

## Giriş yapma

### İstek

```http
POST /api/auth/login
Content-Type: application/json
```

```json
{
  "email": "samet@example.com",
  "password": "ornek-parola"
}
```

| Alan | Tip | Zorunlu | Kural |
|------|-----|---------|-------|
| `email` | string | Evet | Boş olamaz, geçerli e-posta biçiminde olmalıdır. |
| `password` | string | Evet | Boş olamaz. Parolanın başındaki/sonundaki boşluklar değiştirilmez. |

Login sırasında kayıt için kullanılan parola karmaşıklığı kuralları tekrar
uygulanmaz; gönderilen parola mevcut hesapla doğrulanır.

### Başarılı cevap — `200 OK`

```json
{
  "id": "bd814061-b43b-4a92-a28a-a3fe8cfcd580",
  "name": "Samet",
  "email": "samet@example.com",
  "siteRole": "USER"
}
```

| Alan | Tip | Açıklama |
|------|-----|----------|
| `id` | string (UUID) | Kullanıcının kimliği. |
| `name` | string | Görünen adı. |
| `email` | string | Hesabın e-posta adresi. |
| `siteRole` | string | `USER` veya `SITE_ADMIN`. Organizasyon rolünden ayrıdır. |

Backend, oturum bilgisini `Set-Cookie` başlığıyla HttpOnly ve Secure cookie
olarak ayarlar. Oturum token'ı JSON içinde döndürülmez veya localStorage'a
yazılmaz. Frontend cookie'yi JavaScript ile okumaz; sonraki API isteklerinde
tarayıcı gönderir. Cookie'nin teknik ayarları ve durum değiştiren isteklerin
CSRF koruması, auth uygulanırken ayrıca belgelenmelidir.

Frontend başarılı girişten sonra kullanıcı bilgisini uygulama durumuna alır
ve organizasyon listesine yönlendirir. Sayfa yenilendiğinde oturum kontrolü
için planlanan endpoint `GET /api/auth/me`'dir; onun sözleşmesi ayrıca eklenecek.

### Geçersiz bilgiler — `401 Unauthorized`

Hesap bulunamadığında ve parola yanlış olduğunda aynı cevap döndürülür.

```json
{
  "status": 401,
  "code": "INVALID_CREDENTIALS"
}
```

Frontend mesajı: “E-posta veya parola hatalı.”

### Alan hataları — `400 Bad Request`

Zorunlu alan eksikse, `null` ise veya boşsa alan için `REQUIRED` döner.
E-posta dolu fakat biçimi geçersizse `INVALID_EMAIL` döner.

```json
{
  "status": 400,
  "code": "VALIDATION_ERROR",
  "errors": {
    "email": ["REQUIRED"],
    "password": ["REQUIRED"]
  }
}
```

```json
{
  "status": 400,
  "code": "VALIDATION_ERROR",
  "errors": {
    "email": ["INVALID_EMAIL"]
  }
}
```

`errors` yalnızca hatalı alanları içerir. Frontend hataları ilgili input'un
altında gösterir. Geçersiz JSON veya alan tipleri için backend şu cevabı döner:

```json
{
  "status": 400,
  "code": "INVALID_REQUEST"
}
```

### Çok fazla deneme — `429 Too Many Requests`

```json
{
  "status": 429,
  "code": "RATE_LIMITED"
}
```

Backend `Retry-After` başlığında yeniden denemeden önce beklenecek saniyeyi
gönderir. Frontend bu süre boyunca tekrar denemeyi sınırlar.

### Beklenmeyen hata — `500 Internal Server Error`

```json
{
  "status": 500,
  "code": "INTERNAL_ERROR"
}
```

Frontend genel bir hata mesajı gösterir. Cevapta stack trace, SQL veya gizli
bilgi bulunmaz. Ağ hataları HTTP cevabı gelmeden de oluşabilir; frontend bunları
ayrı bir bağlantı hatası olarak ele almalıdır.

## Frontend geliştirme örnekleri

### Backend hazır olmadan: mock

Bu fonksiyon gerçek kimlik doğrulama yapmaz veya cookie oluşturmaz. Yalnızca
giriş ekranının başarılı ve hatalı cevaplarını geliştirmek içindir.

```ts
type LoginRequest = {
  email: string;
  password: string;
};

type LoginResponse = {
  id: string;
  name: string;
  email: string;
  siteRole: "USER" | "SITE_ADMIN";
};

type MockScenario = "success" | "invalidCredentials" | "validationError";

async function mockLogin(
  request: LoginRequest,
  scenario: MockScenario = "success"
): Promise<LoginResponse> {
  if (scenario === "invalidCredentials") {
    throw { status: 401, code: "INVALID_CREDENTIALS" };
  }

  if (scenario === "validationError") {
    throw {
      status: 400,
      code: "VALIDATION_ERROR",
      errors: { email: ["INVALID_EMAIL"] }
    };
  }

  return {
    id: "bd814061-b43b-4a92-a28a-a3fe8cfcd580",
    name: "Samet",
    email: request.email,
    siteRole: "USER"
  };
}
```

### Backend hazır olduğunda: gerçek istek

Aynı `LoginRequest` ve `LoginResponse` tipleri kullanılır. `/api` adresi yerel
geliştirmede frontend proxy'siyle backend'e yönlendirilmelidir; dağıtımda aynı
origin altında nginx üzerinden sunulması planlanır.

```ts
async function login(request: LoginRequest): Promise<LoginResponse> {
  const response = await fetch("/api/auth/login", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(request)
  });

  const data = await response.json();

  if (!response.ok) {
    throw data;
  }

  return data;
}
```

Bu örnek istek/cevap kullanımını gösterir. Auth uygulamasının seçtiği CSRF
mekanizması ek bir token veya başlık gerektirirse örnek de güncellenmelidir.

## Entegrasyon kontrol listesi

- [ ] Geçerli hesapla `200` ve belirtilen kullanıcı alanları dönüyor.
- [ ] Başarılı girişte oturum cookie'si ayarlanıyor.
- [ ] Bilinmeyen hesap ve yanlış parola aynı `401` cevabını veriyor.
- [ ] Boş/geçersiz alanlar belirtilen `400` formatında dönüyor.
- [ ] Geçersiz JSON ve alan tipleri `INVALID_REQUEST` döndürüyor.
- [ ] Fazla giriş denemesi `429` ve `Retry-After` döndürüyor.
- [ ] Frontend yüklenme, alan hatası, giriş hatası ve ağ hatasını gösteriyor.
- [ ] Frontend mock yerine gerçek API ile deneniyor.

## Sonraki sözleşmeler

1. `POST /api/auth/register`
2. `GET /api/auth/me`
3. `POST /api/auth/logout`
4. `GET /api/organizations`
5. `POST /api/organizations`

Her biri için istek, başarılı cevap, hata cevapları ve gerekli yetki eklenir.
Alan veya endpoint değişiklikleri frontend ile paylaşılır ve mock veriler de
aynı değişiklikle güncellenir.
