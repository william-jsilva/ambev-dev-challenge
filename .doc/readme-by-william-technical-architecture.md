# Arquitetura Técnica Detalhada

## 📋 Índice

1. [Visão Geral da Arquitetura](#visão-geral-da-arquitetura)
2. [Padrões Arquiteturais](#padrões-arquiteturais)
3. [Camadas da Aplicação](#camadas-da-aplicação)
4. [Fluxo de Dados](#fluxo-de-dados)
5. [Segurança](#segurança)
6. [Performance e Cache](#performance-e-cache)
7. [Monitoramento e Observabilidade](#monitoramento-e-observabilidade)
8. [Testes](#testes)
9. [Deploy e Infraestrutura](#deploy-e-infraestrutura)

---

## 🏗️ Visão Geral da Arquitetura

### Arquitetura de Microserviços

O projeto segue uma arquitetura de microserviços bem definida, onde cada API é responsável por um domínio específico:

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   IdentityApi   │    │   ProductApi    │    │    CartApi      │    │    SaleApi      │
│                 │    │                 │    │                 │    │                 │
│ • Autenticação  │    │ • Produtos      │    │ • Carrinhos     │    │ • Vendas        │
│ • Usuários      │    │ • Categorias    │    │ • Itens         │    │ • Regras de     │
│ • JWT Tokens    │    │ • Cache Redis   │    │ • Cálculos      │    │   Negócio       │
│                 │    │                 │    │                 │    │ • Eventos       │
└─────────────────┘    └─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │                       │
         │                       │                       │                       │
         ▼                       ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   PostgreSQL    │    │    MongoDB      │    │   PostgreSQL    │    │   PostgreSQL    │
│   (Identity)    │    │   + Redis       │    │    (Cart)       │    │    (Sale)       │
└─────────────────┘    └─────────────────┘    └─────────────────┘    └─────────────────┘
                                                                                │
                                                                                │
                                                                                ▼
                                               ┌─────────────────────────────────────────┐
                                               │              RabbitMQ                   │
                                               │         (Message Broker)               │
                                               │  ┌─────────────────────────────────────┐ │
                                               │  │ • SaleCreatedEvent                  │ │
                                               │  │ • SaleModifiedEvent                 │ │
                                               │  │ • SaleCancelledEvent                │ │
                                               │  │ • ItemCancelledEvent                │ │
                                               │  └─────────────────────────────────────┘ │
                                               └─────────────────────────────────────────┘
```

### Princípios Arquiteturais

1. **Separação de Responsabilidades**: Cada API tem uma responsabilidade específica
2. **Independência**: APIs podem ser desenvolvidas e deployadas independentemente
3. **Escalabilidade**: Cada serviço pode ser escalado independentemente
4. **Resiliência**: Falha em um serviço não afeta outros
5. **Observabilidade**: Monitoramento e logs centralizados

---

## 🎯 Padrões Arquiteturais

### 1. Domain-Driven Design (DDD)

#### Estratégias DDD Implementadas

##### Bounded Contexts
- **Identity Context**: Usuários e autenticação
- **Product Context**: Catálogo de produtos
- **Cart Context**: Carrinhos de compra
- **Sale Context**: Vendas e regras de negócio

##### Entidades de Domínio

```csharp
// SaleApi - Entidade Principal
public class Sale : BaseEntity
{
    public long SaleNumber { get; set; }
    public decimal TotalSaleAmount { get; set; }
    public string Branch { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset Date { get; set; }
    public SaleStatus Status { get; set; }
    public List<SaleProduct> Products { get; set; }
    
    // Métodos de Domínio
    public void CalculateTotalSaleAmount() { ... }
    public void DefineDeleted() { ... }
    public bool IsCompleted() { ... }
    public ValidationResultDetail Validate() { ... }
}
```

##### Value Objects
```csharp
public class SaleNumber : ValueObject
{
    public long Value { get; }
    
    public SaleNumber(long value)
    {
        if (value <= 0)
            throw new DomainException("Sale number must be positive");
        Value = value;
    }
}
```

##### Repositórios
```csharp
public interface ISaleRepository : IRepository<Sale>
{
    Task<Sale> GetBySaleNumberAsync(long saleNumber);
    Task<IEnumerable<Sale>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Sale>> GetByStatusAsync(SaleStatus status);
}
```

### 2. Clean Architecture

#### Camadas da Arquitetura

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐        │
│  │ Controllers │  │   DTOs      │  │ Middleware  │        │
│  └─────────────┘  └─────────────┘  └─────────────┘        │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                         │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐        │
│  │  Commands   │  │   Queries   │  │  Handlers   │        │
│  └─────────────┘  └─────────────┘  └─────────────┘        │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                     Domain Layer                            │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐        │
│  │  Entities   │  │  Services   │  │ Repositories│        │
│  └─────────────┘  └─────────────┘  └─────────────┘        │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐        │
│  │   ORM/EF    │  │   External  │  │   Logging   │        │
│  │             │  │   Services  │  │             │        │
│  └─────────────┘  └─────────────┘  └─────────────┘        │
└─────────────────────────────────────────────────────────────┘
```

### 3. CQRS (Command Query Responsibility Segregation)

#### Commands (Escrita)
```csharp
public class CreateSaleCommand : IRequest<CreateSaleResponse>
{
    public Guid UserId { get; set; }
    public string Branch { get; set; }
    public List<SaleProductRequest> Products { get; set; }
}

public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, CreateSaleResponse>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    
    public async Task<CreateSaleResponse> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        // Lógica de criação da venda
        var createdSale = await saleRepository.CreateAsync(sale, cancellationToken);
        
        // Publicar evento de domínio
        await eventPublisher.PublishAsync(new SaleCreatedEvent(createdSale), cancellationToken);
        
        return result;
    }
}
```

#### Queries (Leitura)
```csharp
public class GetSaleQuery : IRequest<GetSaleResponse>
{
    public Guid Id { get; set; }
}

public class GetSaleQueryHandler : IRequestHandler<GetSaleQuery, GetSaleResponse>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    
    public async Task<GetSaleResponse> Handle(GetSaleQuery request, CancellationToken cancellationToken)
    {
        // Lógica de busca da venda
    }
}
```

### 4. Mediator Pattern

#### Implementação com MediatR
```csharp
// Registro no DI Container
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(ApplicationLayer).Assembly,
        typeof(Program).Assembly
    );
});

// Pipeline Behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Uso no Controller
public class SalesController : BaseController
{
    private readonly IMediator _mediator;
    
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequest request)
    {
        var command = _mapper.Map<CreateSaleCommand>(request);
        var response = await _mediator.Send(command);
        return Created(string.Empty, response);
    }
}
```

### 5. Event-Driven Architecture (EDA) com Rebus

#### SaleApi e RabbitMQ

A **SaleApi** é a **única API** que utiliza o **RabbitMQ** como message broker através do **Rebus** para processar eventos de domínio de forma assíncrona. Esta arquitetura permite:

- **Desacoplamento**: Eventos são processados independentemente da transação principal
- **Escalabilidade**: Múltiplos workers podem processar eventos simultaneamente
- **Resiliência**: Retry automático e Dead Letter Queue para eventos que falharam
- **Auditoria**: Rastreamento completo de todas as operações de venda

**Nota**: As outras APIs (ProductApi, CartApi, IdentityApi) não utilizam message brokers e operam de forma síncrona.

#### Implementação de Eventos de Domínio com Rebus
```csharp
// Interface do Publisher
public interface IEventPublisher
{
    Task PublishAsync(object @event, CancellationToken cancellationToken = default);
}

// Implementação com Rebus (Service Bus)
public class RebusEventPublisher : IEventPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<RebusEventPublisher> _logger;
    
    public RebusEventPublisher(IBus bus, ILogger<RebusEventPublisher> logger)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task PublishAsync(object @event, CancellationToken cancellationToken = default)
    {
        try
        {
            await _bus.Publish(@event);
            _logger.LogInformation("Event published via Rebus: {EventType}", @event.GetType().Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event via Rebus: {EventType}", @event.GetType().Name);
            throw;
        }
    }
}
```

#### Configuração do Rebus na SaleApi
```csharp
// SaleApi/Program.cs - Configuração do Rebus
public static class RebusExtensions
{
    public static IServiceCollection AddRebusServiceBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRebus(configure => configure
            .Logging(l => l.Serilog())
            .Transport(t => t.UseRabbitMq(
                configuration.GetConnectionString("RabbitMQ"), 
                "ambev-sales-queue"))
            .Routing(r => r.TypeBased()
                .Map<SaleCreatedEvent>("ambev-sales-events")
                .Map<SaleModifiedEvent>("ambev-sales-events")
                .Map<SaleCancelledEvent>("ambev-sales-events")
                .Map<ItemCancelledEvent>("ambev-sales-events"))
            .Options(o => o
                .SetNumberOfWorkers(5)
                .SetMaxParallelism(10)
                .SetRetryStrategy(3))
        );
        
        return services;
    }
}

// Registro no DI Container da SaleApi
builder.Services.AddRebusServiceBus(builder.Configuration);
builder.Services.AddScoped<IEventPublisher, RebusEventPublisher>();

// Configuração do appsettings.json da SaleApi
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=db_sale;User Id=developer;Password=ev@luAt10n;TrustServerCertificate=True",
    "RabbitMQ": "amqp://developer:evaluAt10n@localhost:5672/"
  }
}
```

#### Event Handlers com Rebus
```csharp
// Handler para SaleCreatedEvent
public class SaleCreatedEventHandler : IHandleMessages<SaleCreatedEvent>
{
    private readonly ILogger<SaleCreatedEventHandler> _logger;

    public SaleCreatedEventHandler(ILogger<SaleCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(SaleCreatedEvent message)
    {
        _logger.LogInformation("Processing SaleCreatedEvent for sale {SaleId}", message.Sale.Id);
        
        // Lógica de processamento
        await ProcessSaleCreated(message.Sale);
    }

    private async Task ProcessSaleCreated(Sale sale)
    {
        // Implementar lógica de negócio
        // - Enviar email de confirmação
        // - Atualizar estoque
        // - Gerar relatório
        // - etc.
    }
}

// Handler para SaleCancelledEvent
public class SaleCancelledEventHandler : IHandleMessages<SaleCancelledEvent>
{
    private readonly ILogger<SaleCancelledEventHandler> _logger;

    public SaleCancelledEventHandler(ILogger<SaleCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(SaleCancelledEvent message)
    {
        _logger.LogInformation("Processing SaleCancelledEvent for sale {SaleId}", message.Sale.Id);
        
        // Lógica de processamento
        await ProcessSaleCancelled(message.Sale);
    }

    private async Task ProcessSaleCancelled(Sale sale)
    {
        // Implementar lógica de negócio
        // - Enviar email de cancelamento
        // - Restaurar estoque
        // - Atualizar relatórios
        // - etc.
    }
}
```

#### Eventos de Domínio
```csharp
// SaleApi Events
public class SaleCreatedEvent
{
    public Sale Sale { get; }
    public SaleCreatedEvent(Sale sale) => Sale = sale;
}

public class SaleModifiedEvent
{
    public Sale Sale { get; }
    public SaleModifiedEvent(Sale sale) => Sale = sale;
}

public class SaleCancelledEvent
{
    public Sale Sale { get; }
    public SaleCancelledEvent(Sale sale) => Sale = sale;
}

public class ItemCancelledEvent
{
    public SaleProduct SaleProduct { get; }
    public Sale Sale { get; }
    public ItemCancelledEvent(SaleProduct saleProduct, Sale sale)
    {
        SaleProduct = saleProduct;
        Sale = sale;
    }
}

// IdentityApi Events
public class UserRegisteredEvent
{
    public User User { get; }
    public UserRegisteredEvent(User user) => User = user;
}
```

#### Uso nos Handlers
```csharp
// CreateSaleHandler
public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
{
    // ... lógica de criação da venda
    
    var createdSale = await saleRepository.CreateAsync(sale, cancellationToken);
    
    // Publicar evento de domínio
    await eventPublisher.PublishAsync(new SaleCreatedEvent(createdSale), cancellationToken);
    
    return result;
}

// DeleteSaleHandler
public async Task<DeleteSaleResponse> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
{
    // ... lógica de cancelamento da venda
    
    // Publicar evento de domínio
    await eventPublisher.PublishAsync(new SaleCancelledEvent(sale), cancellationToken);
    
    return response;
}
```

---

## 🏛️ Camadas da Aplicação

### 1. Presentation Layer (WebApi)

#### Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
public class SalesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    
    public SalesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateSaleResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequest request)
    {
        // Implementação
    }
}
```

#### DTOs (Data Transfer Objects)
```csharp
public class CreateSaleRequest
{
    public Guid UserId { get; set; }
    public string Branch { get; set; }
    public List<SaleProductRequest> Products { get; set; }
}

public class CreateSaleResponse
{
    public Guid Id { get; set; }
    public long SaleNumber { get; set; }
    public decimal TotalSaleAmount { get; set; }
    public string Branch { get; set; }
    public DateTimeOffset Date { get; set; }
    public SaleStatus Status { get; set; }
}
```

#### Middleware
```csharp
public class ValidationExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationExceptionMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
    }
}
```

### 2. Application Layer

#### Commands e Queries
```csharp
// Commands
public class CreateSaleCommand : IRequest<CreateSaleResponse>
public class UpdateSaleCommand : IRequest<UpdateSaleResponse>
public class DeleteSaleCommand : IRequest<Unit>

// Queries
public class GetSaleQuery : IRequest<GetSaleResponse>
public class ListSalesQuery : IRequest<ListSalesResponse>
```

#### Handlers
```csharp
public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, CreateSaleResponse>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    public async Task<CreateSaleResponse> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        // 1. Validação de negócio
        // 2. Criação da entidade
        // 3. Persistência
        // 4. Retorno da resposta
    }
}
```

#### Mapeamento (AutoMapper)
```csharp
public class SaleMappingProfile : Profile
{
    public SaleMappingProfile()
    {
        CreateMap<CreateSaleRequest, CreateSaleCommand>();
        CreateMap<Sale, CreateSaleResponse>();
        CreateMap<SaleProduct, SaleProductResponse>();
        CreateMap<UpdateSaleRequest, UpdateSaleCommand>();
    }
}
```

### 3. Domain Layer

#### Entidades
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

public class Sale : BaseEntity
{
    public long SaleNumber { get; set; }
    public decimal TotalSaleAmount { get; set; }
    public string Branch { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset Date { get; set; }
    public SaleStatus Status { get; set; }
    public List<SaleProduct> Products { get; set; } = [];
    
    // Métodos de domínio
    public void CalculateTotalSaleAmount()
    {
        foreach (var product in Products)
        {
            product.CalculateDiscount();
            product.CalculateTotalAmount();
        }
        TotalSaleAmount = Products.Sum(product => product.TotalAmount);
    }
}
```

#### Serviços de Domínio
```csharp
public interface ISaleDomainService
{
    Task<bool> CanCreateSaleAsync(Guid userId);
    Task<long> GenerateSaleNumberAsync();
    Task ValidateSaleAsync(Sale sale);
}

public class SaleDomainService : ISaleDomainService
{
    private readonly ISaleRepository _saleRepository;
    
    public async Task<bool> CanCreateSaleAsync(Guid userId)
    {
        var activeSale = await _saleRepository.GetActiveByUserIdAsync(userId);
        return activeSale == null;
    }
}
```

#### Repositórios
```csharp
public interface ISaleRepository : IRepository<Sale>
{
    Task<Sale> GetBySaleNumberAsync(long saleNumber);
    Task<Sale> GetActiveByUserIdAsync(Guid userId);
    Task<IEnumerable<Sale>> GetByStatusAsync(SaleStatus status);
    Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTimeOffset start, DateTimeOffset end);
}
```

### 4. Infrastructure Layer

#### Entity Framework (ORM)
```csharp
public class DefaultContext : DbContext
{
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleProduct> SaleProducts { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DefaultContext).Assembly);
    }
}
```

#### Configurações de Mapeamento
```csharp
public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SaleNumber).IsRequired();
        builder.Property(x => x.TotalSaleAmount).HasPrecision(18, 2);
        builder.Property(x => x.Branch).HasMaxLength(100).IsRequired();
        
        builder.HasMany(x => x.Products)
               .WithOne(x => x.Sale)
               .HasForeignKey(x => x.SaleId);
    }
}
```

#### Repositórios Implementados
```csharp
public class SaleRepository : Repository<Sale>, ISaleRepository
{
    public SaleRepository(DefaultContext context) : base(context)
    {
    }
    
    public async Task<Sale> GetBySaleNumberAsync(long saleNumber)
    {
        return await _context.Sales
            .Include(x => x.Products)
            .FirstOrDefaultAsync(x => x.SaleNumber == saleNumber);
    }
}
```

---

## 🔄 Fluxo de Dados

### Fluxo de Criação de Venda na SaleApi com RabbitMQ

```
1. HTTP Request → SaleApi Controller
   ↓
2. Request Validation (FluentValidation)
   ↓
3. Mapping (AutoMapper) Request → Command
   ↓
4. MediatR Pipeline → ValidationBehavior
   ↓
5. Command Handler
   ↓
6. Domain Service (Business Rules)
   ↓
7. Entity Creation & Validation
   ↓
8. Repository (EF Core)
   ↓
9. Database (PostgreSQL)
   ↓
10. Event Publishing (Rebus)
    ↓
11. RabbitMQ Queue (ambev-sales-queue)
    ↓
12. Event Handlers (Assíncrono)
    ↓
13. Response Mapping
    ↓
14. HTTP Response
```

### Fluxo de Processamento de Eventos da SaleApi com RabbitMQ
```
1. SaleApi Event Published → RebusEventPublisher
   ↓
2. RabbitMQ Queue (ambev-sales-queue)
   ↓
3. Event Handlers (Paralelo)
   ├── SaleCreatedEventHandler
   ├── EmailNotificationHandler
   ├── InventoryUpdateHandler
   └── ReportGenerationHandler
   ↓
4. Processamento Assíncrono
   ↓
5. Logs e Métricas
```

### Fluxo de Consulta de Produtos na ProductApi (com Cache Redis)

```
1. HTTP Request → ProductApi Controller
   ↓
2. Cache Check (Redis)
   ↓
3. Cache Hit? → Return Cached Data
   ↓
4. Cache Miss → Query Handler
   ↓
5. Repository (MongoDB)
   ↓
6. Database Query
   ↓
7. Cache Storage (Redis)
   ↓
8. HTTP Response
```

### Comparação de Message Brokers por API

| API | Message Broker | Propósito | Tecnologia |
|-----|----------------|-----------|------------|
| **SaleApi** | **RabbitMQ** | Eventos de domínio (vendas) | Rebus + RabbitMQ |
| **ProductApi** | **Redis** | Cache de produtos | StackExchange.Redis |
| **CartApi** | - | Sem message broker | - |
| **IdentityApi** | - | Sem message broker | - |

#### SaleApi - RabbitMQ (Eventos de Domínio)
A **SaleApi** é a **única API** que publica eventos no RabbitMQ através do Rebus:

- **SaleCreatedEvent**: Disparado na criação de vendas
- **SaleModifiedEvent**: Disparado na modificação de vendas
- **SaleCancelledEvent**: Disparado no cancelamento de vendas
- **ItemCancelledEvent**: Disparado no cancelamento de itens

#### ProductApi - Redis (Cache)
- Cache de listagem de produtos
- Cache de produtos por categoria
- Cache de produtos por ID
- TTL configurável para invalidação automática

#### CartApi e IdentityApi
- Não utilizam message brokers
- Comunicação síncrona via HTTP
- Operações diretas no banco de dados

---

## 🔒 Segurança

### 1. Autenticação JWT

#### Configuração JWT
```csharp
public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                };
            });
        
        return services;
    }
}
```

#### Geração de Token
```csharp
public class JwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;
    
    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name)
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### 2. Validação de Dados

#### FluentValidation
```csharp
public class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequest>
{
    public CreateSaleRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
            
        RuleFor(x => x.Branch)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Branch must not be empty and maximum 100 characters");
            
        RuleFor(x => x.Products)
            .NotEmpty()
            .WithMessage("At least one product is required");
            
        RuleForEach(x => x.Products)
            .SetValidator(new SaleProductRequestValidator());
    }
}

public class SaleProductRequestValidator : AbstractValidator<SaleProductRequest>
{
    public SaleProductRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");
            
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(20)
            .WithMessage("Quantity must be between 1 and 20");
            
        RuleFor(x => x.UnitPrice)
            .GreaterThan(0)
            .WithMessage("Unit price must be greater than 0");
    }
}
```

### 3. Middleware de Segurança

```csharp
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        
        await _next(context);
    }
}
```

---

## ⚡ Performance e Cache

### 1. Cache Redis (ProductApi)

#### Configuração Redis
```csharp
public static class RedisExtensions
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");
        
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "ProductApi_";
        });
        
        services.AddSingleton<ICacheService, RedisCacheService>();
        
        return services;
    }
}
```

#### Serviço de Cache
```csharp
public interface ICacheService
{
    Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;
    
    public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await _cache.GetStringAsync(key, cancellationToken);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions();
        if (expiration.HasValue)
            options.SetAbsoluteExpiration(expiration.Value);
            
        var serializedValue = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, serializedValue, options, cancellationToken);
    }
}
```

#### Uso no Controller
```csharp
[HttpGet]
public async Task<ActionResult<PaginationResult<ProductDto>>> GetProducts(CancellationToken ct)
{
    var cacheKey = $"products:list:p={queryParams.Page}:s={queryParams.Size}";
    
    var cached = await cache.GetAsync<PaginationResult<ProductDto>>(cacheKey, ct);
    if (cached is not null) return Ok(cached);
    
    var result = await mediator.Send(new GetProductsQuery(queryParams), ct);
    await cache.SetAsync(cacheKey, result, TimeSpan.FromSeconds(60), ct);
    
    return Ok(result);
}
```

### 2. Paginação Otimizada

#### Query Parameters
```csharp
public class QueryParameters
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string Order { get; set; } = "id";
    public Dictionary<string, string> Equality { get; set; } = new();
    public Dictionary<string, string> Wildcards { get; set; } = new();
    public Dictionary<string, RangeFilter> Ranges { get; set; } = new();
}

public class RangeFilter
{
    public object Min { get; set; }
    public object Max { get; set; }
}
```

#### Implementação de Paginação
```csharp
public class PaginationResult<T>
{
    public IEnumerable<T> Items { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
}

public static class PaginationExtensions
{
    public static async Task<PaginationResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, 
        QueryParameters parameters)
    {
        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)parameters.Size);
        
        var items = await query
            .Skip((parameters.Page - 1) * parameters.Size)
            .Take(parameters.Size)
            .ToListAsync();
            
        return new PaginationResult<T>
        {
            Items = items,
            TotalItems = totalItems,
            TotalPages = totalPages,
            CurrentPage = parameters.Page,
            PageSize = parameters.Size,
            HasNext = parameters.Page < totalPages,
            HasPrevious = parameters.Page > 1
        };
    }
}
```

---

## 📊 Monitoramento e Observabilidade

### 1. Health Checks

#### Configuração
```csharp
public static class HealthCheckExtensions
{
    public static IServiceCollection AddBasicHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddDbContextCheck<DefaultContext>("database");
            
        return services;
    }
    
    // SaleApi - Health Checks específicos
    public static IServiceCollection AddSaleApiHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddDbContextCheck<DefaultContext>("database")
            .AddCheck<RebusHealthCheck>("rebus");
            
        return services;
    }
    
    // ProductApi - Health Checks específicos
    public static IServiceCollection AddProductApiHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddMongoDb("mongodb")
            .AddRedis("redis");
            
        return services;
    }
}

// Health Check para Rebus (SaleApi)
public class RebusHealthCheck : IHealthCheck
{
    private readonly IBus _bus;

    public RebusHealthCheck(IBus bus)
    {
        _bus = bus;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar se o bus está funcionando
            await _bus.Advanced.Topics.Publish("health-check", "ping");
            return HealthCheckResult.Healthy("SaleApi Rebus is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SaleApi Rebus is unhealthy", ex);
        }
    }
}
```

#### Endpoints
```csharp
app.UseBasicHealthChecks();

// Endpoints disponíveis:
// GET /health - Status geral
// GET /health/ready - Verificação de dependências
// GET /health/live - Verificação de vida
```

### 2. Logging Estruturado

#### Configuração Serilog
```csharp
public static class LoggingExtensions
{
    public static WebApplicationBuilder AddDefaultLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
            
        builder.Host.UseSerilog();
        
        return builder;
    }
}
```

#### Logs Estruturados
```csharp
public class SalesController : BaseController
{
    private readonly ILogger<SalesController> _logger;
    
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequest request)
    {
        _logger.LogInformation("Creating sale for user {UserId} with {ProductCount} products", 
            request.UserId, request.Products.Count);
            
        try
        {
            var result = await _mediator.Send(command);
            _logger.LogInformation("Sale {SaleId} created successfully", result.Id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sale for user {UserId}", request.UserId);
            throw;
        }
    }
}
```

### 3. Métricas de Performance

#### Middleware de Timing
```csharp
public class PerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var sw = Stopwatch.StartNew();
        
        await next(context);
        
        sw.Stop();
        
        _logger.LogInformation("Request {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}",
            context.Request.Method,
            context.Request.Path,
            sw.ElapsedMilliseconds,
            context.Response.StatusCode);
    }
}
```

---

## 🧪 Testes

### 1. Testes Unitários

#### Estrutura
```csharp
[TestClass]
public class SaleTests
{
    private Sale _sale;
    
    [TestInitialize]
    public void Setup()
    {
        _sale = new Sale
        {
            UserId = Guid.NewGuid(),
            Branch = "Test Branch",
            Products = new List<SaleProduct>
            {
                new SaleProduct
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 5,
                    UnitPrice = 10.00m
                }
            }
        };
    }
    
    [TestMethod]
    public void CalculateTotalSaleAmount_WithDiscount_ShouldApplyCorrectDiscount()
    {
        // Arrange
        var product = _sale.Products.First();
        product.Quantity = 5; // 10% discount
        
        // Act
        _sale.CalculateTotalSaleAmount();
        
        // Assert
        Assert.AreEqual(45.00m, _sale.TotalSaleAmount); // 5 * 10 * 0.9
    }
}
```

#### Testes com xUnit e FluentAssertions
```csharp
[Fact]
public async Task CreateSaleHandler_ValidCommand_ShouldPublishEvent()
{
    // Arrange
    var mockRepository = new Mock<ISaleRepository>();
    var mockEventPublisher = new Mock<IEventPublisher>();
    var handler = new CreateSaleCommandHandler(mockRepository.Object, mockEventPublisher.Object);
    
    var command = new CreateSaleCommand { /* ... */ };
    
    // Act
    await handler.Handle(command, CancellationToken.None);
    
    // Assert
    mockEventPublisher.Verify(x => x.PublishAsync(It.IsAny<SaleCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

### 2. Testes de Integração

#### Database Fixture
```csharp
public class DatabaseFixture : IDisposable
{
    public DefaultContext Context { get; }
    
    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        Context = new DefaultContext(options);
        Context.Database.EnsureCreated();
    }
    
    public void Dispose()
    {
        Context?.Dispose();
    }
}

[TestClass]
public class SaleRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly ISaleRepository _repository;
    
    public SaleRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new SaleRepository(_fixture.Context);
    }
    
    [TestMethod]
    public async Task GetBySaleNumber_ExistingSale_ShouldReturnSale()
    {
        // Arrange
        var sale = new Sale { SaleNumber = 1001 };
        await _fixture.Context.Sales.AddAsync(sale);
        await _fixture.Context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetBySaleNumberAsync(1001);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1001, result.SaleNumber);
    }
}
```

### 3. Testes Funcionais

#### WebApplicationFactory
```csharp
public class WebApplicationFactory<TStartup> : Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<TStartup>
    where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<DefaultContext>));
                
            if (descriptor != null)
                services.Remove(descriptor);
                
            services.AddDbContext<DefaultContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });
        });
    }
}

[TestClass]
public class SalesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public SalesControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    
    [TestMethod]
    public async Task CreateSale_ValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateSaleRequest
        {
            UserId = Guid.NewGuid(),
            Branch = "Test Branch",
            Products = new List<SaleProductRequest>
            {
                new SaleProductRequest
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 5,
                    UnitPrice = 10.00m
                }
            }
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", request);
        
        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }
}
```

---

## 🚀 Deploy e Infraestrutura

### 1. Docker

#### Dockerfile Multi-stage
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Ambev.DeveloperEvaluation.WebApi/Ambev.DeveloperEvaluation.WebApi.csproj", "src/Ambev.DeveloperEvaluation.WebApi/"]
COPY ["src/Ambev.DeveloperEvaluation.Application/Ambev.DeveloperEvaluation.Application.csproj", "src/Ambev.DeveloperEvaluation.Application/"]
COPY ["src/Ambev.DeveloperEvaluation.Domain/Ambev.DeveloperEvaluation.Domain.csproj", "src/Ambev.DeveloperEvaluation.Domain/"]
COPY ["src/Ambev.DeveloperEvaluation.ORM/Ambev.DeveloperEvaluation.ORM.csproj", "src/Ambev.DeveloperEvaluation.ORM/"]
COPY ["src/Ambev.DeveloperEvaluation.Common/Ambev.DeveloperEvaluation.Common.csproj", "src/Ambev.DeveloperEvaluation.Common/"]
COPY ["src/Ambev.DeveloperEvaluation.IoC/Ambev.DeveloperEvaluation.IoC.csproj", "src/Ambev.DeveloperEvaluation.IoC/"]

RUN dotnet restore "src/Ambev.DeveloperEvaluation.WebApi/Ambev.DeveloperEvaluation.WebApi.csproj"
COPY . .
WORKDIR "/src/src/Ambev.DeveloperEvaluation.WebApi"
RUN dotnet build "Ambev.DeveloperEvaluation.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ambev.DeveloperEvaluation.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ambev.DeveloperEvaluation.WebApi.dll"]
```

### 2. Docker Compose

#### Configuração Completa
```yaml
version: '3.8'

services:
  # Database
  postgres:
    image: postgres:13
    environment:
      POSTGRES_DB: db_sale
      POSTGRES_USER: developer
      POSTGRES_PASSWORD: ev@luAt10n
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - ambev_network

  # Cache
  redis:
    image: redis:7.4.1-alpine
    command: redis-server --requirepass ev@luAt10n
    ports:
      - "6379:6379"
    networks:
      - ambev_network

  # Message Broker (RabbitMQ)
  rabbitmq:
    image: rabbitmq:3-management
    environment:
      RABBITMQ_DEFAULT_USER: developer
      RABBITMQ_DEFAULT_PASS: evaluAt10n
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    networks:
      - ambev_network

  # SaleApi (única API que usa RabbitMQ)
  sale-api:
    build:
      context: .
      dockerfile: src/Ambev.DeveloperEvaluation.WebApi/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_HTTP_PORTS=8080
      - ConnectionStrings__DefaultConnection=Server=postgres;Database=db_sale;User Id=developer;Password=ev@luAt10n
      - ConnectionStrings__RabbitMQ=amqp://developer:evaluAt10n@rabbitmq:5672/
    ports:
      - "1080:8080"
    depends_on:
      - postgres
      - rabbitmq
    networks:
      - ambev_network
    restart: unless-stopped

  # ProductApi (com Redis)
  product-api:
    build:
      context: .
      dockerfile: src/Ambev.DeveloperEvaluation.WebApi/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_HTTP_PORTS=8080
      - ConnectionStrings__DefaultConnection=Server=mongodb;Database=db_product;User Id=developer;Password=evaluAt10n
      - Redis__ConnectionString=redis:6379,password=ev@luAt10n
    ports:
      - "3080:8080"
    depends_on:
      - mongodb
      - redis
    networks:
      - ambev_network
    restart: unless-stopped

  # CartApi
  cart-api:
    build:
      context: .
      dockerfile: src/Ambev.DeveloperEvaluation.WebApi/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_HTTP_PORTS=8080
      - ConnectionStrings__DefaultConnection=Server=postgres;Database=db_cart;User Id=developer;Password=ev@luAt10n
    ports:
      - "2080:8080"
    depends_on:
      - postgres
    networks:
      - ambev_network
    restart: unless-stopped

  # IdentityApi
  identity-api:
    build:
      context: .
      dockerfile: src/Ambev.DeveloperEvaluation.WebApi/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_HTTP_PORTS=8080
      - ConnectionStrings__DefaultConnection=Server=postgres;Database=db_identity;User Id=developer;Password=ev@luAt10n
    ports:
      - "4080:8080"
    depends_on:
      - postgres
    networks:
      - ambev_network
    restart: unless-stopped

volumes:
  postgres_data:
  rabbitmq_data:

networks:
  ambev_network:
    driver: bridge
```

### 3. Variáveis de Ambiente

#### Desenvolvimento
```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_HTTP_PORTS=8080
ASPNETCORE_HTTPS_PORTS=8081
```

#### Produção

**SaleApi (única que usa RabbitMQ):**
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_HTTP_PORTS=80
ASPNETCORE_HTTPS_PORTS=443
ConnectionStrings__DefaultConnection=Server=prod-db;Database=db_sale;User Id=prod_user;Password=prod_password
ConnectionStrings__RabbitMQ=amqp://prod_user:prod_password@prod-rabbitmq:5672/
```

**ProductApi (usa Redis para cache):**
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_HTTP_PORTS=80
ASPNETCORE_HTTPS_PORTS=443
ConnectionStrings__DefaultConnection=Server=prod-mongodb;Database=db_product;User Id=prod_user;Password=prod_password
Redis__ConnectionString=prod-redis:6379,password=prod_password
```

**CartApi e IdentityApi (sem message brokers):**
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_HTTP_PORTS=80
ASPNETCORE_HTTPS_PORTS=443
ConnectionStrings__DefaultConnection=Server=prod-db;Database=db_cart;User Id=prod_user;Password=prod_password
```

---

## 📈 Considerações de Escalabilidade

### 1. Horizontal Scaling

- **Load Balancer**: Distribuição de carga entre múltiplas instâncias
- **Stateless Design**: APIs sem estado para facilitar scaling
- **Database Sharding**: Particionamento de dados por domínio

### 2. Performance

- **Caching Strategy**: Cache em múltiplas camadas
- **Database Optimization**: Índices e queries otimizadas
- **Async/Await**: Operações assíncronas para melhor throughput

### 3. Monitoramento

- **APM Tools**: Application Performance Monitoring
- **Distributed Tracing**: Rastreamento de requisições
- **Metrics Collection**: Coleta de métricas de performance

### 4. Event-Driven Scaling com Rebus (SaleApi)

- **Event Sourcing**: Rastreamento completo de mudanças de vendas
- **Event Store**: Armazenamento de eventos para auditoria
- **Event Replay**: Reconstrução de estado através de eventos
- **Event Streaming**: Processamento de eventos em tempo real
- **Service Bus (Rebus)**: Mensageria e filas para processamento assíncrono

#### Benefícios do Rebus para Escalabilidade da SaleApi

1. **Processamento Assíncrono**: Eventos de venda são processados independentemente da transação principal
2. **Paralelização**: Múltiplos handlers podem processar o mesmo evento simultaneamente
3. **Retry Automático**: Tentativas automáticas em caso de falha
4. **Dead Letter Queue**: Eventos que falharam são movidos para DLQ para análise
5. **Horizontal Scaling**: Fácil adição de novos workers para processar eventos de venda

#### Configuração de Escalabilidade com Rebus na SaleApi

```csharp
// Configuração para alta escalabilidade
services.AddRebus(configure => configure
    .Transport(t => t.UseRabbitMq(connectionString, "ambev-sales-queue"))
    .Options(o => o
        .SetNumberOfWorkers(10)           // Múltiplos workers
        .SetMaxParallelism(20)            // Processamento paralelo
        .SetRetryStrategy(5)              // Mais tentativas
        .SetBackoffTimes(TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(5)) // Backoff exponencial
    )
);
```

#### Monitoramento de Performance

```csharp
public class RebusMetrics
{
    private readonly Counter _messagesProcessed;
    private readonly Histogram _processingTime;

    public RebusMetrics(IMetricsFactory metricsFactory)
    {
        _messagesProcessed = metricsFactory.CreateCounter("rebus_messages_processed_total", "Total messages processed");
        _processingTime = metricsFactory.CreateHistogram("rebus_processing_time_seconds", "Message processing time");
    }

    public void RecordMessageProcessed(string eventType)
    {
        _messagesProcessed.Add(1, new KeyValuePair<string, object>("event_type", eventType));
    }

    public void RecordProcessingTime(double seconds)
    {
        _processingTime.Record(seconds);
    }
}
```

---

**Esta arquitetura técnica garante um sistema robusto, escalável e maintainable, seguindo as melhores práticas de desenvolvimento de software.**
