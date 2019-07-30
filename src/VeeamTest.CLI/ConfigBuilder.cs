using System.IO;
using Microsoft.Extensions.Configuration;

namespace VeeamTest.CLI
{
    internal static class ConfigBuilder
    {
        public static Config Build()
        {
            var executionDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var configFile = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(executionDirectory, "config.json"))
                .Build();

            return new Config(configFile.GetValue<int>("buffer-size"));
        }
    }
}
