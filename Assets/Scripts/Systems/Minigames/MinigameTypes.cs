public enum MinigameType
{
  None,
  Bowling,
  Golf,
  Baseball
}

[System.Serializable]
public class MinigamePrompt
{
  public MinigameType type;
  public string prompt;
}
