using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StupidLayoutUpdate : MonoBehaviour
{

    public RectTransform dumbObject;

    void Start()
    {
        UpdateLayout();
    }

    public void UpdateLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(dumbObject);
    }
}
