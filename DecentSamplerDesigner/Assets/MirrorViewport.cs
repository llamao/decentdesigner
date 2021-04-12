using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MirrorViewport : MonoBehaviour
{
    public RectTransform sourceRect;
    public RectTransform targetRect;
    void LateUpdate()
    {
        targetRect.sizeDelta = sourceRect.sizeDelta;

        targetRect.anchoredPosition = sourceRect.anchoredPosition;
        targetRect.localScale = sourceRect.localScale;
    }
}
