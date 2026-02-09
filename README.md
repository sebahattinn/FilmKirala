Açıklama yok kendine iyi bak 


# .NET tabanli bir film kiralama uygulamasi. Proje, modern yazilim mimarisi prensiplerini benimseyerek Clean Architecture, CQRS Pattern ve MediatR kutuphanesi uzerine insa edilmistir. Amac; okunabilir, test edilebilir, genisletilebilir ve bakimi kolay bir backend API sunmaktir.

---

## Proje Mimarisi

Proje, **Clean Architecture** (Temiz Mimari) yaklasimini benimser. Bagimlilik yonu her zaman iceriden disariya dogrudur. Yani Domain katmani hicbir seye bagimli degildir; Infrastructure katmani ise Domain ve Application katmanlarina bagimlidir.

### Katman Yapisi

```
FilmKirala/
|
|-- FilmKirala.Domain           --> En ic katman: Entity'ler, Enum'lar, Interface tanimlari
|-- FilmKirala.Application      --> Is kurallari: CQRS (Command/Query), Handler'lar, Validator'lar, DTO'lar
|-- FilmKirala.Infrastructure   --> Dis dunyayla iletisim: EF Core, Repository implementasyonlari, JWT, Serilog
|-- FilmKirala.Api              --> Sunum katmani: Controller'lar, Middleware'ler, Program.cs yapilandirmasi
|-- FilmKirala.sln              --> Solution dosyasi
```

Bu yapiyla her katmanin sorumlulugu nettir ve bir katmandaki degisiklik diger katmanlari dogrudan etkilemez. Ornegin veritabani teknolojisini degistirmek istediginizde sadece Infrastructure katmanini degistirmeniz yeterlidir; Application ve Domain katmanlarina dokunmaniza gerek kalmaz.

---

## Katmanlarin Detayli Aciklamasi

### FilmKirala.Domain

Projenin kalbidir. Hicbir dis kutupane bagimliligi yoktur (veya olmamalidir). Icerisinde su yapilar bulunur:

- **Entity'ler**: Film, Kullanici, Kiralama gibi veritabani tablolarina karsilik gelen siniflar. Projede DDD (Domain-Driven Design) prensipleri benimsendigi icin entity'lerin property'leri `private set` olarak tanimlanmistir. Bunun sebebi sudur: bir entity'nin state'i (durumu) disaridan dogrudan degistirilememeli, sadece entity'nin kendi metotlari uzerinden kontrollü bir sekilde degistirilmelidir. Bu yaklasim, is kurallarinin entity icinde kapsullenmesini (encapsulation) garanti eder ve "anemic domain model" anti-pattern'inden kacinilmasini saglar. Entity olusturma islemi constructor uzerinden, guncelleme islemleri ise anlamli isimlere sahip metotlar uzerinden yapilir.

Ornegin bir `Film` entity'si soyle gorunur:

```csharp
public class Film : BaseEntity
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public string Genre { get; private set; }

    // Entity olusturma islemi constructor uzerinden yapilir
    public Film(string title, string description, decimal price, int stock, string genre)
    {
        Title = title;
        Description = description;
        Price = price;
        Stock = stock;
        Genre = genre;
    }

    // State degisiklikleri anlamli metotlar uzerinden yapilir
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new DomainException("Fiyat sifirdan buyuk olmalidir.");
        Price = newPrice;
    }

    public void DecreaseStock(int quantity)
    {
        if (Stock < quantity)
            throw new DomainException("Yeterli stok bulunmamaktadir.");
        Stock -= quantity;
    }
}
```

Goruldugu gibi `Price` veya `Stock` gibi alanlar disaridan `film.Price = 50` seklinde degistirilemiyor. Bunun yerine `film.UpdatePrice(50)` veya `film.DecreaseStock(1)` gibi is kuralini icinde barindiran metotlar kullaniliyor. Bu sayede is kurallari (fiyat negatif olamaz, stok yetersizse kiralama yapilamaz gibi) entity icinde kalir, servislere veya handler'lara dagilmaz.

- **Base Entity**: Ortak alanlari (Id, CreatedDate, UpdatedDate gibi) tek bir yerde tanimlar. Bu, DRY (Don't Repeat Yourself) prensibinin guzel bir uygulamasidir. Base Entity'de de ayni sekilde `private set` kullanilir:

```csharp
public abstract class BaseEntity
{
    public int Id { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? UpdatedDate { get; private set; }

    protected void SetUpdatedDate()
    {
        UpdatedDate = DateTime.UtcNow;
    }
}
```

- **Interface Tanimlari**: Repository interface'leri burada tanimlanir. Ornegin `IFilmRepository`, `IUnitOfWork` gibi. Bu sayede Application katmani somut implementasyona degil, soyutlamaya bagimli olur (Dependency Inversion Principle).

```csharp
public interface IFilmRepository : IGenericRepository
{
    Task<List> GetFilmsByGenreAsync(string genre);
}
```

### FilmKirala.Application

Is mantigi ve CQRS yapisinin yasadigi katmandir. Burada MediatR kutuphanesi kullanilarak Command ve Query'ler birbirinden ayrilmistir.

- **Commands**: Veri degistiren islemler (Create, Update, Delete). Her command bir `IRequest<T>` implement eder ve kendi Handler sinifina sahiptir.

```csharp
// Command tanimlamasi
public class CreateFilmCommand : IRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Genre { get; set; }
}

// Handler tanimlamasi
public class CreateFilmCommandHandler : IRequestHandler
{
    private readonly IFilmRepository _filmRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFilmCommandHandler(IFilmRepository filmRepository, IUnitOfWork unitOfWork)
    {
        _filmRepository = filmRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CreateFilmCommand request, CancellationToken cancellationToken)
    {
        // private set oldugu icin object initializer degil, constructor kullanilir
        var film = new Film(
            request.Title,
            request.Description,
            request.Price,
            request.Stock,
            request.Genre
        );

        await _filmRepository.AddAsync(film);
        await _unitOfWork.SaveChangesAsync();
        return film.Id;
    }
}
```

- **Queries**: Sadece veri okuyan islemler (GetAll, GetById, Search). Yazma ve okuma islemlerinin ayrilmasi CQRS'in temel felsefesidir.

```csharp
public class GetAllFilmsQuery : IRequest<List>
{
}

public class GetAllFilmsQueryHandler : IRequestHandler>
{
    private readonly IFilmRepository _filmRepository;
    private readonly IMapper _mapper;

    public async Task<List> Handle(GetAllFilmsQuery request, CancellationToken cancellationToken)
    {
        var films = await _filmRepository.GetAllAsync();
        return _mapper.Map<List>(films);
    }
}
```

- **Validators**: FluentValidation kutuphanesi ile her command icin ayri validasyon kurallari tanimlanir. Bu sayede validasyon mantigi handler'in icine karistirilmaz.

```csharp
public class CreateFilmCommandValidator : AbstractValidator
{
    public CreateFilmCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Film adi bos olamaz.")
            .MaximumLength(200).WithMessage("Film adi 200 karakterden uzun olamaz.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Fiyat sifirdan buyuk olmalidir.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stok negatif olamaz.");
    }
}
```

- **DTOs ve Mapping Profilleri**: Entity'lerin dogrudan disariya acilmasi yerine DTO'lar (Data Transfer Object) kullanilir. AutoMapper ile entity-DTO donusumleri yapilir.

### FilmKirala.Infrastructure

Dis dunyayla olan tum iletisim bu katmanda gerceklesir:

- **Entity Framework Core**: Veritabani islemleri icin ORM olarak kullanilir. `AppDbContext` sinifi burada tanimlidir.
- **Repository Pattern Implementasyonlari**: Domain katmanindaki interface'lerin somut karsiliklari burada yazilir. Generic Repository kullanilarak tekrar eden CRUD kodlari minimize edilmistir.

```csharp
public class GenericRepository : IGenericRepository where T : BaseEntity
{
    protected readonly AppDbContext _context;
    private readonly DbSet _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set();
    }

    public async Task GetByIdAsync(int id) => await _dbSet.FindAsync(id);
    public async Task<List> GetAllAsync() => await _dbSet.ToListAsync();
    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
    public void Update(T entity) => _dbSet.Update(entity);
    public void Delete(T entity) => _dbSet.Remove(entity);
}
```

- **Unit of Work Pattern**: Birden fazla repository islemini tek bir transaction altinda toplamak icin kullanilir.
- **JWT (JSON Web Token)**: Kullanici kimlik dogrulama ve yetkilendirme islemleri JWT token mekanizmasi ile saglenir.
- **Serilog**: Yapilandirilmis (structured) loglama icin kullanilir. Console, dosya veya baska sink'lere log yazilabilir.

### FilmKirala.Api

Kullanicinin (veya frontend uygulamanin) dogrudan muhatap oldugu katmandir:

- **Controller'lar**: HTTP isteklerini karsilayip MediatR uzerinden ilgili handler'a yonlendirir. Controller'larin icinde is mantigi yoktur; sadece `_mediator.Send()` cagrisi yapilir. Bu, Controller'lari ince ve sorumluluksuz (thin controller) tutar.

```csharp
[ApiController]
[Route("api/[controller]")]
public class FilmsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FilmsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task Create([FromBody] CreateFilmCommand command)
    {
        var result = await _mediator.Send(command);
        return Created("", result);
    }

    [HttpGet]
    public async Task GetAll()
    {
        var result = await _mediator.Send(new GetAllFilmsQuery());
        return Ok(result);
    }
}
```

- **Middleware'ler**: Global exception handling, loglama gibi cross-cutting concern'ler burada ele alinir.
- **Program.cs**: Tum servislerin DI Container'a kaydedildigi, pipeline'in yapilandirildigi giris noktasidir.

---

## Kullanilan Design Pattern'ler

### 1. CQRS (Command Query Responsibility Segregation)

Projenin bel kemigi olan pattern budur. Yazma (Command) ve okuma (Query) islemleri farkli siniflar uzerinden yurutulur. Bunun sagladigi avantajlar sunlardir:

- Okuma ve yazma islemleri birbirinden bagimsiz olarak optimize edilebilir.
- Her islemin kendi sorumluluklari net bir sekilde bellidir.
- Buyuyen projelerde karmasiklik kontrol altinda tutulur.

Projede her Command ve Query, MediatR'in `IRequest<T>` interface'ini implement eder. Karsilik gelen Handler ise `IRequestHandler<TRequest, TResponse>` interface'ini implement eder. MediatR, araya giren bir Mediator gorevi gorur ve request'i dogru handler'a yonlendirir.

### 2. Mediator Pattern (MediatR)

MediatR kutuphanesi ile Controller'lar dogrudan servis siniflarini cagirmak yerine bir "araci" uzerinden iletisim kurar. Bu sayede:

- Controller'lar sadece `IMediator` interface'ine bagimlidir, baska hicbir servisi bilmez.
- Yeni bir ozellik eklemek icin mevcut kodu degistirmek gerekmez; yeni bir Command/Query ve Handler eklenir (Open/Closed Principle).
- Pipeline Behavior ozelligi sayesinde loglama, validasyon, caching gibi cross-cutting concern'ler handler'dan once veya sonra calistirilabilir.

### 3. Repository Pattern

Veritabani erisim mantigi, is mantigi katmanindan soyutlanmistir. Application katmani `IFilmRepository` gibi interface'lerle calisir, somut implementasyonu bilmez. Bu yaklasimdaki kazanimlar:

- Veritabani teknolojisi degistirilebilir (EF Core yerine Dapper, MongoDB vs.) sadece Infrastructure katmanini degistirerek.
- Unit test yazarken repository'ler kolayca mock'lanabilir.

### 4. Unit of Work Pattern

Birden fazla repository islemini tek bir `SaveChanges` cagrisi altinda gruplar. Boylece ya hepsi basarili olur ya da hicbiri veritabanina yansimaz (atomicity).

### 5. Dependency Injection (Built-in .NET DI)

Projedeki tum bagimliliklar constructor injection ile cozumlenir. `Program.cs` dosyasinda servisler DI Container'a kaydedilir. Bu sayede siniflar birbirine dogrudan bagimli degil, soyutlamalar (interface'ler) uzerinden gevse baglidir (loose coupling).

---

## SOLID Prensiplerinin Uygulanisi

### S - Single Responsibility Principle (Tek Sorumluluk)

Her sinifin tek bir sorumluluklugu vardir. `CreateFilmCommandHandler` sadece film olusturma isini yapar. `CreateFilmCommandValidator` sadece validasyon isini yapar. Controller sadece HTTP istegini karsilayip mediator'a iletir. Hicbir sinif birden fazla is yapmaz.

### O - Open/Closed Principle (Acik/Kapali)

Yeni bir ozellik eklemek icin mevcut kodda degisiklik yapmaya gerek yoktur. Ornegin yeni bir film sorgulama tipi eklemek istediginizde mevcut handler'lara dokunmadan yeni bir Query ve Handler sinifi olusturursunuz. MediatR, yeni handler'i otomatik olarak tanir.

### L - Liskov Substitution Principle

GenericRepository'den tureterek olusturulan `FilmRepository` gibi siniflar, ust sinifin yerine sorunsuzca kullanilabilir. Interface'ler uzerinden calisan kod, hangi somut implementasyonun geldigini bilmez ve bilmek zorunda degildir.

### I - Interface Segregation Principle (Arayuz Ayrimi)

`IGenericRepository` temel CRUD islemlerini tanimlar. Eger bir entity'ye ozel sorgular gerekiyorsa, `IFilmRepository` gibi ayrı bir interface olusturulur ve GenericRepository'yi extend eder. Boylece istemciler kullanmadiklari metodlara bagimli olmazlar.

### D - Dependency Inversion Principle (Bagimlilik Tersine Cevirme)

Application katmani, Infrastructure katmanindaki somut siniflarla degil, Domain katmanindaki interface'lerle calisir. Ornegin `CreateFilmCommandHandler`, `IFilmRepository` interface'ine bagimlidir; `FilmRepository` sinifina degil. Somut implementasyon, DI Container uzerinden calisma zamaninda enjekte edilir.

---

## Kullanilan Kutuphane ve Extension'lar

### MediatR

Mediator pattern implementasyonu. Command ve Query nesnelerini ilgili handler'lara yonlendirmek icin kullanilir. Pipeline Behavior destegiyle validasyon, loglama gibi islemler handler zincirinin icine eklenir.

**NuGet Paketi**: `MediatR`, `MediatR.Extensions.Microsoft.DependencyInjection`

### FluentValidation

Kurallar zincirleme (fluent) bir API ile tanimlanir. Her Command icin ayri bir Validator sinifi olusturulur. MediatR Pipeline Behavior ile entegre edildiginde, handler calismadan once otomatik validasyon yapilir. Validasyon basarisiz olursa handler'a hic ulasilmaz.

**NuGet Paketi**: `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`

### JWT (JSON Web Token)

Kullanici giris islemlerinden sonra bir token uretilir. Bu token, sonraki isteklerde `Authorization` header'inda gonderilir. API tarafinda token dogrulanir ve kullanicinin kim oldugu, hangi rollere sahip oldugu belirlenir.

**NuGet Paketi**: `Microsoft.AspNetCore.Authentication.JwtBearer`

### Serilog

Yapilandirilmis (structured) loglama kutuphanesidir. .NET'in varsayilan `ILogger` implementasyonunun yerine gecebilir. Farkli cikis noktalarina (console, dosya, Seq, Elasticsearch vb.) log yazmak icin sink'ler eklenir.

**NuGet Paketi**: `Serilog.AspNetCore`, `Serilog.Sinks.Console`, `Serilog.Sinks.File`

### Entity Framework Core

Microsoft'un ORM kutuphanesidir. Code-First yaklasimi ile entity siniflari uzerinden veritabani tablolari olusturulur ve yonetilir. Migration destegi sayesinde veritabani sema degisiklikleri versiyonlanabilir.

**NuGet Paketi**: `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer` (veya kullanilan provider)

### AutoMapper

Entity ve DTO siniflari arasindaki donusumleri otomatiklestirir. Mapping profilleri tanimlayarak tekrar eden `entity.Property = dto.Property` satirlarindan kurtulunur.

**NuGet Paketi**: `AutoMapper`, `AutoMapper.Extensions.Microsoft.DependencyInjection`

---

## Clean Code Degerlendirmesi

Projede clean code acisindan dikkat ceken olumlu noktalar sunlardir:

**Anlamli isimlendirmeler**: Sinif, metot ve degisken isimleri ne yaptiklarina dair net bilgi verir. `CreateFilmCommand`, `GetAllFilmsQuery`, `FilmRepository` gibi isimler kodu okuyan kisinin ekstra dokumantasyona ihtiyac duymadan ne oldugunu anlamasini saglar.

**Kucuk ve odakli siniflar**: Her sinif tek bir is yapar. Handler siniflari genellikle tek bir `Handle` metoduna sahiptir. Bu, kodun okunabilirligini ve test edilebilirligini arttirir.

**Katmanli yapi**: Sorumluluklar katmanlara dagitilmistir. API katmaninda is mantigi yoktur, Application katmaninda veritabani erisim kodu yoktur. Her sey kendi yerindedir.

**Tekrar eden kodun onlenmesi**: Generic Repository ile temel CRUD islemleri tek bir yerde tanimlanmistir. Base Entity ile ortak alanlar tekrarlanmamistir.

**Konfigurasyonun merkezlesmesi**: Tum servis kayitlari `Program.cs` veya extension metotlari uzerinden yapilandirilir. Konfigrasyon dagitilmis degil, merkezi bir noktadadir.

---

## Projeyi Calistirma

### On Kosullar

- .NET 6.0 SDK (veya projenin hedefledigi SDK versiyonu)
- SQL Server (veya yapilandirmadaki veritabani)
- Visual Studio 2022 / VS Code / Rider

### Adimlar

```bash
# Repoyu klonla
git clone https://github.com/sebahattinn/FilmKirala.git
cd FilmKirala

# Bagimliliklari yukle
dotnet restore

# Veritabani migration'larini uygula
dotnet ef database update --project FilmKirala.Infrastructure --startup-project FilmKirala.Api

# Uygulamayi calistir
dotnet run --project FilmKirala.Api
```

Uygulama ayaga kalktiktan sonra Swagger arayuzune `https://localhost:{port}/swagger` adresinden ulasabilirsiniz.

### appsettings.json Yapilandirmasi

`appsettings.json` dosyasinda su ayarlari kendi ortaminiza gore guncelleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=FilmKiralaDb;Trusted_Connection=true;"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-here",
    "Issuer": "FilmKirala",
    "Audience": "FilmKirala",
    "ExpirationInMinutes": 60
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/log-.txt", "rollingInterval": "Day" } }
    ]
  }
}
```

---

## Proje Yapisi Ozet Tablosu

| Katman | Sorumluluk | Bagimlilik Yonu |
|--------|-----------|-----------------|
| Domain | Entity'ler, Interface tanimlari, Enum'lar | Hicbir katmana bagimli degil |
| Application | CQRS (Command/Query/Handler), Validator, DTO, Mapping | Sadece Domain'e bagimli |
| Infrastructure | EF Core, Repository impl., JWT, Serilog, Dis servisler | Domain ve Application'a bagimli |
| Api | Controller, Middleware, DI yapilandirmasi | Tum katmanlara bagimli (Composition Root) |

---

## Katkida Bulunma

1. Bu repoyu fork'layin.
2. Yeni bir branch olusturun: `git checkout -b feature/yeni-ozellik`
3. Degisikliklerinizi commit'leyin: `git commit -m "Yeni ozellik eklendi"`
4. Branch'inizi push'layin: `git push origin feature/yeni-ozellik`
5. Pull Request acin.

---

## Lisans

Bu proje acik kaynak olarak paylasilmistir. Detaylar icin repo sahibiyle iletisime gecebilirsiniz.

---

*Halil Abi'ye sevgilerle hazirlanmistir.*
