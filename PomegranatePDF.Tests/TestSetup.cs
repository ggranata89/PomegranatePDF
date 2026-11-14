using System.Runtime.CompilerServices;
using PdfSharp;

namespace PomegranatePDF.Tests;

internal static class TestSetup
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        PdfSharpLicense.SetAgplLicense();
    }
}

