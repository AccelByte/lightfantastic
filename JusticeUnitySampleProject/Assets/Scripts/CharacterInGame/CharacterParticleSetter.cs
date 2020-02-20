using UnityEngine;

[RequireComponent(typeof(ParticleSystemRenderer))]
public class CharacterParticleSetter : MonoBehaviour
{
    [SerializeField] 
    public CharacterParticleLibraryAndResolver particleLibraryAndResolver;

    public void SetItem(string item)
    {
        particleLibraryAndResolver.Select(item);
    }
}
