using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroupTagObject : MonoBehaviour
{
    public Main.GroupElement groupReference;

    public List<SampleTagObject> children = new List<SampleTagObject>();

    public Transform pianoBlockContainerReference;

    public Text groupName;
    public GameObject selection;

    public Text button;

    public bool shown;

    public void AssignReference(Main.GroupElement element)
    {
        groupReference = element;
        UpdateText();
    }

    public void UpdateSelection(GroupTagObject newGroup)
    {
        if (newGroup == this)
        {
            selection.SetActive(true);
            pianoBlockContainerReference.gameObject.SetActive(true);
        }
        else
        {
            selection.SetActive(false);
            pianoBlockContainerReference.gameObject.SetActive(false);
        }
    }

    public void Select()
    {
        Editor.current.SelectGroupTag(groupReference, this, true);
    }

    public void HideShow()
    {
        shown = !shown;

        if (children.Count < 1)
        {
            DeleteGroup();
        }
        foreach (SampleTagObject item in children)
        {
            if (shown)
            {
                item.Expand();
                button.text = "-";
            }
            else
            {
                item.Minimise();
                button.text = "+";
            }

        }
        Editor.current.RebuildTagLayout();
    }

    public void UpdateText()
    {
        groupName.text = groupReference.GetAttribute("name").value + " [" + children.Count + "]";
        if (children.Count == 0)
        {
            button.text = "x";
        }
        else
        {
            button.text = shown ? "-" : "+";
        }
    }

    public void DeleteGroup()
    {
        Editor.current.DeleteGroupTag(this);
    }

    public void AddChild(SampleTagObject child)
    {
        children.Add(child);
        UpdateText();
    }

    public void RemoveChild(SampleTagObject child)
    {
        children.Remove(child);
        UpdateText();
    }
}
