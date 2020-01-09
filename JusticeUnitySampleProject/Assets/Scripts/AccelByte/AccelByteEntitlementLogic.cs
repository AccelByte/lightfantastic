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
using Utf8Json;

static class Equipments
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
            customAttributes.hatItemId = hat == null ? null : hat.itemId ;
            customAttributes.effectItemId = effect == null ? null : effect.itemId ;
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
            return null;
        }
        
        var result = new EquipmentList();
        
        foreach (var itemInformation in itemInformations)
        {
            if (itemInformation.itemId == customAttributes.hatItemId){ result.hat = itemInformation; }
            if (itemInformation.itemId == customAttributes.effectItemId){ result.effect = itemInformation; }
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
    
    public class AccelByteEntitlementLogic : MonoBehaviour
    {
        private Entitlements abEntitlements;
        private Items abItems;
        
        private readonly ItemCriteria ALL_ITEM_CRITERIA = new ItemCriteria();
        private readonly ItemPagingSlicedResult allItemInfo = new ItemPagingSlicedResult();
        private Equipments.EquipmentList activeEquipmentList = new Equipments.EquipmentList();
        private Equipments.EquipmentList originalEquipmentList = new Equipments.EquipmentList();
        private AllPrefabsCollection allPrefabsCollection;

        #region Inventory Viewer Component 

        [SerializeField] private GameObject samplePrefab;
        [SerializeField] private UiUtilities uiUtilities;
        [SerializeField] private InventoryGridLayout gridLayoutHats;
        [SerializeField] private InventoryGridLayout gridLayoutEffects;
        [SerializeField] private AccelByteButtonScriptStyle buttonHat;
        [SerializeField] private AccelByteButtonScriptStyle buttonEffect;
        [SerializeField] private CanvasGroup promptPanel;
        [SerializeField] private AccelByteUserProfileLogic abUserProfileLogic;

        #endregion

        private void Start()
        {
            abEntitlements = AccelBytePlugin.GetEntitlements();
            abItems = AccelBytePlugin.GetItems();
            ALL_ITEM_CRITERIA.offset = 0;
            ALL_ITEM_CRITERIA.language = LightFantasticConfig.DEFAULT_LANGUAGE;
            ALL_ITEM_CRITERIA.itemType = ItemType.NONE;
        }

        public void GetEntitlement()
        {
            //TODO: fix FadeLoadingIn
            //uiHandler.FadeLoading();
            
            activeEquipmentList = null;
            originalEquipmentList = null;
            HidePromptPanel();
            
            if (allItemInfo.data == null)
            {
                abItems.GetItemsByCriteria(ALL_ITEM_CRITERIA, result =>
                {
                    if (!result.IsError)
                    {
                        allItemInfo.data = result.Value.data;
                        abEntitlements.GetUserEntitlements(0, 99, OnGetEntitlement);
                    }
                });
            }
            else
            {
                abEntitlements.GetUserEntitlements(0, 99, OnGetEntitlement);
            }
        }

        private void OnGetEntitlement(Result<EntitlementPagingSlicedResult> result)
        {
            //TODO: fix FadeLoadingOut
            //uiHandler.FadeLoading();
            if (result.IsError)
            {
                // handle
            }
            else
            {
                abUserProfileLogic.GetMine(profileResult =>
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
                                activeEquipmentList = (Equipments.EquipmentList) originalEquipmentList.Clone();
                                EquipFromList(originalEquipmentList);
                            }
                        }
                        
                        buttonHat.SetEnable(false);
                        ShowHatInventories(true);
                        buttonEffect.SetEnable(true);
                        ShowEffectInventories(false);
                    }
                }
                );
            }
        }

        private void EquipFromList(Equipments.EquipmentList list)
        {
            foreach (Equipments.Type type in Enum.GetValues(typeof(Equipments.Type)))
            {
                if (type == Equipments.Type.None){ break; }
                var listItemInfo = list.GetItemInfo(type);
                foreach (var prefab in allPrefabsCollection.GetItemInventoryPrefabs(type))
                {
                    if (listItemInfo != null && listItemInfo.itemId == prefab.GetItemInfo().itemId)
                    {
                        prefab.Select();
                    }
                    else
                    {
                        prefab.Unselect();
                    }
                }
            }
        }

        private void PopulateInventories(EntitlementInfo[] results)
        {
            allPrefabsCollection = new AllPrefabsCollection();
            
            ItemInfo GetItemFromCache(string input)
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
            
            //TODO: duplication tag handle
            var tag_entitlement_scrollview = new[]
            {
                new Tuple<Equipments.Type, List<EntitlementInfo>, InventoryGridLayout>(Equipments.Type.Hat, new List<EntitlementInfo>(), gridLayoutHats),
                new Tuple<Equipments.Type, List<EntitlementInfo>, InventoryGridLayout>(Equipments.Type.Effect, new List<EntitlementInfo>(), gridLayoutEffects)
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
                var prefabs = row.Item3.PopulateChild<ItemInventoryPrefab>(row.Item2.Count, samplePrefab);
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
                                    uiUtilities.DownloadImage(image.smallImageUrl, prefabs[index].image);
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
            }
            else
            {
                prefab.Unselect();
            }
        }

        public void RevertToOriginalEquipment()
        {
            activeEquipmentList = (Equipments.EquipmentList) originalEquipmentList.Clone();
            EquipFromList(originalEquipmentList);
        }

        public void UploadEquipment()
        {
            UpdateUserProfileRequest savedEquipment = new UpdateUserProfileRequest();
            savedEquipment.customAttributes = activeEquipmentList.ToCustomAttribute();
            
            abUserProfileLogic.UpdateMine(savedEquipment, result =>
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

        public void ShowPromptPanel()
        {
            promptPanel.alpha = 1;
            promptPanel.gameObject.SetActive(true);
        }

        public void HidePromptPanel()
        {
            promptPanel.alpha = 0;
            promptPanel.gameObject.SetActive(false);
        }

        public void ShowHatInventories(bool show)
        {
            gridLayoutHats.SetVisibility(show);
        }
        
        public void ShowEffectInventories(bool show)
        {
            gridLayoutEffects.SetVisibility(show);
        }
    }
}