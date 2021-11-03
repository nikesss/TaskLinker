using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Funq;
using Microsoft.Extensions.Configuration;
using ServiceStack;
using TaskTT.ServiceInterface;
using ServiceStack.OrmLite;
using ServiceStack.Data;
using TaskTT.ServiceModel;
using TaskTT.ServiceModel.Types;

namespace TaskTT
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls(Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://localhost:5000/")
                .Build();

            host.Run();
        }
    }

    public class Startup : ModularStartup
    {
        public Startup(IConfiguration configuration) : base(configuration) { }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public new void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseServiceStack(new AppHost());

            app.Run(context =>
            {
                context.Response.Redirect("");
                return Task.FromResult(0);
            });
        }
    }

    public class AppHost : AppHostBase
    {
        public AppHost()
            : base("TaskTT", typeof(ParsePage).Assembly) { }

        public override void Configure(Container container)
        {
            var connectionString = "Server=127.0.0.1;Port=5432;UserName=postgres;Password=12344;Database=postgres;Pooling=true;MinPoolSize=0;MaxPoolSize=200";
            var dbFactory = new OrmLiteConnectionFactory(connectionString, PostgreSqlDialect.Provider);
            container.Register<IDbConnectionFactory>(dbFactory);
            using (var db = dbFactory.Open())
            {

                if (!db.TableExists<ParsedPage>())
                {
                    db.CreateTable<ParsedPage>();
                    db.CreateTable<Entitis>();
                }

            }
            Pullenti.Sdk.InitializeAll();
            Plugins.Add(new CorsFeature(
                allowOriginWhitelist: new[] { "http://localhost", "http://localhost:3000", "http://run.plnkr.co" },
                allowCredentials: true,
                allowedHeaders: "Content-Type, Allow, Authorization, X-Args"));

        }
    }
}