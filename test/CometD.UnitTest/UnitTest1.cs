using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Xunit;

namespace CometD.UnitTest
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var dic = new Dictionary<string, string>
            {
                { "Salesforce:Id", "-2" },
                { "Salesforce:Name", string.Empty }
            };

            IConfiguration configuration = null;

            var host = new HostBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    configuration = builder.AddInMemoryCollection(dic).Build();
                })
                .ConfigureServices((services) =>
                {
                });
        }
    }
}
