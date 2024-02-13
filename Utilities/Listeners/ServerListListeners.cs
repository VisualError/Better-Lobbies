using Better_Lobbies.Utilities.Coroutines;
using Better_Lobbies.Utilities.MonoBehaviours;
using UnityEngine;

namespace Better_Lobbies.Utilities.Listeners
{
    internal class ServerListListeners
    {
        internal static void OnEndEdit(string value)
        {
            SteamLobbyManager lobbyManager = Object.FindObjectOfType<SteamLobbyManager>();
            if (ulong.TryParse(value, out ulong result))
            {
                CoroutineHandler.Instance.NewCoroutine(lobbyManager, SearchCoroutines.JoinLobby(result, lobbyManager));
                return;
            }
            lobbyManager.LoadServerList();
        }
    }
}
