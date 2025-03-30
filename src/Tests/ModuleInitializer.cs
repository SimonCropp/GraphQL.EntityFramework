public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySqlServer.Initialize();
        VerifierSettings.IgnoreMember("HasTransaction");
    }
}