using AutoMapper;
using SystemChecker.Model.DTO;
using SystemChecker.Model.Data.Entities;
using AutoMapper.EquivalencyExpression;

namespace SystemChecker.Model.Data
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CheckScheduleDTO, CheckSchedule>()
                .EqualityComparison((odto, o) => odto.ID == o.ID)
                .ForMember(d => d.Check, o => o.Ignore())
                .ReverseMap()
                .ForMember(d => d.Check, o => o.Ignore());
            CreateMap<CheckDataDTO, CheckData>().ReverseMap();
            CreateMap<CheckDetailDTO, Check>().ReverseMap();
            CreateMap<CheckDTO, Check>().ReverseMap();
            CreateMap<CheckTypeDTO, CheckType>().ReverseMap();
            CreateMap<CheckResultDTO, CheckResult>().ReverseMap();
            CreateMap<SubCheckTypeDTO, SubCheckType>().ReverseMap();
            CreateMap<CheckNotificationTypeDTO, CheckNotificationType>().ReverseMap();
            CreateMap<ContactTypeDTO, ContactType>().ReverseMap();
            CreateMap<SubCheckDTO, SubCheck>()
                .EqualityComparison((odto, o) => odto.ID == o.ID)
                .ReverseMap();
            CreateMap<CheckNotificationDTO, CheckNotification>()
                .EqualityComparison((odto, o) => odto.ID == o.ID)
                .ReverseMap();
            CreateMap<OptionDTO, Option>().ReverseMap();
            CreateMap<ConnStringDTO, ConnString>()
                .EqualityComparison((odto, o) => odto.ID == o.ID)
                .ReverseMap();
            CreateMap<LoginDTO, Login>()
                .EqualityComparison((odto, o) => odto.ID == o.ID)
                .ReverseMap();
            CreateMap<EnvironmentDTO, Environment>()
                .EqualityComparison((odto, o) => odto.ID == o.ID)
                .ReverseMap();
            CreateMap<ContactDTO, Contact>()
                .EqualityComparison((odto, o) => odto.ID == o.ID)
                .ReverseMap();
            CreateMap<CheckGroupDTO, CheckGroup>()
                .EqualityComparison((odto, o) => odto.ID == o.ID)
                .ReverseMap();
            CreateMap<UserDTO, User>()
                .EqualityComparison((odto, o) => odto.ID == o.ID)
                .ForMember(x => x.Password, x => x.Ignore())
                .ReverseMap();
            CreateMap<ApiKeyDTO, ApiKey>()
                .EqualityComparison((odto, o) => odto.ID == o.ID)
                .ReverseMap();
        }
    }
}
