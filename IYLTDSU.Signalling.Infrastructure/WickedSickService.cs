static class WickedSickService
{
    public const string DOTNET_ENVIRONMENT = "Development";
    public const string COMPANY_NAME = "Flyingdarts";
    public const string COMPANY_SLOGAN = "If you love the darts, stand up!"; // TODO: DescriptionAspect
    public const string COMPANY_SHORT = "IYLTDSU"; // TODO: ResourceIdentifierAspect

    public static string GetIdentifierFor(string nameOfResource)
    {
        return $"{COMPANY_NAME}-{DOTNET_ENVIRONMENT}-{nameOfResource}";
    }
}