using IYLTDSU.Signalling.Runtime.Interfaces;
using Microsoft.Extensions.Configuration;

namespace IYLTDSU.Signalling.Runtime.Models;

// Creates a BaseFunction for the hosting environment.
public class BaseFunction
{
    protected IHostEnvir HostingEnvironment { get; }
    protected IConfiguration Configuration { get; private set; }

    public BaseFunction()
    {
        HostingEnvironment = new HostEnvir();
        Configuration = BuildConfiguration(HostingEnvironment);
    }

    // Creates a configuration for the given environment.
    public IConfiguration BuildConfiguration(IHostEnvir environment)
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddSystemsManager(builder =>
            {
                builder.Path = $"/WebSockets/Signalling/{environment.EnvironmentName.ToLower()}";
                builder.Optional = true;
                builder.ReloadAfter = TimeSpan.FromDays(1);
            })
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}