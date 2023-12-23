using Better_Lobbies.Utilities.Coroutines;
using Better_Lobbies.Utilities.MonoBehaviours;
using TMPro;
using UnityEngine;

namespace Better_Lobbies.Utilities.Listeners
{
    internal class MenuLobbyCodeButtonListeners
    {
        internal static void OnClick(TextMeshProUGUI LobbyCodeTextMesh)
        {
            CoroutineHandler.Instance.NewCoroutine(LobbyCoroutines.CopyCode(LobbyCodeTextMesh));
        }
    }
}
