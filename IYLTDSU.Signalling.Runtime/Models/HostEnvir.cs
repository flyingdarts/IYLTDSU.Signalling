using IYLTDSU.Signalling.Runtime.Interfaces;
using IYLTDSU.Signalling.Core;
namespace IYLTDSU.Signalling.Runtime.Models;

public class HostEnvir : IHostEnvir
{
    public HostEnvir()
    {
        EnvironmentName = Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.AspnetCoreEnvironment) ?? Constants.Environments.Production;
    }

    public string EnvironmentName { get; set; }
    public bool IsProduction() => EnvironmentName != null && !string.IsNullOrWhiteSpace(EnvironmentName) &&
                                  EnvironmentName.Equals("production", StringComparison.CurrentCultureIgnoreCase);
}