using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RawImage))]
public class AudioWaveFormVisualizer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public AudioClip clip;

    public int width = 500; // texture width 
    public int height = 100; // texture height 
    public Color backgroundColor = Color.black;
    public Color waveformColor = Color.green;
    public int size = 2048; // size of sound segment displayed in texture
    public float scrollSpeed = 1000;
    public int offset = 0;
    [Range(1,10)]
    public int inc = 1;

    private Color[] blank; // blank image array 
    private Texture2D texture;
    private float[] samples; // audio samples array

    public bool highlight;
    private Vector2 lastMousePosition;

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlight = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        highlight = false;
    }

    private void Update()
    {
        if (highlight)
        {
            size += (int)(-Input.mouseScrollDelta.y * scrollSpeed);
            //if (Input.GetMouseButton(2))
            //{
            //    offset += (int)(-Input.GetAxis("Mouse X") * 100);
            //    offset = Mathf.Clamp(offset, 0, (clip.samples * clip.channels - 1));
            //}
        }
    }

    IEnumerator Start()
    {

        // create the samples array 
        samples = new float[size];

        // create the texture and assign to the guiTexture: 
        texture = new Texture2D(width, height);

        GetComponent<RawImage>().texture = texture;

        // create a 'blank screen' image 
        blank = new Color[width * height];

        for (int i = 0; i < blank.Length; i++)
        {
            blank[i] = backgroundColor;
        }

        // refresh the display each 100mS 
        while (true)
        {
            GetCurWave();
            yield return new WaitForSeconds(0.04f);
        }
    }

    public void GetCurWave()
    {
        // clear the texture 
        texture.SetPixels(blank, 0);

        // get samples from channel 0 (left) 
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, offset);
        //GetComponent<AudioSource>().GetOutputData(samples, 0);

        size = Mathf.Clamp(size, 30, samples.Length-1);
        int prev = 0;
        // draw the waveform 
        //for (int i = 0; i < size; i++)
        //for (int i = 0; i < size; i = Mathf.Clamp(i + size / width, prev + 1, size))
        for (int i = 0; i < size; i+= inc)
        {
            texture.SetPixel((int)(width * i / size), (int)(height * (samples[i] + 1f) / 2f), waveformColor);
            prev = i;
        } // upload to the graphics card 

        texture.Apply();
    }
}