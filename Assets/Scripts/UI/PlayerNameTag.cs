using TMPro;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using UnityEngine;

public class PlayerNameTag : MonoBehaviour
{
  [SerializeField] private TMP_Text nameText;
  [SerializeField] private PlayerName playerName;

  private void Awake()
  {
    if (playerName == null) playerName = GetComponentInParent<PlayerName>();

  }

  private void OnEnable()
  {
    playerName.playerName.OnValueChanged += HandleNameChanged;
  }

  private void OnDisable()
  {
    playerName.playerName.OnValueChanged -= HandleNameChanged;
  }

  private void UpdateName()
  {
    if (nameText == null || playerName == null) return;
    nameText.text = playerName.Name;
  }

  private void HandleNameChanged(FixedString64Bytes previous, FixedString64Bytes current)
  {
    UpdateName();
  }
}
