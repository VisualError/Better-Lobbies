using HarmonyLib;
using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace Better_Lobbies.Patches
{
    class LobbyConnectionFixes
    {
        internal static Lobby? previousLobby;

        [HarmonyPatch(typeof(GameNetworkManager), "SteamMatchmaking_OnLobbyMemberJoined")]
		[HarmonyPrefix]
		static bool SteamMatchmaking_OnLobbyMemberJoined(ref Lobby lobby, ref Friend friend)
		{
			if (NetworkManager.Singleton == null)
			{
				Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, "NetworkManager Singleton is set to null.");
			}
			List<Friend> membersList = GameNetworkManager.Instance.currentLobby.Value.Members.ToList();
			if (membersList == null)
			{
				Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, "CurrentLobby members does not exist for some reason.");
			}
			return true;
		}
		[HarmonyPatch(typeof(GameNetworkManager), "OnEnable")]
		[HarmonyPrefix]
		static bool GameNetworkManager_Subscribe()
		{
			SteamMatchmaking.OnLobbyEntered += SteamMatchmaking_OnLobbyEntered;
			return true;
		}

		private static void SteamMatchmaking_OnLobbyEntered(Lobby obj)
		{
			Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, "Entered lobby successfully!");
			previousLobby = obj;

        }

		[HarmonyPatch(typeof(NetworkConnectionManager), "HandleConnectionApproval")]
		[HarmonyPrefix]
		static bool HandleConnectionApproval(NetworkManager.ConnectionApprovalResponse response)
		{
			if (!response.Approved)
			{
				Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, "Connection not approved!");
			}
			else
			{
				Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, "Connection approved!");
			}
			return true;
		}

		[HarmonyPatch(typeof(NetworkConnectionManager), "SendConnectionRequest")]
		[HarmonyPrefix]
		static bool SendConnectionRequest()
		{
			Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, "SendConnectionRequest method called!");
			return true;
		}

		[HarmonyPatch(typeof(NetworkConnectionManager), "ApproveConnection")]
		[HarmonyPrefix]
		static bool ApproveConnection(ref ConnectionRequestMessage connectionRequestMessage, ref NetworkContext context)
		{
			Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, "approval called!");
			return true;
		}

		[HarmonyPatch(typeof(NetworkMessageManager), "ClientDisconnected")]
		[HarmonyPrefix]
		static bool ClientDisconnected()
		{
			Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, "ClientDisconnected called!");
			LobbyPatches.QuickMenu = null;
            return true;
		}


		[HarmonyPatch(typeof(GameNetworkManager), "OnDisable")]
		[HarmonyPrefix]
		static bool GameNetworkManager_Unsubscribe()
		{
			SteamMatchmaking.OnLobbyEntered -= SteamMatchmaking_OnLobbyEntered;
			return true;
		}
	}


	// Currently all just debugging.
}
