using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PlumExtract;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddTransient<Runner>();

var host = builder.Build();

var runner = host.Services.GetRequiredService<Runner>();

await runner.RunAsync(args);