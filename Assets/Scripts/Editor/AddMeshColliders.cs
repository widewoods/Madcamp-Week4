using UnityEngine;
using UnityEditor;

public class AddMeshColliders
{
  [MenuItem("Tools/Colliders/Add MeshColliders To Selected (Children)")]
  static void AddToSelected()
  {
    var selected = Selection.gameObjects;
    if (selected == null || selected.Length == 0) return;

    int added = 0;

    foreach (var root in selected)
    {
      // Includes inactive children
      var transforms = root.GetComponentsInChildren<Transform>(true);

      foreach (var t in transforms)
      {
        // Need a MeshFilter with a mesh to use MeshCollider
        var mf = t.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) continue;

        // Skip if it already has a collider
        if (t.GetComponent<Collider>() != null) continue;

        Undo.AddComponent<MeshCollider>(t.gameObject);
        added++;
      }
    }

    Debug.Log($"Added MeshCollider to {added} objects.");
  }
}
