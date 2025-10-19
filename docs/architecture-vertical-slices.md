# MM Application - Vertical Slice Architecture with Service Separation

## Philosophy: Simple Made Easy

Following Rich Hickey's principles, this architecture:
- **Un-braids** services (each service is independent)
- **Separates concerns vertically** (each feature owns its full stack)
- **Composes** services (services communicate via well-defined contracts)
- **Enables independent deployment** (notification service can deploy separately)
- **Prevents complecting** (changing notifications doesn't affect transactions)

---

## 🏗️ Architecture Overview

```
MM.Solution/
│
├── src/
│   │
│   ├── Core/                                    # Shared kernel (minimal)
│   │   ├── MM.Core.Domain/                      # Shared domain primitives
│   │   │   ├── Common/
│   │   │   │   ├── BaseEntity.cs
│   │   │   │   ├── AuditableEntity.cs
│   │   │   │   └── ValueObject.cs
│   │   │   └── Interfaces/
│   │   │       ├── IEntity.cs
│   │   │       └── IAuditable.cs
│   │   │
│   │   └── MM.Core.Contracts/                   # Shared contracts (DTOs, responses)
│   │       ├── Common/
│   │       │   ├── ApiResponse.cs
│   │       │   ├── ApiError.cs
│   │       │   ├── ApiMetadata.cs
│   │       │   ├── PagedRequest.cs
│   │       │   ├── PagedResponse.cs
│   │       │   ├── ErrorCodes.cs
│   │       │   └── Result.cs
│   │       └── Events/                          # Cross-service events
│   │           ├── IEvent.cs
│   │           ├── TransactionCreatedEvent.cs
│   │           └── AccountBalanceChangedEvent.cs
│   │
│   ├── Services/                                # Vertical slices (one per domain)
│   │   │
│   │   ├── MM.Services.Finance/                 # Finance service (transactions, accounts, balances)
│   │   │   ├── MM.Services.Finance.Domain/
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── Transaction.cs
│   │   │   │   │   ├── Account.cs
│   │   │   │   │   ├── Balance.cs
│   │   │   │   │   ├── Bank.cs
│   │   │   │   │   └── Card.cs
│   │   │   │   ├── ValueObjects/
│   │   │   │   │   ├── Money.cs
│   │   │   │   │   ├── TransactionCategory.cs
│   │   │   │   │   └── Iban.cs
│   │   │   │   └── Events/
│   │   │   │       ├── TransactionCreated.cs
│   │   │   │       └── BalanceUpdated.cs
│   │   │   │
│   │   │   ├── MM.Services.Finance.Application/
│   │   │   │   ├── Contracts/                   # DTOs specific to Finance
│   │   │   │   │   ├── Transactions/
│   │   │   │   │   │   ├── TransactionDto.cs
│   │   │   │   │   │   ├── CreateTransactionRequest.cs
│   │   │   │   │   │   └── UpdateTransactionRequest.cs
│   │   │   │   │   ├── Accounts/
│   │   │   │   │   │   ├── AccountDto.cs
│   │   │   │   │   │   └── CreateAccountRequest.cs
│   │   │   │   │   └── Balances/
│   │   │   │   │       └── BalanceDto.cs
│   │   │   │   ├── Features/                    # Vertical slices by feature
│   │   │   │   │   ├── Transactions/
│   │   │   │   │   │   ├── CreateTransaction/
│   │   │   │   │   │   │   ├── CreateTransactionCommand.cs
│   │   │   │   │   │   │   ├── CreateTransactionHandler.cs
│   │   │   │   │   │   │   └── CreateTransactionValidator.cs
│   │   │   │   │   │   ├── GetTransactions/
│   │   │   │   │   │   │   ├── GetTransactionsQuery.cs
│   │   │   │   │   │   │   └── GetTransactionsHandler.cs
│   │   │   │   │   │   ├── CategorizeTransaction/
│   │   │   │   │   │   │   ├── CategorizeTransactionCommand.cs
│   │   │   │   │   │   │   └── CategorizeTransactionHandler.cs
│   │   │   │   │   │   └── UploadTransactions/
│   │   │   │   │   │       ├── UploadTransactionsCommand.cs
│   │   │   │   │   │       └── UploadTransactionsHandler.cs
│   │   │   │   │   ├── Accounts/
│   │   │   │   │   │   ├── CreateAccount/
│   │   │   │   │   │   ├── GetAccounts/
│   │   │   │   │   │   └── UpdateAccount/
│   │   │   │   │   └── Balances/
│   │   │   │   │       ├── GetBalances/
│   │   │   │   │       └── UpdateBalance/
│   │   │   │   ├── Services/
│   │   │   │   │   └── TransactionCategorizationService.cs
│   │   │   │   └── Interfaces/
│   │   │   │       ├── ITransactionRepository.cs
│   │   │   │       └── IAccountRepository.cs
│   │   │   │
│   │   │   ├── MM.Services.Finance.Infrastructure/
│   │   │   │   ├── Persistence/
│   │   │   │   │   ├── FinanceDbContext.cs
│   │   │   │   │   ├── Configurations/
│   │   │   │   │   │   ├── TransactionConfiguration.cs
│   │   │   │   │   │   ├── AccountConfiguration.cs
│   │   │   │   │   │   └── BalanceConfiguration.cs
│   │   │   │   │   ├── Repositories/
│   │   │   │   │   │   ├── TransactionRepository.cs
│   │   │   │   │   │   ├── AccountRepository.cs
│   │   │   │   │   │   └── BalanceRepository.cs
│   │   │   │   │   └── Migrations/
│   │   │   │   ├── MachineLearning/
│   │   │   │   │   ├── CategoryPredictor.cs
│   │   │   │   │   └── ModelTrainer.cs
│   │   │   │   └── DependencyInjection.cs
│   │   │   │
│   │   │   └── MM.Services.Finance.Api/         # Finance API (can be standalone)
│   │   │       ├── Controllers/
│   │   │       │   ├── TransactionsController.cs
│   │   │       │   ├── AccountsController.cs
│   │   │       │   └── BalancesController.cs
│   │   │       ├── Program.cs
│   │   │       ├── appsettings.json
│   │   │       └── Dockerfile
│   │   │
│   │   ├── MM.Services.Bills/                   # Bills service
│   │   │   ├── MM.Services.Bills.Domain/
│   │   │   │   └── Entities/
│   │   │   │       ├── Bill.cs
│   │   │   │       ├── Supplier.cs
│   │   │   │       └── ReadInBill.cs
│   │   │   ├── MM.Services.Bills.Application/
│   │   │   │   ├── Contracts/
│   │   │   │   │   └── Bills/
│   │   │   │   ├── Features/
│   │   │   │   │   ├── CreateBill/
│   │   │   │   │   ├── UploadBillPdf/
│   │   │   │   │   └── ParseBill/
│   │   │   │   └── Interfaces/
│   │   │   ├── MM.Services.Bills.Infrastructure/
│   │   │   │   ├── Persistence/
│   │   │   │   │   └── BillsDbContext.cs
│   │   │   │   ├── Storage/
│   │   │   │   │   └── PdfStorageService.cs
│   │   │   │   └── OcrService/
│   │   │   │       └── BillOcrService.cs
│   │   │   └── MM.Services.Bills.Api/
│   │   │       ├── Controllers/
│   │   │       │   ├── BillsController.cs
│   │   │       │   └── SuppliersController.cs
│   │   │       └── Program.cs
│   │   │
│   │   ├── MM.Services.Salary/                  # Salary service
│   │   │   ├── MM.Services.Salary.Domain/
│   │   │   │   └── Entities/
│   │   │   │       └── Salary.cs
│   │   │   ├── MM.Services.Salary.Application/
│   │   │   │   ├── Contracts/
│   │   │   │   └── Features/
│   │   │   ├── MM.Services.Salary.Infrastructure/
│   │   │   │   └── Persistence/
│   │   │   │       └── SalaryDbContext.cs
│   │   │   └── MM.Services.Salary.Api/
│   │   │       └── Controllers/
│   │   │           └── SalariesController.cs
│   │   │
│   │   ├── MM.Services.HouseThings/             # House inventory service
│   │   │   ├── MM.Services.HouseThings.Domain/
│   │   │   ├── MM.Services.HouseThings.Application/
│   │   │   ├── MM.Services.HouseThings.Infrastructure/
│   │   │   └── MM.Services.HouseThings.Api/
│   │   │
│   │   ├── MM.Services.Statistics/              # Statistics & Analytics service
│   │   │   ├── MM.Services.Statistics.Domain/
│   │   │   ├── MM.Services.Statistics.Application/
│   │   │   │   ├── Features/
│   │   │   │   │   ├── Dashboard/
│   │   │   │   │   ├── SpendingAnalysis/
│   │   │   │   │   └── BalanceTrends/
│   │   │   │   └── Services/
│   │   │   │       └── AggregationService.cs
│   │   │   ├── MM.Services.Statistics.Infrastructure/
│   │   │   │   └── ReadModels/                  # Optimized read models
│   │   │   │       └── StatisticsDbContext.cs
│   │   │   └── MM.Services.Statistics.Api/
│   │   │
│   │   ├── MM.Services.Notifications/           # NEW: Notifications service
│   │   │   ├── MM.Services.Notifications.Domain/
│   │   │   │   └── Entities/
│   │   │   │       ├── Notification.cs
│   │   │   │       ├── NotificationTemplate.cs
│   │   │   │       └── UserNotificationPreference.cs
│   │   │   ├── MM.Services.Notifications.Application/
│   │   │   │   ├── Contracts/
│   │   │   │   │   ├── NotificationDto.cs
│   │   │   │   │   └── SendNotificationRequest.cs
│   │   │   │   ├── Features/
│   │   │   │   │   ├── SendNotification/
│   │   │   │   │   │   ├── SendNotificationCommand.cs
│   │   │   │   │   │   └── SendNotificationHandler.cs
│   │   │   │   │   ├── GetNotifications/
│   │   │   │   │   └── MarkAsRead/
│   │   │   │   ├── EventHandlers/               # Listens to other services
│   │   │   │   │   ├── TransactionCreatedEventHandler.cs
│   │   │   │   │   └── AccountBalanceChangedEventHandler.cs
│   │   │   │   └── Interfaces/
│   │   │   │       ├── INotificationRepository.cs
│   │   │   │       └── INotificationService.cs
│   │   │   ├── MM.Services.Notifications.Infrastructure/
│   │   │   │   ├── Persistence/
│   │   │   │   │   └── NotificationsDbContext.cs
│   │   │   │   ├── SignalR/
│   │   │   │   │   ├── NotificationHub.cs
│   │   │   │   │   └── NotificationHubService.cs
│   │   │   │   ├── Email/
│   │   │   │   │   └── EmailNotificationService.cs
│   │   │   │   ├── Push/
│   │   │   │   │   └── PushNotificationService.cs
│   │   │   │   └── EventBus/
│   │   │   │       ├── RabbitMqEventBus.cs       # Or your choice
│   │   │   │       └── InMemoryEventBus.cs       # For development
│   │   │   └── MM.Services.Notifications.Api/
│   │   │       ├── Hubs/
│   │   │       │   └── NotificationHub.cs
│   │   │       ├── Controllers/
│   │   │       │   └── NotificationsController.cs
│   │   │       ├── Program.cs
│   │   │       └── Dockerfile
│   │   │
│   │   ├── MM.Services.Identity/                # Identity & Access service
│   │   │   ├── MM.Services.Identity.Domain/
│   │   │   │   └── Entities/
│   │   │   │       ├── ApplicationUser.cs
│   │   │   │       ├── ISA_Account.cs
│   │   │   │       └── ISA_PasswordsOld.cs
│   │   │   ├── MM.Services.Identity.Application/
│   │   │   │   ├── Features/
│   │   │   │   │   ├── Register/
│   │   │   │   │   ├── Login/
│   │   │   │   │   └── RefreshToken/
│   │   │   │   └── Services/
│   │   │   │       └── JwtTokenService.cs
│   │   │   ├── MM.Services.Identity.Infrastructure/
│   │   │   │   └── Persistence/
│   │   │   │       └── IdentityDbContext.cs
│   │   │   └── MM.Services.Identity.Api/
│   │   │       └── Controllers/
│   │   │           └── AuthController.cs
│   │   │
│   │   └── MM.Services.Ancillary/               # Ancillary data service (countries, currencies)
│   │       ├── MM.Services.Ancillary.Domain/
│   │       ├── MM.Services.Ancillary.Application/
│   │       ├── MM.Services.Ancillary.Infrastructure/
│   │       └── MM.Services.Ancillary.Api/
│   │
│   ├── ApiGateway/                              # API Gateway (optional)
│   │   └── MM.ApiGateway/
│   │       ├── Program.cs
│   │       ├── ocelot.json                      # Ocelot or YARP config
│   │       └── Dockerfile
│   │
│   ├── Clients/                                 # Frontend clients
│   │   ├── MM.Web/                              # Blazor WebAssembly
│   │   │   ├── Services/
│   │   │   │   ├── ApiClients/
│   │   │   │   │   ├── FinanceApiClient.cs
│   │   │   │   │   ├── BillsApiClient.cs
│   │   │   │   │   └── NotificationsApiClient.cs
│   │   │   │   └── SignalR/
│   │   │   │       └── NotificationHubClient.cs
│   │   │   ├── Pages/
│   │   │   └── Program.cs
│   │   │
│   │   └── MM.MobileApp/                        # .NET MAUI
│   │       ├── Services/
│   │       │   ├── ApiClients/
│   │       │   └── SignalR/
│   │       └── MauiProgram.cs
│   │
│   └── BuildingBlocks/                          # Shared infrastructure
│       ├── MM.BuildingBlocks.EventBus/
│       │   ├── Abstractions/
│       │   │   ├── IEventBus.cs
│       │   │   ├── IIntegrationEvent.cs
│       │   │   └── IIntegrationEventHandler.cs
│       │   └── RabbitMQ/
│       │       └── RabbitMqEventBus.cs
│       │
│       ├── MM.BuildingBlocks.Logging/
│       │   └── SerilogConfiguration.cs
│       │
│       └── MM.BuildingBlocks.HealthChecks/
│           └── HealthCheckExtensions.cs
│
└── tests/
    ├── MM.Services.Finance.Tests/
    ├── MM.Services.Notifications.Tests/
    └── MM.IntegrationTests/

```

---

## 🎯 Key Architectural Decisions

### 1. Vertical Slice Per Service

Each service is **completely independent**:

```
MM.Services.Finance/       # Owns transactions, accounts, balances
MM.Services.Bills/         # Owns bills, suppliers, PDF storage
MM.Services.Notifications/ # Owns notifications, SignalR, email, push
MM.Services.Identity/      # Owns authentication, users
MM.Services.Statistics/    # Owns analytics, dashboards
```

**Benefits:**
- ✅ Change notification logic without touching finance code
- ✅ Deploy notifications service independently
- ✅ Scale notifications separately from finance
- ✅ Different teams can own different services
- ✅ Each service can use different DB if needed

### 2. Feature Folders (Vertical Slices Within Service)

Instead of organizing by technical layers (Controllers, Services, Repositories), organize by **features**:

```
MM.Services.Finance.Application/
└── Features/
    ├── Transactions/
    │   ├── CreateTransaction/
    │   │   ├── CreateTransactionCommand.cs      # Request
    │   │   ├── CreateTransactionHandler.cs      # Logic
    │   │   └── CreateTransactionValidator.cs    # Validation
    │   ├── GetTransactions/
    │   │   ├── GetTransactionsQuery.cs
    │   │   └── GetTransactionsHandler.cs
    │   └── CategorizeTransaction/
    │       ├── CategorizeTransactionCommand.cs
    │       └── CategorizeTransactionHandler.cs
```

**Why Simple:**
- Everything for "CreateTransaction" is in one folder
- No jumping between Controllers → Services → Repositories
- Easy to find and change
- Clear boundaries

### 3. Event-Driven Communication Between Services

Services communicate via **events** (not direct calls):

```csharp
// MM.Core.Contracts/Events/TransactionCreatedEvent.cs
public record TransactionCreatedEvent : IIntegrationEvent
{
    public int TransactionId { get; init; }
    public int AccountId { get; init; }
    public decimal Amount { get; init; }
    public string Category { get; init; }
    public DateTime CreatedAt { get; init; }
}

// In Finance Service - Publish event
public class CreateTransactionHandler
{
    private readonly IEventBus _eventBus;

    public async Task<Result<TransactionDto>> Handle(CreateTransactionCommand command)
    {
        // ... create transaction ...

        // Publish event
        await _eventBus.PublishAsync(new TransactionCreatedEvent
        {
            TransactionId = transaction.Id,
            AccountId = transaction.AccountId,
            Amount = transaction.Amount,
            Category = transaction.Category,
            CreatedAt = DateTime.UtcNow
        });

        return Result.Success(dto);
    }
}

// In Notifications Service - Handle event
public class TransactionCreatedEventHandler : IIntegrationEventHandler<TransactionCreatedEvent>
{
    private readonly INotificationService _notificationService;

    public async Task Handle(TransactionCreatedEvent @event)
    {
        // Send notification to user
        await _notificationService.SendAsync(new Notification
        {
            UserId = @event.UserId,
            Title = "New Transaction",
            Message = $"Transaction of {@event.Amount:C} was created",
            Type = NotificationType.TransactionCreated
        });
    }
}
```

**Why Simple:**
- Finance service doesn't know about Notifications
- Notifications service doesn't know about Finance
- Un-braided dependencies
- Can add/remove services without breaking others

### 4. Separate Database Per Service (Optional)

Each service can have its own database:

```
FinanceDbContext      → MM_Finance.db
BillsDbContext        → MM_Bills.db
NotificationsDbContext → MM_Notifications.db
IdentityDbContext     → MM_Identity.db
```

**Or shared database with schema separation:**

```sql
-- Finance schema
finance.Transactions
finance.Accounts
finance.Balances

-- Bills schema
bills.Bills
bills.Suppliers

-- Notifications schema
notifications.Notifications
notifications.Templates
```

**Why Simple:**
- Each service owns its data
- No shared tables = no conflicts
- Can optimize per service (NoSQL for notifications, SQL for finance)
- Independent migrations

---

## 📡 Notifications Service Architecture

### Complete Notifications Service Example

```csharp
// MM.Services.Notifications.Domain/Entities/Notification.cs
public class Notification : AuditableEntity
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public Dictionary<string, object>? Data { get; set; } // JSON data
}

public enum NotificationType
{
    TransactionCreated,
    LargeExpense,
    MonthlyReport,
    BillDue,
    SystemAlert
}

// MM.Services.Notifications.Application/Contracts/NotificationDto.cs
public record NotificationDto
{
    public int Id { get; init; }
    public string Title { get; init; }
    public string Message { get; init; }
    public string Type { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
    public Dictionary<string, object>? Data { get; init; }
}

// MM.Services.Notifications.Application/Features/SendNotification/SendNotificationCommand.cs
public record SendNotificationCommand : IRequest<Result<NotificationDto>>
{
    public string UserId { get; init; }
    public string Title { get; init; }
    public string Message { get; init; }
    public NotificationType Type { get; init; }
    public NotificationChannel[] Channels { get; init; } // SignalR, Email, Push
    public Dictionary<string, object>? Data { get; init; }
}

public enum NotificationChannel
{
    SignalR,    // Real-time to web/mobile
    Email,      // Email notification
    Push,       // Mobile push notification
    InApp       // Store in database only
}

// MM.Services.Notifications.Application/Features/SendNotification/SendNotificationHandler.cs
public class SendNotificationHandler : IRequestHandler<SendNotificationCommand, Result<NotificationDto>>
{
    private readonly INotificationRepository _repository;
    private readonly INotificationHubService _hubService;
    private readonly IEmailService _emailService;
    private readonly IPushNotificationService _pushService;
    private readonly IMapper _mapper;

    public async Task<Result<NotificationDto>> Handle(SendNotificationCommand command, CancellationToken ct)
    {
        // 1. Save to database (InApp)
        var notification = new Notification
        {
            UserId = command.UserId,
            Title = command.Title,
            Message = command.Message,
            Type = command.Type,
            Data = command.Data,
            IsRead = false
        };

        await _repository.AddAsync(notification);
        await _repository.SaveChangesAsync();

        // 2. Send via SignalR if requested
        if (command.Channels.Contains(NotificationChannel.SignalR))
        {
            await _hubService.SendToUserAsync(command.UserId, notification);
        }

        // 3. Send email if requested
        if (command.Channels.Contains(NotificationChannel.Email))
        {
            await _emailService.SendAsync(command.UserId, command.Title, command.Message);
        }

        // 4. Send push notification if requested
        if (command.Channels.Contains(NotificationChannel.Push))
        {
            await _pushService.SendAsync(command.UserId, command.Title, command.Message);
        }

        var dto = _mapper.Map<NotificationDto>(notification);
        return Result.Success(dto);
    }
}

// MM.Services.Notifications.Infrastructure/SignalR/NotificationHub.cs
public class NotificationHub : Hub
{
    private readonly INotificationRepository _repository;

    public async Task MarkAsRead(int notificationId)
    {
        var notification = await _repository.GetByIdAsync(notificationId);
        if (notification != null && notification.UserId == Context.User?.Identity?.Name)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _repository.UpdateAsync(notification);
            await _repository.SaveChangesAsync();
        }
    }

    public async Task GetUnreadCount()
    {
        var userId = Context.User?.Identity?.Name;
        if (userId != null)
        {
            var count = await _repository.GetUnreadCountAsync(userId);
            await Clients.Caller.SendAsync("UnreadCount", count);
        }
    }
}

// MM.Services.Notifications.Infrastructure/SignalR/NotificationHubService.cs
public class NotificationHubService : INotificationHubService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationHubService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToUserAsync(string userId, Notification notification)
    {
        await _hubContext.Clients
            .User(userId)
            .SendAsync("ReceiveNotification", new
            {
                notification.Id,
                notification.Title,
                notification.Message,
                notification.Type,
                notification.CreatedDate
            });
    }

    public async Task SendToAllAsync(Notification notification)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
    }
}

// MM.Services.Notifications.Application/EventHandlers/TransactionCreatedEventHandler.cs
public class TransactionCreatedEventHandler : IIntegrationEventHandler<TransactionCreatedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<TransactionCreatedEventHandler> _logger;

    public async Task Handle(TransactionCreatedEvent @event)
    {
        _logger.LogInformation("Handling TransactionCreated event for transaction {Id}", @event.TransactionId);

        // Send notification via SendNotificationCommand
        var command = new SendNotificationCommand
        {
            UserId = @event.UserId,
            Title = "New Transaction",
            Message = $"A transaction of {@event.Amount:C} was created in category {@event.Category}",
            Type = NotificationType.TransactionCreated,
            Channels = new[] { NotificationChannel.SignalR, NotificationChannel.InApp },
            Data = new Dictionary<string, object>
            {
                ["transactionId"] = @event.TransactionId,
                ["accountId"] = @event.AccountId,
                ["amount"] = @event.Amount
            }
        };

        await _mediator.Send(command);
    }
}

// MM.Services.Notifications.Api/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add SignalR
builder.Services.AddSignalR();

// Add MediatR for CQRS
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(SendNotificationHandler).Assembly));

// Add event bus
builder.Services.AddSingleton<IEventBus, RabbitMqEventBus>();

// Subscribe to events
builder.Services.AddHostedService<NotificationEventSubscriber>();

var app = builder.Build();

// Map SignalR hub
app.MapHub<NotificationHub>("/hubs/notifications");

app.MapControllers();
app.Run();

// MM.Services.Notifications.Api/BackgroundServices/NotificationEventSubscriber.cs
public class NotificationEventSubscriber : BackgroundService
{
    private readonly IEventBus _eventBus;
    private readonly IServiceProvider _serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Subscribe to events from other services
        await _eventBus.SubscribeAsync<TransactionCreatedEvent, TransactionCreatedEventHandler>();
        await _eventBus.SubscribeAsync<AccountBalanceChangedEvent, AccountBalanceChangedEventHandler>();
        await _eventBus.SubscribeAsync<BillDueEvent, BillDueEventHandler>();

        await Task.CompletedTask;
    }
}
```

---

## 🌐 Client Integration

### Web Client (Blazor)

```csharp
// MM.Web/Services/SignalR/NotificationHubClient.cs
public class NotificationHubClient : IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<NotificationHubClient> _logger;

    public event Action<NotificationDto>? NotificationReceived;
    public event Action<int>? UnreadCountReceived;

    public NotificationHubClient(IConfiguration config, ILogger<NotificationHubClient> logger)
    {
        _logger = logger;

        var hubUrl = config["ApiGateway:Url"] + "/hubs/notifications";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = async () => await GetAccessTokenAsync();
            })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<NotificationDto>("ReceiveNotification", notification =>
        {
            _logger.LogInformation("Notification received: {Title}", notification.Title);
            NotificationReceived?.Invoke(notification);
        });

        _hubConnection.On<int>("UnreadCount", count =>
        {
            UnreadCountReceived?.Invoke(count);
        });
    }

    public async Task StartAsync()
    {
        await _hubConnection.StartAsync();
        await _hubConnection.InvokeAsync("GetUnreadCount");
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        await _hubConnection.InvokeAsync("MarkAsRead", notificationId);
    }

    public async ValueTask DisposeAsync()
    {
        await _hubConnection.DisposeAsync();
    }
}

// MM.Web/Pages/Layout/MainLayout.razor
@inject NotificationHubClient NotificationHub
@implements IAsyncDisposable

<div class="notification-bell">
    <button @onclick="ShowNotifications">
        <i class="bell-icon"></i>
        @if (unreadCount > 0)
        {
            <span class="badge">@unreadCount</span>
        }
    </button>
</div>

@code {
    private int unreadCount = 0;

    protected override async Task OnInitializedAsync()
    {
        NotificationHub.NotificationReceived += OnNotificationReceived;
        NotificationHub.UnreadCountReceived += OnUnreadCountReceived;

        await NotificationHub.StartAsync();
    }

    private void OnNotificationReceived(NotificationDto notification)
    {
        // Show toast
        ToastService.ShowInfo(notification.Title, notification.Message);

        // Update count
        unreadCount++;
        StateHasChanged();
    }

    private void OnUnreadCountReceived(int count)
    {
        unreadCount = count;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        NotificationHub.NotificationReceived -= OnNotificationReceived;
        NotificationHub.UnreadCountReceived -= OnUnreadCountReceived;
        await NotificationHub.DisposeAsync();
    }
}
```

---

## 🚀 Deployment Options

### Option 1: Monolith (All services in one API Gateway)
- Single deployment
- Shared process
- Good for starting

### Option 2: Separate Services (Microservices)
- Each service is a separate API
- Independent deployment
- Can scale independently
- Use API Gateway (Ocelot/YARP) to route

### Option 3: Hybrid
- Core services together (Finance, Bills, Salary)
- Notifications separate (can scale independently)
- Statistics separate (read-heavy, can optimize)

---

## 📊 Benefits Summary

| Aspect | Traditional Layered | Vertical Slices |
|--------|-------------------|----------------|
| **Add Notification Service** | Touch multiple layers | Add new service folder |
| **Find CreateTransaction Code** | Jump between layers | One folder |
| **Change SignalR Logic** | Might break other features | Isolated in Notifications |
| **Team Ownership** | Hard (shared layers) | Easy (own service) |
| **Deploy Notifications Only** | Deploy everything | Deploy one service |
| **Scale Notifications** | Scale everything | Scale one service |
| **Test Notifications** | Need full stack | Test service only |

---

## ✅ Migration Strategy

1. **Start with monolith** (all services in one solution)
2. **Separate databases** (or schemas) early
3. **Use events** for cross-service communication
4. **Extract services** when needed (start with Notifications)
5. **Add API Gateway** when you have 3+ separate services

---

This architecture is:
- ✅ **Simple** (each service is un-braided)
- ✅ **Composable** (services communicate via events)
- ✅ **Scalable** (scale services independently)
- ✅ **Maintainable** (features are colocated)
- ✅ **Testable** (services are isolated)
- ✅ **Future-proof** (easy to add new services)
