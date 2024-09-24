using Steamworks;
using Steamworks.Data;
using Unity.Netcode;

namespace Better_Lobbies.Hooks;
internal class LobbyConnection
{
  internal static Lobby? PreviousLobby;

  [HookInit]
  private static void Init()
  {
    On.GameNetworkManager.OnEnable += GameNetworkManager_OnEnable;
  }

  private static void GameNetworkManager_OnEnable(On.GameNetworkManager.orig_OnEnable orig, GameNetworkManager self)
  {
    orig(self);
    SteamMatchmaking.OnLobbyEntered += SetPreviouslyJoinedLobby;
  }

  private static void SetPreviouslyJoinedLobby(Lobby obj)
  {
    if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) return;
    PreviousLobby = obj;
    Plugin.Log.LogDebug($"Setting previously joined lobby to {PreviousLobby.Value.Id}");
  }
}
