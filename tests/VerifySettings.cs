using System.Runtime.CompilerServices;
using DiffEngine;

namespace DerivateTests;
public static class VerifySettings
{
    [ModuleInitializer]
    public static void Initialize() {
        UseProjectRelativeDirectory("Snapshots");
        VerifyDiffPlex.Initialize();
    }
}