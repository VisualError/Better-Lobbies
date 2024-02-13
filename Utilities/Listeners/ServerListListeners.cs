using Better_Lobbies.Patches;
using Better_Lobbies.Utilities.MonoBehaviours;
using Steamworks.Data;
using Steamworks;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;
using BepInEx;

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

        internal static IEnumerator JoinLobby(ulong lobbyId, SteamLobbyManager lobbyManager)
        {
            Plugin.Logger.LogWarning("Getting Lobby");
            Task<Lobby?> joinTask = SteamMatchmaking.JoinLobbyAsync(lobbyId);
            yield return new WaitUntil(() => joinTask.IsCompleted);
            if (joinTask.Result.HasValue)
            {
                Plugin.Logger.LogWarning("Getting Lobby Value");
                Lobby lobby = joinTask.Result.Value;
                if (!lobby.GetData("vers").IsNullOrWhiteSpace())
                {
                    LobbySlot.JoinLobbyAfterVerifying(lobby, lobby.Id);
                    Plugin.Logger.LogWarning("Success!");
                    ServerListPatch.searchInputField.text = "";
                }
                else
                {
                    Plugin.Logger.LogWarning($"Failed to join lobby {lobbyId}");
                    lobbyManager.LoadServerList();
                }
            }
            else
            {
                Plugin.Logger.LogWarning("Failed to join lobby.");
            }
        }
    }
}
