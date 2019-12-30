#pragma warning disable 0649

using System.Diagnostics.PerformanceData;
using UnityEngine;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UITools;
using UnityEngine.UIElements;

namespace ABRuntimeLogic
{
    public class AccelByteEntitlementLogic : MonoBehaviour
    {
        private Entitlements abEntitlements;
        private Items abItems;
        private const string ITEMINFO_IMAGE_AS = "product-cover";
        private const string ITEMINFO_LANGUAGE = "en";
        private ItemCriteria ITEMCRITERIA_TO_ENTITLEMENT = new ItemCriteria();
        private ItemPagingSlicedResult allItemInfo = new ItemPagingSlicedResult();
        
        #region Inventory Viewer Component 
        [SerializeField]
        private GameObject samplePrefab;
        [SerializeField]
        private RectTransform scrollViewContentContainer;
        [SerializeField]
        private UiUtilities uiUtilities;
        [SerializeField]
        private UIElementHandler uiHandler;
        #endregion

        void Start()
        {
            abEntitlements = AccelBytePlugin.GetEntitlements();
            abItems = AccelBytePlugin.GetItems();
            ITEMCRITERIA_TO_ENTITLEMENT.offset = 0;
            ITEMCRITERIA_TO_ENTITLEMENT.language = ITEMINFO_LANGUAGE;
            ITEMCRITERIA_TO_ENTITLEMENT.itemType = ItemType.NONE;
        }

        public void GetEntitlement()
        {
            //TODO: fix FadeLoadingIn
            //uiHandler.FadeLoading();
            if (allItemInfo.data == null)
            {
                abItems.GetItemsByCriteria(ITEMCRITERIA_TO_ENTITLEMENT, result =>
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
                int entitlemenCount = result.Value.data.Length;
                int existingPrefab = scrollViewContentContainer.gameObject.transform.childCount;
                int requiredPrefab = 0;
                
                if (existingPrefab <= entitlemenCount)
                {
                    requiredPrefab = entitlemenCount - existingPrefab;
                }
                else //need to delete exisiting item
                {
                    while(existingPrefab > entitlemenCount)
                    {
                        DestroyImmediate(scrollViewContentContainer.gameObject.transform.GetChild(0).gameObject);
                    }
                }
                
                VerticalScrollViewPopulation<ItemInventoryPrefab>.
                    Populate(requiredPrefab, samplePrefab, scrollViewContentContainer);

                var populatedEntries = scrollViewContentContainer.GetComponentsInChildren<ItemInventoryPrefab>();

                for (int i = 0 ; i < populatedEntries.Length ; i++)
                {
                    int index = i;
                    foreach (var entry in allItemInfo.data)
                    {
                        if (result.Value.data[index].itemId == entry.itemId)
                        {
                            populatedEntries[index].count.text = result.Value.data[index].quantity.ToString();
                            populatedEntries[index].title.text = entry.title;
                            populatedEntries[index].description.text = entry.longDescription;
                            foreach (var image in entry.images)
                            {
                                if (image.As == ITEMINFO_IMAGE_AS)
                                {
                                    uiUtilities.DownloadImage(image.smallImageUrl, populatedEntries[index].image);
                                }
                            }
                        }
                    }

                    if (index == populatedEntries.Length - 1)
                    {
                    }
                }
            }
        }
    }
}
