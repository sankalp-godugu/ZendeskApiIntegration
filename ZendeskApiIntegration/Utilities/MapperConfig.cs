using AutoMapper;
using Microsoft.Extensions.Configuration;

namespace ZendeskContactsProcessJob.Utilities
{
    public class MapperConfig
    {
        public static Mapper InitializeAutomapper(IConfiguration configuration)
        {
            //Provide all the Mapping Configuration
            MapperConfiguration config = new(cfg =>
            {
            });

            //Create an Instance of Mapper and return that Instance
            Mapper mapper = new(config);
            return mapper;
        }
    }
}
