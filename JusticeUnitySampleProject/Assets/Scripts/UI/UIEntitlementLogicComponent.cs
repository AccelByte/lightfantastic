﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.U2D.Animation;

public class UIEntitlementLogicComponent : MonoBehaviour
{
    #region Inventory Viewer Component 
    public GameObject itemInventoryPrefab;
    public UiUtilities uiUtilities; // Held by GameLogic, need to be refreshed on scene loaded
    public InventoryGridLayout gridLayoutHats;
    public InventoryGridLayout gridLayoutEffects;
    public AccelByteButtonScriptStyle buttonHat;
    public AccelByteButtonScriptStyle buttonEffect;
    public CanvasGroup promptPanel;
    public AccelByteUserProfileLogic abUserProfileLogic; // Held by GameLogic, need to be refreshed on scene loaded
    public SpriteResolver hatSpriteResolver;
    #endregion

    // Buttons
    public Button inventoryButton;
    public Button hatTabButton;
    public Button effectTabButton;
    public Button backButton;
    public Button promptPanelSaveButton;
    public Button promptPanelDontSaveButton;
}
