using GameNetcodeStuff;
using HarmonyLib;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

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

		[HarmonyPatch(typeof(NetworkManager), nameof(NetworkManager.SetSingleton))]
		[HarmonyPostfix]
		static void NetworkAwake()
		{
            NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
        }
		
		private static void Singleton_OnClientConnectedCallback(ulong obj)
        {
            Plugin.Logger.LogWarning($"Custom Debug Singleton_OnClientConnectedCallback event listener: {obj}");
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnClientConnect))]
		[HarmonyPostfix]
		private static void ClientConnect()
		{
            Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, "ClientConnectCalled");
		}

        // Goofy ahh patch.
        [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.Update))]
		[HarmonyPostfix]
		private static void SoundUpdate()
		{
            if (NetworkManager.Singleton.LocalClientId == 0 && !(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
            {
                GameNetworkManager.Instance.Disconnect();
            }
        }

		private static void SteamMatchmaking_OnLobbyEntered(Lobby obj)
		{
			Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, "Entered lobby successfully!");
			if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) return;
			previousLobby = obj;

        }

        [HarmonyPatch(typeof(NetworkConnectionManager), "HandleConnectionApproval")]
        [HarmonyPrefix]
        static bool HandleConnectionApproval(ulong ownerClientId, NetworkManager.ConnectionApprovalResponse response)
        {
            if (!response.Approved)
            {
                Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, $"Connection not approved! {ownerClientId}");
            }
            else
            {
                Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, $"Connection approved! {ownerClientId}");
            }
            return true;
        }

        static IEnumerator shit(ulong ownerClientId)
        {
            Plugin.Logger.LogWarning("called!");
            if (!(NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)) yield break;
            yield return new WaitUntil(() => NetworkManager.Singleton.ConnectedClientsIds.Contains(ownerClientId) && NetworkManager.Singleton.ConnectedClients[ownerClientId].IsConnected && StartOfRound.Instance != null && NetworkManager.Singleton.IsConnectedClient);
            StartOfRound.Instance.OnClientConnect(ownerClientId);
            GameNetworkManager.Instance.connectedPlayers++;
            yield break;
        }

		[HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnPlayerConnectedClientRpc))]
		[HarmonyPrefix]
		static void what()
		{
			Plugin.Logger.LogWarning($"{NetworkManager.Singleton.IsListening}");
		}

        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.FallWithCurve))]
        [HarmonyPrefix]
        static bool IMightHaveToRemoveThis()
        {
			return StartOfRound.Instance != null && StartOfRound.Instance.objectFallToGroundCurve != null && StartOfRound.Instance.objectFallToGroundCurveNoBounce != null;
        }


        static IEnumerator fuck2()
		{
			yield return new WaitForSeconds(5f);
			if (GameNetworkManager.Instance.localPlayerController == null && GameNetworkManager.Instance.currentLobby.HasValue)
			{
				GameNetworkManager.Instance.currentLobby.Value.Leave();
			}
		}

		[HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.ConnectionApproval))]
		[HarmonyPostfix]
		private static void ConnectionApprovalPostFix(GameNetworkManager __instance, ref NetworkManager.ConnectionApprovalRequest request)
		{
            if (request.ClientNetworkId == NetworkManager.Singleton.LocalClientId)
                return;
			__instance.StartCoroutine(shit(request.ClientNetworkId));
        }

        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Singleton_OnClientConnectedCallback))]
        [HarmonyPrefix]
        private static bool Singleton_OnClientConnectedCallback()
        {
			return false; // Just hook to your own OnClientConnectedCallback event modders. Im too lazy for this.
        }

		// Me commenting out this transpiler makes me wanna cry sometimes.
        /*
        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Singleton_OnClientConnectedCallback))]
		[HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Singleton_OnClientConnectedCallback_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
        {
			var startOfRoundInstanceGetter =
				AccessTools.PropertyGetter(typeof(StartOfRound), nameof(StartOfRound.Instance));
			var onClientConnectMethod =
				AccessTools.Method(typeof(StartOfRound), nameof(StartOfRound.OnClientConnect));
			var shitMethod =
				AccessTools.Method(typeof(LobbyConnectionFixes), nameof(shit));
            var startCoroutineMethod =
                AccessTools.Method(typeof(MonoBehaviour), nameof(MonoBehaviour.StartCoroutine), new Type[] { typeof(IEnumerator) });
            var inequalityMethod = AccessTools.Method(typeof(UnityEngine.Object), "op_Inequality", new Type[] { typeof(UnityEngine.Object), typeof(UnityEngine.Object) });

			var codeMatcher = new CodeMatcher(instructions, ilGenerator)
				.MatchForward(false, [
					new CodeMatch(OpCodes.Call, startOfRoundInstanceGetter),
					new CodeMatch(OpCodes.Ldnull),
					new CodeMatch(OpCodes.Call, inequalityMethod)
				]).Advance(1).RemoveInstructions(4) // magik
				.MatchForward(false, [
					new CodeMatch(OpCodes.Callvirt, onClientConnectMethod),
				]).RemoveInstruction().Advance(-2).Set(OpCodes.Ldarg_0, null).Advance(2).InsertAndAdvance([ // What.
					new CodeInstruction(OpCodes.Call, shitMethod),
					new CodeInstruction(OpCodes.Callvirt, startCoroutineMethod),
					new CodeInstruction(OpCodes.Pop)
				]).End();
                codeMatcher.Labels.RemoveAt(0);
            foreach (var a in codeMatcher.InstructionEnumeration())
			{
				Plugin.Logger.LogWarning(a.ToString());
			}
			return codeMatcher.InstructionEnumeration();
        }*/


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
			Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, "Approval called!");
			return true;
		}

		[HarmonyPatch(typeof(NetworkMessageManager), "ClientDisconnected")]
		[HarmonyPrefix]
		static bool ClientDisconnected(ulong clientId)
		{
            Plugin.Logger.Log(BepInEx.Logging.LogLevel.All, $"ClientDisconnected called!, {clientId} {NetworkManager.Singleton.LocalClientId}");
			/*if(clientId == NetworkManager.Singleton.LocalClientId && GameNetworkManager.Instance.currentLobby.HasValue)
			{
                GameNetworkManager.Instance.currentLobby.Value.Leave();
            } */
			LobbyPatches.QuickMenu = null;
            return true;
		}


		[HarmonyPatch(typeof(GameNetworkManager), "OnDisable")]
		[HarmonyPrefix]
		static bool GameNetworkManager_Unsubscribe()
		{
			SteamMatchmaking.OnLobbyEntered -= SteamMatchmaking_OnLobbyEntered;
            NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
            return true;
		}
	}


	// Currently all just debugging.
}
