// Runs Unity's UNET weaver over a compiled Assembly-CSharp.dll, the same post-processing the editor
// does, so invalid [Command]/[ClientRpc]/[SyncVar] usage fails CI instead of the Unity build.
// Usage: mono UNetWeaveCheck.exe <Editor/Data dir> <Assembly-CSharp.dll> <output dir>
using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

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

        if (ok && errors == 0) errors += CheckSyncVarDeserialization(Path.Combine(output, Path.GetFileName(assembly)));

        Console.WriteLine(ok && errors == 0 ? "UNET weaving OK" : "UNET weaving FAILED");
        return ok && errors == 0 ? 0 : 1;
    }

    // The weaver silently skips generating OnDeserialize when a class overrides it by hand, so its
    // [SyncVar]s are sent by the server but never applied on clients. Flag such classes.
    static int CheckSyncVarDeserialization(string wovenAssembly)
    {
        int errors = 0;
        ModuleDefinition module = ModuleDefinition.ReadModule(wovenAssembly);
        foreach (TypeDefinition type in module.Types)
        {
            var syncVars = type.Fields
                .Where(f => f.CustomAttributes.Any(a => a.AttributeType.Name == "SyncVarAttribute"))
                .Select(f => f.Name).ToList();
            if (syncVars.Count == 0) continue;

            MethodDefinition deserialize = type.Methods.FirstOrDefault(m => m.Name == "OnDeserialize" && m.HasBody);
            bool writesSyncVars = deserialize != null && deserialize.Body.Instructions.Any(i =>
                i.OpCode == OpCodes.Stfld && i.Operand is FieldReference && syncVars.Contains(((FieldReference) i.Operand).Name));
            if (!writesSyncVars)
            {
                errors++;
                Console.WriteLine("error: " + type.FullName + " has [SyncVar]s but its OnDeserialize never sets them " +
                                  "(a hand-written OnDeserialize override stops the weaver from generating it).");
            }
        }
        return errors;
    }
}
