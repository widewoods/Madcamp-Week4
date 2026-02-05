using System;
using UnityEngine;
using UnityEngine.UI;

public class SeatNotesBackgroundBySeat : MonoBehaviour
{
  [Serializable]
  private class SeatBackground
  {
    public int seatId = -1;
    public Sprite background;
  }

  [Header("References")]
  [SerializeField] private Image panelBackground;

  [Header("Defaults")]
  [SerializeField] private Sprite defaultBackground;
  [SerializeField] private bool keepCurrentIfNoMatch = false;

  [Header("Per Seat")]
  [SerializeField] private SeatBackground[] seatBackgrounds;

  public void ApplySeatBackground(int seatId)
  {
    if (panelBackground == null) return;

    Sprite matched = FindBackground(seatId);
    if (matched != null)
    {
      panelBackground.sprite = matched;
      return;
    }

    if (keepCurrentIfNoMatch) return;
    panelBackground.sprite = defaultBackground;
  }

  private Sprite FindBackground(int seatId)
  {
    if (seatBackgrounds == null) return null;
    for (int i = 0; i < seatBackgrounds.Length; i++)
    {
      var entry = seatBackgrounds[i];
      if (entry == null) continue;
      if (entry.seatId != seatId) continue;
      return entry.background;
    }
    return null;
  }
}
