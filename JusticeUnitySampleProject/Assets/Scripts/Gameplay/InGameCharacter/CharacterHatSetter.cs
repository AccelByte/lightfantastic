using UnityEngine;
using UnityEngine.Experimental.U2D.Animation;

[RequireComponent(typeof(SpriteResolver))]
public class CharacterHatSetter : MonoBehaviour
{
    [SerializeField] 
    public SpriteResolver spriteResolver;

    public void SetHatSprite(string spriteName)
    {
        spriteResolver.SetCategoryAndLabel(LightFantasticConfig.ItemTags.hat, spriteName);
    }
}
