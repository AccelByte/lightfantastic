//Disables the warning messages generated from private [SerializeField]
#pragma warning disable 0649
using System.Collections;
using UnityEngine;

namespace UITools
{
    public class UIElementHandler : MonoBehaviour
    {
        #region UIPanels
        [SerializeField]
        private CanvasGroup registerPanel;
        [SerializeField]
        private CanvasGroup verifyPanel;
        [SerializeField]
        private CanvasGroup loginPanel;
        [SerializeField]
        private CanvasGroup menuPanel;
        [SerializeField]
        private CanvasGroup persistentFriendsPanel;
        [SerializeField]
        private CanvasGroup friendPanel;
        [SerializeField]
        private CanvasGroup searchFriendPanel;
        [SerializeField]
        private CanvasGroup matchmakingPanel;
        [SerializeField]
        private CanvasGroup multiplayerOptionPanel;
        [SerializeField]
        private CanvasGroup inventoryPanel;
        [SerializeField]
        private CanvasGroup loadingPanel;
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
        private const float TRANSITION_SPEED = 4f;

        //Fade In/Out the Login UI Panel
        public void FadeLogin()
        {
            if (loginPanel.alpha == MAX_ALPHA)
            {
                StartCoroutine(FadeOut(loginPanel));
            }
            else
            {
                StartCoroutine(FadeIn(loginPanel));
            }
        }

        //Fade In/Out the Register UI Panel
        public void FadeRegister()
        {
            if (registerPanel.alpha == MAX_ALPHA)
            {
                StartCoroutine(FadeOut(registerPanel));
            }
            else
            {
                StartCoroutine(FadeIn(registerPanel));
            }
        }

        //Fade In/Out the Verify UI Panel
        public void FadeVerify()
        {
            if (verifyPanel.alpha == MAX_ALPHA)
            {
                StartCoroutine(FadeOut(verifyPanel));
            }
            else
            {
                StartCoroutine(FadeIn(verifyPanel));
            }
        }
        
        //Fade In/Out the Verify UI Panel
        public void FadeMenu()
        {
            if (menuPanel.alpha == MAX_ALPHA)
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
            if (persistentFriendsPanel.alpha == MAX_ALPHA)
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
            if (friendPanel.alpha == MAX_ALPHA)
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
            if (searchFriendPanel.alpha == MAX_ALPHA)
            {
                StartCoroutine(FadeOut(searchFriendPanel));
            }
            else
            {
                StartCoroutine(FadeIn(searchFriendPanel));
            }
        }

        public void FadeMatchmaking()
        {
            if (matchmakingPanel.alpha == MAX_ALPHA)
            {
                StartCoroutine(FadeOut(matchmakingPanel));
            }
            else
            {
                StartCoroutine(FadeIn(matchmakingPanel));
            }
        }

        public void FadeMultiplayerOption()
        {
            if (multiplayerOptionPanel.alpha == MAX_ALPHA)
            {
                StartCoroutine(FadeOut(multiplayerOptionPanel));
                currentPanel = menuPanel;
            }
            else
            {
                StartCoroutine(FadeIn(multiplayerOptionPanel));
            }
        }

        public void FadeInventory()
        {
            if (inventoryPanel.alpha == MAX_ALPHA)
            {
                StartCoroutine(FadeOut(inventoryPanel));
            }
            else
            {
                StartCoroutine(FadeIn(inventoryPanel));
            }
        }

        public void FadeLoading()
        {
            if (loadingPanel.alpha == MAX_ALPHA)
            {
                StartCoroutine(FadeOut(loadingPanel));
                currentPanel = menuPanel;
            }
            else
            {
                StartCoroutine(FadeIn(loadingPanel));
            }
        }

        public void FadePlayerProfile()
        {
            if (playerProfilePanel.alpha == MAX_ALPHA)
            {
                StartCoroutine(FadeOut(playerProfilePanel));
            }
            else
            {
                StartCoroutine(FadeIn(playerProfilePanel));
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
            panelToFade.alpha = MIN_ALPHA;
            panelToFade.gameObject.SetActive(true);
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