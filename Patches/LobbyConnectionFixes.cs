using GameNetcodeStuff;
using HarmonyLib;
using Steamworks;
using Steamworks.Data;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Better_Lobbies.Patches
{
    class LobbyConnectionFixes
    {
		[HarmonyPatch(typeof(GameNetworkManager), "SteamMatchmaking_OnLobbyMemberJoined")]
		[HarmonyPrefix]
		static bool a(ref Lobby lobby, ref Friend friend)
		{
			if (NetworkManager.Singleton == null)
			{
				Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, "NetworkManager Singleton is set to null");
			}
			Friend[] array = GameNetworkManager.Instance.currentLobby.Value.Members.ToArray();
			if (array == null)
			{
				Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, "currentlobby members does not exist for some reason");
			}
			return true;
		}
		[HarmonyPatch(typeof(GameNetworkManager), "OnEnable")]
		[HarmonyPrefix]
		static bool bruh()
		{
			SteamMatchmaking.OnLobbyEntered += SteamMatchmaking_OnLobbyEntered;
			return true;
		}

		private static void SteamMatchmaking_OnLobbyEntered(Lobby obj)
		{
			Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, "Entered lobby successfully!");
		}

		[HarmonyPatch(typeof(NetworkConnectionManager), "HandleConnectionApproval")]
		[HarmonyPrefix]
		static bool asds(NetworkManager.ConnectionApprovalResponse response)
		{
			if (!response.Approved)
			{
				Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, "connection not approved!");
			}
			else
			{
				Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, "connection approved!");
			}
			return true;
		}

		[HarmonyPatch(typeof(NetworkConnectionManager), "SendConnectionRequest")]
		[HarmonyPrefix]
		static bool asdd()
		{
			Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, "SendConnectionRequest method called!");
			return true;
		}

		[HarmonyPatch(typeof(NetworkConnectionManager), "ApproveConnection")]
		[HarmonyPrefix]
		static bool weee(ref ConnectionRequestMessage connectionRequestMessage, ref NetworkContext context)
		{
			Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, "approval called!");
			return true;
		}

		[HarmonyPatch(typeof(NetworkMessageManager), "ClientDisconnected")]
		[HarmonyPrefix]
		static bool ClientDisconnected()
		{
			Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, "ClientDisconnected called!");
			return true;
		}


		[HarmonyPatch(typeof(GameNetworkManager), "OnDisable")]
		[HarmonyPrefix]
		static bool bruh2()
		{
			SteamMatchmaking.OnLobbyEntered -= SteamMatchmaking_OnLobbyEntered;
			return true;
		}
	}


	// Currently all just debugging.
}
