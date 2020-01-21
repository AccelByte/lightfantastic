using UnityEngine;

[RequireComponent(typeof(ParticleSystemRenderer))]
public class CharacterParticleSetter : MonoBehaviour
{
    [SerializeField] 
    public ParticleSystemRenderer particleSystemRenderer;

    public void SetMaterial(Material material)
    {
        particleSystemRenderer.material = material;
    }
}
