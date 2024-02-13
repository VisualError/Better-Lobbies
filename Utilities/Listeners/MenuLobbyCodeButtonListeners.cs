using Better_Lobbies.Utilities.MonoBehaviours;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Better_Lobbies.Utilities.Listeners
{
    internal class MenuLobbyCodeButtonListeners
    {
        internal static void OnClick(TextMeshProUGUI LobbyCodeTextMesh)
        {
            CoroutineHandler.Instance.NewCoroutine(LobbyCodeTextMesh, CopyCode(LobbyCodeTextMesh));
        }

        internal static IEnumerator CopyCode(TextMeshProUGUI textMesh)
        {
            string oldtext = textMesh.text;
            if (GameNetworkManager.Instance.currentLobby.HasValue)
            {
                textMesh.text = "(Copied to clipboard!)";
                string id = GameNetworkManager.Instance.currentLobby.Value.Id.ToString();
                GUIUtility.systemCopyBuffer = id;
                Plugin.Logger.LogWarning("Lobby code copied to clipboard: " + id);
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
