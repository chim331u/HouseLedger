# HouseLedger - Development Plan

**Project:** HouseLedger - Personal Finance Management System
**Target Framework:** .NET 8.0
**Architecture:** Vertical Slice Architecture with Hybrid MediatR
**Deployment:** QNAP TS-231P NAS (ARM32, 1GB RAM)
**Database:** SQLite
**Philosophy:** Simple Made Easy (Rich Hickey)

**Last Updated:** October 19, 2025

---

## 📊 Implementation Status Summary

| Phase | Task | Status | Start Date | Completion Date | Notes |
|-------|------|--------|------------|-----------------|-------|
| **Phase 0** | **Preparation & Planning** | ✅ **Completed** | Oct 18, 2025 | Oct 18, 2025 | Architecture designed |
| 0.1 | Analyze old MM project | ✅ Completed | Oct 18 | Oct 18 | Documented in old_mm_project_claude.md |
| 0.2 | Design vertical slice architecture | ✅ Completed | Oct 18 | Oct 18 | Documented in architecture-vertical-slices.md |
| 0.3 | Plan database migration strategy | ✅ Completed | Oct 18 | Oct 18 | Separate DbContext per service, single MM.db file |
| 0.4 | Create solution structure | ✅ Completed | Oct 18 | Oct 18 | HouseLedger.sln with proper folders |
| | | | | | |
| **Phase 1** | **Core Domain Foundation** | ✅ **Completed** | Oct 18, 2025 | Oct 18, 2025 | Shared kernel established |
| 1.1 | Create Core.Domain project | ✅ Completed | Oct 18 | Oct 18 | BaseEntity, AuditableEntity, ValueObject |
| 1.2 | Define base entity classes | ✅ Completed | Oct 18 | Oct 18 | Inheritance hierarchy designed |
| 1.3 | Create domain interfaces | ✅ Completed | Oct 18 | Oct 18 | IEntity, IAuditable |
| | | | | | |
| **Phase 2** | **Finance Service - Domain** | ✅ **Completed** | Oct 18, 2025 | Oct 18, 2025 | 5 entities + 1 value object |
| 2.1 | Create Finance.Domain project | ✅ Completed | Oct 18 | Oct 18 | Domain layer established |
| 2.2 | Migrate Bank entity | ✅ Completed | Oct 18 | Oct 18 | From BankMasterData |
| 2.3 | Migrate Account entity | ✅ Completed | Oct 18 | Oct 18 | Improved naming: Conto → AccountNumber |
| 2.4 | Migrate Transaction entity | ✅ Completed | Oct 18 | Oct 18 | TxnDate → TransactionDate, TxnAmount → Amount |
| 2.5 | Migrate Balance entity | ✅ Completed | Oct 18 | Oct 18 | BalanceValue → Amount |
| 2.6 | Migrate Card entity | ✅ Completed | Oct 18 | Oct 18 | Added proper relationships |
| 2.7 | Create TransactionCategory value object | ✅ Completed | Oct 18 | Oct 18 | Combines Area + IsCatConfirmed |
| | | | | | |
| **Phase 3** | **Finance Service - Infrastructure** | ✅ **Completed** | Oct 18, 2025 | Oct 18, 2025 | EF Core 8 with ComplexProperty |
| 3.1 | Create Finance.Infrastructure project | ✅ Completed | Oct 18 | Oct 18 | Infrastructure layer |
| 3.2 | Create FinanceDbContext | ✅ Completed | Oct 18 | Oct 18 | Points to existing MM.db |
| 3.3 | Create EF Core configurations | ✅ Completed | Oct 18 | Oct 18 | 5 configurations with column mappings |
| 3.4 | Map TransactionCategory value object | ✅ Completed | Oct 18 | Oct 18 | Used EF Core 8 ComplexProperty |
| 3.5 | Test with existing database | ✅ Completed | Oct 18 | Oct 18 | Tested with 6,959 transactions |
| 3.6 | Fix EF Core mapping issues | ✅ Completed | Oct 18 | Oct 18 | Resolved ComplexProperty + BankId issues |
| | | | | | |
| **Phase 4** | **Logging Infrastructure** | ✅ **Completed** | Oct 19, 2025 | Oct 19, 2025 | Serilog with File + Console |
| 4.1 | Create BuildingBlocks.Logging project | ✅ Completed | Oct 19 | Oct 19 | Serilog configuration |
| 4.2 | Configure File + Console output | ✅ Completed | Oct 19 | Oct 19 | Text to console, JSON to file |
| 4.3 | Environment-specific configuration | ✅ Completed | Oct 19 | Oct 19 | Development vs Production settings |
| 4.4 | Implement sensitive data masking | ✅ Completed | Oct 19 | Oct 19 | Production-only feature |
| 4.5 | Create appsettings examples | ✅ Completed | Oct 19 | Oct 19 | Dev and Prod templates |
| 4.6 | Document Serilog usage | ✅ Completed | Oct 19 | Oct 19 | Comprehensive README |
| | | | | | |
| **Phase 5** | **Hybrid MediatR Planning** | ✅ **Completed** | Oct 19, 2025 | Oct 19, 2025 | Strategy defined |
| 5.1 | Analyze MediatR pros/cons | ✅ Completed | Oct 19 | Oct 19 | Detailed analysis documented |
| 5.2 | Design hybrid approach | ✅ Completed | Oct 19 | Oct 19 | MediatR for complex, Services for simple |
| 5.3 | Create implementation plan | ✅ Completed | Oct 19 | Oct 19 | hybrid-mediator-implementation-plan.md |
| 5.4 | Define decision matrix | ✅ Completed | Oct 19 | Oct 19 | When to use MediatR vs Services |
| | | | | | |
| **Phase 6** | **Finance Service - Application Layer** | ✅ **Completed** | Oct 19, 2025 | Oct 19, 2025 | Hybrid MediatR + Services |
| 6.1 | Create Finance.Application project | ✅ Completed | Oct 19 | Oct 19 | Application layer |
| 6.2 | Add NuGet packages | ✅ Completed | Oct 19 | Oct 19 | MediatR 12.4.1, FluentValidation 11.11.0, AutoMapper 13.0.1 |
| 6.3 | Create DTOs/Contracts | ✅ Completed | Oct 19 | Oct 19 | 6 DTOs with paging support |
| 6.4 | Create MediatR handlers (complex) | ✅ Completed | Oct 19 | Oct 19 | CreateTransactionCommand + Handler + Validator |
| 6.5 | Create traditional services (simple) | ✅ Completed | Oct 19 | Oct 19 | AccountQueryService, TransactionQueryService |
| 6.6 | Create pipeline behaviors | ✅ Completed | Oct 19 | Oct 19 | LoggingBehavior, ValidationBehavior |
| 6.7 | Create AutoMapper profile | ✅ Completed | Oct 19 | Oct 19 | FinanceMappingProfile with Value Object handling |
| 6.8 | Add Serilog logging to infrastructure | ✅ Completed | Oct 19 | Oct 19 | FinanceDbContext logging, TestConsole updated |
| | | | | | |
| **Phase 7** | **Finance Service - API Layer** | 🔲 **Not Started** | - | - | ASP.NET Core Web API |
| 7.1 | Create Finance.Api project | 🔲 Not Started | - | - | Web API project |
| 7.2 | Configure Serilog integration | 🔲 Not Started | - | - | Use BuildingBlocks.Logging |
| 7.3 | Configure MediatR + DI | 🔲 Not Started | - | - | Dependency injection setup |
| 7.4 | Create controllers | 🔲 Not Started | - | - | Transactions, Accounts, Balances |
| 7.5 | Add Swagger/OpenAPI | 🔲 Not Started | - | - | API documentation |
| 7.6 | Create appsettings files | 🔲 Not Started | - | - | Development, Production |
| 7.7 | Test API endpoints | 🔲 Not Started | - | - | Manual testing |
| | | | | | |
| **Phase 8** | **Finance Service - Features** | 🔲 **Not Started** | - | - | Implement business logic |
| 8.1 | Implement CreateTransaction (MediatR) | 🔲 Not Started | - | - | Command + Handler + Validator |
| 8.2 | Implement UploadTransactions (MediatR) | 🔲 Not Started | - | - | CSV upload handler |
| 8.3 | Implement CategorizeTransaction (MediatR) | 🔲 Not Started | - | - | ML categorization |
| 8.4 | Implement GetTransactions (Service) | 🔲 Not Started | - | - | Query service |
| 8.5 | Implement GetAccounts (Service) | 🔲 Not Started | - | - | Query service |
| 8.6 | Implement GetBalances (Service) | 🔲 Not Started | - | - | Query service |
| | | | | | |
| **Phase 9** | **Bills Service** | 🔲 **Not Started** | - | - | Bills management |
| 9.1 | Create Bills.Domain | 🔲 Not Started | - | - | Bill, Supplier, ReadInBill entities |
| 9.2 | Create Bills.Infrastructure | 🔲 Not Started | - | - | BillsDbContext |
| 9.3 | Create Bills.Application | 🔲 Not Started | - | - | Features + Services |
| 9.4 | Create Bills.Api | 🔲 Not Started | - | - | API endpoints |
| | | | | | |
| **Phase 10** | **Salary Service** | 🔲 **Not Started** | - | - | Salary tracking |
| 10.1 | Create Salary.Domain | 🔲 Not Started | - | - | Salary entity |
| 10.2 | Create Salary.Infrastructure | 🔲 Not Started | - | - | SalaryDbContext |
| 10.3 | Create Salary.Application | 🔲 Not Started | - | - | Features + Services |
| 10.4 | Create Salary.Api | 🔲 Not Started | - | - | API endpoints |
| | | | | | |
| **Phase 11** | **Identity Service** | 🔲 **Not Started** | - | - | Authentication & Authorization |
| 11.1 | Create Identity.Domain | 🔲 Not Started | - | - | User entities |
| 11.2 | Create Identity.Infrastructure | 🔲 Not Started | - | - | IdentityDbContext + JWT |
| 11.3 | Create Identity.Application | 🔲 Not Started | - | - | Login, Register, RefreshToken |
| 11.4 | Create Identity.Api | 🔲 Not Started | - | - | Auth endpoints |
| | | | | | |
| **Phase 12** | **Other Services** | 🔲 **Not Started** | - | - | Remaining services |
| 12.1 | Create HouseThings service | 🔲 Not Started | - | - | Inventory management |
| 12.2 | Create Ancillary service | 🔲 Not Started | - | - | Reference data |
| 12.3 | Create Statistics service | 🔲 Not Started | - | - | Dashboard, analytics |
| | | | | | |
| **Phase 13** | **Deployment Preparation** | 🔲 **Not Started** | - | - | ARM32 NAS deployment |
| 13.1 | Create Dockerfile (ARM32) | 🔲 Not Started | - | - | linux-arm base image |
| 13.2 | Create docker-compose.yml | 🔲 Not Started | - | - | Monolith configuration |
| 13.3 | Configure volume mounts | 🔲 Not Started | - | - | Data, logs, bills paths |
| 13.4 | Test on ARM32 environment | 🔲 Not Started | - | - | QEMU or Raspberry Pi |
| 13.5 | Optimize for 1GB RAM | 🔲 Not Started | - | - | Memory limits, monitoring |
| 13.6 | Create deployment guide | 🔲 Not Started | - | - | Documentation for NAS |
| | | | | | |
| **Phase 14** | **Testing & Quality** | 🔲 **Not Started** | - | - | Comprehensive testing |
| 14.1 | Unit tests for handlers | 🔲 Not Started | - | - | Business logic tests |
| 14.2 | Integration tests for repositories | 🔲 Not Started | - | - | Database tests |
| 14.3 | API tests for endpoints | 🔲 Not Started | - | - | Controller tests |
| 14.4 | E2E tests for scenarios | 🔲 Not Started | - | - | Full workflow tests |
| | | | | | |
| **Phase 15** | **Frontend Migration** | 🔲 **Not Started** | - | - | Blazor Web + MAUI |
| 15.1 | Update Blazor Web | 🔲 Not Started | - | - | API client updates |
| 15.2 | Update MAUI app | 🔲 Not Started | - | - | Mobile app updates |
| 15.3 | Add SignalR for notifications | 🔲 Not Started | - | - | Real-time updates |

**Legend:**
- ✅ Completed
- 🔄 In Progress
- 🔲 Not Started
- ⚠️ Blocked
- ❌ Cancelled

---

## 🎯 Current Focus: Phase 7 - Finance API Layer

### What We Just Completed (Phase 6)

✅ **Finance.Application Project** created with:
- **Contracts/DTOs** - 6 DTOs with paging support (PagedRequest, PagedResult, TransactionDto, AccountDto, BalanceDto, CreateTransactionRequest)
- **AutoMapper Profile** - Entity ↔ DTO mappings with Value Object handling
- **Traditional Services** - AccountQueryService, TransactionQueryService (simple CRUD)
- **MediatR Handler** - CreateTransactionCommand with validation and business logic
- **Pipeline Behaviors** - LoggingBehavior, ValidationBehavior (automatic for all requests)
- **Serilog Integration** - Added logging to FinanceDbContext and TestConsole

### Next Steps (Immediate - Phase 7)

1. **Create Finance.Api Project**
   ```bash
   dotnet new webapi -n HouseLedger.Services.Finance.Api -f net8.0 \
     -o src/Services/HouseLedger.Services.Finance/HouseLedger.Services.Finance.Api
   ```

2. **Configure Dependencies**
   - Add reference to Finance.Application
   - Add reference to BuildingBlocks.Logging
   - Configure MediatR in DI
   - Configure AutoMapper in DI
   - Configure Serilog using BuildingBlocks.Logging

3. **Create Controllers (Hybrid Approach)**
   ```csharp
   public class TransactionsController
   {
       private readonly IMediator _mediator;                    // For complex operations
       private readonly ITransactionQueryService _queryService; // For simple queries

       [HttpPost] // Complex → MediatR
       public async Task<ActionResult> Create(CreateTransactionCommand command)
           => Ok(await _mediator.Send(command));

       [HttpGet("{id}")] // Simple → Service
       public async Task<ActionResult> GetById(int id)
           => Ok(await _queryService.GetByIdAsync(id));
   }
   ```

4. **Configure Swagger/OpenAPI**
   - Add XML documentation
   - Configure API versioning

---

## 🏗️ Architecture Overview

### Project Structure

```
HouseLedger/
├── src/
│   ├── Core/
│   │   └── HouseLedger.Core.Domain/              ✅ Completed
│   │
│   ├── Services/
│   │   └── HouseLedger.Services.Finance/
│   │       ├── Finance.Domain/                   ✅ Completed
│   │       ├── Finance.Infrastructure/           ✅ Completed
│   │       ├── Finance.Application/              🔄 Next
│   │       └── Finance.Api/                      🔲 Pending
│   │
│   └── BuildingBlocks/
│       └── HouseLedger.BuildingBlocks.Logging/   ✅ Completed
│
├── tools/
│   └── HouseLedger.TestConsole/                  ✅ Completed
│
└── docs/
    ├── claude.md                                  ✅ Base rules
    ├── development-plan.md                        ✅ This file
    ├── development-plan-done.md                   🔄 Archive
    ├── architecture-vertical-slices.md            ✅ Architecture
    ├── simple-mindset.md                          ✅ Philosophy
    └── old_mm_project_claude.md                   ✅ Old project info
```

### Technology Stack

**Backend:**
- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core 8 (SQLite)
- MediatR (CQRS)
- FluentValidation
- AutoMapper
- Serilog

**Frontend:**
- Blazor WebAssembly
- .NET MAUI (mobile)

**Deployment:**
- Docker (ARM32v7)
- QNAP TS-231P NAS (ARM32, 1GB RAM)

**Database:**
- SQLite (single file: MM.db)
- Separate DbContext per service

---

## 📖 Key Documents Reference

| Document | Purpose | Status |
|----------|---------|--------|
| claude.md | Base rules + NAS deployment specs | ✅ Active |
| development-plan.md | Current plan (this file) | ✅ Active |
| development-plan-done.md | Completed implementation archive | ✅ Active |
| architecture-vertical-slices.md | Architecture design | ✅ Reference |
| simple-mindset.md | Development philosophy | ✅ Reference |
| old_mm_project_claude.md | Old project documentation | ✅ Reference |
| hybrid-mediator-implementation-plan.md | MediatR strategy | ⚠️ Will be archived |

---

## 🔑 Key Architectural Decisions

### ADR-001: Vertical Slice Architecture
- Organize by feature/capability, not technical layers
- Each feature has all code in one folder

### ADR-002: Separate DbContext per Service
- Each service owns its data
- All point to same MM.db file
- True service independence

### ADR-003: Hybrid MediatR Approach
- **Complex features** → MediatR (multi-step, business logic, side effects)
- **Simple CRUD** → Traditional Services (get by ID, lists, basic operations)

### ADR-004: Keep Existing Database Schema
- Map new clean entities to old table/column names
- No data migration required
- Can transition gradually

### ADR-005: Selective Value Objects
- Use Value Objects only where they add real value
- TransactionCategory: Type safety + validation
- Keep simple types (double, string) for others

### ADR-006: ARM32 Deployment (NAS)
- Build for linux-arm runtime
- Use ARM32v7 Docker images
- Memory limit: 512MB max per container

### ADR-007: Monolith Deployment
- Single application, not microservices
- 1GB RAM constraint requires efficiency
- All services in one process

### ADR-008: Serilog Logging
- File + Console output
- Environment-specific configuration
- Sensitive data masking in production

---

## 📊 Progress Metrics

**Total Phases:** 15
**Completed Phases:** 6 (40%)
**In Progress:** 0
**Not Started:** 9 (60%)

**Total Tasks:** 80
**Completed Tasks:** 40 (50%)
**In Progress:** 0
**Not Started:** 40 (50%)

**Lines of Code (Estimate):**
- Completed: ~2,500 lines
- Total Expected: ~15,000 lines

**Projects Created:** 5
- Core.Domain ✅
- Finance.Domain ✅
- Finance.Infrastructure ✅
- Finance.Application ✅
- BuildingBlocks.Logging ✅

---

## 🚀 Timeline Estimate

| Phase | Duration | Dependencies | Risk Level |
|-------|----------|--------------|------------|
| Phase 6: Finance Application | 3-5 days | Phase 2, 3 | Low |
| Phase 7: Finance API | 2-3 days | Phase 6 | Low |
| Phase 8: Finance Features | 5-7 days | Phase 7 | Medium |
| Phase 9-12: Other Services | 15-20 days | Phase 8 | Medium |
| Phase 13: Deployment Prep | 3-5 days | Phase 12 | High (ARM32) |
| Phase 14: Testing | 5-7 days | Phase 13 | Low |
| Phase 15: Frontend | 7-10 days | Phase 14 | Medium |

**Total Estimated Time:** 40-57 days (~8-12 weeks)

---

## ⚠️ Risks & Mitigations

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| ARM32 compatibility issues | High | Medium | Test early on ARM32 device (Raspberry Pi) |
| 1GB RAM insufficient | High | Low | Monitor memory, optimize, use monolith |
| EF Core mapping complexity | Medium | Low | Already tested, use configurations |
| Value Object mapping issues | Medium | Low | Already solved with ComplexProperty |
| Frontend breaking changes | Medium | Medium | Version APIs, use DTOs |

---

## 📝 Notes

### Important Decisions Made

1. **Data Types:** Keep `double` for money (not `decimal`) - user preference
2. **Database:** Single MM.db file, separate DbContext per service
3. **Table Names:** Keep old names (TX_Transaction, MM_AccountMasterData, etc.)
4. **Column Mappings:** Handle in EF Core configurations
5. **Value Objects:** Selective use (TransactionCategory only for now)
6. **Deployment:** ARM32 NAS with 1GB RAM constraint
7. **Architecture:** Hybrid MediatR (pragmatic approach)

### Success Criteria

- ✅ Finance Domain compiles without errors
- ✅ Finance Infrastructure compiles without errors
- ✅ Maps to existing database successfully
- ✅ TransactionCategory Value Object works
- ✅ Serilog logging configured
- ⏳ Finance API runs successfully
- ⏳ All CRUD operations work
- ⏳ Complex features implemented (upload, categorize)
- ⏳ Deploys to ARM32 NAS
- ⏳ Runs within 512MB memory limit

---

**Last Updated:** October 19, 2025
**Version:** 1.0
**Status:** Active Development - Phase 6 Starting
