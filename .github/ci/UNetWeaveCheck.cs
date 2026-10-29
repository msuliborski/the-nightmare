// Runs Unity's UNET weaver over a compiled Assembly-CSharp.dll, the same post-processing the editor
// does, so invalid [Command]/[ClientRpc]/[SyncVar] usage fails CI instead of the Unity build.
// Usage: mono UNetWeaveCheck.exe <Editor/Data dir> <Assembly-CSharp.dll> <output dir>
using System;
using System.IO;
using Mono.Cecil;

static class UNetWeaveCheck
{
    static int Main(string[] args)
    {
        string data = args[0], assembly = args[1], output = args[2];
        var resolver = new DefaultAssemblyResolver();
        foreach (string dir in new[]
                 {
                     data + "/Managed/UnityEngine", data + "/Managed",
                     data + "/UnityExtensions/Unity/Networking", data + "/UnityExtensions/Unity/GUISystem",
                     data + "/MonoBleedingEdge/lib/mono/4.7.1-api", data + "/MonoBleedingEdge/lib/mono/4.7.1-api/Facades",
                     Path.GetDirectoryName(Path.GetFullPath(assembly))
                 })
            resolver.AddSearchDirectory(dir);

        int errors = 0;
        bool ok = Unity.UNetWeaver.Program.Process(
            data + "/Managed/UnityEngine/UnityEngine.CoreModule.dll",
            data + "/UnityExtensions/Unity/Networking/UnityEngine.Networking.dll",
            output, new[] { assembly }, new string[0], resolver,
            warning => Console.WriteLine("warning: " + warning),
            error => { errors++; Console.WriteLine("error: " + error); });

        Console.WriteLine(ok && errors == 0 ? "UNET weaving OK" : "UNET weaving FAILED");
        return ok && errors == 0 ? 0 : 1;
    }
}
