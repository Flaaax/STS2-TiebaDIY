using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Interop;

namespace TiebaDIY.Scripts;

[ModInitializer("Init")]
public static class Entry
{
    public const string ModId = "TiebaDIY";

    public static Logger Log { get; } = RitsuLibFramework.CreateLogger(ModId);

    public static void Init()
    {
        var assembly = Assembly.GetExecutingAssembly();
        AssociateRuntimeAssemblyWithMod(assembly);

        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Log);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

        Log.Info("Mod initialized!");
    }

    private static void AssociateRuntimeAssemblyWithMod(Assembly assembly)
    {
        // A normal single-DLL load is already associated by the game. Dispatch
        // runtimes need to replace the bootstrap assembly association.
        if (assembly.GetName().Name == ModId)
            return;

#if STS2_0_107_1
        Action<Mod>? onModDetected = null;
        onModDetected = mod =>
        {
            if (mod.manifest?.id != ModId)
                return;

            mod.assembly = assembly;
            Traverse.Create(typeof(ReflectionHelper)).Field("_modTypes").SetValue(null);
            ModManager.OnModDetected -= onModDetected;
            Log.Info($"Associated runtime assembly {assembly} with mod {ModId} (0.107.1 compatibility path).");
        };
        ModManager.OnModDetected += onModDetected;
#else
        ModManager.AssociateAssemblyWithMod(ModId, assembly);
#endif
    }
}
