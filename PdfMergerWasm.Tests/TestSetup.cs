using System.Runtime.CompilerServices;
using PdfSharp;

namespace PdfMergerWasm.Tests;

internal static class TestSetup
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        PdfSharpLicense.SetAgplLicense();
    }
}

