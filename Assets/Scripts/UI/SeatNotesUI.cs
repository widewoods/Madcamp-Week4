using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeatNotesUI : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private TMP_Text notesText;
  [SerializeField] private TMP_InputField inputField;
  [SerializeField] private Button submitButton;
  [SerializeField] private Button closeButton;
  [SerializeField] private GameObject root;
  [SerializeField] private SeatNotesBackgroundBySeat backgroundBySeat;
  [SerializeField] private SeatNotesGridUI notesGridUI;

  private int currentSeatId = -1;

  private void Awake()
  {
    if (root != null)
      root.SetActive(false);
  }

  private void OnEnable()
  {
    if (submitButton != null) submitButton.onClick.AddListener(HandleSubmit);
    if (closeButton != null) closeButton.onClick.AddListener(Close);
  }

  private void OnDisable()
  {
    if (submitButton != null) submitButton.onClick.RemoveListener(HandleSubmit);
    if (closeButton != null) closeButton.onClick.RemoveListener(Close);
  }

  public void Open(int seatId)
  {
    currentSeatId = seatId;
    if (root != null) root.SetActive(true);
    if (backgroundBySeat != null)
      backgroundBySeat.ApplySeatBackground(seatId);

    var service = NotesService.Instance;
    if (service == null) return;

    service.FetchAllNotes(() =>
    {
      RefreshNotes();
    });
  }

  public void Close()
  {
    if (root != null) root.SetActive(false);
  }

  private void RefreshNotes()
  {
    if (notesText == null) return;

    var service = NotesService.Instance;
    if (service == null) return;

    var notes = service.GetNotesForSeat(currentSeatId);
    if (notesGridUI != null)
    {
      notesGridUI.Render(notes);
      return;
    }

    var sb = new StringBuilder();
    foreach (var note in notes)
    {
      sb.AppendLine(note.content);
    }

    notesText.text = sb.Length > 0 ? sb.ToString() : "아직 쓴 롤링페이퍼가 없습니다";
  }

  private void HandleSubmit()
  {
    if (inputField == null) return;
    if (string.IsNullOrWhiteSpace(inputField.text)) return;

    var service = NotesService.Instance;
    if (service == null) return;

    string content = inputField.text.Trim();
    inputField.text = string.Empty;

    service.CreateNote(currentSeatId, content, "anon", _ => RefreshNotes());
  }
}
