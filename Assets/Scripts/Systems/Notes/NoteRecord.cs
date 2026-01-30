using System;
using UnityEngine;

[Serializable]
public class NoteRecord
{
  public string id;
  public string created_at;
  public int seat_id;
  public string content;
  public string author_id;
}

[Serializable]
public class NoteList
{
  public NoteRecord[] data;

  public static NoteList FromJsonArray(string jsonArray)
  {
    if (string.IsNullOrEmpty(jsonArray))
      return new NoteList { data = Array.Empty<NoteRecord>() };

    string wrapped = "{\"data\":" + jsonArray + "}";
    return JsonUtility.FromJson<NoteList>(wrapped);
  }
}

[Serializable]
public class CreateNotePayload
{
  public int seat_id;
  public string content;
  public string author_id;
}
