#pragma warning disable 0649
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

namespace UITools
{
    [Serializable]
    public enum ExclusivePanelType
    {
        _empty_, // none
        REGISTER,
        VERIFY,
        LOGIN,
        MAIN_MENU,
        FRIENDS,
        SEARCH_FRIEND,
        INVENTORY,
        PLAYER_PROFILE,
        SETTINGS
    }

    [Serializable]
    public enum NonExclusivePanelType
    {
        _empty_, // none
        PARENT_OF_OVERLAY_PANELS, // Held tons of overlay panel
        MATCHMAKING, // Matchamaking mode selection panel
        LOADING,
        EQUIPMENT_BACK_PROMPT_PANEL
    }
    
    [Serializable]
    public struct ExclusivePanel
    {
        public CanvasGroup canvasGroup;
        public ExclusivePanelType type;
    }

    [Serializable]
    public struct NonExclusivePanel
    {
        public CanvasGroup canvasGroup;
        public NonExclusivePanelType type;
    }
    
    public class UIElementHandler : MonoBehaviour
    {
#region MainMenuButtons
        [Header("Main Menu Buttons")] 
        [SerializeField] public Button onlineButton;
        [SerializeField] public Button localButton;
        [SerializeField] public Button inventoryButton;
        [SerializeField] public Button leaderboardButton;
        [SerializeField] public Button settingsButton;
        [SerializeField] public Button logoutButton;
        [SerializeField] public Button exitButton;
        [SerializeField] public Button chatButton;
        [SerializeField] public Button profileButton;
        [SerializeField] public Button[] partyButtons;
#endregion

        #region UIPanels

        [SerializeField] [Header("Exclusive Panels")]
        private ExclusivePanel[] exclusivePanels;
        [SerializeField] [Header("Non Exclusive Panels")]
        private NonExclusivePanel[] nonExclusivePanels;

        [SerializeField]
        private CanvasGroup menuPanel;
        [SerializeField]
        private CanvasGroup persistentFriendsPanel;
        [SerializeField]
        private CanvasGroup friendPanel;
        [SerializeField]
        private CanvasGroup searchFriendPanel;
        [SerializeField]
        public CanvasGroup loadingPanel;
        [SerializeField]
        private CanvasGroup playerProfilePanel;

        private CanvasGroup currentPanel;

        //Notifications
        public Transform generalNotification;
        public Transform inviteNotification;
        #endregion

        private const float MAX_ALPHA = 1f;
        private const float MIN_ALPHA = 0f;
        private const float TRIGGER_MAX_ALPHA = 0.9f;
        private const float TRIGGER_ZERO_ALPHA = 0.1f;
        private const float TRANSITION_SPEED = 80f;

        private void Start()
        {
            settingsButton.onClick.AddListener(delegate { ShowExclusivePanel(ExclusivePanelType.SETTINGS); });
            profileButton.onClick.AddListener(delegate { ShowExclusivePanel(ExclusivePanelType.PLAYER_PROFILE); });
            foreach (var partyButton in partyButtons)
            {
                partyButton.onClick.AddListener(delegate { ShowExclusivePanel(ExclusivePanelType.FRIENDS); });
            }
        }

        public void ShowExclusivePanel(ExclusivePanelType param)
        {
            foreach (var panel in exclusivePanels)
            {
                if (panel.type == param)
                {
                    StartCoroutine(FadeIn(panel.canvasGroup));
                }
                else
                {
                    StartCoroutine(FadeOut(panel.canvasGroup));
                }
            }
        }

        public void HideAllExclusivePanel()
        {
            foreach (var panel in exclusivePanels)
            {
                StartCoroutine(FadeOut(panel.canvasGroup));
            }
        }

        public void ShowNonExclusivePanel(NonExclusivePanelType param)
        {
            foreach (var panel in nonExclusivePanels)
            {
                if (panel.type == param)
                {
                    StartCoroutine(FadeIn(panel.canvasGroup));
                }
            }
        }

        public void HideNonExclusivePanel(NonExclusivePanelType param)
        {
            foreach (var panel in nonExclusivePanels)
            {
                if (panel.type == param)
                {
                    StartCoroutine(FadeOut(panel.canvasGroup));
                }
            }
        }
        
        public void HideAllNonExclusivePanel()
        {
            foreach (var panel in nonExclusivePanels)
            {
                StartCoroutine(FadeOut(panel.canvasGroup));
            }
        }

        public void BackToMainMenu(){ ShowExclusivePanel(ExclusivePanelType.MAIN_MENU); }
        
        //Fade In/Out the Verify UI Panel
        public void FadeMenu()
        {
            if (menuPanel.interactable)
            {
                StartCoroutine(FadeOut(menuPanel));
            }
            else
            {
                StartCoroutine(FadeIn(menuPanel));
            }
        }

        public void FadePersistentFriends()
        {
            if (persistentFriendsPanel.interactable)
            {
                StartCoroutine(FadeOut(persistentFriendsPanel));
            }
            else
            {
                StartCoroutine(FadeIn(persistentFriendsPanel));
            }
        }

        public void FadeFriends()
        {
            if (friendPanel.interactable)
            {
                StartCoroutine(FadeOut(friendPanel));
            }
            else
            {
                StartCoroutine(FadeIn(friendPanel));
            }
        }

        public void FadeSearchFriends()
        {
            if (searchFriendPanel.interactable)
            {
                StartCoroutine(FadeOut(searchFriendPanel));
            }
            else
            {
                StartCoroutine(FadeIn(searchFriendPanel));
            }
        }

        public void ShowLoadingPanel()
        {
            TweenComponent loadingTweenAnimator = loadingPanel.gameObject.GetComponent<TweenComponent>();
            if (loadingTweenAnimator != null)
            {
                loadingTweenAnimator.AnimateFadeIn();
            }
        }

        public void HideLoadingPanel()
        {
            TweenComponent loadingTweenAnimator = loadingPanel.gameObject.GetComponent<TweenComponent>();
            if (loadingTweenAnimator != null)
            {
                loadingTweenAnimator.AnimateFadeOut();
            }
        }

        //Fade In/Out the whatever the current UI Panel is
        public void FadeCurrent()
        {
            StartCoroutine(FadeOut(currentPanel));
        }

        //Lerp the target panel's alpha down to 0 and disable the gameObject
        public IEnumerator FadeOut(CanvasGroup panelToFade)
        {
            panelToFade.interactable = false;
            panelToFade.alpha = MAX_ALPHA;
            float startTime = Time.time;
            float amountToFade = 0 + panelToFade.alpha;
            while (panelToFade.alpha > TRIGGER_ZERO_ALPHA)
            {
                float transitionCovered = (Time.time - startTime) * TRANSITION_SPEED;
                float fractionOfTransition = transitionCovered / amountToFade;
                panelToFade.alpha = Mathf.Lerp(panelToFade.alpha, MIN_ALPHA, fractionOfTransition * Time.deltaTime);
                yield return null;
            }
            panelToFade.alpha = MIN_ALPHA;
            panelToFade.gameObject.SetActive(false);
        }

        //Enable the gameObject and lerp the target panel's alpha up to 1
        public IEnumerator FadeIn(CanvasGroup panelToFade)
        {
            panelToFade.gameObject.SetActive(true);
            panelToFade.interactable = true;
            panelToFade.alpha = MIN_ALPHA;
            float startTime = Time.time;
            float amountToFade = MAX_ALPHA - panelToFade.alpha;
            while (panelToFade.alpha < TRIGGER_MAX_ALPHA)
            {
                float transitionCovered = (Time.time - startTime) * TRANSITION_SPEED;
                float fractionOfTransition = transitionCovered / amountToFade;
                panelToFade.alpha = Mathf.Lerp(panelToFade.alpha, MAX_ALPHA, fractionOfTransition * Time.deltaTime);
                yield return null;
            }
            panelToFade.alpha = MAX_ALPHA;
            currentPanel = panelToFade;
        }
        public void ShowNotification(Transform notificationToShow)
        {
            StartCoroutine(TimedNotification(notificationToShow));
        }

        public IEnumerator TimedNotification(Transform notificationToShow)
        {
            notificationToShow.gameObject.SetActive(true);

            yield return new WaitForSeconds(5f);

            notificationToShow.gameObject.SetActive(false);
        }
    }

}