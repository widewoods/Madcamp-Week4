using UnityEngine;
using System.Collections.Generic;

public class PlayerMaterialTrigger : MonoBehaviour
{
  [Header("Material")]
  [SerializeField] private Material targetMaterial;
  [SerializeField] private bool revertOnExit = false;
  [SerializeField] private AudioSource audioSource;

  private readonly Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

  private void OnTriggerEnter(Collider other)
  {
    ApplyMaterial(other, targetMaterial);
    audioSource.loop = true;
    audioSource.Play();
  }

  private void OnTriggerExit(Collider other)
  {
    if (!revertOnExit) return;
    RestoreMaterial(other);
  }

  private void ApplyMaterial(Collider other, Material mat)
  {
    if (mat == null) return;

    var renderers = other.GetComponentsInChildren<Renderer>(true);
    if (renderers == null || renderers.Length == 0) return;

    for (int i = 0; i < renderers.Length; i++)
    {
      var r = renderers[i];
      if (r == null) continue;
      if (!originalMaterials.ContainsKey(r))
        originalMaterials[r] = r.materials;

      var mats = r.materials;
      for (int m = 0; m < mats.Length; m++)
        mats[m] = mat;
      r.materials = mats;
    }
  }

  private void RestoreMaterial(Collider other)
  {
    var renderers = other.GetComponentsInParent<Renderer>(true);
    if (renderers == null || renderers.Length == 0) return;

    for (int i = 0; i < renderers.Length; i++)
    {
      var r = renderers[i];
      if (r == null) continue;
      if (!originalMaterials.TryGetValue(r, out var mats)) continue;
      r.materials = mats;
      originalMaterials.Remove(r);
    }
  }
}
