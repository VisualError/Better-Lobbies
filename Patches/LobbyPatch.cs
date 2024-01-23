using Better_Lobbies.Utilities.Coroutines;
using Better_Lobbies.Utilities.Listeners;
using Better_Lobbies.Utilities.MonoBehaviours;
using HarmonyLib;
using Steamworks.Data;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Better_Lobbies.Patches
{
    internal class LobbyPatch
    {
        [HarmonyPatch(typeof(SteamLobbyManager), nameof(SteamLobbyManager.LoadServerList))]
        [HarmonyPostfix]
        [HarmonyAfter("me.swipez.melonloader.morecompany")]
        public static void loadserverListPatch(ref SteamLobbyManager __instance)
        {
            CoroutineHandler.Instance.NewCoroutine(LobbyCoroutines.LoadServerList(__instance));
        }

        [HarmonyPatch(typeof(SteamLobbyManager), "loadLobbyListAndFilter")]
        [HarmonyAfter("me.swipez.melonloader.morecompany")]
        [HarmonyPrefix]
        public static bool loadLobbyPrefixPatch(ref SteamLobbyManager __instance, ref Lobby[] ___currentLobbyList, ref float ___lobbySlotPositionOffset, ref IEnumerator __result)
        {
            __result = LobbyCoroutines.modifiedLoadLobbyIEnumerator(__instance, ___currentLobbyList, ___lobbySlotPositionOffset);
            return false;
        }

        [HarmonyPatch(typeof(QuickMenuManager), "Start")]
        [HarmonyPostfix]
        private static void QuickMenuStart()
        {
            GameObject ResumeObj = GameObject.Find("/Systems/UI/Canvas/QuickMenu/MainButtons/Resume/");
            GameObject PlayerListObj = GameObject.Find("/Systems/UI/Canvas/QuickMenu/PlayerList/");
            if (ResumeObj != null && PlayerListObj != null && PlayerListObj.transform.Find("CopyLobbyCode") == null)
            {
                GameObject LobbyCodeObj = Object.Instantiate(ResumeObj.gameObject, PlayerListObj.transform);
                LobbyCodeObj.name = "CopyLobbyCode";

                RectTransform rect = LobbyCodeObj.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(125f, 185f, 0f);
                rect.localScale = new Vector3(1f, 1f, 1f);

                TextMeshProUGUI LobbyCodeTextMesh = LobbyCodeObj.GetComponentInChildren<TextMeshProUGUI>();
                LobbyCodeTextMesh.text = "> Lobby Code";

                Button LobbyCodeButton = LobbyCodeObj.GetComponent<Button>();
                LobbyCodeButton!.onClick = new Button.ButtonClickedEvent();
                LobbyCodeButton!.onClick.AddListener(() => MenuLobbyCodeButtonListeners.OnClick(LobbyCodeTextMesh));
            }
        }
    }
}