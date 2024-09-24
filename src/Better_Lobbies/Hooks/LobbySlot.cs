using BepInEx;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Better_Lobbies.Hooks;
internal class LobbySlot
{
  [HookInit]
  private static void Init()
  {
    On.LobbySlot.Awake += LobbySlot_Awake;
  }

  private static void LobbySlot_Awake(On.LobbySlot.orig_Awake orig, global::LobbySlot self)
  {
    orig(self);
    // Fix lobby count for any lobbies that has more than 4 max players.
    self.playerCount.text = string.Format("{0} / {1}", self.thisLobby.MemberCount, self.thisLobby.MaxMembers);

    // We'll create the lobby code button using this method so I don't have to open unity to create my own LobbySlot prefab lol.
    var JoinButton = self.GetComponentInChildren<Button>();
    if (JoinButton != null)
    {
      var CodeButton = Object.Instantiate(JoinButton, JoinButton.transform.parent);
      CodeButton.name = "LobbyCode";

      // Position the button properly.
      RectTransform RectTransform = CodeButton.GetComponent<RectTransform>();
      RectTransform.anchoredPosition -= new Vector2(78f, 0f);

      var TextMesh = CodeButton.GetComponentInChildren<TextMeshProUGUI>();
      TextMesh.text = "Code";
      CodeButton.onClick.m_PersistentCalls.Clear();
      CodeButton.onClick.AddListener(() =>
      {
        self.StartCoroutine(CopyCode(self, TextMesh));
      });
      self.StartCoroutine(InitializeRejoin(self, JoinButton));
    }
  }

  private static IEnumerator CopyCode(global::LobbySlot lobbySlot, TextMeshProUGUI textMesh)
  {
    string LobbyCode = lobbySlot.lobbyId.ToString();

    if (LobbyCode.IsNullOrWhiteSpace()) textMesh.text = "Can't get Lobby code!";
    else
    {
      GUIUtility.systemCopyBuffer = $"Lobby Code: {LobbyCode}\nLobby Name: \"{lobbySlot.LobbyName.text}\"";
      Plugin.Log.LogInfo($"Lobby code copied to clipboard: {LobbyCode}");
      textMesh.text = "Copied!";
    }

    yield return new WaitForSeconds(1.2f);
    textMesh.text = "Code";

    yield break;
  }

  private static IEnumerator InitializeRejoin(global::LobbySlot lobbySlot, Button joinButton)
  {
    if (!LobbyConnection.PreviousLobby.HasValue) yield break;

    // For some reason, the lobby id isn't fully loaded in yet in Awake() so, we do this before comparing PreviousLobbyId and LobbySlotId
    yield return new WaitUntil(() => lobbySlot.thisLobby.Id != 0);
    if (LobbyConnection.PreviousLobby.Value.Id != lobbySlot.lobbyId) yield break;

    var TextMesh = joinButton.GetComponentInChildren<TextMeshProUGUI>();
    TextMesh.text = "Rejoin";
  }
}
