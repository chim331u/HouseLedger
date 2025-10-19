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
| **Phase 7** | **Finance Service - API Layer** | ✅ **Completed** | Oct 19, 2025 | Oct 19, 2025 | Minimal APIs with versioning |
| 7.1 | Create Finance.Api project | ✅ Completed | Oct 19 | Oct 19 | Minimal APIs (not controllers) |
| 7.2 | Configure Serilog integration | ✅ Completed | Oct 19 | Oct 19 | BuildingBlocks.Logging integrated |
| 7.3 | Configure MediatR + DI | ✅ Completed | Oct 19 | Oct 19 | Full DI setup with pipeline behaviors |
| 7.4 | Create Minimal API endpoints | ✅ Completed | Oct 19 | Oct 19 | Transactions, Accounts with URL versioning |
| 7.5 | Add Swagger/OpenAPI | ✅ Completed | Oct 19 | Oct 19 | SwaggerGen configured |
| 7.6 | Create appsettings files | ✅ Completed | Oct 19 | Oct 19 | Development, Production, base config |
| 7.7 | Add global exception handling | ✅ Completed | Oct 19 | Oct 19 | RFC 7807 ProblemDetails |
| 7.8 | Add CORS + health checks | ✅ Completed | Oct 19 | Oct 19 | Flexible CORS, /health endpoints |
| 7.9 | Add request/response logging | ✅ Completed | Oct 19 | Oct 19 | Custom middleware with Serilog |
| | | | | | |
| **Phase 8** | **Ancillary Service (Currency, Country)** | 🔲 **Not Started** | - | - | Separate service for reference data |
| 8.1 | Create Ancillary.Domain project | 🔲 Not Started | - | - | Currency, Country entities |
| 8.2 | Create Ancillary.Infrastructure project | 🔲 Not Started | - | - | AncillaryDbContext + EF configurations |
| 8.3 | Create Ancillary.Application project | 🔲 Not Started | - | - | Query services + CRUD commands |
| 8.4 | Create Ancillary.Api project | 🔲 Not Started | - | - | Minimal APIs |
| 8.5 | Create Currency CRUD | 🔲 Not Started | - | - | Domain, Commands, Services, API |
| 8.6 | Create Country CRUD | 🔲 Not Started | - | - | Domain, Commands, Services, API |
| 8.7 | Configure DI and Serilog | 🔲 Not Started | - | - | Full service configuration |
| 8.8 | Test Ancillary API | 🔲 Not Started | - | - | Verify all endpoints work |
| | | | | | |
| **Phase 9** | **Complete Finance CRUD** | 🔲 **Not Started** | - | - | Bank, Account, Transaction, Balance |
| 9.1 | Complete Bank CRUD | 🔲 Not Started | - | - | Create, Update, Delete commands + API |
| 9.2 | Complete Account CRUD | 🔲 Not Started | - | - | Create, Update, Delete commands + API |
| 9.3 | Complete Transaction CRUD | 🔲 Not Started | - | - | Update, Delete commands + API |
| 9.4 | Implement CSV Upload | 🔲 Not Started | - | - | Bulk transaction import |
| 9.5 | Implement Manual Categorization | 🔲 Not Started | - | - | Set category on transaction |
| 9.6 | Complete Balance CRUD | 🔲 Not Started | - | - | Full CRUD + query service |
| | | | | | |
| **Phase 10** | **ML.NET Integration** | 🔲 **Not Started** | - | - | Transaction categorization |
| 10.1 | Add ML.NET packages | 🔲 Not Started | - | - | Microsoft.ML |
| 10.2 | Create ML model training | 🔲 Not Started | - | - | Train on confirmed categories |
| 10.3 | Create prediction service | 🔲 Not Started | - | - | Categorize transactions |
| 10.4 | Create ML API endpoints | 🔲 Not Started | - | - | Train, CategorizeAll |
| | | | | | |
| **Phase 11** | **Bills Service** | 🔲 **Not Started** | - | - | Bills management |
| 11.1 | Create Bills.Domain | 🔲 Not Started | - | - | Bill, Supplier, ReadInBill entities |
| 11.2 | Create Bills.Infrastructure | 🔲 Not Started | - | - | BillsDbContext |
| 11.3 | Create Bills.Application | 🔲 Not Started | - | - | Features + Services |
| 11.4 | Create Bills.Api | 🔲 Not Started | - | - | API endpoints |
| | | | | | |
| **Phase 12** | **Salary Service** | 🔲 **Not Started** | - | - | Salary tracking |
| 12.1 | Create Salary.Domain | 🔲 Not Started | - | - | Salary entity |
| 12.2 | Create Salary.Infrastructure | 🔲 Not Started | - | - | SalaryDbContext |
| 12.3 | Create Salary.Application | 🔲 Not Started | - | - | Features + Services |
| 12.4 | Create Salary.Api | 🔲 Not Started | - | - | API endpoints |
| | | | | | |
| **Phase 13** | **Identity Service** | 🔲 **Not Started** | - | - | Authentication & Authorization |
| 13.1 | Create Identity.Domain | 🔲 Not Started | - | - | User entities |
| 13.2 | Create Identity.Infrastructure | 🔲 Not Started | - | - | IdentityDbContext + JWT |
| 13.3 | Create Identity.Application | 🔲 Not Started | - | - | Login, Register, RefreshToken |
| 13.4 | Create Identity.Api | 🔲 Not Started | - | - | Auth endpoints |
| | | | | | |
| **Phase 14** | **Other Services** | 🔲 **Not Started** | - | - | Remaining services |
| 14.1 | Create HouseThings service | 🔲 Not Started | - | - | Inventory management |
| 14.2 | Create Statistics service | 🔲 Not Started | - | - | Dashboard, analytics |
| | | | | | |
| **Phase 15** | **Deployment Preparation** | 🔲 **Not Started** | - | - | ARM32 NAS deployment |
| 15.1 | Create Dockerfile (ARM32) | 🔲 Not Started | - | - | linux-arm base image |
| 15.2 | Create docker-compose.yml | 🔲 Not Started | - | - | Monolith configuration |
| 15.3 | Configure volume mounts | 🔲 Not Started | - | - | Data, logs, bills paths |
| 15.4 | Test on ARM32 environment | 🔲 Not Started | - | - | QEMU or Raspberry Pi |
| 15.5 | Optimize for 1GB RAM | 🔲 Not Started | - | - | Memory limits, monitoring |
| 15.6 | Create deployment guide | 🔲 Not Started | - | - | Documentation for NAS |
| | | | | | |
| **Phase 16** | **Testing & Quality** | 🔲 **Not Started** | - | - | Comprehensive testing |
| 16.1 | Unit tests for handlers | 🔲 Not Started | - | - | Business logic tests |
| 16.2 | Integration tests for repositories | 🔲 Not Started | - | - | Database tests |
| 16.3 | API tests for endpoints | 🔲 Not Started | - | - | Controller tests |
| 16.4 | E2E tests for scenarios | 🔲 Not Started | - | - | Full workflow tests |
| | | | | | |
| **Phase 17** | **Frontend Migration** | 🔲 **Not Started** | - | - | Blazor Web + MAUI |
| 17.1 | Update Blazor Web | 🔲 Not Started | - | - | API client updates |
| 17.2 | Update MAUI app | 🔲 Not Started | - | - | Mobile app updates |
| 17.3 | Add SignalR for notifications | 🔲 Not Started | - | - | Real-time updates |

**Legend:**
- ✅ Completed
- 🔄 In Progress
- 🔲 Not Started
- ⚠️ Blocked
- ❌ Cancelled

---

## 🎯 Current Focus: Phase 8 - Ancillary Service (Currency, Country)

### What We Just Completed (Phase 7)

✅ **Finance.Api Project** created with:
- **Minimal APIs** - Modern .NET approach with lambda-based endpoints
- **API Versioning** - URL path versioning (/api/v1/)
- **Swagger/OpenAPI** - Basic Swagger UI for Development
- **Global Exception Handling** - RFC 7807 ProblemDetails
- **Health Checks** - /health, /health/live, /health/ready
- **Request/Response Logging** - Custom middleware with Serilog
- **Endpoints:** Transactions (4), Accounts (3)

### What's Next (Phase 8 - Ancillary Service)

**Why Separate Ancillary Service?**
- Currency is a foreign key in Bank entity (Finance service dependency)
- Country is reference data shared across services
- Clean separation of concerns (reference data vs transactional data)
- Can be reused by other services (Bills, Salary, etc.)

**Phase 8 Tasks:**
1. Create **Ancillary.Domain** project - Currency, Country entities
2. Create **Ancillary.Infrastructure** project - AncillaryDbContext, EF configurations, points to MM.db
3. Create **Ancillary.Application** project - Query services, CRUD commands (MediatR), DTOs
4. Create **Ancillary.Api** project - Minimal APIs for /api/v1/currencies and /api/v1/countries
5. Configure DI, Serilog, Swagger, health checks
6. Test all CRUD endpoints

**No Seed Data:**
- Currencies and countries will be added manually via API
- Keeps database migration simple
- User has full control over reference data

**After Phase 8:**
- Phase 9: Complete Finance CRUD (Bank depends on Currency being available)
- Phase 10: ML.NET Integration

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
│   │   ├── HouseLedger.Services.Finance/
│   │   │   ├── Finance.Domain/                   ✅ Completed
│   │   │   ├── Finance.Infrastructure/           ✅ Completed
│   │   │   ├── Finance.Application/              ✅ Completed
│   │   │   └── Finance.Api/                      ✅ Completed
│   │   │
│   │   └── HouseLedger.Services.Ancillary/
│   │       ├── Ancillary.Domain/                 🔄 Phase 8
│   │       ├── Ancillary.Infrastructure/         🔄 Phase 8
│   │       ├── Ancillary.Application/            🔄 Phase 8
│   │       └── Ancillary.Api/                    🔄 Phase 8
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

**Total Phases:** 17 (restructured from 15)
**Completed Phases:** 7 (41%)
**In Progress:** 0
**Not Started:** 10 (59%)

**Total Tasks:** 99 (Phase 8: Ancillary +8, Phase 9: Finance CRUD +6, Phase 10: ML.NET +4)
**Completed Tasks:** 49 (49%)
**In Progress:** 0
**Not Started:** 50 (51%)

**Lines of Code (Estimate):**
- Completed: ~3,500 lines
- Total Expected: ~15,000 lines

**Projects Created:** 6
- Core.Domain ✅
- Finance.Domain ✅
- Finance.Infrastructure ✅
- Finance.Application ✅
- Finance.Api ✅
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
