using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterSpeedSetter : MonoBehaviour
{
    [SerializeField] 
    public Animator animator;

    public void SetSpeed(float speed)
    {
        animator.SetFloat("Speed", speed);
    }
}
