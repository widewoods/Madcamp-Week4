using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeatNotesGridUI : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private RectTransform contentRoot;
  [SerializeField] private GameObject noteItemPrefab;
  [SerializeField] private TMP_Text emptyStateText;

  [Header("Layout")]
  [SerializeField] private int columns = 3;
  [SerializeField] private Vector2 cellSize = new Vector2(220f, 220f);
  [SerializeField] private Vector2 spacing = new Vector2(16f, 16f);

  [Header("Visual")]
  [SerializeField] private Sprite[] noteBackgrounds;
  [SerializeField] private bool forceItemSize = true;
  [SerializeField] private bool forceBackgroundStretch = true;

  public void Render(IReadOnlyList<NoteRecord> notes)
  {
    if (contentRoot == null || noteItemPrefab == null) return;
    EnsureContentLayout();
    ClearItems();

    bool hasNotes = notes != null && notes.Count > 0;
    if (emptyStateText != null)
      emptyStateText.gameObject.SetActive(!hasNotes);
    if (!hasNotes)
    {
      SetContentHeight(0);
      return;
    }

    for (int i = 0; i < notes.Count; i++)
    {
      var item = Instantiate(noteItemPrefab, contentRoot);
      var label = item.GetComponentInChildren<TMP_Text>(true);
      var bg = item.GetComponentInChildren<Image>(true);

      if (forceItemSize)
        ApplyItemSize(item);

      if (label != null)
        label.text = notes[i].content;
      if (bg != null && noteBackgrounds != null && noteBackgrounds.Length > 0)
      {
        bg.sprite = noteBackgrounds[i % noteBackgrounds.Length];
        if (forceBackgroundStretch)
          StretchToParent(bg.rectTransform);
      }
    }

    SetContentHeight(notes.Count);
    Canvas.ForceUpdateCanvases();
    LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
  }

  private void EnsureContentLayout()
  {
    var grid = contentRoot.GetComponent<GridLayoutGroup>();
    if (grid == null)
      grid = contentRoot.gameObject.AddComponent<GridLayoutGroup>();

    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
    grid.constraintCount = Mathf.Max(1, columns);
    grid.cellSize = cellSize;
    grid.spacing = spacing;
    grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
    grid.startAxis = GridLayoutGroup.Axis.Horizontal;
    grid.childAlignment = TextAnchor.UpperLeft;

    var fitter = contentRoot.GetComponent<ContentSizeFitter>();
    if (fitter == null)
      fitter = contentRoot.gameObject.AddComponent<ContentSizeFitter>();
    fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
    fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

    contentRoot.anchorMin = new Vector2(0f, 1f);
    contentRoot.anchorMax = new Vector2(1f, 1f);
    contentRoot.pivot = new Vector2(0.5f, 1f);
  }

  private void SetContentHeight(int noteCount)
  {
    var grid = contentRoot.GetComponent<GridLayoutGroup>();
    int rows = noteCount <= 0 ? 1 : Mathf.CeilToInt(noteCount / (float)Mathf.Max(1, columns));
    int top = grid != null ? grid.padding.top : 0;
    int bottom = grid != null ? grid.padding.bottom : 0;
    float totalHeight = top + bottom + rows * cellSize.y + Mathf.Max(0, rows - 1) * spacing.y;

    var size = contentRoot.sizeDelta;
    size.y = totalHeight;
    contentRoot.sizeDelta = size;
  }

  private void ClearItems()
  {
    for (int i = contentRoot.childCount - 1; i >= 0; i--)
      Destroy(contentRoot.GetChild(i).gameObject);
  }

  private void ApplyItemSize(GameObject item)
  {
    var rect = item.GetComponent<RectTransform>();
    if (rect != null)
      rect.sizeDelta = cellSize;

    var layout = item.GetComponent<LayoutElement>();
    if (layout == null)
      layout = item.AddComponent<LayoutElement>();

    layout.preferredWidth = cellSize.x;
    layout.preferredHeight = cellSize.y;
    layout.minWidth = cellSize.x;
    layout.minHeight = cellSize.y;
    layout.flexibleWidth = 0f;
    layout.flexibleHeight = 0f;
  }

  private static void StretchToParent(RectTransform rt)
  {
    if (rt == null) return;
    rt.anchorMin = Vector2.zero;
    rt.anchorMax = Vector2.one;
    rt.offsetMin = Vector2.zero;
    rt.offsetMax = Vector2.zero;
  }
}
