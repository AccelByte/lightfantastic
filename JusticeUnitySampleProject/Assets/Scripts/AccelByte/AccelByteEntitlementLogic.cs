#pragma warning disable 0649

using System;
using System.Collections.Generic;
using System.Linq;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;
using UnityEngine.Events;

static class Equipments
{
    public enum Type
    {
        Glasses,
        Mask,
        Shirt,
        Wristband,
        Pants,
        Shoes,
        None
    }

    public struct StringEntry
    {
        public Type type;
        public string tag;
    }

    public class List
    {
        public ItemInfo glasses;
        public ItemInfo mask;
        public ItemInfo shirt;
        public ItemInfo wristband;
        public ItemInfo pants;
        public ItemInfo shoes;
    }

    private static readonly StringEntry[] equipmentStringEntry =
    {
        new StringEntry {type = Type.Glasses, tag = "glasses"},
        new StringEntry {type = Type.Mask, tag = "mask"},
        new StringEntry {type = Type.Shirt, tag = "shirt"},
        new StringEntry {type = Type.Wristband, tag = "wristband"},
        new StringEntry {type = Type.Pants, tag = "pants"},
        new StringEntry {type = Type.Shoes, tag = "shoes"}
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
    public class AccelByteEntitlementLogic : MonoBehaviour
    {
        private Entitlements abEntitlements;
        private Items abItems;
        private const string ITEMINFO_IMAGE_AS = "product-cover";
        private const string ITEMINFO_LANGUAGE = "en";
        private readonly ItemCriteria ALL_ITEM_CRITERIA = new ItemCriteria();
        private readonly ItemPagingSlicedResult allItemInfo = new ItemPagingSlicedResult();
        private ItemInventoryPrefab[] allItemPrefabs;
        private readonly Equipments.List activeEquipment = new Equipments.List();

        #region Inventory Viewer Component 

        [SerializeField] private GameObject samplePrefab;
        [SerializeField] private UiUtilities uiUtilities;
        [SerializeField] private CustomScrollView scrollViewGlasses;
        [SerializeField] private CustomScrollView scrollViewMask;
        [SerializeField] private CustomScrollView scrollViewShirt;
        [SerializeField] private CustomScrollView scrollViewWristBand;
        [SerializeField] private CustomScrollView scrollViewPants;
        [SerializeField] private CustomScrollView scrollViewShoes;

        #endregion

        private void Start()
        {
            abEntitlements = AccelBytePlugin.GetEntitlements();
            abItems = AccelBytePlugin.GetItems();
            ALL_ITEM_CRITERIA.offset = 0;
            ALL_ITEM_CRITERIA.language = ITEMINFO_LANGUAGE;
            ALL_ITEM_CRITERIA.itemType = ItemType.NONE;
        }

        public void GetEntitlement()
        {
            //TODO: fix FadeLoadingIn
            //uiHandler.FadeLoading();
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
                PopulateInventories(result.Value.data);
            }
        }

        private void PopulateInventories(EntitlementInfo[] results)
        {
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
                new Tuple<Equipments.Type, List<EntitlementInfo>, CustomScrollView>(Equipments.Type.Glasses, new List<EntitlementInfo>(), scrollViewGlasses),
                new Tuple<Equipments.Type, List<EntitlementInfo>, CustomScrollView>(Equipments.Type.Shoes, new List<EntitlementInfo>(), scrollViewShoes),
                new Tuple<Equipments.Type, List<EntitlementInfo>, CustomScrollView>(Equipments.Type.Mask, new List<EntitlementInfo>(), scrollViewMask),
                new Tuple<Equipments.Type, List<EntitlementInfo>, CustomScrollView>(Equipments.Type.Shirt, new List<EntitlementInfo>(), scrollViewShirt),
                new Tuple<Equipments.Type, List<EntitlementInfo>, CustomScrollView>(Equipments.Type.Wristband, new List<EntitlementInfo>(), scrollViewWristBand),
                new Tuple<Equipments.Type, List<EntitlementInfo>, CustomScrollView>(Equipments.Type.Pants, new List<EntitlementInfo>(), scrollViewPants)
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
                                if (image.As == ITEMINFO_IMAGE_AS)
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
                            EquipItem(false, selectedPrefab.GetItemInfo());
                            prefab.Unselect();
                        }
                        else
                        {
                            EquipItem(true, selectedPrefab.GetItemInfo());
                            prefab.Select();
                        }
                    }
                    else if (prefab != selectedPrefab)
                    {
                        prefab.Unselect();
                    }
                }
            };
        }

        private void EquipItem(bool equipping, ItemInfo itemInfo)
        {
            foreach (var tag in itemInfo.tags)
            {
                switch (Equipments.TypeFromString(tag))
                {
                    case Equipments.Type.Glasses:
                        activeEquipment.glasses = equipping ? itemInfo : null;
                        return;
                    case Equipments.Type.Mask:
                        activeEquipment.mask = equipping ? itemInfo : null;
                        return;
                    case Equipments.Type.Shirt:
                        activeEquipment.shirt = equipping ? itemInfo : null;
                        return;
                    case Equipments.Type.Wristband:
                        activeEquipment.wristband = equipping ? itemInfo : null;
                        return;
                    case Equipments.Type.Pants:
                        activeEquipment.pants = equipping ? itemInfo : null;
                        return;
                    case Equipments.Type.Shoes:
                        activeEquipment.shoes = equipping ? itemInfo : null;
                        return;
                    case Equipments.Type.None:
                        return;
                }
            }
        }
    }
}