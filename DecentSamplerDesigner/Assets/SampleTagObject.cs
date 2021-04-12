using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SampleTagObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Main.SampleElement elementReference;
    public GroupTagObject ownerGroup;
    public SampleObject sampleObjectOwner;
    public Text text;
    public GameObject selectionObj;

    public PIanoViewerBlock block;

    public GameObject deleteButton;

    public void AssignReference(Main.SampleElement element, GroupTagObject group)
    {
        elementReference = element;
        ownerGroup = group;
        if (!ownerGroup.children.Contains(this))
        {
            ownerGroup.children.Add(this);
            ownerGroup.UpdateText();
        }

        if (!group.shown)
        {
            group.HideShow();
        }
        UpdateText();
    }

    public void UpdateText()
    {
        text.text = elementReference.GetAttribute("rootNote").value + ":" + (Path.GetFileName(elementReference.GetAttribute("path").value) + ", " + elementReference.GetAttribute("start").value);
        Editor.current.tagHeader.text = "Tags, (SEL:" + text.text + ")";
        block.nameText.text = text.text;
    }

    public void UpdateBlock()
    {
        block.lowNote = int.Parse(elementReference.GetAttribute("loNote").value);
        block.hiNote = int.Parse(elementReference.GetAttribute("hiNote").value);
        block.lowVel = int.Parse(elementReference.GetAttribute("loVel").value);
        block.hiVel = int.Parse(elementReference.GetAttribute("hiVel").value);
    }

    public void Select()
    {
        Editor.current.SelectSampleTag(elementReference, this);
        //Editor.current.SelectSample(elementReference);
    }

    public void UpdateSelection(SampleTagObject obj)
    {
        if (obj == this)
        {
            selectionObj.SetActive(true);
            block.selected = true;
        }
        else
        {
            selectionObj.SetActive(false);
            block.selected = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        deleteButton.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        deleteButton.SetActive(false);

    }

    public void DeleteMe()
    {
        Editor.current.DeleteSampleTag(this);
    }

    public void OnDestroy()
    {
        ownerGroup.children.Remove(this);
        Destroy(block.gameObject);
        ownerGroup.UpdateText();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount > 1)
        {
            Editor.current.pianoViewer.startNote = -(block.lowNote - 5);
        }
    }

    public void Minimise()
    {
        GetComponent<RectTransform>().sizeDelta = Vector2.zero;
    }

    public void Expand()
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(300, 30);

    }
}