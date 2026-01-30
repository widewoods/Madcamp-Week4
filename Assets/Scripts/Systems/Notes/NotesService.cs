using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class NotesService : MonoBehaviour
{
  [Header("Supabase")]
  [SerializeField] private string supabaseUrl;
  [SerializeField] private string anonKey;
  [SerializeField] private string tableName = "Notes";
  [SerializeField] private bool fetchOnStart = true;
  [SerializeField] private bool enableDebugLogs = true;

  private readonly Dictionary<int, List<NoteRecord>> notesBySeat = new Dictionary<int, List<NoteRecord>>();

  public static NotesService Instance { get; private set; }

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }
    Instance = this;
  }

  private void Start()
  {
    if (fetchOnStart)
      FetchAllNotes();
  }

  public IReadOnlyList<NoteRecord> GetNotesForSeat(int seatId)
  {
    if (notesBySeat.TryGetValue(seatId, out var list))
      return list;
    return Array.Empty<NoteRecord>();
  }

  public void FetchAllNotes(Action onCompleted = null)
  {
    StartCoroutine(FetchAllNotesCoroutine(onCompleted));
  }

  public void CreateNote(int seatId, string content, string authorId, Action<NoteRecord> onCreated = null)
  {
    StartCoroutine(CreateNoteCoroutine(seatId, content, authorId, onCreated));
  }

  private IEnumerator<UnityWebRequestAsyncOperation> FetchAllNotesCoroutine(Action onCompleted)
  {
    if (string.IsNullOrWhiteSpace(supabaseUrl) || string.IsNullOrWhiteSpace(anonKey))
      yield break;

    string url = $"{supabaseUrl}/rest/v1/{tableName}?select=*";
    using var req = UnityWebRequest.Get(url);
    AddSupabaseHeaders(req);
    if (enableDebugLogs)
      Debug.Log($"[NotesService] GET {url}");
    yield return req.SendWebRequest();

    if (req.result != UnityWebRequest.Result.Success)
    {
      if (enableDebugLogs)
        Debug.LogWarning($"[NotesService] GET failed: {req.result} {req.error}");
      yield break;
    }

    string json = req.downloadHandler.text;
    if (enableDebugLogs)
      Debug.Log($"[NotesService] GET response: {json}");
    var list = NoteList.FromJsonArray(json);
    notesBySeat.Clear();

    if (list?.data != null)
    {
      foreach (var note in list.data)
      {
        if (!notesBySeat.TryGetValue(note.seat_id, out var seatList))
        {
          seatList = new List<NoteRecord>();
          notesBySeat[note.seat_id] = seatList;
        }
        seatList.Add(note);
      }
    }

    onCompleted?.Invoke();
  }

  private IEnumerator<UnityWebRequestAsyncOperation> CreateNoteCoroutine(
    int seatId,
    string content,
    string authorId,
    Action<NoteRecord> onCreated)
  {
    if (string.IsNullOrWhiteSpace(supabaseUrl) || string.IsNullOrWhiteSpace(anonKey))
      yield break;

    string url = $"{supabaseUrl}/rest/v1/{tableName}";

    var payload = new CreateNotePayload
    {
      seat_id = seatId,
      content = content,
      author_id = authorId
    };

    string body = JsonUtility.ToJson(payload);
    byte[] bytes = Encoding.UTF8.GetBytes(body);

    using var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
    req.uploadHandler = new UploadHandlerRaw(bytes);
    req.downloadHandler = new DownloadHandlerBuffer();
    AddSupabaseHeaders(req);
    req.SetRequestHeader("Content-Type", "application/json");
    req.SetRequestHeader("Prefer", "return=representation");
    if (enableDebugLogs)
      Debug.Log($"[NotesService] POST {url} body={body}");

    yield return req.SendWebRequest();

    if (req.result != UnityWebRequest.Result.Success)
    {
      if (enableDebugLogs)
      {
        string errBody = req.downloadHandler != null ? req.downloadHandler.text : "no body";
        Debug.LogWarning($"[NotesService] POST failed: {req.result} {req.error} body={errBody}");
      }
      yield break;
    }

    var list = NoteList.FromJsonArray(req.downloadHandler.text);
    if (enableDebugLogs)
      Debug.Log($"[NotesService] POST response: {req.downloadHandler.text}");
    if (list?.data == null || list.data.Length == 0)
      yield break;

    var created = list.data[0];
    if (!notesBySeat.TryGetValue(created.seat_id, out var seatList))
    {
      seatList = new List<NoteRecord>();
      notesBySeat[created.seat_id] = seatList;
    }
    seatList.Add(created);

    onCreated?.Invoke(created);
  }

  private void AddSupabaseHeaders(UnityWebRequest req)
  {
    req.SetRequestHeader("apikey", anonKey);
    req.SetRequestHeader("Authorization", $"Bearer {anonKey}");
  }
}
