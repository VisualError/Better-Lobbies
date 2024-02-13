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
    [HarmonyPatch]
    [HarmonyPriority(Priority.First)]
    [HarmonyWrapSafe]
    internal class LoadServerListTranspiler
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
            return GetAsyncInfo(typeof(SteamLobbyManager), nameof(SteamLobbyManager.LoadServerList));
        }

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
        {
            var currentLobbyListField =
                AccessTools.Field(typeof(SteamLobbyManager), nameof(SteamLobbyManager.currentLobbyList));

            var FilterWithSearchMethod =
                AccessTools.Method(typeof(LoadServerListTranspiler), nameof(FilterWithSearch));

            var codeMatcher = new CodeMatcher(instructions, ilGenerator);
            codeMatcher
                .MatchForward(true, [
                new CodeMatch(OpCodes.Ldloc_1),
                new CodeMatch(inst => inst.operand is LocalBuilder local && local.LocalType == typeof(Lobby[])), // Dynamically changing ldloc_s.
                new CodeMatch(OpCodes.Stfld, currentLobbyListField)])
                .InsertAndAdvance(
                [
                    new CodeInstruction(OpCodes.Call, FilterWithSearchMethod),
                ]);
            return codeMatcher.InstructionEnumeration();
        }

        private static Lobby[] FilterWithSearch(Lobby[] lobbyList) // i suck at naming methods
        {
            var list = lobbyList.ToList();
            var searchText = ServerListPatch.searchInputField.text;
            if (searchText.IsNullOrWhiteSpace()) return lobbyList; // Return the original lobby list because theres nothing to filter.
            var filteredArray = list.Where(x => x.GetData("name").Contains(searchText, System.StringComparison.OrdinalIgnoreCase)).ToArray();
            return filteredArray;
        }
    }
}
