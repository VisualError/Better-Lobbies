using Better_Lobbies.Utilities.Listeners;
using HarmonyLib;
using System;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Better_Lobbies.Patches
{
    [HarmonyPatch(typeof(MenuManager))]
    internal class ServerListPatch
    {
        internal static TMP_InputField? searchInputField;
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void AwakePatch(ref MenuManager __instance)
        {
            GameObject obj = GameObject.Find("/Canvas/MenuContainer/LobbyList/JoinCode");
            if(obj != null)
            {
                try
                {
                    GameObject searchBoxObject = Object.Instantiate(obj.gameObject, obj.transform.parent);
                    searchBoxObject.SetActive(true);
                    searchInputField = searchBoxObject.GetComponent<TMP_InputField>();
                    searchInputField.interactable = true;
                    searchInputField.placeholder.gameObject.GetComponent<TextMeshProUGUI>().text = "Search or Enter a room code...";
                    searchInputField.onEndEdit.m_PersistentCalls.Clear();
                    searchInputField.onEndTextSelection.m_PersistentCalls.Clear();
                    searchInputField.onSubmit.m_PersistentCalls.Clear();
                    searchInputField.onSubmit.AddListener(ServerListListeners.OnEndEdit);
                }
                catch (Exception err)
                {
                    Plugin.Logger.LogError(err);
                }
            }
        }
    }
}