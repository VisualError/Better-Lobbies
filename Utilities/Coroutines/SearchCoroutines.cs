using BepInEx;
using Better_Lobbies.Patches;
using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Better_Lobbies.Utilities.Coroutines
{
    internal class SearchCoroutines
    {
        internal static IEnumerator JoinLobby(ulong lobbyId, SteamLobbyManager lobbyManager)
        {
            BetterLobbiesBase.Logger.LogWarning("Getting Lobby");
            Task<Lobby?> joinTask = SteamMatchmaking.JoinLobbyAsync(lobbyId);
            yield return new WaitUntil(() => joinTask.IsCompleted);
            if (joinTask.Result.HasValue)
            {
                BetterLobbiesBase.Logger.LogWarning("Getting Lobby Value");
                Lobby lobby = joinTask.Result.Value;
                if (!lobby.GetData("vers").IsNullOrWhiteSpace())
                {
                    LobbySlot.JoinLobbyAfterVerifying(lobby, lobby.Id);
                    BetterLobbiesBase.Logger.LogWarning("Success!");
                    ServerListPatch.searchInputField.text = "";
                }
                else
                {
                    BetterLobbiesBase.Logger.LogWarning($"Failed to join lobby {lobbyId}");
                    lobbyManager.LoadServerList();
                }
            }
            else
            {
                BetterLobbiesBase.Logger.LogWarning("Failed to join lobby.");
            }
        }
    }
}
