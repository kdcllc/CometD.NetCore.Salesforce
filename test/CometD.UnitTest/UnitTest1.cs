using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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
                {"Salesforce:Id", "-2" },
                {"Salesforce:Name", "" }
            };

            IConfiguration Configuration = null;

            var host = new WebHostBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    Configuration = builder.AddInMemoryCollection(dic).Build();
                })
                .ConfigureServices((services)=>
                {

                });

        }
    }
}
