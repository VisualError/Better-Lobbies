using BepInEx;
using Steamworks.Data;
using TMPro;
using UnityEngine;

namespace Better_Lobbies.Hooks;
internal class EnterLobbyCode
{
  private static GameObject? FieldObj;
  internal static TMP_InputField? InputField;

  [HookInit]
  internal static void Init()
  {
    On.MenuManager.Awake += InitializeField;
  }

  private static void InitializeField(On.MenuManager.orig_Awake orig, MenuManager self)
  {
    orig(self);

    GetFieldObj();
    if (FieldObj == null) return;
    if (FieldObj.activeInHierarchy) return;
    FieldObj.SetActive(true);

    GetInputField(FieldObj);
    InitializeInputField(InputField);
    Plugin.Log.LogInfo("Initialized Lobby search field!");
  }

  private static void GetFieldObj()
  {
    if (FieldObj == null)
    {
      FieldObj = GameObject.Find("/Canvas/MenuContainer/LobbyList/JoinCode");
      Plugin.Log.LogDebug($"Trying to find Lobby Code Field OBJ. Got: {FieldObj != null}");
    }
  }

  private static void GetInputField(GameObject gameObject)
  {
    if (InputField == null)
    {
      InputField = gameObject.GetComponent<TMP_InputField>();
      Plugin.Log.LogDebug($"Trying to find Input Field. Got: {InputField != null}");
    }
  }

  private static void InitializeInputField(TMP_InputField? field)
  {
    if (field == null) return;
    field.interactable = true;
    field.placeholder.gameObject.GetComponent<TextMeshProUGUI>().text = "Search or Enter a room code...";

    // Clears input based events of the TMP_InputField incase it has any, to avoid any base-game conflicts.
    field.onEndEdit.m_PersistentCalls.Clear();
    field.onEndTextSelection.m_PersistentCalls.Clear();
    field.onSubmit.m_PersistentCalls.Clear();

    field.onSubmit.AddListener(onSubmit);
    field.onEndEdit.AddListener(onEndEdit);
    field.onDeselect.AddListener(onDeselect);
    field.onValidateInput += ValidateInput;
  }
  // This just filters code copying. If the copied lobby code is
  /*
   Code: 21323121333
   Lobby: "Name here"

  it will only allow: 21323121333 to be in the input.
   */
  private static bool stop = false;
  private static void onEndEdit(string value)
  {
    stop = false;
  }
  private static void onDeselect(string value)
  {
    stop = false;
    if (InputField != null)
      InputField.text = "";
  }
  private static char ValidateInput(string text, int charIndex, char addedChar)
  {
    Plugin.Log.LogWarning(addedChar);
    if (addedChar == '"') stop = true;
    return (char.IsDigit(addedChar) || char.IsControl(addedChar)) && !stop ? addedChar : char.MinValue;
  }

  private static void onSubmit(string value)
  {
    if (!ulong.TryParse(value, out ulong result)) SteamLobbyManager.Instance?.LoadServerList();

    Lobby LobbyQuery = new Lobby(result);

    if (LobbyQuery.GetData("vers").IsNullOrWhiteSpace())
    {
      SteamLobbyManager.Instance?.LoadServerList();
      return;
    }
    global::LobbySlot.JoinLobbyAfterVerifying(LobbyQuery, LobbyQuery.Owner.Id);
  }
}
