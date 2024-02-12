using BepInEx;
using Better_Lobbies.Patches;
using Better_Lobbies.Utilities.Listeners;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Better_Lobbies.Utilities.Coroutines
{
    internal class LobbyCoroutines
    {

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
