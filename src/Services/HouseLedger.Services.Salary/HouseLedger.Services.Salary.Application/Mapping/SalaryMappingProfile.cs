using AutoMapper;
using HouseLedger.Services.Salary.Application.Contracts.Salaries;
using HouseLedger.Services.Salary.Domain.Entities;

namespace HouseLedger.Services.Salary.Application.Mapping;

/// <summary>
/// AutoMapper profile for Salary mappings.
/// </summary>
public class SalaryMappingProfile : Profile
{
    public SalaryMappingProfile()
    {
        // Entity to DTO
        CreateMap<Domain.Entities.Salary, SalaryDto>()
            .ForMember(dest => dest.CurrencyName, opt => opt.Ignore())
            .ForMember(dest => dest.CurrencyCode, opt => opt.Ignore())
            .ForMember(dest => dest.UserName, opt => opt.Ignore());

        // Request to Entity
        CreateMap<CreateSalaryRequest, Domain.Entities.Salary>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SalaryValueEur, opt => opt.Ignore())
            .ForMember(dest => dest.ExchangeRate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());

        CreateMap<UpdateSalaryRequest, Domain.Entities.Salary>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SalaryValueEur, opt => opt.Ignore())
            .ForMember(dest => dest.ExchangeRate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());
    }
}
