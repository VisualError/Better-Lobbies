using Better_Lobbies.Patches;
using Better_Lobbies.Utilities.MonoBehaviours;
using Steamworks.Data;
using Steamworks;
using UnityEngine;
using System.Collections;
using System.Linq;

namespace Better_Lobbies.Utilities.Listeners
{
    internal class ServerListListeners
    {
        internal static void OnEndEdit(string value)
        {
            SteamLobbyManager lobbyManager = Object.FindObjectOfType<SteamLobbyManager>();
            if (ulong.TryParse(value, out ulong result))
            {
                CoroutineHandler.Instance.NewCoroutine(lobbyManager, JoinLobby(result, lobbyManager));
                return;
            }
            lobbyManager.LoadServerList();
        }

        // this code sucks fr.
        internal static IEnumerator JoinLobby(ulong lobbyId, SteamLobbyManager lobbyManager)
        {
            Plugin.Logger.LogWarning("Getting Lobby");
            var joinTask = SteamMatchmaking.JoinLobbyAsync(lobbyId);
            yield return new WaitUntil(() => joinTask.IsCompleted);
            if (!joinTask.Result.HasValue)
            {
                Plugin.Logger.LogWarning("Failed to get lobby. Join task had no value.");
                lobbyManager.LoadServerList();
                yield break;
            }
            Plugin.Logger.LogWarning("Getting Lobby Value");
            Lobby lobby = joinTask.Result.Value;
            if (joinTask.Result.Value.Data.Any())
            {
                Plugin.Logger.LogWarning($"Failed to join lobby {lobbyId}. Searching instead.");
                lobbyManager.LoadServerList();
                yield break;
            }
            LobbySlot.JoinLobbyAfterVerifying(lobby, lobby.Id);
            Plugin.Logger.LogWarning("Successfully joined lobby using lobby code!");
            ServerListPatch.searchInputField.text = "";
        }
    }
}
