using Better_Lobbies.Utilities.Listeners;
using HarmonyLib;
using Steamworks.Data;
using System.Collections.Generic;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Better_Lobbies.Patches
{
    internal class LobbyPatches
    {
        // Taken from https://github.com/MaxWasUnavailable/LobbyCompatibility/blob/master/LobbyCompatibility/Patches/LoadLobbyListAndFilterTranspiler.cs#L22. Slightly modified.
        [HarmonyPatch(typeof(SteamLobbyManager), nameof(SteamLobbyManager.loadLobbyListAndFilter), MethodType.Enumerator)]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> loadLobbyListAndFilter_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var currentLobbyListField =
            AccessTools.Field(typeof(SteamLobbyManager), nameof(SteamLobbyManager.currentLobbyList));
            var thisLobbyField =
                AccessTools.Field(typeof(LobbySlot), nameof(LobbySlot.thisLobby));

            var initializeLobbySlotMethod =
                AccessTools.Method(typeof(LobbyPatches), nameof(InitializeLobbySlot));

            // Does the following:
            // - Adds dup before last componentInChildren line to keep componentInChildren value on the stack
            // - Calls InitializeLobbySlot(lobbySlot)

            return new CodeMatcher(instructions)
                
                .MatchForward(false, [
                new CodeMatch(OpCodes.Ldloc_1),
                new CodeMatch(OpCodes.Ldfld, currentLobbyListField),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(inst => inst.opcode == OpCodes.Ldfld), // Compiler-generated field
                new CodeMatch(OpCodes.Ldelem, typeof(Lobby)),
                new CodeMatch(OpCodes.Stfld, thisLobbyField) ])
                .ThrowIfNotMatch("Unable to find LobbySlot.thisLobby line.")
                .InsertAndAdvance([
                new CodeInstruction(OpCodes.Dup) ])
                .Advance(6)
                .InsertAndAdvance([
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Call, initializeLobbySlotMethod) ])
                .InstructionEnumeration();
        }

        private void InitializeLobbySlot(LobbySlot lobbySlot)
        {
            lobbySlot.playerCount.text = string.Format("{0} / {1}", lobbySlot.thisLobby.MemberCount, lobbySlot.thisLobby.MaxMembers);
            var JoinButton = lobbySlot.GetComponentInChildren<Button>();
            if (JoinButton != null)
            {
                InitializeJoinButton(lobbySlot, JoinButton);
                var CopyCodeButton = Object.Instantiate(JoinButton, JoinButton.transform.parent);
                CopyCodeButton.name = "CopyCodeButton";
                RectTransform rectTransform = CopyCodeButton.GetComponent<RectTransform>();
                rectTransform!.anchoredPosition -= new Vector2(78f, 0f);
                var TextMesh = CopyCodeButton.GetComponentInChildren<TextMeshProUGUI>();
                TextMesh.text = "Code";
                CopyCodeButton!.onClick.m_PersistentCalls.Clear();
                CopyCodeButton!.onClick.AddListener(() => LobbySlotListeners.CopyLobbyCodeToClipboard(lobbySlot, TextMesh));
            }
        }

        private void InitializeJoinButton(LobbySlot lobbySlot, Button joinButton)
        {
            if (!LobbyConnectionFixes.previousLobby.HasValue) return;
            if (LobbyConnectionFixes.previousLobby.Value.Id != lobbySlot.thisLobby.Id) return;
            var TextMesh = joinButton.GetComponentInChildren<TextMeshProUGUI>();
            TextMesh.text = "Rejoin";
        }

        // TODO: Stop using GameObject.Find. Either cache the results or find a different way to programatically insert these into the UI.
        public static GameObject? QuickMenu;
        [HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.Start))]
        [HarmonyPostfix]
        private static void QuickMenuStart()
        {
            if (QuickMenu == null)
                QuickMenu = GameObject.Find("/Systems/UI/Canvas/QuickMenu/");
            GameObject? ResumeObj = QuickMenu.transform.Find("MainButtons/Resume/")?.gameObject;
            if (ResumeObj != null && GameObject.Find("CopyLobbyCode") == null)
            {
                GameObject LobbyCodeObj = Object.Instantiate(ResumeObj.gameObject, ResumeObj.transform.parent);
                LobbyCodeObj.name = "CopyLobbyCode";

                TextMeshProUGUI LobbyCodeTextMesh = LobbyCodeObj.GetComponentInChildren<TextMeshProUGUI>();
                LobbyCodeTextMesh.text = "> Lobby Code";

                Button LobbyCodeButton = LobbyCodeObj.GetComponent<Button>();
                LobbyCodeButton.onClick.m_PersistentCalls.Clear();
                LobbyCodeButton.onClick.AddListener(() => MenuLobbyCodeButtonListeners.OnClick(LobbyCodeTextMesh));

                RectTransform rect = LobbyCodeObj.GetComponent<RectTransform>();
                rect.localPosition = ResumeObj.transform.localPosition + new Vector3(0f, 182f, 0f);
                rect.localScale = ResumeObj.transform.localScale;
            }
        }

        [HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.OpenQuickMenu))]
        [HarmonyPostfix]
        private static void OpenQuickMenu()
        {
            if (QuickMenu == null)
                QuickMenu = GameObject.Find("/Systems/UI/Canvas/QuickMenu/");
            AdjustLobbyCodeButton();
            AddCrewCount();
        }

        private static void AddCrewCount()
        {
            TextMeshProUGUI CrewHeaderText = QuickMenu!.transform.Find("PlayerList/Image/Header").GetComponentInChildren<TextMeshProUGUI>();
            if (CrewHeaderText != null)
            {
                CrewHeaderText.text = $"CREW ({(StartOfRound.Instance?.connectedPlayersAmount ?? 0) + 1}):";
            }
        }

        private static void AdjustLobbyCodeButton()
        {
            GameObject? ResumeObj = QuickMenu!.transform.Find("MainButtons/Resume/")?.gameObject;
            if (ResumeObj != null)
            {
                GameObject? DebugMenu = QuickMenu.transform.Find("DebugMenu")?.gameObject;
                GameObject? LobbyCodeObj = DebugMenu?.transform.Find("CopyLobbyCode")?.gameObject;
                if (LobbyCodeObj == null)
                    LobbyCodeObj = QuickMenu.transform.Find("MainButtons/CopyLobbyCode/")?.gameObject;
                RectTransform? rect = LobbyCodeObj?.GetComponent<RectTransform>();
                if (rect == null)
                {
                    Plugin.Logger.LogWarning("Rect for LobbyCodeObj is null.");
                    return;
                }
                if (DebugMenu != null && DebugMenu.activeSelf)
                {
                    LobbyCodeObj?.transform.SetParent(DebugMenu.transform);
                    rect.localPosition = new Vector3(125f, 185f, 0f);
                    rect.localScale = new Vector3(1f, 1f, 1f);
                }
                else
                {
                    LobbyCodeObj?.transform.SetParent(ResumeObj.transform.parent);
                    RectTransform resumeRect = ResumeObj.GetComponent<RectTransform>();
                    rect.localPosition = resumeRect.localPosition + new Vector3(0f, 182f, 0f);
                    rect.localScale = resumeRect.localScale;
                }
            }
        }
    }
}