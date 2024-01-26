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

        internal static IEnumerator LoadServerList(SteamLobbyManager lobbyManager)
        {
            yield return new WaitUntil(() => (lobbyManager.levelListContainer.GetComponent<RectTransform>().childCount - 1 != 0) && (lobbyManager.levelListContainer.GetComponent<RectTransform>().childCount - 1) == Object.FindObjectsOfType(typeof(LobbySlot)).Length && !GameNetworkManager.Instance.waitingForLobbyDataRefresh);
            try
            {
                LobbySlot[] lobbies = Object.FindObjectsOfType<LobbySlot>();
                string searchText = ServerListPatch.searchInputField.text;
                int i = 0;
                float lobbySlotPositionOffset = 0f;
                foreach (LobbySlot slot in lobbies)
                {
                    i++;
                    GameObject JoinButton = slot.transform.Find("JoinButton")?.gameObject;
                    Lobby lobby = slot.thisLobby;
                    if (JoinButton != null)
                    {
                        GameObject CopyCodeButton = Object.Instantiate(JoinButton, JoinButton.transform.parent);
                        CopyCodeButton.SetActive(true);
                        RectTransform rectTransform = CopyCodeButton.GetComponent<RectTransform>();
                        rectTransform!.anchoredPosition -= new Vector2(78f, 0f);
                        CopyCodeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Code";
                        Button ButtonComponent = CopyCodeButton.GetComponent<Button>();
                        ButtonComponent!.onClick = new Button.ButtonClickedEvent();
                        ButtonComponent!.onClick.AddListener(() => LobbySlotListeners.CopyLobbyCodeToClipboard(slot));
                    }
                    if(!searchText.IsNullOrWhiteSpace() && !slot.LobbyName.text.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    {
                        i--;
                        Object.DestroyImmediate(slot.gameObject);
                        continue;
                    }
                    slot.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, lobbySlotPositionOffset);
                    lobbySlotPositionOffset -= 42f;
                }
                RectTransform rect = lobbyManager.levelListContainer.GetComponent<RectTransform>();
                float newWidth = rect.sizeDelta.x;
                float newHeight = Mathf.Max(0, (i) * 42f);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
            }
            catch(Exception ex)
            {
                Plugin.Logger.LogError(ex);
            }
        }

        internal static IEnumerator modifiedLoadLobbyIEnumerator(SteamLobbyManager __instance, Lobby[] ___currentLobbyList, float ___lobbySlotPositionOffset)
        {
            string[] offensiveWords = new string[]
        {
            "nigger", "faggot", "n1g", "nigers", "cunt", "pussies", "pussy", "minors", "chink", "buttrape",
            "molest", "rape", "coon", "negro", "beastiality", "cocks", "cumshot", "ejaculate", "pedophile", "furfag",
            "necrophilia", "yiff", "sex"
        };
            foreach (Lobby currentLobby in ___currentLobbyList)
            {
                List<Friend> blockedUsers = SteamFriends.GetBlocked().ToList();
                if (blockedUsers != null)
                {
                    foreach (Friend blockedUser in blockedUsers)
                    {
                        if (currentLobby.IsOwnedBy(blockedUser.Id))
                        {
                            continue;
                        }
                    }
                }
                string lobbyName = currentLobby.GetData("name");
                if (!lobbyName.IsNullOrWhiteSpace())
                {
                    if (__instance.censorOffensiveLobbyNames)
                    {
                        foreach (string word in offensiveWords)
                        {
                            if (lobbyName.Contains(word, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                        }
                    }
                    GameObject gameObject;
                    if (currentLobby.GetData("chal") == "t")
                    {
                        gameObject = __instance.LobbySlotPrefabChallenge;
                    }
                    else
                    {
                        gameObject = __instance.LobbySlotPrefab;
                    }
                    GameObject gameObject2 = Object.Instantiate(gameObject, __instance.levelListContainer);
                    gameObject2.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, ___lobbySlotPositionOffset);
                    ___lobbySlotPositionOffset -= 42f;
                    LobbySlot componentInChildren = gameObject2.GetComponentInChildren<LobbySlot>();
                    componentInChildren.LobbyName.text = lobbyName.Substring(0, Mathf.Min(lobbyName.Length, 40));
                    componentInChildren.playerCount.text = string.Format("{0} / {1}", currentLobby.MemberCount, currentLobby.MaxMembers);
                    componentInChildren.lobbyId = currentLobby.Id;
                    componentInChildren.thisLobby = currentLobby;
                    lobbyName = null;
                }
            }
            yield break;
        }
    }
}
