// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class BaseHoveringText : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The hovering text in scene.")]
        private TextMeshPro hoveringText = null;
        [SerializeField]
        [Tooltip("The text to display.")]
        private string textInput = "";
        [SerializeField]
        private Vector3 offsetPosition = Vector3.up;
        [SerializeField]
        private bool isUsingMainCamera = true;
        [SerializeField]
        private Camera alternativeCamera = null;

        private Camera currentCamera;

        // Start is called before the first frame update
        void Start()
        {
            if (isUsingMainCamera)
            {
                currentCamera = Camera.main;
            }
            else
            {
                currentCamera = alternativeCamera;
            }
        }

        void OnDestroy() { }

        // Update is called once per frame
        void Update(){}

        public void ChangeTextLabel(string text)
        {
            hoveringText.text = text;
        }

        public void ChangeTextColor(uint indexColor)
        {
            switch(indexColor)
            {
                case 0:
                    hoveringText.color = Color.red;
                    break;
                case 1:
                    hoveringText.color = Color.green;
                    break;
                case 2:
                    hoveringText.color = Color.yellow;
                    break;
                case 3:
                    hoveringText.color = Color.blue;
                    break;
            }
        }
    }
}
