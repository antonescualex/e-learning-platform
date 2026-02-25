using System;
using System.Collections;
using UnityEngine;

namespace MainMenu
{
    public class LevelBarService : MonoBehaviour
    {
        [SerializeField] private RectTransform fillMask;
        [SerializeField] private RectTransform maskContainer;

        public void SetProgress(float currentXP, float requiredXP)
        {
            if (requiredXP <= 0f) return;

            float normalized = Mathf.Clamp01(currentXP / requiredXP);
            float maxWidth = maskContainer.rect.width;

            fillMask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth * normalized);
        }
    }
}