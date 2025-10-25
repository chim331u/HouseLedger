using AutoMapper;
using HouseLedger.Services.Finance.Application.Contracts.Accounts;
using HouseLedger.Services.Finance.Application.Contracts.Balances;
using HouseLedger.Services.Finance.Application.Contracts.Banks;
using HouseLedger.Services.Finance.Application.Contracts.Transactions;
using HouseLedger.Services.Finance.Domain.Entities;
using HouseLedger.Services.Finance.Domain.ValueObjects;

namespace HouseLedger.Services.Finance.Application.Mapping;

/// <summary>
/// AutoMapper profile for Finance entities → DTOs mappings.
/// </summary>
public class FinanceMappingProfile : Profile
{
    public FinanceMappingProfile()
    {
        // Transaction → TransactionDto
        CreateMap<Transaction, TransactionDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.IsCategoryConfirmed, opt => opt.MapFrom(src => src.Category != null && src.Category.IsConfirmed))
            .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.Account != null ? src.Account.Name : null));

        // CreateTransactionRequest → Transaction
        CreateMap<CreateTransactionRequest, Transaction>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src =>
                !string.IsNullOrWhiteSpace(src.CategoryName)
                    ? new TransactionCategory(src.CategoryName, src.IsCategoryConfirmed)
                    : null))
            .ForMember(dest => dest.UniqueKey, opt => opt.Ignore())
            .ForMember(dest => dest.Account, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        // Account → AccountDto
        CreateMap<Account, AccountDto>()
            .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.Bank != null ? src.Bank.Name : null));

        // CreateAccountRequest → Account
        CreateMap<CreateAccountRequest, Account>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Bank, opt => opt.Ignore())
            .ForMember(dest => dest.Transactions, opt => opt.Ignore())
            .ForMember(dest => dest.Balances, opt => opt.Ignore())
            .ForMember(dest => dest.Cards, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        // UpdateAccountRequest → Account
        CreateMap<UpdateAccountRequest, Account>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Bank, opt => opt.Ignore())
            .ForMember(dest => dest.Transactions, opt => opt.Ignore())
            .ForMember(dest => dest.Balances, opt => opt.Ignore())
            .ForMember(dest => dest.Cards, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());

        // Balance → BalanceDto
        CreateMap<Balance, BalanceDto>()
            .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.Account != null ? src.Account.Name : null));

        // CreateBalanceRequest → Balance
        CreateMap<CreateBalanceRequest, Balance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Account, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        // UpdateBalanceRequest → Balance
        CreateMap<UpdateBalanceRequest, Balance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Account, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());

        // Bank → BankDto
        CreateMap<Bank, BankDto>();

        // CreateBankRequest → Bank
        CreateMap<CreateBankRequest, Bank>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Accounts, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        // UpdateBankRequest → Bank
        CreateMap<UpdateBankRequest, Bank>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Accounts, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());
    }
}
