# Phase 6: Finance Application Layer - Summary

**Date:** October 19, 2025
**Status:** ✅ Completed

---

## Overview

Phase 6 successfully implements the Finance Application layer using a **Hybrid MediatR approach**:
- **MediatR** for complex operations (CreateTransaction with validation, business logic)
- **Traditional Services** for simple CRUD (AccountQueryService, TransactionQueryService)

---

## What Was Created

### Project: Finance.Application

**Location:** `src/Services/HouseLedger.Services.Finance/HouseLedger.Services.Finance.Application/`

### Project Structure

```
Finance.Application/
├── Contracts/                       # DTOs
│   ├── Common/
│   │   ├── PagedRequest.cs         ✅ Paging request base class
│   │   └── PagedResult.cs          ✅ Paging result wrapper
│   ├── Transactions/
│   │   ├── TransactionDto.cs       ✅ Transaction DTO
│   │   └── CreateTransactionRequest.cs  ✅ Create request
│   ├── Accounts/
│   │   └── AccountDto.cs           ✅ Account DTO
│   └── Balances/
│       └── BalanceDto.cs           ✅ Balance DTO
│
├── Features/                        # MediatR handlers (complex operations)
│   └── Transactions/
│       └── CreateTransaction/
│           ├── CreateTransactionCommand.cs      ✅ MediatR command
│           ├── CreateTransactionValidator.cs    ✅ FluentValidation rules
│           └── CreateTransactionHandler.cs      ✅ Business logic handler
│
├── Services/                        # Traditional services (simple CRUD)
│   ├── AccountQueryService.cs      ✅ Get accounts (simple queries)
│   └── TransactionQueryService.cs  ✅ Get transactions (simple queries)
│
├── Interfaces/
│   ├── IAccountQueryService.cs     ✅ Account query interface
│   └── ITransactionQueryService.cs ✅ Transaction query interface
│
├── Behaviors/                       # MediatR pipeline behaviors
│   ├── LoggingBehavior.cs          ✅ Automatic logging for all requests
│   └── ValidationBehavior.cs       ✅ Automatic validation using FluentValidation
│
└── Mapping/
    └── FinanceMappingProfile.cs    ✅ AutoMapper entity ↔ DTO mappings
```

### Dependencies Added

**NuGet Packages:**
- `MediatR` (12.4.1) - CQRS pattern
- `FluentValidation` (11.11.0) - Validation
- `AutoMapper` (13.0.1) - Entity ↔ DTO mapping

**Project References:**
- `Finance.Domain` - Domain entities and value objects
- `Finance.Infrastructure` - DbContext and data access

---

## Files Created (18 total)

### Contracts/DTOs (6 files)

1. **PagedRequest.cs** - Base class for paged requests
   - Page number (1-based)
   - Page size (max 100, default 50)
   - Skip calculation

2. **PagedResult.cs** - Wrapper for paged results
   - Items, TotalCount, Page, PageSize, TotalPages
   - HasPrevious, HasNext properties

3. **TransactionDto.cs** - Transaction data transfer object
   - All transaction fields
   - CategoryName + IsCategoryConfirmed (flattened from Value Object)
   - AccountName (from navigation property)
   - Audit fields

4. **CreateTransactionRequest.cs** - Request to create transaction
   - Input fields for new transaction
   - Used by API controllers

5. **AccountDto.cs** - Account data transfer object
   - Account fields + BankName

6. **BalanceDto.cs** - Balance data transfer object
   - Balance fields + AccountName

### Mapping (1 file)

7. **FinanceMappingProfile.cs** - AutoMapper configuration
   - `Transaction` → `TransactionDto` (with Value Object flattening)
   - `CreateTransactionRequest` → `Transaction` (with Value Object construction)
   - `Account` → `AccountDto`
   - `Balance` → `BalanceDto`

### Traditional Services (4 files)

8. **IAccountQueryService.cs** - Interface for account queries
   - GetByIdAsync
   - GetAllAsync
   - GetByBankIdAsync

9. **AccountQueryService.cs** - Implementation
   - Simple EF Core queries
   - Uses AutoMapper for DTO conversion
   - Includes logging

10. **ITransactionQueryService.cs** - Interface for transaction queries
    - GetByIdAsync
    - GetByAccountIdAsync (with paging and date filters)
    - GetRecentAsync (with paging)

11. **TransactionQueryService.cs** - Implementation
    - Simple EF Core queries with paging
    - Uses AutoMapper for DTO conversion
    - Includes logging

### MediatR Features (3 files)

12. **CreateTransactionCommand.cs** - MediatR command
    - Request object implementing `IRequest<TransactionDto>`
    - Input properties for creating transaction

13. **CreateTransactionValidator.cs** - FluentValidation rules
    - TransactionDate: required, not in future
    - Amount: cannot be zero
    - AccountId: must be > 0
    - Description: max 500 chars
    - CategoryName: max 100 chars
    - Note: max 1000 chars

14. **CreateTransactionHandler.cs** - Business logic handler
    - Implements `IRequestHandler<CreateTransactionCommand, TransactionDto>`
    - **Business logic:**
      1. Verify account exists
      2. Create transaction entity
      3. Set category (Value Object)
      4. Generate unique key
      5. Check for duplicates
      6. Save to database
      7. Return DTO
    - Includes comprehensive logging

### MediatR Pipeline Behaviors (2 files)

15. **LoggingBehavior.cs** - Automatic logging
    - Logs before and after every MediatR request
    - Measures execution time
    - Logs exceptions

16. **ValidationBehavior.cs** - Automatic validation
    - Runs FluentValidation validators before handlers
    - Throws ValidationException if validation fails
    - Supports multiple validators per request

---

## Key Design Patterns

### 1. Hybrid Approach

**MediatR for Complex Operations:**
```csharp
// CreateTransaction - Uses MediatR because:
// - Has validation (FluentValidation)
// - Has business logic (duplicate detection, unique key generation)
// - Interacts with multiple entities
// - Could have side effects (events, notifications)

public class CreateTransactionHandler : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    public async Task<TransactionDto> Handle(CreateTransactionCommand request, ...)
    {
        // 1. Verify account exists
        // 2. Create entity
        // 3. Set category (Value Object)
        // 4. Generate unique key
        // 5. Check duplicates
        // 6. Save
        // 7. Return DTO
    }
}
```

**Traditional Services for Simple CRUD:**
```csharp
// AccountQueryService - Traditional service because:
// - Simple EF Core queries
// - No business logic
// - No side effects
// - Just map and return

public class AccountQueryService : IAccountQueryService
{
    public async Task<AccountDto?> GetByIdAsync(int id, ...)
    {
        var account = await _context.Accounts
            .Include(a => a.Bank)
            .FirstOrDefaultAsync(a => a.Id == id);

        return _mapper.Map<AccountDto>(account);
    }
}
```

### 2. MediatR Pipeline

**Request Flow:**
```
API Controller
    ↓
Send command to MediatR
    ↓
LoggingBehavior (logs request start)
    ↓
ValidationBehavior (runs FluentValidation)
    ↓
Handler (business logic)
    ↓
LoggingBehavior (logs completion time)
    ↓
Response back to controller
```

### 3. AutoMapper Integration

**Entity → DTO (with Value Object flattening):**
```csharp
// Transaction entity has Category (Value Object)
// TransactionDto has CategoryName + IsCategoryConfirmed (flattened)

CreateMap<Transaction, TransactionDto>()
    .ForMember(dest => dest.CategoryName,
        opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
    .ForMember(dest => dest.IsCategoryConfirmed,
        opt => opt.MapFrom(src => src.Category != null && src.Category.IsConfirmed));
```

**Request → Entity (with Value Object construction):**
```csharp
CreateMap<CreateTransactionRequest, Transaction>()
    .ForMember(dest => dest.Category, opt => opt.MapFrom(src =>
        !string.IsNullOrWhiteSpace(src.CategoryName)
            ? new TransactionCategory(src.CategoryName, src.IsCategoryConfirmed)
            : null));
```

### 4. Logging Throughout

**Services have logging:**
```csharp
public class AccountQueryService
{
    private readonly ILogger<AccountQueryService> _logger;

    public async Task<AccountDto?> GetByIdAsync(int id, ...)
    {
        _logger.LogDebug("Getting account by ID: {AccountId}", id);
        // ... query ...
        _logger.LogInformation("Account found: {AccountId} - {AccountName}", id, account.Name);
    }
}
```

**Handlers have logging:**
```csharp
public class CreateTransactionHandler
{
    private readonly ILogger<CreateTransactionHandler> _logger;

    public async Task<TransactionDto> Handle(...)
    {
        _logger.LogInformation("Creating transaction for account {AccountId} with amount {Amount}", ...);
        // ... business logic ...
        _logger.LogInformation("Transaction created successfully with ID {TransactionId}", ...);
    }
}
```

**Pipeline behaviors log automatically:**
```csharp
public class LoggingBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(...)
    {
        _logger.LogInformation("Handling {RequestName}", requestName);
        var response = await next();
        _logger.LogInformation("Handled {RequestName} in {ElapsedMilliseconds}ms", ...);
    }
}
```

---

## Code Examples

### Example 1: Simple Query (Traditional Service)

```csharp
// GET /api/accounts/5
// Uses traditional service - simple, straightforward

var account = await _accountQueryService.GetByIdAsync(5);
if (account == null)
    return NotFound();

return Ok(account);
```

### Example 2: Complex Command (MediatR)

```csharp
// POST /api/transactions
// Uses MediatR - validation, business logic, potential side effects

var command = new CreateTransactionCommand
{
    TransactionDate = DateTime.Now,
    Amount = 50.0,
    AccountId = 5,
    CategoryName = "SPESA",
    IsCategoryConfirmed = true
};

var transaction = await _mediator.Send(command);
return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
```

**What happens:**
1. LoggingBehavior logs "Handling CreateTransactionCommand"
2. ValidationBehavior runs CreateTransactionValidator
3. CreateTransactionHandler executes business logic:
   - Verifies account exists
   - Creates transaction entity
   - Sets TransactionCategory Value Object
   - Generates unique key
   - Checks for duplicates
   - Saves to database
4. LoggingBehavior logs "Handled CreateTransactionCommand in Xms"
5. Returns TransactionDto

### Example 3: Paged Query (Traditional Service)

```csharp
// GET /api/transactions?accountId=5&page=1&pageSize=20

var result = await _transactionQueryService.GetByAccountIdAsync(
    accountId: 5,
    page: 1,
    pageSize: 20);

// Result contains:
// - Items (20 transactions)
// - TotalCount (total transactions for account)
// - Page (1)
// - PageSize (20)
// - TotalPages (calculated)
// - HasPrevious (false)
// - HasNext (true if more pages)

return Ok(result);
```

---

## Benefits Achieved

### ✅ Hybrid Approach Benefits

**Simple CRUD remains simple:**
- `GetById` → direct query, no ceremony
- Easy to understand and maintain
- Fast to write

**Complex operations get structure:**
- Validation automatic (FluentValidation)
- Logging automatic (LoggingBehavior)
- Clear separation of concerns
- Testable business logic

### ✅ Clean Architecture

**Layers clearly separated:**
- Contracts (DTOs) - what API exposes
- Features (MediatR) - complex business logic
- Services (Traditional) - simple queries
- Behaviors (Pipeline) - cross-cutting concerns

### ✅ Testability

**Easy to test:**
- Validators: unit tests without database
- Handlers: unit tests with mocked DbContext
- Services: integration tests with real database
- Behaviors: unit tests with mocked handlers

### ✅ Maintainability

**Clear patterns:**
- Need complex logic? Create MediatR handler
- Need simple query? Create traditional service
- Need validation? Add FluentValidation validator
- Need cross-cutting concern? Add pipeline behavior

---

## Next Steps

**Phase 7: Finance API Layer**

Will create:
1. ASP.NET Core Web API project
2. Controllers using both MediatR and Services:
   ```csharp
   public class TransactionsController
   {
       private readonly IMediator _mediator;                    // Complex operations
       private readonly ITransactionQueryService _queryService; // Simple queries

       [HttpPost] // Complex → MediatR
       public async Task<ActionResult> Create(CreateTransactionCommand command)
           => Ok(await _mediator.Send(command));

       [HttpGet("{id}")] // Simple → Service
       public async Task<ActionResult> GetById(int id)
           => Ok(await _queryService.GetByIdAsync(id));
   }
   ```

3. Dependency injection configuration
4. Serilog integration (BuildingBlocks.Logging)
5. Swagger/OpenAPI documentation

---

## Build Verification

✅ **Finance.Application builds successfully**
```
dotnet build
Compilazione completata.
    Avvisi: 0
    Errori: 0
```

---

## Files Summary

| Category | Files | Lines (approx) |
|----------|-------|----------------|
| Contracts/DTOs | 6 | 150 |
| Mapping | 1 | 50 |
| Interfaces | 2 | 40 |
| Services | 2 | 200 |
| MediatR Features | 3 | 250 |
| Pipeline Behaviors | 2 | 120 |
| **Total** | **16** | **~810** |

---

**Status:** ✅ Phase 6 Complete
**Ready For:** Phase 7 - Finance API Layer
**Build:** ✅ Success (0 warnings, 0 errors)
