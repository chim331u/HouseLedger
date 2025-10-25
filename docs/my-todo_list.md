# HouseLedger - TODO List

**Project:** HouseLedger Background Jobs
**Location:** `src/BuildingBlocks/HouseLedger.BuildingBlocks.BackgroundJobs/`
**Created:** October 19, 2025
**Last Updated:** October 19, 2025

---

## ✅ Completed Tasks Summary

#| Phase                                            | Task                                                      | Status        | Effort Estimated/ Completed | Notes                                                                      |
--|--------------------------------------------------|-----------------------------------------------------------|---------------|-----------------------------|----------------------------------------------------------------------------|
1| JWT Bearer Access                                | Migrate the identity managemement using jwt berer token   | ✅ Completed   | 10 m/D                      | Use data from old db, migrate from dotnet6                                 |
2| Vault `Secrety managmeent`                       | Service that manage secrets                               | 🚧 To start   | 30 m/D                      | Try and find a solution: other analysis and architecture followup required |
3| ML.net learning service `Transaction categorize` | Service that manage Categorizion of transactions          | 🚧 To start   | 10 m/D                      | ML.net to categorize Transactions: analysis required                       |
4| Read bills - .pdf documents `Using AI`?          | Service that manage read docs by AI and save in db        | 🚧 To start   | 50 m/D                      | AI or other to read docs                                                   |
5| Currency Rate update  `Schedule daily job`       | Service that update the currency rate, daily job hangfire | 🚧 To start   | 5 m/D                       | service + schedule recurring job                                           |
6| Balance                                          | Balance controller and services                           | 🚧 To start   | 2 m/D                       |                                            |
7| Salary                                           | Salary controller and services                            | ✅ Completed  | 2 m/D                       |                                            |
8| House thinks                                     | House thinks controller and services                      | 🚧 To start   | 3 m/D                       |                                            |
9| Bill                                             | Bill management --> see AI                                | 🚧 To start   | x m/D                       |                                            |
10| Identiy Access - Secrets                         | ex Identity Access --> secrets: see vault                 | 🚧 To start   | x m/D                       |                                            |
11| Statistics                                       | Dashboard, statistics and charts                          | 🚧 To start   | x m/D                       |                                            |
12| Local Users                                      | Local Users for salary and identity                       | ✅ Completed    | x m/D                       |                                            |
13| House things                                     |                                                    | 🔨 In Progress   | x m/D                       |                                            |

---

## 📋 FUTURE WORK
**Implementation Checklist:**
- [ ] Access system via token jwt: use current model and data
- [ ] Vault Homemade to manage `secrets`
- [ ] Machine learning for Transaction / Classification
- [ ] Read `bills` in pdf format using AI and save content in app
- [ ] Update currency rate from web + schedule as Hangfire daily job
- [ ] Add unit tests
- [ ] Document API source and configuration


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

**Last Updated**: October 20, 2025
**Next Review**: asap
