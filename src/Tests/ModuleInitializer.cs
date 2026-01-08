public static partial class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifyDiffPlex.Initialize(OutputType.Compact);
        VerifySqlServer.Initialize();
        VerifierSettings.IgnoreMember("HasTransaction");
        VerifierSettings.ScrubLinesWithReplace(_ => VersionRegex().Replace(_, "Version={Scrubbed}"));
    }

    [GeneratedRegex(@"Version=[\d.]+")]
    private static partial Regex VersionRegex();
}
