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
		[HarmonyPatch(typeof(GameNetworkManager))]
		[HarmonyPatch("OnLocalClientConnectionDisapproved")]
		[HarmonyPostfix]
		static void OnLocalClientConnectionDisapproved()
		{
			if (NetworkManager.Singleton == null) return;
			if (!NetworkManager.Singleton.IsConnectedClient)
			{
				Object.FindObjectOfType<MenuManager>().SetLoadingScreen(false, RoomEnter.Error, "Failed to connect to lobby!\nConnection was not approved!");
			}
		}

		[HarmonyPatch(typeof(GameNetworkManager), "SteamMatchmaking_OnLobbyMemberJoined")]
		[HarmonyPrefix]
		static bool a(ref Lobby lobby, ref Friend friend)
		{
			if (NetworkManager.Singleton == null)
			{
				BetterLobbiesBase.Logger.Log(BepInEx.Logging.LogLevel.All, "NetworkManager Singleton is set to null");
			}
			Friend[] array = GameNetworkManager.Instance.currentLobby.Value.Members.ToArray();
			if (array == null)
			{
				BetterLobbiesBase.Logger.Log(BepInEx.Logging.LogLevel.All, "currentlobby members does not exist for some reason");
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
			BetterLobbiesBase.Logger.Log(BepInEx.Logging.LogLevel.All, "Entered lobby successfully!");
		}

		[HarmonyPatch(typeof(NetworkConnectionManager), "HandleConnectionApproval")]
		[HarmonyPrefix]
		static bool asds(NetworkManager.ConnectionApprovalResponse response)
		{
			if (!response.Approved)
			{
				BetterLobbiesBase.Logger.Log(BepInEx.Logging.LogLevel.All, "connection not approved!");
			}
			else
			{
				BetterLobbiesBase.Logger.Log(BepInEx.Logging.LogLevel.All, "connection approved!");
			}
			return true;
		}

		[HarmonyPatch(typeof(NetworkConnectionManager), "SendConnectionRequest")]
		[HarmonyPrefix]
		static bool asdd()
		{
			BetterLobbiesBase.Logger.Log(BepInEx.Logging.LogLevel.All, "SendConnectionRequest method called!");
			return true;
		}

		[HarmonyPatch(typeof(NetworkConnectionManager), "ApproveConnection")]
		[HarmonyPrefix]
		static bool weee(ref ConnectionRequestMessage connectionRequestMessage, ref NetworkContext context)
		{
			BetterLobbiesBase.Logger.Log(BepInEx.Logging.LogLevel.All, "approval called!");
			return true;
		}

		[HarmonyPatch(typeof(NetworkMessageManager), "ClientDisconnected")]
		[HarmonyPrefix]
		static bool ClientDisconnected()
		{
			BetterLobbiesBase.Logger.Log(BepInEx.Logging.LogLevel.All, "ClientDisconnected called!");
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
