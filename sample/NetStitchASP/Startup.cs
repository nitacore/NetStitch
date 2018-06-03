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
            app.UseSwagger();
            app.UseNetStitch(this.GetType(), new NetStitchOption() { });
        }
    }

    public class PerfTest : SharedInterface.IComplexType, IPerf
    {

        public async ValueTask<MyClass> EchoAsync(string name, int x, int y, MyEnum e)
        {
            return new MyClass { Name = name, Sum = (x + y) * (int)e };
        }

        public async ValueTask<MyClass> EchoAsync(MyClass myClass)
        {
            return myClass;
        }

        public async ValueTask<MyClass> FooAsync(string a, int? x, int[] array)
        {
            return new MyClass { Name = a, Sum = array.Sum() };
        }

        public async Task FugaAsync()
        {
            Console.WriteLine("hoge");
        }

        public async ValueTask<int> HogeAsync(string a, int x, MyEnum e)
        {
            return 1;
        }

        public async ValueTask<DateTime> Now()
        {
            return DateTime.UtcNow.AddHours(9);
        }

        public async ValueTask<int> SumAsync(int[] array)
        {
            return array.Sum();
        }

        public Task VoidTaskAsync()
        {
            throw new NotImplementedException();
        }
    }
}
