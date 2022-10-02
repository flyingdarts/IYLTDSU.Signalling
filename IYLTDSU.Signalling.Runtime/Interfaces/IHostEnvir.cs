namespace IYLTDSU.Signalling.Runtime.Interfaces;

/// <summary>
/// A simple abstraction of the IHostingEnvironment
/// </summary>
public interface IHostEnvir
{
    /// <summary>
    /// 
    /// </summary>
    string EnvironmentName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    bool IsProduction();
}