using AutoMapper;
using HouseLedger.Services.Ancillary.Application.Contracts.Countries;
using HouseLedger.Services.Ancillary.Application.Contracts.Currencies;
using HouseLedger.Services.Ancillary.Application.Contracts.CurrencyConversionRates;
using HouseLedger.Services.Ancillary.Application.Contracts.Suppliers;
using HouseLedger.Services.Ancillary.Domain.Entities;

namespace HouseLedger.Services.Ancillary.Application.Mapping;

/// <summary>
/// AutoMapper profile for Ancillary entities → DTOs mappings.
/// </summary>
public class AncillaryMappingProfile : Profile
{
    public AncillaryMappingProfile()
    {
        // Currency → CurrencyDto
        CreateMap<Currency, CurrencyDto>();

        // CreateCurrencyRequest → Currency
        CreateMap<CreateCurrencyRequest, Currency>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        // UpdateCurrencyRequest → Currency
        CreateMap<UpdateCurrencyRequest, Currency>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());

        // Country → CountryDto
        CreateMap<Country, CountryDto>();

        // CreateCountryRequest → Country
        CreateMap<CreateCountryRequest, Country>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        // UpdateCountryRequest → Country
        CreateMap<UpdateCountryRequest, Country>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());

        // CurrencyConversionRate → CurrencyConversionRateDto
        CreateMap<CurrencyConversionRate, CurrencyConversionRateDto>();

        // CreateCurrencyConversionRateRequest → CurrencyConversionRate
        CreateMap<CreateCurrencyConversionRateRequest, CurrencyConversionRate>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        // UpdateCurrencyConversionRateRequest → CurrencyConversionRate
        CreateMap<UpdateCurrencyConversionRateRequest, CurrencyConversionRate>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());

        // Supplier → SupplierDto
        CreateMap<Supplier, SupplierDto>();

        // CreateSupplierRequest → Supplier
        CreateMap<CreateSupplierRequest, Supplier>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        // UpdateSupplierRequest → Supplier
        CreateMap<UpdateSupplierRequest, Supplier>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());
    }
}
