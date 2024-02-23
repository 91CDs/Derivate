using System.Runtime.CompilerServices;

namespace DerivateTests;
public static class VerifySettings
{
    [ModuleInitializer]
    public static void Initialize() {
        UseProjectRelativeDirectory("Snapshots");
        VerifyDiffPlex.Initialize();
    }
}