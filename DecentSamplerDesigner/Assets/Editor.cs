using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;

public class Editor : MonoBehaviour
{
    public static Editor current;
    public SimpleFileBrowser.FileBrowser fileBrowser;
    [Header("DecentSamplerDesigner API")]
    public Main DesignerAPI;

    [Header("Current Selections")]
    public Main.GroupElement currentGroup;
    public Main.SampleElement currentSample;

    [Header("Sample Heirarchy")]
    public List<SampleObject> samplePaths;
    List<string> samplePathStrings = new List<string>();
    public SampleObject currentSamplePath;
    public Transform sampleHeirarchyContainer;
    public GameObject sampleHeirarchyItem;

    public Text heirarchyHeader;

    [Header("Sample Editor")]
    public GameObject[] disableWhileInactive;
    public GameObject[] enableWhileInactive;
    public AudioWaveFormVisualizer sampleVisualiser;
    public WavTextureBaker sampleVisualizer;
    public WavTexture.WaveformRenderer sampleRenderer;
    public AudioController audioLoader;
    public Text sampleNameDisplay;
    public InputField sampleRootNoteInput;
    public InputField sampleStartInput;
    public InputField sampleEndInput;

    [Header("Parameter Panel")]
    public GameObject parameterObjectPrefab;
    public Transform parameterObjectContainer;

    public Dropdown parameterNameDropdown;
    public InputField parameterValInput;

    public Main.DSAttribute currentAttribute;

    [Header("SampleTag & Groups Panel")]
    public GameObject sampleTagPrefab;
    public GameObject sampleGroupPrefab;
    public Transform sampleTagContainer;

    public SampleTagObject currentTag;
    public GroupTagObject currentGroupTag;

    [Header("Piano Panel")]
    public PianoViewer pianoViewer;
    public GameObject pianoBlockPrefab;
    public GameObject pianoBlockContainerPrefab;
    public Transform pianoBlockContainerContainer;

    [Header("Alt Menu")]
    public GameObject altMenu;
    public GameObject altWaveform;
    public Camera altWaveformCam;
    public WavTexture.MiniWaveform miniWaveform;
    public RectTransform altMenuRect;
    public RectTransform altWfRect;
    public Text altMenuText;

    int groupIterator;

    Coroutine loadingCoroutine;

    public List<string> renderQueue = new List<string>();
    public bool canLoad = false;
    public bool loaded = false;

    public Text tagHeader;

    public enum TopElement
    {
        ui,
        groups,
        effects
    }

    public enum GroupAttribute
    {
        volume,
        ampVelTrack,
        pan,
        trigger,
        onLoCCN,
        onHiCCN,
        loopCrossfade,
        loopCrossFadeMode,
        attack,
        decay,
        sustain,
        release,
        seqMode, // Valid values are random, true_random, round_robin, and always
        seqPosition,

    }

    public enum SampleAttribute
    {
        path,
        rootNote,
        loNote,
        hiNote,
        loVel,
        hiVel,
        start,
        end,
        tuning,
        volume,
        pan,
        trigger,
        onLoCCN,
        onHiCCN,
        loopStart,
        loopEnd,
        loopCrossfade,
        loopCrossfadeMode,
        loopEnabled,
        attack,
        decay,
        sustain,
        release,
        seqMode,
        seqPosition
    }

    public TopElement currentElement;

    private void Start()
    {
        current = this;

        currentGroup = DesignerAPI.groupElements[0];


        //currentGroups = DesignerAPI.groupsElement;

        Application.targetFrameRate = 70;

        //StartCoroutine(Load());
        //Load();
    }

    public void RenderWaveforms()
    {
        foreach (string path in DesignerAPI.samplePaths)
        {
            AddSamplePath(path);
            //samplePathStrings.Add(e.GetAttribute("path").value);
        }
    }

    IEnumerator CheckRenderComplete()
    {
        yield return new WaitForSeconds(0.2f);
        if (renderQueue.Count < 1)
        {
            //StartCoroutine(Load());
            canLoad = true;
        }

    }

    IEnumerator Load()
    {

        yield return new WaitForSeconds(0.5f);
        foreach (Main.GroupElement item in DesignerAPI.groupElements)
        {
            CreateGroupTagPrefab(item);

            foreach (Main.SampleElement e in item.samples)
            {
                //if (!samplePathStrings.Contains(e.GetAttribute("path").value))
                //{
                //    AddSamplePath(e.GetAttribute("path").value);
                //    samplePathStrings.Add(e.GetAttribute("path").value);
                //    //yield return new WaitForEndOfFrame();
                //}
                currentSamplePath = GetSamplePath(e.GetAttribute("path").value);
                CreateSampleTagPrefab(e, true);

                //yield return new WaitForSeconds(0.1f);
                //yield return new WaitForSeconds(0.1f);
                yield return new WaitForEndOfFrame();
            }

            //yield return new WaitForSeconds(0.1f);
        }

        loadingCoroutine = null;
    }

    public void RefreshAll()
    {
        // clear parameters
        //foreach (Transform child in parameterObjectContainer)
        //{
        //    Destroy(child.gameObject);
        //}

        // clear piano blocks
        //foreach (Transform child in pianoBlockContainerContainer)
        //{
        //    Destroy(child.gameObject);
        //}

        //// clear parameters
        //foreach (Transform child in sampleHeirarchyContainer)
        //{
        //    Destroy(child.gameObject);
        //}

        // clear parameters
        foreach (Transform child in sampleTagContainer)
        {
            Destroy(child.gameObject);
        }

        StartCoroutine(Load());
    }

    private void Update()
    {
        if (canLoad && !loaded)
        {
            loaded = true;
            if (loadingCoroutine == null)
            {
                foreach (Transform child in sampleTagContainer)
                {
                    Destroy(child.gameObject);
                }

                loadingCoroutine = StartCoroutine(Load());
            }

        }
        if (currentSample == null)
        {
            foreach (GameObject item in disableWhileInactive)
            {
                item.SetActive(false);
            }
            foreach (GameObject item in enableWhileInactive)
            {
                item.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject item in disableWhileInactive)
            {
                item.SetActive(true);
            }
            foreach (GameObject item in enableWhileInactive)
            {
                item.SetActive(false);
            }
        }

        if ((Input.GetKeyDown(KeyCode.D) && Input.GetKey(KeyCode.LeftControl)))
        {
            if (currentTag != null)
            {
                CloneSampleTag(currentTag);
            }
            else
            {
                AddSampleTag();
            }
            
        }

        List<RaycastResult> objectsUnderMouse = RaycastMouse();

        if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.LeftControl)) && objectsUnderMouse.Count > 0)
        {
            bool success = false;

            int index = 0;
            if (Input.GetKey(KeyCode.LeftControl) && objectsUnderMouse.Count > 1)
            {
                index = 1;
            }

            //altMenuText.text = objectsUnderMouse[0].gameObject.name;

            PIanoViewerBlock block = objectsUnderMouse[index].gameObject.GetComponent<PIanoViewerBlock>();
            SampleTagObject tag = objectsUnderMouse[index].gameObject.GetComponent<SampleTagObject>();
            GroupTagObject group = objectsUnderMouse[index].gameObject.GetComponent<GroupTagObject>();
            SampleObject sample = objectsUnderMouse[index].gameObject.GetComponent<SampleObject>();
            if (block)
            {
                success = true;

                altMenuText.text = block.tagOwner.text.text;
                altWaveform.SetActive(false);

                foreach (Main.DSAttribute item in block.tagOwner.elementReference.attributes)
                {
                    altMenuText.text += "\n" + item.name + ": " + item.value;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    SelectSampleTag(block.tagOwner.elementReference, block.tagOwner);
                    block.transform.SetAsLastSibling();
                }
            }
            else if (tag)
            {
                success = true;

                altMenuText.text = tag.text.text;
                altWaveform.SetActive(false);

                foreach (Main.DSAttribute item in tag.elementReference.attributes)
                {
                    altMenuText.text += "\n" + item.name + ": " + item.value;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    SelectSampleTag(tag.elementReference, tag);
                }

            }
            else if (group)
            {
                success = true;

                altMenuText.text = group.groupName.text;
                altWaveform.SetActive(false);

                altMenuText.text += " (" + group.children.Count + ")";

                //foreach (Main.DSAttribute item in group.groupReference.attributes)
                //{
                //    altMenuText.text += "\n" + item.name + ": " + item.value;
                //}

                //if (Input.GetMouseButtonDown(0))
                //{
                //    SelectSampleTag(tag.elementReference, tag);
                //}
            }
            else if (sample)
            {
                success = false;
                altWaveform.SetActive(true);
                altWaveformCam.enabled = true;
                miniWaveform.wavTexture._textures[0] = sample.waveformTexture;

            }

            altMenu.SetActive(success && Input.GetKey(KeyCode.LeftAlt));


        }
        else
        {
            altMenu.SetActive(false);
            altWaveform.SetActive(false);
            altWaveformCam.enabled = false;
        }

        if (Input.GetKeyDown(KeyCode.Delete) || (Input.GetKeyDown(KeyCode.X) && Input.GetKey(KeyCode.LeftControl)))
        {
            if (currentTag != null)
            {
                DeleteSampleTag(currentTag);
            }
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
        {
            if (loadingCoroutine != null)
            {
                StopCoroutine(loadingCoroutine);
            }
            RefreshAll();
        }

        altMenuRect.pivot = new Vector2((Mathf.InverseLerp(Screen.width, 0, Input.mousePosition.x)) > 0.3f ? 0 : 1, (Mathf.InverseLerp(Screen.height, 0, Input.mousePosition.y)) > 0.7f ? 0:1);
        altWfRect.pivot = altMenuRect.pivot;
        altMenu.transform.position = new Vector2 (Input.mousePosition.x, Input.mousePosition.y) + (Vector2.one * (altMenuRect.pivot * 20));
        altWaveform.transform.position = altMenu.transform.position;
        //altWaveform.transform.position = ((Input.mousePosition - (new Vector3(Screen.width/2, Screen.height/2))) / 100);
    }

    public List<RaycastResult> RaycastMouse()
    {

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            pointerId = -1,
        };

        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        //Debug.Log(results.Count);

        return results;
    }

    public void AddSamplePath(string path)
    {
        // create sample in ui, and keep track of it
        GameObject i = Instantiate(sampleHeirarchyItem);
        i.transform.SetParent(sampleHeirarchyContainer,true);
        i.transform.localScale = Vector3.one;
        SampleObject sObj = i.GetComponent<SampleObject>();
        sObj.SetPath(path);
        //Main.SampleElement element = currentGroup.AddSample(path, "64");
        //sObj.elementReference = element;
        samplePaths.Add(sObj);

        SetCurrentSamplePath(sObj);
    }

    public SampleObject GetSamplePath(string path)
    {
        SampleObject result = null;
        foreach (SampleObject item in samplePaths)
        {
            if (item.samplePath == path)
            {
                result = item;
            }
        }
        return result;
    }

    public void RemoveSamplePath(SampleObject sample)
    {
        samplePaths.Remove(sample);
    }

    public void AddGroup(string name)
    {
        Main.GroupElement g = DesignerAPI.AddGroup();
        g.AddAttribute("name", name);
        g.AddAttribute("volume", "0.0db");
    }

    public void SelectSample(Main.SampleElement element)
    {
        currentSample = element;
        
        //Debug.Log(element);
        sampleNameDisplay.text = Path.GetFileName(currentSample.GetAttribute("path").value);
        sampleRootNoteInput.text = currentSample.GetAttribute("rootNote").value;
        //audioLoader.GetFileFromPath(currentSample.GetAttribute("path").value);
        SelectAttribute("start");
        TrySetSamplePos(currentAttribute.value.ToString());

        
    }

    public void SetCurrentSamplePath(SampleObject obj)
    {
        currentSamplePath = obj;
        heirarchyHeader.text = "Library, (SEL:" + obj.text.text + ")";

        foreach (SampleObject item in sampleHeirarchyContainer.GetComponentsInChildren< SampleObject>())
        {
            item.UpdateSelection(obj);
        }
    }

    public void AddAttribute(Main.SampleElement element, SampleAttribute attribute, string value)
    {
        element.AddAttribute(attribute.ToString(), value, currentGroup);
    }

    public void AddSampleTag()
    {
        Main.SampleElement s = currentGroup.AddSample(currentSamplePath.samplePath, "60");
        AddAttribute(s, SampleAttribute.start, "0");
        AddAttribute(s, SampleAttribute.end, "0");
        //AddAttribute(s, SampleAttribute.attack, "0.1");
        //AddAttribute(s, SampleAttribute.decay, "0.2");
        //AddAttribute(s, SampleAttribute.sustain, "0.5");
        //AddAttribute(s, SampleAttribute.release, "0.5");
        AddAttribute(s, SampleAttribute.loNote, "60");
        AddAttribute(s, SampleAttribute.hiNote, "60");
        AddAttribute(s, SampleAttribute.loVel, "0");
        AddAttribute(s, SampleAttribute.hiVel, "127");


        CreateSampleTagPrefab(s,false);

        //tagHeader.text = "Tags, (SEL:" + stObj.text.text + ")";
    }

    public void CreateSampleTagPrefab(Main.SampleElement element, bool soft)
    {
        // create tag in heirarchy
        GameObject tagObj = Instantiate(sampleTagPrefab);
        tagObj.transform.SetParent(sampleTagContainer, true);
        tagObj.transform.localScale = Vector3.one;
        SampleTagObject tag = tagObj.GetComponent<SampleTagObject>();

        // create block on viewer
        GameObject blockObj = Instantiate(pianoBlockPrefab);
        blockObj.transform.SetParent(currentGroupTag.pianoBlockContainerReference, true);
        blockObj.transform.localScale = Vector3.one;
        PIanoViewerBlock block = blockObj.GetComponent<PIanoViewerBlock>();

        tag.sampleObjectOwner = currentSamplePath;
        block.tagOwner = tag;
        tag.block = block;
        tag.AssignReference(element, currentGroupTag);
        tag.transform.SetSiblingIndex(currentGroupTag.transform.GetSiblingIndex() + 1);
        tag.UpdateBlock();
        //block.lowNote = int.Parse(element.GetAttribute("loNote").value);
        //block.hiNote = int.Parse(element.GetAttribute("hiNote").value);
        //block.lowVel = int.Parse(element.GetAttribute("loVel").value);
        //block.hiVel = int.Parse(element.GetAttribute("hiVel").value);

        if (!soft)
        {
            SelectSampleTag(element, tag);
        }
        else
        {
            //SelectGroupTag(tag.elementReference.parent, tag.ownerGroup, false);
            tag.UpdateSelection(null);
        }

    }

    public void SelectSampleTag(Main.SampleElement element, SampleTagObject tagOwner)
    {
        PopulateDropDownWithSampleAttributes();
        currentSample = element;
        tagHeader.text = "Tags, (SEL:" + tagOwner.text.text + ")";
        currentTag = tagOwner;
        ReloadParameters(element.attributes, tagOwner);

        SelectSample(element);
        SelectGroupTag(tagOwner.elementReference.parent, tagOwner.ownerGroup, false);

        //DisplayWaveform(tagOwner.sampleObjectOwner);

        sampleRenderer.wavTexture._textures[0] = tagOwner.sampleObjectOwner.waveformTexture;
        //sampleRenderer.startTime = 0;
        sampleRenderer.wavTexture._length = tagOwner.sampleObjectOwner.waveformTexture.width * tagOwner.sampleObjectOwner.waveformTexture.height;

        if (element.HasAttribute("start"))
        {
            sampleRenderer.startTime = int.Parse(element.GetAttribute("start").value) - (sampleRenderer.zoom / 4);
        }
        else if (element.HasAttribute("end"))
        {
            sampleRenderer.startTime = int.Parse(element.GetAttribute("end").value) - (sampleRenderer.zoom / 4);
        }
        SelectAttribute(parameterNameDropdown.options[parameterNameDropdown.value].text);
        TrySetSamplePos(currentAttribute.value);
        //sampleRenderer.selectedSample = int.Parse(currentAttribute.value);

        foreach (SampleTagObject item in sampleTagContainer.GetComponentsInChildren<SampleTagObject>())
        {
            item.UpdateSelection(tagOwner);
        }

        //PopulateDropDown(typeof(SampleAttribute));

    }

    public void DeleteSampleTag(SampleTagObject tag)
    {
        currentGroup.RemoveSample(tag.elementReference);
        currentSample = null;
        //ReloadSampleTags();

        Destroy(tag.gameObject);

        if (currentSample == null)
        {
            ClearParameters();
        }
        else
        {
            ReloadParameters(currentSample.attributes, currentTag);
            SelectAttribute("start");
        }
    }

    public void ReloadSampleTags()
    {
        foreach (Transform child in sampleTagContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Main.SampleElement sample in currentGroup.samples)
        {
            CreateSampleTagPrefab(sample, false);
        }

    }

    public void CloneSampleTag(SampleTagObject tag)
    {
        Main.SampleElement s = currentGroup.AddSample("", "");
        s.attributes = tag.elementReference.DeepCopy();


        CreateSampleTagPrefab(s,false);
    }

    public void AddGroupTag()
    {
        groupIterator++;

        Main.GroupElement g = DesignerAPI.AddGroup();
        g.AddAttribute("name", "Group " + groupIterator);
        g.AddAttribute("volume", "0.0db");
        CreateGroupTagPrefab(g);

        //tagHeader.text = "Tags, (SEL:" + stObj.text.text + ")";
    }

    public void CreateGroupTagPrefab(Main.GroupElement element)
    {
        // create tag in heirarchy
        GameObject groupTag = Instantiate(sampleGroupPrefab);
        groupTag.transform.SetParent(sampleTagContainer, true);
        groupTag.transform.localScale = Vector3.one;
        GroupTagObject gt = groupTag.GetComponent<GroupTagObject>();

        // create container
        GameObject container = Instantiate(pianoBlockContainerPrefab);
        container.transform.SetParent(pianoBlockContainerContainer, true);
        container.transform.localScale = Vector3.one;

        gt.pianoBlockContainerReference = container.transform;

        //gt.sampleObjectOwner = currentSamplePath;
        //block.tagOwner = gt;
        //gt.block = block;
        gt.AssignReference(element);

        SelectGroupTag(element, gt, true);
        //gt.UpdateBlock();
        //block.lowNote = int.Parse(element.GetAttribute("loNote").value);
        //block.hiNote = int.Parse(element.GetAttribute("hiNote").value);
        //block.lowVel = int.Parse(element.GetAttribute("loVel").value);
        //block.hiVel = int.Parse(element.GetAttribute("hiVel").value);

        //SelectSampleTag(element, gt);
    }

    public void SelectGroupTag(Main.GroupElement group, GroupTagObject tagOwner, bool viewParameters)
    {

        currentGroup = group;
        currentGroupTag = tagOwner;

        foreach (GroupTagObject item in sampleTagContainer.GetComponentsInChildren<GroupTagObject>())
        {
            item.UpdateSelection(tagOwner);
        }

        if (!viewParameters)
        {
            if (currentTag.elementReference.parent != group)
            {
                if (currentGroupTag.children.Count > 0)
                {
                    SelectSampleTag(currentGroupTag.children[0].elementReference, currentGroupTag.children[0]);

                }

            }
        }
        else
        {
            PopulateDropDownGroupAttributes();
            currentSample = null;
            currentTag = null;

            foreach (SampleTagObject item in sampleTagContainer.GetComponentsInChildren<SampleTagObject>())
            {
                item.UpdateSelection(null);
            }


            ReloadParameters(currentGroup.attributes, tagOwner);
        }

        //PopulateDropDown(typeof(GroupAttribute));


    }

    public void DeleteGroupTag(GroupTagObject tag)
    {
        if (DesignerAPI.groupElements.Count > 1)
        {
            DesignerAPI.DeleteGroup(tag.groupReference);
            Destroy(tag.gameObject);

            //if (currentSample == null)
            //{
            //    ClearParameters();
            //}
            //else
            //{
            //    ReloadParameters(currentSample.attributes, currentTag);
            //    SelectAttribute("start");
            //}


            currentGroupTag = sampleTagContainer.GetComponentInChildren<GroupTagObject>();
            currentGroup = currentGroupTag.groupReference;

            SelectGroupTag(currentGroup, currentGroupTag, true);
        }


    }

    public void RebuildTagLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(sampleTagContainer.GetComponent<RectTransform>());
    }

    public void PopulateDropDown(Type _enum)
    {
        string[] vals = Enum.GetNames(_enum);
        List<string> opts = new List<string>(vals);

        parameterNameDropdown.ClearOptions();
        parameterNameDropdown.AddOptions(opts);
    }

    public void PopulateDropDownWithSampleAttributes()
    {
        string[] vals = Enum.GetNames(typeof(SampleAttribute));
        List<string> opts = new List<string>(vals);

        parameterNameDropdown.ClearOptions();
        parameterNameDropdown.AddOptions(opts);
    }

    public void PopulateDropDownGroupAttributes()
    {
        string[] vals = Enum.GetNames(typeof(GroupAttribute));
        List<string> opts = new List<string>(vals);

        parameterNameDropdown.ClearOptions();
        parameterNameDropdown.AddOptions(opts);
    }

    public void ReloadParameters(List<Main.DSAttribute> elements, SampleTagObject tagOwner)
    {
        ClearParameters();

        foreach (Main.DSAttribute attr in elements)
        {
            GameObject i = Instantiate(parameterObjectPrefab);
            i.transform.SetParent(parameterObjectContainer, true);
            i.transform.localScale = Vector3.one;
            ParameterObject pObj = i.GetComponent<ParameterObject>();
            pObj.AssignAttribute(attr);
            pObj.ownerTag = tagOwner;
        }

        SelectAttribute(parameterNameDropdown.options[parameterNameDropdown.value].text);
    }

    public void ReloadParameters(List<Main.DSAttribute> elements, GroupTagObject tagOwner)
    {
        ClearParameters();

        foreach (Main.DSAttribute attr in elements)
        {
            GameObject i = Instantiate(parameterObjectPrefab);
            i.transform.SetParent(parameterObjectContainer, true);
            i.transform.localScale = Vector3.one;
            ParameterObject pObj = i.GetComponent<ParameterObject>();
            pObj.AssignAttribute(attr);
            pObj.ownerGroupTag = tagOwner;
        }

        SelectAttribute(parameterNameDropdown.options[parameterNameDropdown.value].text);
    }

    public void ClearParameters()
    {
        foreach (Transform child in parameterObjectContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void UpdateParameters()
    {
        foreach (Transform child in parameterObjectContainer)
        {
            ParameterObject p = child.GetComponent<ParameterObject>();
            if (p)
            {
                p.UpdateInputBox();
                if (p.attribute.name == "path")
                {
                    p.DisableInput();
                }
                if (p.attribute.name == "start")
                {
                    p.ownerTag.UpdateText();
                }
            }
        }
    }

    public void AddParameter()
    {
        if (currentSample != null)
        {
            if (currentSample.GetAttribute(parameterNameDropdown.options[parameterNameDropdown.value].text) == null)
            {
                currentSample.AddAttribute(parameterNameDropdown.options[parameterNameDropdown.value].text, parameterValInput.text, currentGroup);
            }
            else
            {
                currentSample.GetAttribute(parameterNameDropdown.options[parameterNameDropdown.value].text).SetValue(parameterValInput.text);
            }

            ReloadParameters(currentSample.attributes, currentTag);
            SelectAttribute(parameterNameDropdown.options[parameterNameDropdown.value].text);
        }
        else if (currentGroup != null)
        {
            if (currentGroup.GetAttribute(parameterNameDropdown.options[parameterNameDropdown.value].text) == null)
            {
                currentGroup.AddAttribute(parameterNameDropdown.options[parameterNameDropdown.value].text, parameterValInput.text, currentGroup);
            }
            else
            {
                currentGroup.GetAttribute(parameterNameDropdown.options[parameterNameDropdown.value].text).SetValue(parameterValInput.text);
            }

            ReloadParameters(currentGroup.attributes, currentGroupTag);
            SelectAttribute(parameterNameDropdown.options[parameterNameDropdown.value].text);
        }


    }

    public void RemoveParameter(Main.DSAttribute attr, Main.SampleElement element)
    {
        element.RemoveAttribute(attr);
        ReloadParameters(element.attributes, currentTag);
    }

    public void RemoveParameter(Main.DSAttribute attr, Main.GroupElement element)
    {
        element.RemoveAttribute(attr);
        ReloadParameters(element.attributes, currentTag);
    }

    public void SetPath(string newPath, SampleObject sampleObj)
    {
        currentTag.elementReference.GetAttribute("path").value = newPath;
        currentTag.sampleObjectOwner = sampleObj;
        SelectSampleTag(currentTag.elementReference, currentTag);
        currentTag.UpdateText();
    }

    public void SelectAttribute(ParameterObject param)
    {
        if (param.attribute.name == "path")
        {
            return;
        }
        currentAttribute = param.attribute;

        foreach (ParameterObject item in parameterObjectContainer.GetComponentsInChildren<ParameterObject>())
        {
            item.UpdateSelection(param);
        }

        TrySetSamplePos(currentAttribute.value.ToString());

        for (int i = 0; i < parameterNameDropdown.options.Count; i++)
        {
            if (parameterNameDropdown.options[i].text == param.attribute.name)
            {
                parameterNameDropdown.value = i;
            }
        }
        
    }

    public void SelectAttribute(string attributeName)
    {
        bool success = false;
        ParameterObject fallback = null;
        foreach (ParameterObject item in parameterObjectContainer.GetComponentsInChildren<ParameterObject>())
        {
            if (item.attribute.name == attributeName)
            {
                SelectAttribute(item);
                success = true;
            }
            if (item.attribute.name == "start")
            {
                fallback = item;
            }
        }

        if (success == false)
        {
            if (fallback != null)
            {
                SelectAttribute(fallback);
            }
        }
    }

    public void TrySetSamplePos(string pos)
    {
        if (currentAttribute.name == "start" || currentAttribute.name == "end")
        {
            int r;
            if (int.TryParse(pos, out r))
            {
                sampleRenderer.selectedSample = r;
            }
            else
            {
                sampleRenderer.selectedSample = 0;
            }
        }
        else
        {
            float r;
            if (float.TryParse(pos, out r))
            {
                sampleRenderer.selectedSample = (int)((r / 100) * (sampleRenderer.wavTexture._length * 4));
            }
            else
            {
                sampleRenderer.selectedSample = 0;
            }
        }

    }

    //public static T ImportXml<T>(string path)
    //{
    //    try
    //    {
    //        XmlSerializer serializer = new XmlSerializer(typeof(T));
    //        using (var stream = new FileStream(path, FileMode.Open))
    //        {
    //            return (T)serializer.Deserialize(stream);
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.LogError("Exception importing xml file: " + e);
    //        return default;
    //    }
    //}

    public void LoadFromPath(string _path)
    {
        renderQueue = new List<string>();
        canLoad = false;
        loaded = false;


        DesignerAPI.LoadXml(_path);

        renderQueue = DesignerAPI.samplePaths;
        StartCoroutine(CheckRenderComplete());
        RenderWaveforms();
    }

    private void OnGUI()
    {
        //if (GUI.Button(new Rect(100,80,200,20), "Create new <sample>"))
        //{
        //    AddSampleTag();
        //}
        return;
        // group create button
        //if (GUI.Button(new Rect(100, 40, 100, 20), "New Group"))
        //{
        //    //AddGroup("gr " + Random.value.ToString("f3"));
        //}

        //if (GUI.Button(new Rect(100, 60, 100, 20), "Create"))
        //{
        //    DesignerAPI.GenerateFile();
        //}

        int startX = 200;
        // foreach group
        foreach (Main.GroupElement group in DesignerAPI.groupElements)
        {
            // create a button to create a new sample within that group
            if (GUI.Button(new Rect(startX, 20, 100, 20), group.GetAttribute("name").value + " (+new)"))
            {
                //group.AddSample("path " + Random.value.ToString("f3"), "");
            }

            int startY = 40;
            // foreach sample
            foreach (Main.SampleElement sample in group.samples)
            {
                GUI.Label(new Rect(startX, startY, 200, 20), Path.GetFileName(sample.GetAttribute("path").value));
                startY += 20;
            }

            startX += 100;
        }


    }
}
