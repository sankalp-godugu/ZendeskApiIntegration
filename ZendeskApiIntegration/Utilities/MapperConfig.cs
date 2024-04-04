using AutoMapper;

namespace ZendeskApiIntegration.Utilities
{
    public class MapperConfig
    {
        public static Mapper InitializeAutomapper()
        {
            //Provide all the Mapping Configuration
            MapperConfiguration mapperConfig = new(cfg =>
            {
            });

            //Create an Instance of Mapper and return that Instance
            Mapper mapper = new(mapperConfig);
            return mapper;
        }
    }
}
