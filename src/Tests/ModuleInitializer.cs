public static partial class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifierSettings.IgnoreMember("HasTransaction");
        VerifierSettings.ScrubLinesWithReplace(_ => VersionRegex().Replace(_, "Version={Scrubbed}"));
        VerifierSettings.InitializePlugins();
    }

    [GeneratedRegex(@"Version=[\d.]+")]
    private static partial Regex VersionRegex();
}
