// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
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
    public Button promptPanelCloseButton;
}
