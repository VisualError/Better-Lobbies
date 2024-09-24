using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Better_Lobbies.Hooks;
internal class InGameMenu
{
  private static GameObject? QuickMenu;
  private static GameObject? DebugMenu;
  private static GameObject? LobbyCodeObj;
  private static GameObject? QuitObj;
  private static TextMeshProUGUI? LobbyCodeTextMesh;
  private static RectTransform? LobbyCodeRect;
  private static RectTransform? ResumeRect;
  [HookInit]
  private static void Init()
  {
    On.QuickMenuManager.Start += InsertCustomUI;
    On.QuickMenuManager.OpenQuickMenu += OpenQuickMenu;
    On.QuickMenuManager.CloseQuickMenu += QuickMenuManager_CloseQuickMenu;
  }

  private static void QuickMenuManager_CloseQuickMenu(On.QuickMenuManager.orig_CloseQuickMenu orig, QuickMenuManager self)
  {
    orig(self);
    if (LobbyCodeTextMesh != null)
      LobbyCodeTextMesh.text = "> Lobby Code";
  }

  private static void OpenQuickMenu(On.QuickMenuManager.orig_OpenQuickMenu orig, QuickMenuManager self)
  {
    orig(self);
    AdjustLobbyCode();
    AddCrewCount();
  }

  private static void AddCrewCount()
  {
    TextMeshProUGUI? CrewHeaderText = QuickMenu?.transform.Find("PlayerList/Image/Header").GetComponentInChildren<TextMeshProUGUI>();
    if (CrewHeaderText == null) return;
    CrewHeaderText.text = $"CREW ({(StartOfRound.Instance?.connectedPlayersAmount ?? 0) + 1}/{GameNetworkManager.Instance.currentLobby!.Value.MaxMembers}):\n{GameNetworkManager.Instance.currentLobby!.Value.GetData("name")}";
  }

  private static void AdjustLobbyCode()
  {
    if (LobbyCodeRect == null || LobbyCodeObj == null) return;
    if (DebugMenu != null && DebugMenu.activeSelf)
    {
      LobbyCodeObj.transform.SetParent(DebugMenu.transform);
      LobbyCodeRect.localPosition = new Vector3(125f, 185f, 0f);
      LobbyCodeRect.localScale = new Vector3(1f, 1f, 1f);
    }
    else if (ResumeRect != null && QuitObj != null)
    {
      LobbyCodeObj?.transform.SetParent(QuitObj.transform.parent);
      LobbyCodeRect.localPosition = ResumeRect.localPosition + new Vector3(0f, -55.5941f, 0f);
      LobbyCodeRect.localScale = ResumeRect.localScale;
    }
  }

  private static void InsertCustomUI(On.QuickMenuManager.orig_Start orig, QuickMenuManager self)
  {
    orig(self);

    QuickMenu = GameObject.Find("/Systems/UI/Canvas/QuickMenu/");
    DebugMenu = QuickMenu.transform.Find("DebugMenu")?.gameObject;

    QuitObj = QuickMenu.transform.Find("MainButtons/Quit/")?.gameObject;
    if (QuitObj == null) return;

    ResumeRect = QuitObj.GetComponent<RectTransform>();
    LobbyCodeObj = Object.Instantiate(QuitObj.gameObject, QuitObj.transform.parent);
    LobbyCodeTextMesh = LobbyCodeObj.GetComponentInChildren<TextMeshProUGUI>();
    Button LobbyCodeButton = LobbyCodeObj.GetComponent<Button>();

    LobbyCodeRect = LobbyCodeObj.GetComponent<RectTransform>();
    LobbyCodeRect.localPosition = QuitObj.transform.localPosition + new Vector3(0f, -55.5941f, 0f);
    LobbyCodeRect.localScale = QuitObj.transform.localScale;

    LobbyCodeButton.onClick.m_PersistentCalls.Clear();
    LobbyCodeButton.onClick.AddListener(() =>
    {
      LobbyCodeButton.StartCoroutine(CopyCode(LobbyCodeTextMesh));
    });

    LobbyCodeObj.name = "CopyLobbyCode";
    LobbyCodeTextMesh.text = "> Lobby Code";
  }

  private static IEnumerator CopyCode(TextMeshProUGUI textMesh)
  {
    if (!GameNetworkManager.Instance.currentLobby.HasValue) textMesh.text = "Can't get Lobby code!";
    else
    {
      textMesh.text = "(Copied to clipboard!)";
      string id = GameNetworkManager.Instance.currentLobby.Value.Id.ToString();
      GUIUtility.systemCopyBuffer = $"Lobby Code: {id}\nLobby Name: \"{GameNetworkManager.Instance.currentLobby.Value.GetData("name")}\"";
      Plugin.Log.LogInfo("Lobby code copied to clipboard: " + id);
    }
    yield return new WaitForSeconds(1.2f);
    textMesh.text = "> Lobby Code";
    yield break;
  }
}
