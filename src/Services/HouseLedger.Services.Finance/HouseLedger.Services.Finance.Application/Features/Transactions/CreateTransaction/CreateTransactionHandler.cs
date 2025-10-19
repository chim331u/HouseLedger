using AutoMapper;
using HouseLedger.Services.Finance.Application.Contracts.Transactions;
using HouseLedger.Services.Finance.Domain.Entities;
using HouseLedger.Services.Finance.Domain.ValueObjects;
using HouseLedger.Services.Finance.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Finance.Application.Features.Transactions.CreateTransaction;

/// <summary>
/// Handler for CreateTransactionCommand.
/// Demonstrates MediatR usage for complex operation with validation and business logic.
/// </summary>
public class CreateTransactionHandler : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    private readonly FinanceDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateTransactionHandler> _logger;

    public CreateTransactionHandler(
        FinanceDbContext context,
        IMapper mapper,
        ILogger<CreateTransactionHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TransactionDto> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating transaction for account {AccountId} with amount {Amount}",
            request.AccountId, request.Amount);

        // 1. Verify account exists
        var accountExists = await _context.Accounts
            .AnyAsync(a => a.Id == request.AccountId && a.IsActive, cancellationToken);

        if (!accountExists)
        {
            _logger.LogWarning("Account not found or inactive: {AccountId}", request.AccountId);
            throw new InvalidOperationException($"Account {request.AccountId} not found or inactive");
        }

        // 2. Create transaction entity
        var transaction = new Transaction
        {
            TransactionDate = request.TransactionDate,
            Amount = request.Amount,
            Description = request.Description,
            AccountId = request.AccountId,
            Note = request.Note,
            CreatedDate = DateTime.Now,
            IsActive = true
        };

        // 3. Set category (Value Object)
        if (!string.IsNullOrWhiteSpace(request.CategoryName))
        {
            transaction.Category = new TransactionCategory(request.CategoryName, request.IsCategoryConfirmed);
            _logger.LogDebug("Transaction category set: {CategoryName} (Confirmed: {IsConfirmed})",
                request.CategoryName, request.IsCategoryConfirmed);
        }

        // 4. Generate unique key (for duplicate detection)
        transaction.UniqueKey = GenerateUniqueKey(transaction);

        // 5. Check for duplicate
        var isDuplicate = await _context.Transactions
            .AnyAsync(t => t.UniqueKey == transaction.UniqueKey && t.IsActive, cancellationToken);

        if (isDuplicate)
        {
            _logger.LogWarning("Duplicate transaction detected: {UniqueKey}", transaction.UniqueKey);
            throw new InvalidOperationException("Duplicate transaction detected");
        }

        // 6. Save to database
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Transaction created successfully with ID {TransactionId}", transaction.Id);

        // 7. Return DTO
        var dto = _mapper.Map<TransactionDto>(transaction);

        // Load account name for DTO
        var account = await _context.Accounts.FindAsync(new object[] { request.AccountId }, cancellationToken);
        if (account != null)
        {
            dto.AccountName = account.Name;
        }

        return dto;
    }

    private static string GenerateUniqueKey(Transaction transaction)
    {
        // Simple unique key: AccountId_Date_Amount
        // In production, you might want a more sophisticated algorithm
        var date = transaction.TransactionDate.ToString("yyyyMMdd");
        var amount = Math.Abs(transaction.Amount).ToString("F2");
        return $"{transaction.AccountId}_{date}_{amount}";
    }
}
