using Microsoft.Extensions.Configuration;

namespace Dinfo.Test.helpers
{
    class ConfigurationHelper
    {
        public static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
            return config;
        }

        public const string TemplatePath = @"D:\Files";
    }
}
