using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using Better_Lobbies.Patches;
using HarmonyLib;

namespace Better_Lobbies
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Lethal Company.exe")]
    [BepInDependency("BiggerLobby", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Ryokune.CompatibilityChecker", BepInDependency.DependencyFlags.SoftDependency)]
    internal class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        internal static Plugin Instance;
        internal ConfigEntry<bool> CensorLobbyNames;
        private readonly Harmony Harmony = new Harmony(PluginInfo.PLUGIN_GUID);

        private void Awake()
        {
            // Plugin startup logic
            Instance = this;
            Logger = base.Logger;

            //Config
            CensorLobbyNames = Config.Bind("Lobby List", "Censor Lobby Names", true, "Censor offensive lobby names?");

            if (!Chainloader.PluginInfos.ContainsKey("Ryokune.CompatibilityChecker"))
            {
                Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
                Harmony.PatchAll(typeof(LobbyPatch));
                Harmony.PatchAll(typeof(ServerListPatch));
            }
            else if(Chainloader.PluginInfos.TryGetValue("Ryokune.CompatibilityChecker", out BepInEx.PluginInfo checker))
            {
                Logger.Log(LogLevel.All, $"\n{checker.Metadata.Name} v{checker.Metadata.Version} is already installed!\nThis plugin is made by the same developer, and has the same features as Better Servers with an additional mod checking feature.");
            }
            Harmony.PatchAll(typeof(FixLobbyDataInform)); // Lobby information patch.
            Harmony.PatchAll(typeof(LobbyConnectionFixes)); // Currently all just debugging.
        }
    }
}
