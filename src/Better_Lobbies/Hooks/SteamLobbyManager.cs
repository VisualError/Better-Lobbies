namespace Better_Lobbies.Hooks;

class SteamLobbyManager
{
  internal static global::SteamLobbyManager? Instance;
  [HookInit]
  private static void Init()
  {
    On.SteamLobbyManager.OnEnable += SteamLobbyManager_OnEnable;
  }

  private static void SteamLobbyManager_OnEnable(On.SteamLobbyManager.orig_OnEnable orig, global::SteamLobbyManager self)
  {
    orig(self);
    Instance = self;
  }
}
