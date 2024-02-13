using BepInEx;
using Better_Lobbies.Utilities.MonoBehaviours;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Better_Lobbies.Utilities.Listeners
{
    internal class LobbySlotListeners
    {
        internal static void CopyLobbyCodeToClipboard(LobbySlot slot, TextMeshProUGUI textMesh)
        {
            CoroutineHandler.Instance.NewCoroutine(LobbySlotCopyCode(slot, textMesh));
        }

        internal static IEnumerator LobbySlotCopyCode(LobbySlot slot, TextMeshProUGUI textMesh)
        {
            string oldtext = textMesh.text;

            string lobbyCode = slot.lobbyId.ToString();
            if (!lobbyCode.IsNullOrWhiteSpace())
            {
                GUIUtility.systemCopyBuffer = lobbyCode;
                Plugin.Logger.LogInfo("Lobby code copied to clipboard: " + lobbyCode);
                textMesh.text = "Copied!";
            }
            else
            {
                textMesh.text = "Can't get Lobby code!";
            }
            yield return new WaitForSeconds(1.2f);
            textMesh.text = oldtext;
            yield break;
        }
    }
}
