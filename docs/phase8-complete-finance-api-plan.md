# Phase 8: Complete Finance API - Implementation Plan

**Date:** October 19, 2025
**Status:** 📋 Planning
**Based on:** Old MM_RESTAPI analysis

---

## 🎯 Overview

This phase completes the Finance API by implementing all CRUD operations for Banks, Accounts, Transactions, and Balances, matching the functionality of the old MM_RESTAPI while using the new Hybrid MediatR architecture.

**Dependencies:**
- ✅ Phase 7 Complete (Basic API structure exists)
- ⚠️ **Requires:** Ancillary Service (Currency, Country) - **Create first**

---

## 📊 Old API Analysis

### Controllers Found in mm_restapi/MM_RESTAPI/Controllers/

| Controller | Endpoints | Service | Notes |
|------------|-----------|---------|-------|
| **BankController** | GetList, Get, Add, Update, Delete | IBankAccountService | ✅ Simple CRUD |
| **AccountController** | GetList, Get, Add, Update, Delete | IBankAccountService | ✅ Simple CRUD |
| **TransactionController** | GetList, Get, Add, Update, Delete, **UploadCsv**, **Categorize**, **Train** | ITransactionService | ⚠️ Complex (ML features) |
| **BalanceController** | GetList, Get, Add, Update, Delete | IBalanceService | ✅ Simple CRUD |
| **CurrencyController** | GetList, Get, Add, Update, Delete | IAncillaryService | ⚠️ **Dependency** |
| **CountryController** | GetList, Get, Add, Update, Delete | IAncillaryService | ⚠️ **Dependency** |

### Key Findings:

1. **Ancillary Service is a dependency** - Currency and Country are required for Banks
2. **Transaction has ML features** - Categorization and training need ML.NET
3. **CSV Upload** - Bulk import transactions
4. **All use soft delete** - DeleteX endpoints set IsActive = false

---

## 🏗️ Architecture Decision

### Hybrid MediatR Strategy

**Simple CRUD (Traditional Services):**
- ✅ Get by ID
- ✅ Get all (list)
- ✅ Get by parent (e.g., accounts by bank)

**Complex Operations (MediatR Commands):**
- ⚠️ Create (validation + business logic)
- ⚠️ Update (validation + optimistic concurrency)
- ⚠️ Delete (soft delete + validation)
- ⚠️ Upload CSV (bulk operations + duplicate detection)
- ⚠️ Categorize (ML integration)

---

## 📋 Implementation Order

### Phase 8.1: Ancillary Service (Currency, Country) - **REQUIRED FIRST**

**Why First:**
- Bank entity has Currency foreign key
- Country is reference data needed for setup
- Both are simple CRUD (no complex logic)

**Components:**
1. **Ancillary.Domain** (if needed) or add to Finance.Domain
   - Currency entity
   - Country entity
2. **Ancillary.Infrastructure** or extend Finance.Infrastructure
   - DbContext configuration
   - Seed data (common currencies/countries)
3. **Ancillary.Application** or extend Finance.Application
   - Traditional services only (simple CRUD)
   - CurrencyQueryService, CountryQueryService
4. **Ancillary.Api** or extend Finance.Api
   - Minimal API endpoints
   - GET /api/v1/currencies, GET /api/v1/currencies/{id}
   - GET /api/v1/countries, GET /api/v1/countries/{id}

**Decision:** **Add to existing Finance service** (simpler, less overhead)
- Entities in Finance.Domain
- Configurations in Finance.Infrastructure
- Query services in Finance.Application
- Endpoints in Finance.Api

---

### Phase 8.2: Complete Bank API

**Current Status:**
- ✅ BankDto exists
- ❌ Only query endpoints exist
- ❌ No CRUD commands

**To Implement:**

#### Application Layer

**MediatR Commands:**
1. **CreateBankCommand** + Handler + Validator
   - Validation: Name required, Currency exists
   - Business logic: Check duplicate names
   - Returns: BankDto

2. **UpdateBankCommand** + Handler + Validator
   - Validation: ID exists, Name required
   - Business logic: Optimistic concurrency (audit fields)
   - Returns: BankDto

3. **DeleteBankCommand** + Handler + Validator
   - Validation: ID exists, no active accounts
   - Business logic: Soft delete (set IsActive = false)
   - Returns: Success result

**Traditional Services:**
- ✅ Already exists: BankQueryService.GetById, GetAll

#### API Endpoints

**Finance.Api/Endpoints/BankEndpoints.cs:**
```
POST   /api/v1/banks              - Create bank (MediatR)
PUT    /api/v1/banks/{id}         - Update bank (MediatR)
DELETE /api/v1/banks/{id}         - Delete bank (MediatR)
GET    /api/v1/banks              - Get all banks (Service) ✅ Already exists
GET    /api/v1/banks/{id}         - Get bank by ID (Service) ✅ Already exists
```

---

### Phase 8.3: Complete Account API

**Current Status:**
- ✅ AccountDto exists
- ✅ Query endpoints exist (GetById, GetAll, GetByBank)
- ❌ No CRUD commands

**To Implement:**

#### Application Layer

**MediatR Commands:**
1. **CreateAccountCommand** + Handler + Validator
   - Validation: Name required, AccountNumber unique, BankId exists
   - Business logic: Check duplicate account numbers per bank
   - Returns: AccountDto

2. **UpdateAccountCommand** + Handler + Validator
   - Validation: ID exists, Name required, BankId exists
   - Business logic: Optimistic concurrency, validate account number uniqueness
   - Returns: AccountDto

3. **DeleteAccountCommand** + Handler + Validator
   - Validation: ID exists, no active transactions
   - Business logic: Soft delete, check for dependencies
   - Returns: Success result

**Traditional Services:**
- ✅ Already exists: AccountQueryService.GetById, GetAll, GetByBankId

#### API Endpoints

**Finance.Api/Endpoints/AccountEndpoints.cs:**
```
POST   /api/v1/accounts           - Create account (MediatR)
PUT    /api/v1/accounts/{id}      - Update account (MediatR)
DELETE /api/v1/accounts/{id}      - Delete account (MediatR)
GET    /api/v1/accounts           - Get all accounts (Service) ✅ Already exists
GET    /api/v1/accounts/{id}      - Get account by ID (Service) ✅ Already exists
GET    /api/v1/accounts/bank/{bankId} - Get by bank (Service) ✅ Already exists
```

---

### Phase 8.4: Complete Transaction API

**Current Status:**
- ✅ TransactionDto exists
- ✅ CreateTransactionCommand exists (MediatR)
- ✅ Query endpoints exist (GetById, GetByAccount, GetRecent)
- ❌ No Update/Delete commands
- ❌ No CSV upload
- ❌ No categorization

**To Implement:**

#### Application Layer

**MediatR Commands:**
1. **UpdateTransactionCommand** + Handler + Validator
   - Validation: ID exists, Amount != 0, AccountId exists
   - Business logic: Update UniqueKey, check duplicates
   - Returns: TransactionDto

2. **DeleteTransactionCommand** + Handler + Validator
   - Validation: ID exists
   - Business logic: Soft delete (set IsActive = false)
   - Returns: Success result

3. **UploadTransactionsCsvCommand** + Handler + Validator
   - Input: IList<CreateTransactionRequest>
   - Business logic:
     - Validate each transaction
     - Generate UniqueKey for each
     - Check duplicates
     - Bulk insert
   - Returns: { Loaded: int, NotLoaded: int, Duplicated: int }

4. **CategorizeTransactionCommand** + Handler + Validator
   - Input: TransactionId, CategoryName
   - Business logic: Set Category, mark as confirmed
   - Returns: TransactionDto

5. **CategorizeAllTransactionsCommand** + Handler *(Future - ML.NET)*
   - Business logic: Run ML model on all uncategorized transactions
   - Returns: { Total: int, Categorized: int }

6. **TrainModelCommand** + Handler *(Future - ML.NET)*
   - Business logic: Train ML.NET model on confirmed transactions
   - Returns: { Success: bool, Message: string }

**Traditional Services:**
- ✅ Already exists: TransactionQueryService.GetById, GetByAccountId, GetRecent

#### API Endpoints

**Finance.Api/Endpoints/TransactionEndpoints.cs:**
```
POST   /api/v1/transactions                    - Create transaction (MediatR) ✅ Already exists
POST   /api/v1/transactions/upload-csv         - Upload CSV (MediatR)
PUT    /api/v1/transactions/{id}               - Update transaction (MediatR)
PUT    /api/v1/transactions/{id}/categorize    - Categorize transaction (MediatR)
PUT    /api/v1/transactions/categorize-all     - Categorize all (MediatR, ML)
PUT    /api/v1/transactions/train-model        - Train ML model (MediatR, ML)
DELETE /api/v1/transactions/{id}               - Delete transaction (MediatR)
GET    /api/v1/transactions/{id}               - Get by ID (Service) ✅ Already exists
GET    /api/v1/transactions/account/{id}       - Get by account (Service) ✅ Already exists
GET    /api/v1/transactions/recent             - Get recent (Service) ✅ Already exists
```

---

### Phase 8.5: Complete Balance API

**Current Status:**
- ✅ BalanceDto exists
- ❌ No query services
- ❌ No endpoints

**To Implement:**

#### Application Layer

**MediatR Commands:**
1. **CreateBalanceCommand** + Handler + Validator
   - Validation: BalanceDate required, AccountId exists
   - Business logic: Check if balance already exists for date
   - Returns: BalanceDto

2. **UpdateBalanceCommand** + Handler + Validator
   - Validation: ID exists, BalanceDate required
   - Returns: BalanceDto

3. **DeleteBalanceCommand** + Handler + Validator
   - Validation: ID exists
   - Business logic: Soft delete
   - Returns: Success result

**Traditional Services:**
1. **IBalanceQueryService** + BalanceQueryService
   - GetByIdAsync
   - GetAllAsync
   - GetByAccountIdAsync
   - GetLatestByAccountIdAsync (most recent balance for account)

#### API Endpoints

**Finance.Api/Endpoints/BalanceEndpoints.cs:**
```
POST   /api/v1/balances                        - Create balance (MediatR)
PUT    /api/v1/balances/{id}                   - Update balance (MediatR)
DELETE /api/v1/balances/{id}                   - Delete balance (MediatR)
GET    /api/v1/balances                        - Get all balances (Service)
GET    /api/v1/balances/{id}                   - Get balance by ID (Service)
GET    /api/v1/balances/account/{accountId}    - Get by account (Service)
GET    /api/v1/balances/account/{accountId}/latest - Get latest (Service)
```

---

## 📦 Files to Create

### Phase 8.1: Ancillary (Currency, Country)

**Finance.Domain:**
- Entities/Currency.cs (or use existing?)
- Entities/Country.cs

**Finance.Infrastructure:**
- Persistence/Configurations/CurrencyConfiguration.cs
- Persistence/Configurations/CountryConfiguration.cs
- Persistence/Seed/CurrencySeedData.cs
- Persistence/Seed/CountrySeedData.cs

**Finance.Application:**
- Interfaces/ICurrencyQueryService.cs
- Interfaces/ICountryQueryService.cs
- Services/CurrencyQueryService.cs
- Services/CountryQueryService.cs

**Finance.Api:**
- Endpoints/CurrencyEndpoints.cs
- Endpoints/CountryEndpoints.cs

**Total:** ~10 files

---

### Phase 8.2: Bank CRUD

**Finance.Application:**
- Features/Banks/CreateBank/CreateBankCommand.cs
- Features/Banks/CreateBank/CreateBankHandler.cs
- Features/Banks/CreateBank/CreateBankValidator.cs
- Features/Banks/UpdateBank/UpdateBankCommand.cs
- Features/Banks/UpdateBank/UpdateBankHandler.cs
- Features/Banks/UpdateBank/UpdateBankValidator.cs
- Features/Banks/DeleteBank/DeleteBankCommand.cs
- Features/Banks/DeleteBank/DeleteBankHandler.cs
- Features/Banks/DeleteBank/DeleteBankValidator.cs
- Contracts/Banks/CreateBankRequest.cs
- Contracts/Banks/UpdateBankRequest.cs

**Finance.Api:**
- Modify Endpoints/BankEndpoints.cs (add POST, PUT, DELETE)

**Total:** ~12 files (11 new + 1 modified)

---

### Phase 8.3: Account CRUD

**Finance.Application:**
- Features/Accounts/CreateAccount/CreateAccountCommand.cs
- Features/Accounts/CreateAccount/CreateAccountHandler.cs
- Features/Accounts/CreateAccount/CreateAccountValidator.cs
- Features/Accounts/UpdateAccount/UpdateAccountCommand.cs
- Features/Accounts/UpdateAccount/UpdateAccountHandler.cs
- Features/Accounts/UpdateAccount/UpdateAccountValidator.cs
- Features/Accounts/DeleteAccount/DeleteAccountCommand.cs
- Features/Accounts/DeleteAccount/DeleteAccountHandler.cs
- Features/Accounts/DeleteAccount/DeleteAccountValidator.cs
- Contracts/Accounts/CreateAccountRequest.cs
- Contracts/Accounts/UpdateAccountRequest.cs

**Finance.Api:**
- Modify Endpoints/AccountEndpoints.cs (add POST, PUT, DELETE)

**Total:** ~12 files (11 new + 1 modified)

---

### Phase 8.4: Transaction CRUD + Advanced

**Finance.Application:**
- Features/Transactions/UpdateTransaction/UpdateTransactionCommand.cs
- Features/Transactions/UpdateTransaction/UpdateTransactionHandler.cs
- Features/Transactions/UpdateTransaction/UpdateTransactionValidator.cs
- Features/Transactions/DeleteTransaction/DeleteTransactionCommand.cs
- Features/Transactions/DeleteTransaction/DeleteTransactionHandler.cs
- Features/Transactions/UploadCsv/UploadTransactionsCsvCommand.cs
- Features/Transactions/UploadCsv/UploadTransactionsCsvHandler.cs
- Features/Transactions/UploadCsv/UploadTransactionsCsvValidator.cs
- Features/Transactions/Categorize/CategorizeTransactionCommand.cs
- Features/Transactions/Categorize/CategorizeTransactionHandler.cs
- Contracts/Transactions/UpdateTransactionRequest.cs
- Contracts/Transactions/UploadCsvResult.cs

**Finance.Api:**
- Modify Endpoints/TransactionEndpoints.cs (add PUT, DELETE, POST upload, PUT categorize)

**Total:** ~13 files (12 new + 1 modified)

---

### Phase 8.5: Balance CRUD

**Finance.Application:**
- Features/Balances/CreateBalance/CreateBalanceCommand.cs
- Features/Balances/CreateBalance/CreateBalanceHandler.cs
- Features/Balances/CreateBalance/CreateBalanceValidator.cs
- Features/Balances/UpdateBalance/UpdateBalanceCommand.cs
- Features/Balances/UpdateBalance/UpdateBalanceHandler.cs
- Features/Balances/UpdateBalance/UpdateBalanceValidator.cs
- Features/Balances/DeleteBalance/DeleteBalanceCommand.cs
- Features/Balances/DeleteBalance/DeleteBalanceHandler.cs
- Interfaces/IBalanceQueryService.cs
- Services/BalanceQueryService.cs
- Contracts/Balances/CreateBalanceRequest.cs
- Contracts/Balances/UpdateBalanceRequest.cs

**Finance.Api:**
- Endpoints/BalanceEndpoints.cs (new file)

**Total:** ~13 files

---

## 📊 Total Estimate

| Phase | Files | LOC (est) | Complexity |
|-------|-------|-----------|------------|
| 8.1 - Ancillary | 10 | ~400 | Low |
| 8.2 - Bank CRUD | 12 | ~600 | Medium |
| 8.3 - Account CRUD | 12 | ~600 | Medium |
| 8.4 - Transaction CRUD + CSV | 13 | ~800 | High |
| 8.5 - Balance CRUD | 13 | ~650 | Medium |
| **Total** | **60** | **~3,050** | **High** |

---

## 🔄 Deferred to Future Phases

### ML.NET Integration (Phase 9+)

**Not included in Phase 8:**
- ❌ CategorizeAllTransactionsCommand (requires ML.NET)
- ❌ TrainModelCommand (requires ML.NET)
- ❌ ML.NET model training/inference
- ❌ ML model storage

**Reason:** Keep Phase 8 focused on CRUD completion. ML features require:
- ML.NET package
- Model training pipeline
- Model versioning
- Prediction service

---

## ✅ Acceptance Criteria

### Phase 8 Complete When:

1. **All CRUD operations work for:**
   - ✅ Currency
   - ✅ Country
   - ✅ Bank
   - ✅ Account
   - ✅ Transaction
   - ✅ Balance

2. **CSV Upload works:**
   - ✅ Can upload list of transactions
   - ✅ Duplicate detection works
   - ✅ Returns summary (loaded/not loaded/duplicated)

3. **Manual Categorization works:**
   - ✅ Can set category on transaction
   - ✅ Can mark category as confirmed

4. **All endpoints tested:**
   - ✅ Swagger UI works
   - ✅ All endpoints return proper responses
   - ✅ Validation errors return 400 with ProblemDetails
   - ✅ Not found errors return 404

5. **Documentation:**
   - ✅ All endpoints documented in phase8-summary.md
   - ✅ API examples provided
   - ✅ development-plan.md updated

---

## 🎯 Next Steps (After Phase 8)

**Phase 9: ML.NET Integration**
- Implement ML categorization
- Train model command
- Prediction service
- Model versioning

**Phase 10: Bills Service**
- Bill entity
- Supplier entity
- ReadInBill entity
- Bills API

**Phase 11: Salary Service**
- Salary entity
- Salary API

**Phase 12: Identity Service**
- User authentication
- JWT tokens
- Authorization

---

## 📝 Notes

**Key Decisions:**
1. **Ancillary in Finance Service** - Don't create separate service (YAGNI)
2. **ML features deferred** - Focus on CRUD first
3. **Soft deletes everywhere** - Match old API behavior
4. **Hybrid MediatR** - Complex operations use MediatR, simple queries use Services
5. **Validation everywhere** - FluentValidation for all commands

**Dependencies:**
- ✅ Phase 7 complete (API infrastructure)
- ⚠️ Must do Ancillary (8.1) first
- ⚠️ Bank before Account (foreign key)

---

**Created:** October 19, 2025
**Status:** 📋 Ready for Implementation
**Estimated Effort:** 3-4 days (60 files, ~3,050 LOC)
