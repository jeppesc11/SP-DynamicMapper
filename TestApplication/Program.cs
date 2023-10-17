using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SharePoint.Client;
using PnP.Core.Auth;
using PnP.Core.Services;
using PnP.Framework;
using System;
using TestApplication.Models;
using SP_DynamicMapper.Extentions;
using PnP.Core.Model.SharePoint;
using Microsoft.Graph;

namespace TestApplication
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string clientId = "";
            string siteUrl = "";

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Add PnP Core SDK
                    services.AddPnPCore(options =>
                    {
                        // Configure the interactive authentication provider as default
                        options.DefaultAuthenticationProvider = new InteractiveAuthenticationProvider()
                        {
                            ClientId = clientId,
                            RedirectUri = new Uri("http://localhost")
                        };
                    });
                })
                .UseConsoleLifetime()
                .Build();

            // Start the host
            host.StartAsync().GetAwaiter().GetResult();

            using (var scope = host.Services.CreateScope())
            {
                // Ask an IPnPContextFactory from the host
                var pnpContextFactory = scope.ServiceProvider.GetRequiredService<IPnPContextFactory>();

                // Create a PnPContext
                using (PnPContext pnpContext = pnpContextFactory.CreateAsync(new Uri(siteUrl)).GetAwaiter().GetResult())
                {
                    using (ClientContext context = new AuthenticationManager(pnpContext).GetContext(siteUrl))
                    {
                        context.Load(context.Web);

                        var items = context.Web.GetItems<EmployeeTaskModel>(x => x.Employeer_Name, x => x.Employeer_Phone);

                        var item = items.FirstOrDefault();
                        item.Title = "N/A";

                        //var changes = item.GetChanges();

                        //var updateItem = context.Web.UpdateItem(item);

                        Console.WriteLine("Your site title is: " + context.Web.Title);
                    }
                }
            }
        }
    }
}