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

        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        [HarmonyPostfix]
        public static void GameNetworkMangerAwakePatch()
        {
            GameObject ResumeObj = GameObject.Find("/Systems/UI/Canvas/QuickMenu/MainButtons/Resume/");
            GameObject DebugMenu = GameObject.Find("/Systems/UI/Canvas/QuickMenu/DebugMenu/");
            if (ResumeObj != null)
            {
                GameObject LobbyCodeObj = Object.Instantiate(ResumeObj.gameObject, ResumeObj.transform.parent);
                RectTransform rect = LobbyCodeObj.GetComponent<RectTransform>();
                var anchoredPosition = rect.anchoredPosition + new Vector2(0f, 182f);
                if (DebugMenu != null)
                {
                    DebugMenu.gameObject.GetComponent<RectTransform>().anchoredPosition += new Vector2(240f, 0f);
                }
                rect.anchoredPosition = anchoredPosition;
                TextMeshProUGUI LobbyCodeTextMesh = LobbyCodeObj.GetComponentInChildren<TextMeshProUGUI>();
                LobbyCodeTextMesh.text = "> Lobby Code";
                Button LobbyCodeButton = LobbyCodeObj.GetComponent<Button>();
                LobbyCodeButton!.onClick = new Button.ButtonClickedEvent();
                LobbyCodeButton!.onClick.AddListener(() => MenuLobbyCodeButtonListeners.OnClick(LobbyCodeTextMesh));
            }
        }


        [HarmonyPatch(typeof(QuickMenuManager), "OpenQuickMenu")]
        [HarmonyPostfix]
        private static void OpenQuickMenu()
        {
            TextMeshProUGUI CrewHeaderText = GameObject.Find("/Systems/UI/Canvas/QuickMenu/PlayerList/Image/Header")?.GetComponentInChildren<TextMeshProUGUI>();
            if (CrewHeaderText != null)
            {
                CrewHeaderText.text = $"CREW ({(StartOfRound.Instance?.connectedPlayersAmount ?? 0) + 1}):";
            }
        }
    }
}