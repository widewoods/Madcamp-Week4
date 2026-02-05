using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ClassroomStatusUI : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private ClassroomManager classroomManager;
  [SerializeField] private TMP_Text statusText;
  [SerializeField] private Button hostStartButton;
  [SerializeField] private GameObject root;

  private int lastSeated;
  private int lastRequired;
  private bool lastStarted;

  private void Awake()
  {
    if (classroomManager == null)
      classroomManager = FindFirstObjectByType<ClassroomManager>();
  }

  private void OnEnable()
  {
    if (classroomManager != null)
    {
      classroomManager.OnSeatedChanged += HandleSeatedChanged;
      classroomManager.OnStartedChanged += HandleStartedChanged;
    }

    if (hostStartButton != null)
      hostStartButton.onClick.AddListener(HandleHostStartClicked);

    RefreshHostButton();
  }

  private void OnDisable()
  {
    if (classroomManager != null)
    {
      classroomManager.OnSeatedChanged -= HandleSeatedChanged;
      classroomManager.OnStartedChanged -= HandleStartedChanged;
    }

    if (hostStartButton != null)
      hostStartButton.onClick.RemoveListener(HandleHostStartClicked);
  }

  private void Update()
  {
    RefreshHostButton();
  }

  private void HandleSeatedChanged(int seated, int required)
  {
    lastSeated = seated;
    lastRequired = required;
    UpdateStatusText();
  }

  private void HandleStartedChanged(bool started)
  {
    lastStarted = started;
    UpdateStatusText();
    RefreshHostButton();
  }

  private void HandleHostStartClicked()
  {
    if (classroomManager == null) return;
    Debug.Log("Start Pressed");
    classroomManager.RequestStartServerRpc();
    root.SetActive(false);
  }

  private void UpdateStatusText()
  {
    if (statusText == null) return;
    string seatedInfo = lastRequired > 0 ? $"{lastSeated}/{lastRequired}" : $"{lastSeated}";
    statusText.text = lastStarted ? $"시작됨 ({seatedInfo})" : $"앉은 사람 {seatedInfo}";
  }

  private void RefreshHostButton()
  {
    if (hostStartButton == null) return;

    bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
    hostStartButton.gameObject.SetActive(isHost);
    hostStartButton.interactable = isHost && !lastStarted;
  }
}
