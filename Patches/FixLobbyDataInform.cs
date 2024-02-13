using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System;
using Unity.Netcode;
using Steamworks;
using Steamworks.Data;

namespace Better_Lobbies.Patches
{
    // TODO: Add more information for lobby disconnection issues
    internal class FixLobbyDataInform
    {

		[HarmonyPatch(typeof(SteamLobbyManager), nameof(SteamLobbyManager.OnEnable))]
		[HarmonyPostfix]
		static void OffensiveNames(SteamLobbyManager __instance)
        {
			__instance.censorOffensiveLobbyNames = Plugin.Instance.CensorLobbyNames.Value; 
        }

		// TODO: Add join friend notification.
		[HarmonyPatch(typeof(GameNetworkManager), "SteamMatchmaking_OnLobbyInvite")]
		[HarmonyPrefix]
		static bool SteamMatchmaking_OnLobbyInvite(ref GameNetworkManager __instance, ref Friend friend, ref Lobby lobby)
        {
			string text = $"{friend.Name} invited you to join {lobby.GetData("name") ?? "a lobby"}";

			if (__instance.currentLobby.HasValue)
            {
				HUDManager.Instance.DisplayTip("You were invited to join a crew!", text);
            }
            else
            {
				// TODO: Add confirmation/deny button. Add a setting panel if notif should show up.
				/*MenuManager menuManager = Object.FindObjectOfType<MenuManager>();
				menuManager.DisplayMenuNotification($"You were inivted to a game!\n{text}", " [ Back ] ");*/
			}
			return false;
        }

		[HarmonyPatch(typeof(GameNetworkManager))]
		[HarmonyPatch(nameof(GameNetworkManager.LobbyDataIsJoinable))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> LobbyDataIsJoinableTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = instructions.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].operand is string str && str == "An error occured!")
                {
                    list[i] = new CodeInstruction(OpCodes.Ldstr, "A blocked user is hosting this lobby!");
                }
            }
            return list.AsEnumerable();
        }

		[HarmonyPatch(typeof(ConnectionRequestMessage), "Deserialize")]
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction> ConnectionRequestMessageTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			string text = "";
			List<CodeInstruction> list = instructions.ToList();
			List<CodeInstruction> newList = new List<CodeInstruction>();
			for (int i = 0; i < list.Count; i++)
			{
				CodeInstruction codeInstruction = list[i];
				if (codeInstruction.opcode == OpCodes.Ldstr)
				{
					text = codeInstruction.operand.ToString();
				}
				if (codeInstruction.opcode == OpCodes.Callvirt && codeInstruction.operand.ToString().Contains("DisconnectClient") && list[i-1].opcode != OpCodes.Ldstr && !text.IsNullOrWhiteSpace())
				{
					newList.Add(new CodeInstruction(OpCodes.Ldstr, text));
					newList.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(NetworkManager), "DisconnectClient", new Type[]
					{
						typeof(ulong),
						typeof(string)
					}, null)));
				}
				else
				{
					newList.Add(codeInstruction);
				}
			}
			return newList;
		}
	}
}
