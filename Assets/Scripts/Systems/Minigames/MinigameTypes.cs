public enum MinigameType
{
  None,
  Bowling,
  Golf,
  Baseball,
  Iceskate
}

[System.Serializable]
public class MinigamePrompt
{
  public MinigameType type;
  public string prompt;
}
