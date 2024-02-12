using Better_Lobbies.Utilities.Listeners;
using HarmonyLib;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Better_Lobbies.Patches
{
    internal class LobbyPatch
    {
        [HarmonyPatch(typeof(SteamLobbyManager), "loadLobbyListAndFilter")]
        [HarmonyPostfix]
        private static IEnumerator loadLobbyListAndFilter(IEnumerator result)
        {
            SteamLobbyManager lobbyManager = Object.FindFirstObjectByType<SteamLobbyManager>();
            RectTransform rect = lobbyManager.levelListContainer.GetComponent<RectTransform>();
            float newWidth = rect.sizeDelta.x;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
            float newHeight = Mathf.Max(0, 50 * 42f);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);

            while (result.MoveNext())
                yield return result.Current;

            LobbySlot[] lobbySlots = Object.FindObjectsOfType<LobbySlot>();
            foreach (LobbySlot lobbySlot in lobbySlots)
            {
                lobbySlot.playerCount.text = string.Format("{0} / {1}", lobbySlot.thisLobby.MemberCount, lobbySlot.thisLobby.MaxMembers);

                GameObject JoinButton = lobbySlot.transform.Find("JoinButton")?.gameObject;
                if (JoinButton != null)
                {
                    GameObject CopyCodeButton = Object.Instantiate(JoinButton, JoinButton.transform.parent);
                    CopyCodeButton.name = "CopyCodeButton";
                    RectTransform rectTransform = CopyCodeButton.GetComponent<RectTransform>();
                    rectTransform!.anchoredPosition -= new Vector2(78f, 0f);
                    CopyCodeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Code";
                    Button ButtonComponent = CopyCodeButton.GetComponent<Button>();
                    ButtonComponent!.onClick = new Button.ButtonClickedEvent();
                    ButtonComponent!.onClick.AddListener(() => LobbySlotListeners.CopyLobbyCodeToClipboard(lobbySlot));
                    CopyCodeButton.SetActive(true);
                }
            }

            newHeight = Mathf.Max(0, lobbySlots.Length * 42f);
            rect?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
        }

        [HarmonyPatch(typeof(QuickMenuManager), "Start")]
        [HarmonyPostfix]
        private static void QuickMenuStart()
        {
            GameObject ResumeObj = GameObject.Find("/Systems/UI/Canvas/QuickMenu/MainButtons/Resume/");
            if (ResumeObj != null && GameObject.Find("CopyLobbyCode") == null)
            {
                GameObject LobbyCodeObj = Object.Instantiate(ResumeObj.gameObject, ResumeObj.transform.parent);
                LobbyCodeObj.name = "CopyLobbyCode";

                TextMeshProUGUI LobbyCodeTextMesh = LobbyCodeObj.GetComponentInChildren<TextMeshProUGUI>();
                LobbyCodeTextMesh.text = "> Lobby Code";

                Button LobbyCodeButton = LobbyCodeObj.GetComponent<Button>();
                LobbyCodeButton.onClick = new Button.ButtonClickedEvent();
                LobbyCodeButton.onClick.AddListener(() => MenuLobbyCodeButtonListeners.OnClick(LobbyCodeTextMesh));

                RectTransform rect = LobbyCodeObj.GetComponent<RectTransform>();
                rect.localPosition = ResumeObj.transform.localPosition + new Vector3(0f, 182f, 0f);
                rect.localScale = ResumeObj.transform.localScale;
            }
        }

        [HarmonyPatch(typeof(QuickMenuManager), "OpenQuickMenu")]
        [HarmonyPostfix]
        private static void OpenQuickMenu()
        {
            AdjustLobbyCodeButton();
            AddCrewCount();
        }

        private static void AddCrewCount()
        {
            TextMeshProUGUI CrewHeaderText = GameObject.Find("/Systems/UI/Canvas/QuickMenu/PlayerList/Image/Header")?.GetComponentInChildren<TextMeshProUGUI>();
            if (CrewHeaderText != null)
            {
                CrewHeaderText.text = $"CREW ({(StartOfRound.Instance?.connectedPlayersAmount ?? 0) + 1}):";
            }
        }

        private static void AdjustLobbyCodeButton()
        {
            GameObject ResumeObj = GameObject.Find("/Systems/UI/Canvas/QuickMenu/MainButtons/Resume/");
            GameObject PlayerListObj = GameObject.Find("/Systems/UI/Canvas/QuickMenu/PlayerList/");
            if (ResumeObj != null && PlayerListObj != null)
            {
                GameObject DebugMenu = GameObject.Find("/Systems/UI/Canvas/QuickMenu/DebugMenu/");
                GameObject LobbyCodeObj = GameObject.Find("/Systems/UI/Canvas/QuickMenu/DebugMenu/CopyLobbyCode/");
                if(LobbyCodeObj == null)
                {
                    LobbyCodeObj = GameObject.Find("/Systems/UI/Canvas/QuickMenu/MainButtons/CopyLobbyCode/");
                }
                RectTransform rect = LobbyCodeObj.GetComponent<RectTransform>();
                if (DebugMenu != null && DebugMenu.activeSelf)
                {
                    LobbyCodeObj.transform.SetParent(DebugMenu.transform);
                    rect.localPosition = new Vector3(125f, 185f, 0f);
                    rect.localScale = new Vector3(1f, 1f, 1f);
                }
                else
                {
                    LobbyCodeObj.transform.SetParent(ResumeObj.transform.parent);
                    RectTransform resumeRect = ResumeObj.GetComponent<RectTransform>();
                    rect.localPosition = resumeRect.localPosition + new Vector3(0f, 182f, 0f);
                    rect.localScale = resumeRect.localScale;
                }
            }
        }
    }
}