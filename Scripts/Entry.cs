using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using TiebaDIY.Scripts.Cards;

namespace TiebaDIY.Scripts;

[ModInitializer("Init")]
public static class Entry
{
    public const string ModId = "TiebaDIY";

    public static Logger Log { get; } = RitsuLibFramework.CreateLogger(ModId);
    public static bool EnableSierpinskiSponge { get; private set; }

    private static Harmony? _harmony;

    public static void Init()
    {
        // 谢尔宾斯基海绵的内容开关。关闭时不安装效果补丁，也不会加入 GodOfDIY 的选项。
        EnableSierpinskiSponge = true;

        _harmony = new Harmony("STS2.TiebaDIY");
        _harmony.PatchAll();

        var assembly = Assembly.GetExecutingAssembly();
        AssociateRuntimeAssemblyWithMod(assembly);

        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Log);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
        ElectronicSheepCandidateSync.Register();

#if STS2_Stable
        RegisterSavedPropertyModels();
#endif

        Log.Info("Mod initialized!");
    }

#if STS2_Stable
    private static void RegisterSavedPropertyModels()
    {
        const BindingFlags flags =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        foreach (var type in typeof(Entry).Assembly.GetTypes())
        {
            if (!type.IsClass || type.IsAbstract || !typeof(ITiebaModel).IsAssignableFrom(type))
                continue;

            var hasSavedProperty = type
                .GetProperties(flags)
                .Any(property => property.GetCustomAttribute<SavedPropertyAttribute>() != null);

            if (!hasSavedProperty)
                continue;

            SavedPropertiesTypeCache.InjectTypeIntoCache(type);
            Log.Info($"Registered SavedProperty model: {type.FullName}");
        }
    }
#endif

    private static void AssociateRuntimeAssemblyWithMod(Assembly assembly)
    {
        // A normal single-DLL load is already associated by the game. Dispatch
        // runtimes need to replace the bootstrap assembly association.
        if (assembly.GetName().Name == ModId)
            return;

#if STS2_Stable
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
