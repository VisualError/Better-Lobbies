using BepInEx;
using HarmonyLib;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Better_Lobbies.Patches
{
    // keeping this just incase.
    [HarmonyPatch]
    [HarmonyPriority(Priority.First)]
    [HarmonyWrapSafe]
    internal class Transpiler_StartHost
    {
        // Taken from: https://github.com/MaxWasUnavailable/LobbyCompatibility/blob/master/LobbyCompatibility/Features/HarmonyHelper.cs#L21
        public static MethodInfo? GetAsyncInfo(Type type, string method)
        {
            // Get the Method Info of the target Async Method
            return AccessTools.Method(type, method)
                // Find the AsyncStateMachine class from target method
                .GetCustomAttribute<AsyncStateMachineAttribute>()
                // Get the struct type (random compiler junk)
                .StateMachineType.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
        }


        // Taken from: https://github.com/MaxWasUnavailable/LobbyCompatibility/blob/update-filter-transpiler/LobbyCompatibility/Patches/LoadServerListTranspiler.cs#L28
        [HarmonyTargetMethod]
        private static MethodBase? TargetMethod()
        {
            // Target async method using the HarmonyHelper
            return GetAsyncInfo(typeof(GameNetworkManager), nameof(GameNetworkManager.StartHost));
        }

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
        {
            var disableSteamField =
                AccessTools.Field(typeof(GameNetworkManager), nameof(GameNetworkManager.disableSteam));

            var subscribeToConnectionCallbacksMethod =
                AccessTools.Method(typeof(GameNetworkManager), nameof(GameNetworkManager.SubscribeToConnectionCallbacks));

            var codeMatcher = new CodeMatcher(instructions, ilGenerator);

            codeMatcher
                .MatchForward(false, [
                new CodeMatch(OpCodes.Ldloc_1),
                new CodeMatch(OpCodes.Ldfld, disableSteamField),
                new CodeMatch(OpCodes.Brtrue) ])
                .ThrowIfNotMatch("Nothing found.")
                .Advance(1)
                .InsertAndAdvance([
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Call, subscribeToConnectionCallbacksMethod) ])
                .MatchForward(true, [
                new CodeMatch(OpCodes.Ldloc_1),
                new CodeMatch(OpCodes.Call, subscribeToConnectionCallbacksMethod) ])
                .RemoveInstruction()
                .Advance(-1)
                .RemoveInstruction();
            return codeMatcher.InstructionEnumeration();
        }
    }
}
