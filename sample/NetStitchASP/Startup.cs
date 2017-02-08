using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedInterface;
using NetStitch.Server;
using System.IO;
using NetStitch.Option;

namespace NetStitchASP
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseNetStitch(this.GetType(), new NetStitchOption() { });
        }
    }

    public class PerfTest : IEcho
    {
        public int Sum(int[] array) => array.Sum();

        MyClass IEcho.Echo(string name, int x, int y, MyEnum e)
        {
            return new MyClass { Name = name, Sum = (x + y) * (int)e };
        }
    }
}
