using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SampleObject : MonoBehaviour, IPointerClickHandler
{
    public string samplePath;
    public Text text;
    public Text descriptionText;

    public GameObject selectionObj;

    public LayoutElement minSize;


    public GameObject loadingIcon;

    public Texture2D waveformTexture;
    public AudioClip audioClip;

    public string audioName;

    bool highlighted;
    private bool expanded = false;
    public GameObject dropdownButton;

    double mb = 0;

    private void Start()
    {
        loadingIcon.SetActive(true);
        waveformTexture = new Texture2D(1,1);
        audioClip = AudioClip.Create("null", 1,1,1000,false);
        StartWaveformRender();
        UpdateText();

    }
    public void StartWaveformRender()
    {
        //samplePath = "file://" + samplePath;
        audioName = Path.GetFileName("file://" + samplePath);

        //loadingPanel.SetActive(true);
        StartCoroutine(DoWaveformRender());
        //StartCoroutine(DoWaveformRender());
    }

    public IEnumerator DoWaveformRender()
    {
        WWW request = GetAudioFromFile(samplePath);
        yield return request;

        audioClip = request.GetAudioClip();
        audioClip.name = audioName;

        //editor.sampleVisualiser.clip = audioClip;
        waveformTexture = WavTexture.WavTexture.BakeClip(audioClip, 0, TextureFormat.RGBA32);

        mb = (((waveformTexture.width * waveformTexture.height) * 4f) / 1000000f);
        SetPath(samplePath);
        loadingIcon.SetActive(false);
        UpdateText();

        Editor.current.renderQueue.Remove(samplePath);
        //waveformRenderer.zoom = Mathf.Infinity;
        //waveformRenderer.startTime = 0;
        //loadingPanel.SetActive(false);
    }

    private WWW GetAudioFromFile(string path)
    {
        string audioToLoad = string.Format(path);
        WWW request = new WWW(audioToLoad);
        return request;
    }

    public void SetPath(string newPath)
    {
        samplePath = newPath;
        //UpdateText();
    }

    public void UpdateText()
    {
        //text.text = Path.GetFileName(samplePath) + " RAM: ~" + mb.ToString("f2") + "mb";
        text.text = Path.GetFileName(samplePath); ;
        //FileInfo fileInfo = new System.IO.FileInfo(samplePath);
        System.TimeSpan ts = System.TimeSpan.FromSeconds(audioClip.length);
        string lengg = "";
        if (ts.TotalSeconds < 1)
        {
            lengg = ts.TotalMilliseconds + "ms";
        }
        else
        {
            lengg = ts.ToString(@"hh\:mm\:ss");
        }
        descriptionText.text = samplePath + "\nBake: ~" + mb.ToString("f2") + "mb\nLength: " + lengg + " (" + (int)((waveformTexture.width * waveformTexture.height) * 4) + ")";//\nsize" + (fileInfo.Length / 1000000).ToString("f2") + "mb";


    }

    public void Select()
    {
        Editor.current.SetCurrentSamplePath(this);
        //Editor.current.SelectSample(elementReference);
    }

    public void UpdateSelection(SampleObject obj)
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

    public void Expand()
    {
        expanded = !expanded;
        if (expanded)
        {
            minSize.minHeight = 100;
            dropdownButton.transform.eulerAngles = new Vector3(0, 0, 0);

        }
        else
        {
            minSize.minHeight = 30;
            dropdownButton.transform.eulerAngles = new Vector3(0, 0, 90);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount > 1)
        {
            Editor.current.SetPath(samplePath, this);
        }
    }
}
