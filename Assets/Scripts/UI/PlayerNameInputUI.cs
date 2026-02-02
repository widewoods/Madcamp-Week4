using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameInputUI : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private TMP_InputField inputField;
  [SerializeField] private Button submitButton;
  [SerializeField] private GameObject root;

  private PlayerName localPlayerName;

  private void Awake()
  {
    if (root != null) root.SetActive(true);
  }

  private void OnEnable()
  {
    if (submitButton != null) submitButton.onClick.AddListener(HandleSubmit);
  }

  private void OnDisable()
  {
    if (submitButton != null) submitButton.onClick.RemoveListener(HandleSubmit);
  }

  public void Bind(PlayerName playerName)
  {
    localPlayerName = playerName;
  }

  private void HandleSubmit()
  {
    if (localPlayerName == null) return;
    if (inputField == null) return;
    if (string.IsNullOrWhiteSpace(inputField.text)) return;
    localPlayerName.RequestSetName(inputField.text);
    if (root != null) root.SetActive(false);
  }
}
