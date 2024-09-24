using Steamworks.Data;
using System;
using System.Linq;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.ProBuilder;

namespace Better_Lobbies.Hooks;
internal class LobbyList
{
  [HookInit]
  private static void Init()
  {
    On.SteamLobbyManager.loadLobbyListAndFilter += SteamLobbyManager_loadLobbyListAndFilter;
  }

  private static System.Collections.IEnumerator SteamLobbyManager_loadLobbyListAndFilter(On.SteamLobbyManager.orig_loadLobbyListAndFilter orig, global::SteamLobbyManager self, Lobby[] lobbyList)
  {
    if (LobbyConnection.PreviousLobby.HasValue)
    {
      // A fast and reliable way to insert the previously joined lobby as the first entry.
      if (!lobbyList.Contains(LobbyConnection.PreviousLobby.Value))
      {
        lobbyList.Add(lobbyList[0]);
        Plugin.Log.LogInfo("Adding private lobby as rejoinable");
      }
      else
      {
        var index = lobbyList.IndexOf(LobbyConnection.PreviousLobby.Value);
        lobbyList[index] = lobbyList[0];
      }
      lobbyList[0] = LobbyConnection.PreviousLobby.Value;
    }
    return orig(self, lobbyList);
  }
}
