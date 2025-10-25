using AutoMapper;
using HouseLedger.Services.HouseThings.Application.Contracts.HouseThings;
using HouseLedger.Services.HouseThings.Application.Contracts.Rooms;
using HouseLedger.Services.HouseThings.Domain.Entities;

namespace HouseLedger.Services.HouseThings.Application.Mapping;

/// <summary>
/// AutoMapper profile for HouseThings entities → DTOs mappings.
/// </summary>
public class HouseThingsMappingProfile : Profile
{
    public HouseThingsMappingProfile()
    {
        // Room → RoomDto
        CreateMap<Room, RoomDto>();

        // CreateRoomRequest → Room
        CreateMap<CreateRoomRequest, Room>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.HouseThings, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        // UpdateRoomRequest → Room
        CreateMap<UpdateRoomRequest, Room>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.HouseThings, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());

        // HouseThing → HouseThingDto
        CreateMap<HouseThing, HouseThingDto>()
            .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.Room != null ? src.Room.Name : null));

        // CreateHouseThingRequest → HouseThing
        CreateMap<CreateHouseThingRequest, HouseThing>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Room, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        // UpdateHouseThingRequest → HouseThing
        CreateMap<UpdateHouseThingRequest, HouseThing>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Room, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());
    }
}
