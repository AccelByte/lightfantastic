using UnityEngine;
using UnityEngine.Experimental.U2D.Animation;

[RequireComponent(typeof(SpriteResolver))]
public class CharacterPlatformSpriteSetter : MonoBehaviour
{
    [SerializeField]
    private SpriteResolver resolver;

    public void SetSprite(LightFantasticConfig.Platform platform)
    {
        resolver.SetCategoryAndLabel(LightFantasticConfig.PLATFORM_LIBRARY_ASSET_CATEGORY, platform.ToString());
    }
}
