using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AudioController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler, IPointerUpHandler
{
    public const string audioName = "mario.wav";

    public Editor editor;
    public WavTexture.WaveformRenderer waveformRenderer;
    [Header("Audio Stuff")]
    public AudioClip audioClip;
    public string soundPath;

    public GameObject loadingPanel;

    public RectTransform leftBound;
    public RectTransform rightBound;

    public LayoutGroup jank;
    public LayoutGroup jank2;
    public RectTransform samplePosSprite;
    public GameObject samplePosTextObjLeft;
    public GameObject samplePosTextObjRight;
    public Text samplePosTextLeft;
    public Text samplePosTextRight;
    public RectTransform mousePosSprite;

    public Texture2D normalCursor;
    public Texture2D mouseOverCursor;

    bool dragging;

    public void GetFileFromPath(string path)
    {
        soundPath = path;
        loadingPanel.SetActive(true);
        StartCoroutine(LoadAudio());
    }

    private IEnumerator LoadAudio()
    {
        WWW request = GetAudioFromFile("file://" + soundPath);
        yield return request;

        audioClip = request.GetAudioClip();
        audioClip.name = audioName;

        //editor.sampleVisualiser.clip = audioClip;
        editor.sampleVisualizer.Bake(audioClip);
        waveformRenderer.zoom = Mathf.Infinity;
        waveformRenderer.startTime = 0;
        loadingPanel.SetActive(false);
    }

    private WWW GetAudioFromFile(string path)
    {
        string audioToLoad = string.Format(path);
        WWW request = new WWW(audioToLoad);
        return request;
    }

    public bool highlight;
    private Vector2 lastMousePosition;

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlight = true;
        mousePosSprite.gameObject.SetActive(true);
        if (!Input.GetMouseButton(0))
        {
            dragging = true;
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        highlight = false;
        mousePosSprite.gameObject.SetActive(false);
        dragging = false;
    }




    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            lastMousePosition = eventData.position;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            Vector2 currentMousePosition = eventData.position;
            Vector2 diff = currentMousePosition - lastMousePosition;
            RectTransform rect = GetComponent<RectTransform>();

            if (((waveformRenderer.startTime + (-diff.x * (waveformRenderer.zoom / 1000)) + waveformRenderer.zoom) < waveformRenderer.wavTexture._length * 4))
            {
                waveformRenderer.startTime += (-diff.x * (waveformRenderer.zoom / 1000));
            }


            //Vector3 newPosition = rect.position + new Vector3(diff.x, diff.y, transform.position.z);
            //Vector3 oldPos = rect.position;
            //rect.position = newPosition;
            //if (!IsRectTransformInsideSreen(rect))
            //{
            //    rect.position = oldPos;
            //}
            lastMousePosition = currentMousePosition;


        }
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            waveformRenderer.selectedSample = (int)waveformRenderer.SampleNumFromScreenPos(Input.mousePosition, leftBound.position.x, rightBound.position.x);

        }

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("End Drag");
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            SetVal();

        }

    }

    private void Update()
    {
        float boundL = leftBound.position.x;
        float boundR = rightBound.position.x;
        float poss = Mathf.InverseLerp(waveformRenderer.startTime, waveformRenderer.startTime + waveformRenderer.zoom, (int)waveformRenderer.SampleNumFromScreenPos(Input.mousePosition, boundL, boundR));

        if (highlight)
        {
            
            mousePosSprite.position = new Vector3(Mathf.Lerp(boundL, boundR, poss), samplePosSprite.position.y);
            float z = waveformRenderer.zoom;
            if (Input.mouseScrollDelta.y < -0.1f)
            {
                waveformRenderer.zoom *= 1.3f;

                //waveformRenderer.startTime = Mathf.Lerp(waveformRenderer.startTime, (int)waveformRenderer.SampleNumFromScreenPos(Input.mousePosition, boundL, boundR), 0.5f);
                waveformRenderer.zoom = Mathf.Clamp(waveformRenderer.zoom, 10, waveformRenderer.wavTexture._length * 4);
                if (z < waveformRenderer.zoom)
                {
                    waveformRenderer.startTime -= (waveformRenderer.zoom / 10);
                }
            }
            if (Input.mouseScrollDelta.y > 0.1f)
            {
                waveformRenderer.zoom *= 0.7f;
                waveformRenderer.startTime = (Mathf.Lerp(waveformRenderer.startTime, (int)waveformRenderer.SampleNumFromScreenPos(Input.mousePosition, boundL, boundR), 0.5f) - 10);
                waveformRenderer.zoom = Mathf.Clamp(waveformRenderer.zoom, 10, waveformRenderer.wavTexture._length * 4);
                if (z < waveformRenderer.zoom)
                {
                    waveformRenderer.startTime -= (waveformRenderer.zoom / 10);
                }
            }

            waveformRenderer.zoom = Mathf.Clamp(waveformRenderer.zoom, 10, waveformRenderer.wavTexture._length * 4);
            //if (Input.GetMouseButton(2))
            //{
            //    waveformRenderer.startTime += (int)(-Input.GetAxisRaw("Mouse X"));
            //    //waveformRenderer.startTime = Mathf.Clamp(waveformRenderer.startTime, 0, (clip.samples * clip.channels - 1));
            //}
        }

        jank.enabled = false;
        jank.enabled = true;
        jank2.enabled = false;
        jank2.enabled = true;
        //samplePosSprite.position = new Vector3(Mathf.Lerp(boundL, boundR, waveformRenderer.samplePosNormalized), samplePosSprite.localPosition.y);

        //float poss = Mathf.InverseLerp(waveformRenderer.startTime, waveformRenderer.startTime + waveformRenderer.zoom, waveformRenderer.NormalisedPos(Input.mousePosition, boundL, boundR));

        //mousePosSprite.position = new Vector3(Mathf.Lerp(boundL, boundR, waveformRenderer.GetNormalisedPosFromMouse(Input.mousePosition, boundL, boundR)), mousePosSprite.position.y);

        samplePosSprite.position = new Vector3(Mathf.Lerp(boundL, boundR, waveformRenderer.samplePosNormalized), samplePosSprite.position.y);

        //float range = 0.02f;
        //bool hover = true;
        //if (poss > (waveformRenderer.samplePosNormalized + range) || poss < (waveformRenderer.samplePosNormalized - range))
        //{
        //    Cursor.SetCursor(mouseOverCursor, Vector2.zero, CursorMode.Auto);

        //}
        //else
        //{
        //    Cursor.SetCursor(normalCursor, Vector2.zero, CursorMode.Auto);
        //    hover = false;
        //}


        //if (Input.GetMouseButtonDown(0))
        //{
        //    waveformRenderer.selectedSample = (int)waveformRenderer.SampleNumFromScreenPos(Input.mousePosition, boundL, boundR);
        //}

        if (waveformRenderer.samplePosNormalized < 0.09f)
        {
            samplePosTextObjLeft.SetActive(false);
            samplePosTextObjRight.SetActive(true);
        }

        else
        {
            samplePosTextObjLeft.SetActive(true);
            samplePosTextObjRight.SetActive(false);
        }

        string a = editor.currentAttribute.name;
        if (a == "start" || a == "end")
        {

            samplePosTextLeft.text = waveformRenderer.selectedSample.ToString();
            samplePosTextRight.text = waveformRenderer.selectedSample.ToString();
        }
        else
        {

            samplePosTextLeft.text = (waveformRenderer.selectedNormalized * 100).ToString("f3");
            samplePosTextRight.text = (waveformRenderer.selectedNormalized * 100).ToString("f3");

        }

        if (Input.GetMouseButtonUp(0) && highlight)
        {
            SetVal();
        }


    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            SetVal();
        }
    }

    public void SetVal()
    {
        if (!dragging)
        {
            return;
        }
        else
        {
            dragging = false;
        }
        waveformRenderer.selectedSample = (int)waveformRenderer.SampleNumFromScreenPos(Input.mousePosition, leftBound.position.x, rightBound.position.x);

        string a = editor.currentAttribute.name;
        if (a == "start" || a == "end")
        {
            editor.currentAttribute.SetValue(waveformRenderer.selectedSample.ToString());
        }
        else
        {

            if (editor.currentAttribute.name == "rootNote")
            {
                editor.currentAttribute.SetValue(Mathf.RoundToInt(waveformRenderer.selectedNormalized * 100).ToString());
            }
            else
            {
                editor.currentAttribute.SetValue((waveformRenderer.selectedNormalized * 100).ToString("f3"));
            }



        }
        editor.UpdateParameters();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnPointerEnter(eventData);
    }
}