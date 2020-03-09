#pragma warning disable 0649

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.U2D.Animation;
using Utf8Json;
using UITools;
using UnityEngine.SceneManagement;

public static class Equipments
{
    public enum Type
    {
        Hat,
        Effect,
        None
    }

    public struct StringEntry
    {
        public Type type;
        public string tag;
    }

    [DataContract]
    public struct CustomAttributes
    {
        [DataMember] public string hatItemId;
        [DataMember] public string effectItemId;
    }

    public class EquipmentList : ICloneable
    {
        public ItemInfo hat;
        public ItemInfo effect;

        //TODO: proper serializer
        public object ToCustomAttribute()
        {
            CustomAttributes customAttributes = new CustomAttributes();
            customAttributes.hatItemId = hat == null ? null : hat.itemId;
            customAttributes.effectItemId = effect == null ? null : effect.itemId;
            return customAttributes;
        }

        public ref ItemInfo GetItemInfo(Type type)
        {
            switch (type)
            {
                case Type.Hat:
                    return ref hat;
                case Type.Effect:
                    return ref effect;
            }
            throw Exception;
        }
        public Exception Exception { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public static EquipmentList ListFromCustomAttributes(string attribute, ItemInfo[] itemInformations)
    {
        CustomAttributes customAttributes;
        try
        {
            customAttributes = JsonSerializer.Deserialize<CustomAttributes>(attribute);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return null;
        }

        var result = new EquipmentList();

        foreach (var itemInformation in itemInformations)
        {
            if (itemInformation.itemId == customAttributes.hatItemId) { result.hat = itemInformation; }
            if (itemInformation.itemId == customAttributes.effectItemId) { result.effect = itemInformation; }
        }

        return result;
    }

    private static readonly StringEntry[] equipmentStringEntry =
    {
        new StringEntry {type = Type.Hat, tag = LightFantasticConfig.ItemTags.hat},
        new StringEntry {type = Type.Effect, tag = LightFantasticConfig.ItemTags.effect}
    };

    public static Type TypeFromString(string input)
    {
        foreach (var entry in equipmentStringEntry)
        {
            if (input == entry.tag)
            {
                return entry.type;
            }
        }
        return Type.None;
    }

    public static string ToString(Type input)
    {
        foreach (var entry in equipmentStringEntry)
        {
            if (input == entry.type)
            {
                return entry.tag;
            }
        }
        return "";
    }
}

namespace EntitlementUiLogic
{
    class PrefabsInfos
    {
        public Equipments.Type type;
        public ItemInventoryPrefab[] prefabs;

        public PrefabsInfos(Equipments.Type type_)
        {
            prefabs = new ItemInventoryPrefab[0];
            type = type_;
        }
    }

    class AllPrefabsCollection
    {
        public List<PrefabsInfos> prefabsInfos = new List<PrefabsInfos>(0);

        public AllPrefabsCollection()
        {
            foreach (Equipments.Type type in Enum.GetValues(typeof(Equipments.Type)))
            {
                prefabsInfos.Add(new PrefabsInfos(type));
            }
        }

        public ItemInventoryPrefab[] GetItemInventoryPrefabs(Equipments.Type type)
        {
            for (var index = 0; index < prefabsInfos.Count; index++)
            {
                if (prefabsInfos[index].type == type)
                {
                    return prefabsInfos[index].prefabs;
                }
            }

            return null;
        }

        public void SetItemInventoryPrefabs(Equipments.Type type, ItemInventoryPrefab[] prefabs)
        {
            for (var index = 0; index < prefabsInfos.Count; index++)
            {
                if (prefabsInfos[index].type == type)
                {
                    prefabsInfos[index].prefabs = prefabs;
                }
            }
        }
    }
}

public class AccelByteEntitlementLogic : MonoBehaviour
{
    public delegate void GetEntitlementCompletion(bool inMenum, Error error);
    public event GetEntitlementCompletion OnGetEntitlementCompleted;
    private Entitlement abEntitlements;
    private Items abItems;

    private GameObject UIHandler;
    private UIEntitlementLogicComponent UIHandlerEntitlementComponent;
    private UIElementHandler UIElementHandler;

    private readonly ItemCriteria ALL_ITEM_CRITERIA = new ItemCriteria();
    private readonly ItemPagingSlicedResult allItemInfo = new ItemPagingSlicedResult();
    private Equipments.EquipmentList activeEquipmentList = new Equipments.EquipmentList();
    private Equipments.EquipmentList originalEquipmentList = new Equipments.EquipmentList();
    private EntitlementUiLogic.AllPrefabsCollection allPrefabsCollection;

    private void Start()
    {
        abEntitlements = AccelBytePlugin.GetEntitlements();
        abItems = AccelBytePlugin.GetItems();
        ALL_ITEM_CRITERIA.offset = 0;
        ALL_ITEM_CRITERIA.language = LightFantasticConfig.DEFAULT_LANGUAGE;
        ALL_ITEM_CRITERIA.itemType = ItemType.NONE;
    }

    #region UI Listeners
    void OnEnable()
    {
        Debug.Log("ABEntitlement OnEnable called!");

        // Register to onsceneloaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        Debug.Log("ABEntitlement OnDisable called!");

        // Register to onsceneloaded
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (UIHandler != null)
        {
            RemoveListeners();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("ABEntitlement OnSceneLoaded level loaded!");

        RefreshUIHandler();
    }

    public void RefreshUIHandler()
    {
        UIHandler = GameObject.FindGameObjectWithTag("UIHandler");
        if (UIHandler == null)
        {
            Debug.Log("ABEntitlement RefreshUIHandler no reference to UI Handler!");
            return;
        }
        UIHandlerEntitlementComponent = UIHandler.GetComponent<UIEntitlementLogicComponent>();
        UIElementHandler = UIHandler.GetComponent<UIElementHandler>();
        UIHandlerEntitlementComponent.uiUtilities = AccelByteManager.Instance.gameObject.GetComponent<UiUtilities>();
        UIHandlerEntitlementComponent.abUserProfileLogic = AccelByteManager.Instance.gameObject.GetComponent<AccelByteUserProfileLogic>();

        AddEventListeners();
    }

    void AddEventListeners()
    {
        Debug.Log("ABEntitlement AddEventListeners!");
        // Bind Buttons
        UIHandlerEntitlementComponent.inventoryButton.onClick.AddListener(delegate
        {
            GetEntitlement(true);
            UIElementHandler.ShowExclusivePanel(ExclusivePanelType.INVENTORY);
        });
        
        UIHandlerEntitlementComponent.hatTabButton.onClick.AddListener(() => ShowHatInventories(true));
        UIHandlerEntitlementComponent.hatTabButton.onClick.AddListener(() => ShowEffectInventories(false));
        UIHandlerEntitlementComponent.hatTabButton.onClick.AddListener(() => UIHandlerEntitlementComponent.buttonHat.SetEnable(false));
        UIHandlerEntitlementComponent.hatTabButton.onClick.AddListener(() => UIHandlerEntitlementComponent.buttonEffect.SetEnable(true));
        UIHandlerEntitlementComponent.effectTabButton.onClick.AddListener(() => ShowHatInventories(false));
        UIHandlerEntitlementComponent.effectTabButton.onClick.AddListener(() => ShowEffectInventories(true));
        UIHandlerEntitlementComponent.effectTabButton.onClick.AddListener(() => UIHandlerEntitlementComponent.buttonHat.SetEnable(true));
        UIHandlerEntitlementComponent.effectTabButton.onClick.AddListener(() => UIHandlerEntitlementComponent.buttonEffect.SetEnable(false));
        UIHandlerEntitlementComponent.backButton.onClick.AddListener(ShowPromptPanel);
        
        UIHandlerEntitlementComponent.promptPanelSaveButton.onClick.AddListener(delegate
        {
            UploadEquipment();
            HidePromptPanel();
            UIElementHandler.ShowExclusivePanel(ExclusivePanelType.MAIN_MENU);
        });
        
        UIHandlerEntitlementComponent.promptPanelDontSaveButton.onClick.AddListener(delegate
        {
            HidePromptPanel();
            UIElementHandler.ShowExclusivePanel(ExclusivePanelType.MAIN_MENU);
        });
        
        UIHandlerEntitlementComponent.promptPanelDontSaveButton.onClick.AddListener(delegate
        {
            HidePromptPanel();
        });
    }

    void RemoveListeners()
    {
        Debug.Log("ABEntitlement RemoveListeners!");
        UIHandlerEntitlementComponent.inventoryButton.onClick.RemoveAllListeners();
        UIHandlerEntitlementComponent.hatTabButton.onClick.RemoveAllListeners();
        UIHandlerEntitlementComponent.effectTabButton.onClick.RemoveAllListeners();
        UIHandlerEntitlementComponent.backButton.onClick.RemoveListener(ShowPromptPanel);
        UIHandlerEntitlementComponent.promptPanelSaveButton.onClick.RemoveAllListeners();
        UIHandlerEntitlementComponent.promptPanelDontSaveButton.onClick.RemoveAllListeners();
    }
    #endregion // UI Listeners

    public void GetEntitlement(bool inMenu)
    {
        activeEquipmentList = null;
        originalEquipmentList = null;
        if (inMenu)
        {
            UIElementHandler.ShowNonExclusivePanel(NonExclusivePanelType.LOADING);
            HidePromptPanel();
        }
        if (allItemInfo.data == null)
        {
            abItems.GetItemsByCriteria(ALL_ITEM_CRITERIA, result =>
            {
                if (!result.IsError)
                {
                    allItemInfo.data = result.Value.data;
                    if (inMenu)
                    {
                        abEntitlements.QueryUserEntitlements("", "", 0, 99, OnGetEntitlement);
                    }
                    else
                    {
                        abEntitlements.QueryUserEntitlements("", "", 0, 99, OnGetEntitlementNoMenu);
                    }
                }
                else
                {
                    OnGetEntitlementCompleted?.Invoke(inMenu, result.Error);
                }
            });
        }
        else
        {
            if (inMenu)
            {
                abEntitlements.QueryUserEntitlements("", "", 0, 99, OnGetEntitlement);
            }
            else
            {
                abEntitlements.QueryUserEntitlements("", "", 0, 99, OnGetEntitlementNoMenu);
            }
        }
    }

    private void OnGetEntitlementNoMenu(Result<EntitlementPagingSlicedResult> result)
    {
        //TODO: fix FadeLoadingOut
        //uiHandler.FadeLoading();
        if (result.IsError)
        {
            // handle
            OnGetEntitlementCompleted?.Invoke(false, result.Error);
        }
        else
        {
            UIHandlerEntitlementComponent.abUserProfileLogic.GetMine(profileResult =>
            {
                if (!profileResult.IsError)
                {
                    //originalEquipmentList = null;
                    activeEquipmentList = new Equipments.EquipmentList();

                    if (profileResult.Value.customAttributes != null)
                    {
                        originalEquipmentList = Equipments.ListFromCustomAttributes(
                            profileResult.Value.customAttributes.ToJsonString(), allItemInfo.data);
                        if (originalEquipmentList != null)
                        {
                            activeEquipmentList = (Equipments.EquipmentList)originalEquipmentList.Clone();
                            OnGetEntitlementCompleted?.Invoke(false, null);
                        }
                        else
                        {
                            OnGetEntitlementCompleted?.Invoke(false, new Error(ErrorCode.UnknownError, "Null Equipment list"));
                        }
                    }
                    else
                    {
                        OnGetEntitlementCompleted?.Invoke(false, new Error(ErrorCode.UnknownError, "Custom attributes field is empty"));
                    }
                }
                else
                {
                    OnGetEntitlementCompleted?.Invoke(false, profileResult.Error);
                }
            });
        }
    }

    private void OnGetEntitlement(Result<EntitlementPagingSlicedResult> result)
    {
        //TODO: fix FadeLoadingOut
        UIElementHandler.HideNonExclusivePanel(NonExclusivePanelType.LOADING);
        if (result.IsError)
        {
            // handle
            OnGetEntitlementCompleted?.Invoke(true, result.Error);
        }
        else
        {
            UIHandlerEntitlementComponent.abUserProfileLogic.GetMine(profileResult =>
            {
                if (!profileResult.IsError)
                {
                    //originalEquipmentList = null;
                    activeEquipmentList = new Equipments.EquipmentList();
                    PopulateInventories(result.Value.data);

                    if (profileResult.Value.customAttributes != null)
                    {
                        originalEquipmentList = Equipments.ListFromCustomAttributes(
                            profileResult.Value.customAttributes.ToJsonString(), allItemInfo.data);
                        if (originalEquipmentList != null)
                        {
                            activeEquipmentList = (Equipments.EquipmentList)originalEquipmentList.Clone();
                            EquipFromList(originalEquipmentList);
                            OnGetEntitlementCompleted?.Invoke(true, result.Error);
                        }
                        else
                        {
                            OnGetEntitlementCompleted?.Invoke(true, new Error(ErrorCode.UnknownError, "Null Equipment list"));
                        }
                    }
                    else
                    {
                        OnGetEntitlementCompleted?.Invoke(true, new Error(ErrorCode.UnknownError, "Custom attributes field is empty"));
                    }
                    UIHandlerEntitlementComponent.buttonHat.SetEnable(false);
                    ShowHatInventories(true);
                    UIHandlerEntitlementComponent.buttonEffect.SetEnable(true);
                    ShowEffectInventories(false);
                }
                else
                {
                    OnGetEntitlementCompleted?.Invoke(true, profileResult.Error);
                }
            });
        }
    }

    public Equipments.EquipmentList GetActiveEquipmentList()
    {
        return activeEquipmentList;
    }

    private void EquipFromList(Equipments.EquipmentList list)
    {
        foreach (Equipments.Type type in Enum.GetValues(typeof(Equipments.Type)))
        {
            if (type == Equipments.Type.None) { break; }
            var listItemInfo = list.GetItemInfo(type);
            foreach (var prefab in allPrefabsCollection.GetItemInventoryPrefabs(type))
            {
                if (listItemInfo != null && listItemInfo.itemId == prefab.GetItemInfo().itemId)
                {
                    prefab.Select();
                    UpdateAvatar(prefab.GetItemInfo(), true);
                }
                else
                {
                    prefab.Unselect();
                }
            }
        }
    }

    public ItemInfo GetItemFromCache(string input)
    {
        foreach (var entry in allItemInfo.data)
        {
            if (input == entry.itemId)
            {
                return entry;
            }
        }
        return null;
    }

    private void PopulateInventories(EntitlementInfo[] results)
    {
        allPrefabsCollection = new EntitlementUiLogic.AllPrefabsCollection();


        //TODO: duplication tag handle
        var tag_entitlement_scrollview = new[]
        {
                new Tuple<Equipments.Type, List<EntitlementInfo>, InventoryGridLayout>(Equipments.Type.Hat, new List<EntitlementInfo>(), UIHandlerEntitlementComponent.gridLayoutHats),
                new Tuple<Equipments.Type, List<EntitlementInfo>, InventoryGridLayout>(Equipments.Type.Effect, new List<EntitlementInfo>(), UIHandlerEntitlementComponent.gridLayoutEffects)
            };

        // Insert entitlement for each kind of tag / equipment type
        foreach (var entitlement in results)
        {
            foreach (var tag in GetItemFromCache(entitlement.itemId).tags)
            {
                foreach (var entry in tag_entitlement_scrollview)
                {
                    if (Equipments.TypeFromString(tag) == entry.Item1)
                    {
                        entry.Item2.Add(entitlement);
                    }
                }
            }
        }

        foreach (var row in tag_entitlement_scrollview)
        {
            var prefabs = row.Item3.PopulateChild<ItemInventoryPrefab>(row.Item2.Count, UIHandlerEntitlementComponent.itemInventoryPrefab);
            allPrefabsCollection.SetItemInventoryPrefabs(row.Item1, prefabs);

            for (var i = 0; i < prefabs.Length; i++)
            {
                var index = i;
                foreach (var itemInfo in allItemInfo.data)
                {
                    if (row.Item2[index].itemId == itemInfo.itemId)
                    {
                        prefabs[index].Init(OnItemSelected(prefabs[index], prefabs), itemInfo);

                        foreach (var image in itemInfo.images)
                        {
                            if (image.As == LightFantasticConfig.IMAGE_AS)
                            {
                                UIHandlerEntitlementComponent.uiUtilities.DownloadImage(image.smallImageUrl, prefabs[index].image);
                            }
                        }
                    }
                }
            }
        }
    }

    private UnityAction OnItemSelected(ItemInventoryPrefab selectedPrefab, ItemInventoryPrefab[] prefabsInCurrentRow)
    {
        return () =>
        {
            foreach (var prefab in prefabsInCurrentRow)
            {
                if (prefab == selectedPrefab)
                {
                    if (prefab.IsSelected())
                    {
                        ClickItem(false, selectedPrefab, activeEquipmentList);
                    }
                    else
                    {
                        ClickItem(true, selectedPrefab, activeEquipmentList);
                    }
                }
                else if (prefab != selectedPrefab)
                {
                    prefab.Unselect();
                }
            }
        };
    }

    private void ClickItem(bool equipping, ItemInventoryPrefab prefab, Equipments.EquipmentList updatedList)
    {
        var itemInfo = prefab.GetItemInfo();

        foreach (var tag in prefab.GetItemInfo().tags)
        {
            ref var tmp = ref updatedList.GetItemInfo(Equipments.TypeFromString(tag));
            tmp = equipping ? itemInfo : null;
        }
        if (equipping)
        {
            prefab.Select();
            UpdateAvatar(prefab.GetItemInfo(), true);
        }
        else
        {
            prefab.Unselect();
            UpdateAvatar(prefab.GetItemInfo(), false);
        }
    }

    public void RevertToOriginalEquipment()
    {
        activeEquipmentList = (Equipments.EquipmentList)originalEquipmentList.Clone();
        EquipFromList(originalEquipmentList);
    }

    public void UploadEquipment()
    {
        UpdateUserProfileRequest savedEquipment = new UpdateUserProfileRequest();
        savedEquipment.customAttributes = activeEquipmentList.ToCustomAttribute();

        UIHandlerEntitlementComponent.abUserProfileLogic.UpdateMine(savedEquipment, result =>
        {
            //TODO: handle on error and success
            if (result.IsError)
            {
                Debug.Log("Failed to save current equipment!");
            }
            else
            {
                Debug.Log("Current equipment saved!");
            }
        });
    }

    private void ShowPromptPanel()
    {
        UIElementHandler.ShowNonExclusivePanel(NonExclusivePanelType.EQUIPMENT_BACK_PROMPT_PANEL);
    }

    private void HidePromptPanel()
    {
        UIElementHandler.HideNonExclusivePanel(NonExclusivePanelType.EQUIPMENT_BACK_PROMPT_PANEL);
    }

    public void ShowHatInventories(bool show)
    {
        UIHandlerEntitlementComponent.gridLayoutHats.SetVisibility(show);
    }

    public void ShowEffectInventories(bool show)
    {
        UIHandlerEntitlementComponent.gridLayoutEffects.SetVisibility(show);
    }

    private void UpdateAvatar(ItemInfo itemInfo, bool equip)
    {
        // Set Hat
        if (itemInfo.tags.Contains(LightFantasticConfig.ItemTags.hat))
        {
            if (equip)
            {
                UIHandlerEntitlementComponent.hatSpriteResolver.SetCategoryAndLabel(LightFantasticConfig.ItemTags.hat, itemInfo.name);
            }
            else
            {
                UIHandlerEntitlementComponent.hatSpriteResolver.SetCategoryAndLabel(LightFantasticConfig.ItemTags.hat, "");
            }
        }
    }
}