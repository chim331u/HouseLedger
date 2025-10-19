# Hangfire Background Jobs - Implementation TODO List

**Project:** HouseLedger Background Jobs
**Location:** `src/BuildingBlocks/HouseLedger.BuildingBlocks.BackgroundJobs/`
**Created:** October 19, 2025
**Last Updated:** October 19, 2025

---

## ✅ Completed Tasks Summary

| Phase | Task | Status | Date Completed | Notes |
|-------|------|--------|----------------|-------|
| Phase 1 | Core BuildingBlock Setup | ✅ Complete | Oct 19, 2025 | All abstractions, services, filters implemented |
| Phase 1 | Project Structure Created | ✅ Complete | Oct 19, 2025 | Folder structure matches HouseLedger.BuildingBlocks.Logging |
| Phase 1 | NuGet Packages Added | ✅ Complete | Oct 19, 2025 | Using Hangfire.Storage.SQLite (v0.4.2) instead of Hangfire.SQLite |
| Phase 1 | Core Abstractions | ✅ Complete | Oct 19, 2025 | IBackgroundJob, IRecurringJob, IJobScheduler, JobResult |
| Phase 1 | Configuration Classes | ✅ Complete | Oct 19, 2025 | HangfireOptions, HangfireConfiguration, HangfireDashboardAuthFilter |
| Phase 1 | Services Implemented | ✅ Complete | Oct 19, 2025 | HangfireJobScheduler, JobExecutionLogger |
| Phase 1 | Filters Implemented | ✅ Complete | Oct 19, 2025 | AutomaticRetryFilter, JobLoggingFilter |
| Phase 1 | README Documentation | ✅ Complete | Oct 19, 2025 | Comprehensive usage guide created |
| Phase 1 | Added to Solution | ✅ Complete | Oct 19, 2025 | Project added to HouseLedger.sln |
| Phase 3 | Gateway Integration | ✅ Complete | Oct 19, 2025 | Program.cs updated with Hangfire services and dashboard |
| Phase 3 | Project Reference Added | ✅ Complete | Oct 19, 2025 | API project references BackgroundJobs BuildingBlock |
| Phase 3 | Integration Tested | ✅ Complete | Oct 19, 2025 | Dashboard accessible at /hangfire with Basic Auth |
| Phase 4 | Production Configuration | ✅ Complete | Oct 19, 2025 | appsettings.json configured (dashboard disabled) |
| Phase 4 | Development Configuration | ✅ Complete | Oct 19, 2025 | appsettings.Development.json configured (dashboard enabled) |
| Phase 4 | Separate Database Configured | ✅ Complete | Oct 19, 2025 | Using dedicated SQLite file for Hangfire jobs |
| Phase 4 | Worker Count Set | ✅ Complete | Oct 19, 2025 | Limited to 2 workers for NAS memory constraints |
| Phase 4 | Job Retention Configured | ✅ Complete | Oct 19, 2025 | 7-day retention for NAS storage optimization |
| Phase 4 | Dashboard Authentication | ✅ Complete | Oct 19, 2025 | Basic Auth implemented (admin/dev123 for dev) |

**Key Implementation Notes:**
- **Package Change**: Switched from `Hangfire.SQLite` (v1.4.2) to `Hangfire.Storage.SQLite` (v0.4.2) due to connection string handling issues
- **Project Naming**: `HouseLedger.BuildingBlocks.BackgroundJobs` (consistent with Logging BuildingBlock)
- **Memory Optimized**: 2 workers, 7-day retention, optional dashboard (disabled in production)
- **Database**: Separate SQLite file at `/temp/hangfire-jobs-dev.db` (dev) and `/data/housledger-jobs.db` (prod)
- **Authentication**: Dashboard protected with Basic Authentication

---

## 📋 Phase 2: Job Implementation (FUTURE WORK)

### A. Currency & Financial Data Jobs

#### Job 1: Update Currency Rates ⏰ Daily
- **Status**: Not Started
- **Priority**: High
- **Location**: `HouseLedger.Services.Ancillary/Application/Jobs/`
- **Schedule**: Daily at 2:00 AM
- **Queue**: default
- **Description**: Fetch latest currency conversion rates from external API and update database

**Implementation Checklist:**
- [ ] Create `UpdateCurrencyRatesJob.cs` implementing `IRecurringJob`
- [ ] Integrate external currency API service
- [ ] Implement rate fetching logic
- [ ] Add database update using `ICurrencyConversionRateCommandService`
- [ ] Add error handling and logging
- [ ] Add unit tests
- [ ] Document API source and configuration

**Dependencies:**
- External currency API (e.g., exchangerate-api.com, fixer.io)
- ICurrencyConversionRateCommandService
- Configuration for API key

---

#### Job 2: Exchange Rate Validation ⏰ Weekly
- **Status**: Not Started
- **Priority**: Medium
- **Location**: `HouseLedger.Services.Ancillary/Application/Jobs/`
- **Schedule**: Every Monday at 8:00 AM
- **Queue**: default
- **Description**: Verify currency rate data integrity and alert on missing rates

**Implementation Checklist:**
- [ ] Create `ValidateCurrencyRatesJob.cs` implementing `IRecurringJob`
- [ ] Implement validation logic for rate completeness
- [ ] Check for gaps in historical data
- [ ] Add alerting mechanism (logging/notifications)
- [ ] Add unit tests
- [ ] Document validation rules

**Dependencies:**
- ICurrencyConversionRateQueryService
- ICurrencyQueryService

---

### B. Data Maintenance Jobs

#### Job 3: Old Transaction Cleanup ⏰ Monthly
- **Status**: Not Started
- **Priority**: Low
- **Location**: `HouseLedger.Services.Finance/Application/Jobs/`
- **Schedule**: 1st of each month at 3:00 AM
- **Queue**: maintenance
- **Description**: Archive transactions older than 7 years and soft delete inactive records

**Implementation Checklist:**
- [ ] Create `CleanupOldTransactionsJob.cs` implementing `IRecurringJob`
- [ ] Implement archive logic (copy to archive table)
- [ ] Implement soft delete for old transactions
- [ ] Add configuration for retention period (default 7 years)
- [ ] Add safety checks to prevent accidental deletion
- [ ] Add unit tests
- [ ] Document archival process

**Dependencies:**
- ITransactionQueryService
- Database archive schema (if needed)

---

#### Job 4: Database Vacuum ⏰ Weekly
- **Status**: Not Started
- **Priority**: Medium
- **Location**: `HouseLedger.BuildingBlocks.BackgroundJobs/Jobs/`
- **Schedule**: Every Sunday at 4:00 AM
- **Queue**: maintenance
- **Description**: Optimize SQLite database file and reclaim unused space

**Implementation Checklist:**
- [ ] Create `DatabaseVacuumJob.cs` implementing `IRecurringJob`
- [ ] Implement SQLite VACUUM command execution
- [ ] Add database size tracking (before/after)
- [ ] Add logging for vacuum results
- [ ] Add error handling for locked database
- [ ] Add unit tests
- [ ] Document vacuum process and benefits

**Dependencies:**
- DbContext access
- SQLite connection management

---

### C. Reports & Analytics Jobs

#### Job 5: Monthly Financial Summary ⏰ Monthly
- **Status**: Not Started
- **Priority**: Medium
- **Location**: `HouseLedger.Services.Finance/Application/Jobs/`
- **Schedule**: Last day of month at 11:00 PM
- **Queue**: default
- **Description**: Generate monthly expense/income reports and calculate account balances

**Implementation Checklist:**
- [ ] Create `MonthlyFinancialSummaryJob.cs` implementing `IRecurringJob`
- [ ] Implement report generation logic
- [ ] Calculate total income/expenses by category
- [ ] Calculate account balances
- [ ] Store report in database or file system
- [ ] Add email notification (optional)
- [ ] Add unit tests
- [ ] Document report format and storage

**Dependencies:**
- ITransactionQueryService
- IAccountQueryService
- Report storage mechanism

---

#### Job 6: Budget Alert Checks ⏰ Daily
- **Status**: Not Started
- **Priority**: Medium
- **Location**: `HouseLedger.Services.Finance/Application/Jobs/`
- **Schedule**: Every day at 9:00 PM
- **Queue**: default
- **Description**: Check if spending exceeds budget thresholds and send notifications

**Implementation Checklist:**
- [ ] Create `BudgetAlertCheckJob.cs` implementing `IRecurringJob`
- [ ] Implement budget threshold checking
- [ ] Compare current spending against budgets
- [ ] Add notification mechanism (logging/email/push)
- [ ] Add unit tests
- [ ] Document budget alert rules

**Dependencies:**
- Budget service (to be implemented)
- ITransactionQueryService
- Notification service

---

### D. System Health Jobs

#### Job 7: Health Check ⏰ Hourly
- **Status**: Not Started
- **Priority**: High
- **Location**: `HouseLedger.BuildingBlocks.BackgroundJobs/Jobs/`
- **Schedule**: Every hour
- **Queue**: critical
- **Description**: Verify database connectivity, check log file sizes, and monitor memory usage

**Implementation Checklist:**
- [ ] Create `SystemHealthCheckJob.cs` implementing `IRecurringJob`
- [ ] Implement database connectivity check
- [ ] Check log file sizes and rotation
- [ ] Monitor memory usage (if possible on NAS)
- [ ] Add alerting for health check failures
- [ ] Add unit tests
- [ ] Document health check criteria

**Dependencies:**
- DbContext access
- File system access for logs
- System metrics (limited on NAS)

---

#### Job 8: Database Backup ⏰ Daily
- **Status**: Not Started
- **Priority**: High
- **Location**: `HouseLedger.BuildingBlocks.BackgroundJobs/Jobs/`
- **Schedule**: Every day at 1:00 AM
- **Queue**: critical
- **Description**: Create SQLite database backup and retain last 30 days

**Implementation Checklist:**
- [ ] Create `DatabaseBackupJob.cs` implementing `IRecurringJob`
- [ ] Implement SQLite database copy mechanism
- [ ] Add timestamp to backup filename
- [ ] Implement retention policy (delete backups older than 30 days)
- [ ] Add backup verification (file size, integrity)
- [ ] Add logging for backup success/failure
- [ ] Add unit tests
- [ ] Document backup location and restore process

**Dependencies:**
- File system access
- Configuration for backup path
- SQLite connection management

---

## 📊 Job Types Summary

### Scheduled Jobs Overview

| # | Job Name | Type | Schedule | Queue | Priority | Memory Impact | Status |
|---|----------|------|----------|-------|----------|---------------|--------|
| 1 | UpdateCurrencyRates | Recurring | Daily 2 AM | default | High | Low | 📋 Future |
| 2 | ValidateCurrencyRates | Recurring | Weekly Mon 8 AM | default | Medium | Low | 📋 Future |
| 3 | CleanupOldTransactions | Recurring | Monthly 1st 3 AM | maintenance | Low | Medium | 📋 Future |
| 4 | DatabaseVacuum | Recurring | Weekly Sun 4 AM | maintenance | Medium | Low | 📋 Future |
| 5 | MonthlyFinancialSummary | Recurring | Monthly Last Day 11 PM | default | Medium | Medium | 📋 Future |
| 6 | BudgetAlertCheck | Recurring | Daily 9 PM | default | Medium | Low | 📋 Future |
| 7 | SystemHealthCheck | Recurring | Hourly | critical | High | Low | 📋 Future |
| 8 | DatabaseBackup | Recurring | Daily 1 AM | critical | High | Low | 📋 Future |

### Job Implementation Priority

**Phase 2A - Critical Jobs** (Implement First):
1. DatabaseBackup (Job 8) - Essential for data safety
2. SystemHealthCheck (Job 7) - Monitor system health
3. UpdateCurrencyRates (Job 1) - Core business functionality

**Phase 2B - Important Jobs** (Implement Second):
4. DatabaseVacuum (Job 4) - Maintain database performance
5. MonthlyFinancialSummary (Job 5) - User-facing reports
6. ValidateCurrencyRates (Job 2) - Data integrity

**Phase 2C - Nice-to-Have Jobs** (Implement Later):
7. BudgetAlertCheck (Job 6) - Requires budget feature first
8. CleanupOldTransactions (Job 3) - Lower priority, data grows slowly

---

## 🧪 Testing Strategy

### Unit Testing
- [ ] Test each job's `ExecuteAsync` method in isolation
- [ ] Mock external dependencies (APIs, services)
- [ ] Test error handling and retry logic
- [ ] Test `JobResult` success/failure scenarios

### Integration Testing
- [ ] Test job scheduling and execution
- [ ] Test job queue priority
- [ ] Test retry behavior on failures
- [ ] Test concurrent job execution

### Performance Testing on NAS
- [ ] Measure memory usage with all jobs running
- [ ] Test SQLite database performance under job load
- [ ] Verify worker count limits are respected
- [ ] Monitor job execution times

---

## 🚀 Deployment Checklist

### Pre-Deployment
- [x] Review all Hangfire configuration in `appsettings.json`
- [x] Set secure dashboard password
- [x] Verify separate Hangfire database path
- [ ] Test on ARM32 environment (Raspberry Pi or QEMU)
- [ ] Build ARM32 Docker image with Hangfire

### Deployment
- [ ] Deploy to QNAP NAS Container Station
- [ ] Verify Hangfire database file created
- [ ] Check Hangfire dashboard accessibility
- [ ] Monitor initial memory usage
- [ ] Verify jobs are scheduled correctly

### Post-Deployment
- [ ] Monitor job execution logs
- [ ] Verify recurring jobs run on schedule
- [ ] Check database backup files are created
- [ ] Monitor overall system memory usage
- [ ] Document any issues or adjustments needed

---

## 🌐 External Dependencies

### Currency API Services (for Job 1)
Research and select one:
- [ ] **Option 1**: [exchangerate-api.com](https://www.exchangerate-api.com/) - Free tier available
- [ ] **Option 2**: [fixer.io](https://fixer.io/) - Reliable, paid tiers
- [ ] **Option 3**: [currencyapi.com](https://currencyapi.com/) - Modern API
- [ ] **Option 4**: ECB (European Central Bank) - Free, official rates

**Selection Criteria:**
- Free or affordable tier
- Reliable uptime
- Supports currencies in HouseLedger
- API rate limits compatible with daily updates

---

## 🎯 Future Enhancements

### Potential Future Jobs
- Email notification system
- PDF report generation
- Data export jobs (CSV, Excel)
- Integration with external accounting software
- ML model retraining (if transaction categorization is added)
- Automated bill payment reminders
- Tax report generation

### Advanced Features
- Job dependencies (run Job B after Job A completes)
- Dynamic job scheduling based on data changes
- Job execution metrics dashboard
- Job failure alerting via email/SMS
- Custom job retry strategies per job type

---

## 📚 References

- [Hangfire Documentation](https://docs.hangfire.io/)
- [Hangfire.Storage.SQLite](https://github.com/raisedapp/Hangfire.Storage.SQLite)
- [Hangfire Best Practices](https://docs.hangfire.io/en/latest/best-practices.html)
- [ARM32 .NET Deployment](https://github.com/dotnet/runtime/blob/main/docs/design/features/arm32-support.md)
- [SQLite VACUUM](https://www.sqlite.org/lang_vacuum.html)

---

**Last Updated**: October 19, 2025
**Next Review**: Ready for Phase 2 job implementation
