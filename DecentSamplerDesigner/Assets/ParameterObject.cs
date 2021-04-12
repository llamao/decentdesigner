using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParameterObject : MonoBehaviour
{
    public Text nameText;
    public InputField inputBox;
    public GameObject selectionObj;
    public SampleTagObject ownerTag;
    public GroupTagObject ownerGroupTag;

    public Button deleteButton;

    public Main.DSAttribute attribute;

    public void AssignAttribute(Main.DSAttribute attrib)
    {
        attribute = attrib;
        nameText.text = attrib.name;
        inputBox.text = attrib.value;

        if (nameText.text == "path")
        {
            DisableInput();
            DisableDelete();
        }

        if (nameText.text == "rootNote" || nameText.text == "name")
        {
            DisableDelete();
        }
    }

    public void OnFinishEdit()
    {
        if (attribute.name != "path")
        {
            attribute.value = inputBox.text;
        }

        if (nameText.text == "path")// || nameText.text == "rootNote")
        {
            if (ownerTag != null)
            {
                ownerTag.UpdateText();
            }
        }
        else if(nameText.text == "start" || nameText.text == "rootNote")
        {
            //Editor.current.sampleRenderer.selectedSample = int.Parse(attribute.value);
            if (ownerTag != null)
            {
                ownerTag.UpdateText();
            }
        }
        else if (nameText.text == "loNote" || nameText.text == "hiNote" || nameText.text == "loVel" || nameText.text == "hiVel")
        {
            ownerTag.UpdateBlock();
        }
        else if (nameText.text == "name")
        {
            if (ownerGroupTag != null)
            {
                ownerGroupTag.UpdateText();
            }
        }
        SelectAttribute();
        Editor.current.TrySetSamplePos(attribute.value);



    }

    public void DeleteParameter()
    {
        if (ownerTag != null)
        {
            if (attribute.name != "path" && attribute.name != "rootNote") Editor.current.RemoveParameter(attribute, ownerTag.elementReference);
        }
        else if (ownerGroupTag != null)
        {
            if (attribute.name != "name") Editor.current.RemoveParameter(attribute, ownerGroupTag.groupReference);
        }
    }

    public void SelectAttribute()
    {
        Editor.current.SelectAttribute(this);
    }

    public void UpdateInputBox()
    {
        inputBox.text = attribute.value;
    }

    public void DisableInput()
    {
        inputBox.interactable = false;
    }

    public void UpdateSelection(ParameterObject obj)
    {
        if (obj == this)
        {
            selectionObj.SetActive(true);
        }
        else
        {
            selectionObj.SetActive(false);
        }
    }

    public void DisableDelete()
    {
        deleteButton.interactable = false;
        deleteButton.GetComponentInChildren<Text>().text = "";
    }
}
