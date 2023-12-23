using UnityEngine;

namespace Better_Lobbies.Utilities.Listeners
{
    internal class LobbySlotListeners
    {
        internal static void CopyLobbyCodeToClipboard(LobbySlot slot)
        {
            string lobbyCode = slot.lobbyId.ToString();
            GUIUtility.systemCopyBuffer = lobbyCode;
            BetterLobbiesBase.Logger.LogInfo("Lobby code copied to clipboard: " + lobbyCode);
        }
    }
}
