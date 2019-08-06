//Disables the warning messages generated from private [SerializeField]
#pragma warning disable 0649
using System.Collections;
using UnityEngine;

namespace UITools
{
    public class UIElementHandler : MonoBehaviour
    {
        #region
        [SerializeField]
        private CanvasGroup registerPanel;
        [SerializeField]
        private CanvasGroup verifyPanel;
        [SerializeField]
        private CanvasGroup loginPanel;

        private CanvasGroup currentPanel;
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
    }
}