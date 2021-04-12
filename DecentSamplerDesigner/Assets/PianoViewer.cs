using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PianoViewer : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IScrollHandler
{
    public float startNote;
    public float zoom = 1;
    private Vector2 lastMousePosition;

    public RectTransform viewerRect;

    float lastStartNote;

    bool highlight;

    public GameObject notePrefab;
    public Transform noteContainer;
    public Color noteWhite;
    public Color noteBlack;

    List<int> blackNotes = new List<int>{1, 3, 6, 8, 10};

    public List<RectTransform> noteObjects = new List<RectTransform>();
    // Start is called before the first frame update
    void Start()
    {
        int num = 0;
        int oct = 0;
        for (int i = 0; i < 127; i++)
        {
            GameObject note = Instantiate(notePrefab);
            note.transform.parent = noteContainer;
            note.transform.localScale = Vector3.one;

            Image im = note.GetComponent<Image>();
            if (blackNotes.Contains(num))
            {
                im.color = noteBlack;
            }
            else
            {
                im.color = noteWhite;
            }

            if (num == 0)
            {
                Text t = note.GetComponentInChildren<Text>();
                t.text = "C" + oct;
                t.enabled = true;
            }

            num++;
            if (num > 11)
            {
                num = 0;
                oct += 1;
            }
            noteObjects.Add(note.GetComponent<RectTransform>());


        }

        lastStartNote = -45;
        startNote = -45;

        //foreach (RectTransform item in viewerRect.GetComponentsInChildren<RectTransform>())
        //{
        //    noteObjects.Add(item);
        //}
    }

    // Update is called once per frame
    void Update()
    {
        //viewerRect.localScale = new Vector2(zoom, 1);

        foreach (RectTransform item in noteObjects)
        {
            item.sizeDelta = new Vector2(zoom, item.sizeDelta.y);
        }

    }

    private void LateUpdate()
    {
        //viewerRect.position = new Vector3(Mathf.Clamp(viewerRect.position.x, -((noteObjects.Capacity * zoom) + zoom),0), viewerRect.position.y, viewerRect.position.z);
        viewerRect.position = new Vector3(Mathf.Clamp(zoom * startNote, -((noteObjects.Capacity * zoom) + zoom),0), viewerRect.position.y, viewerRect.position.z);
        viewerRect.position += new Vector3(viewerRect.transform.parent.position.x, 0, 0);
        //viewerRect.position = new Vector3(Mathf.Clamp(viewerRect.position.x, 0, Mathf.Infinity), viewerRect.position.y, viewerRect.position.z);

    }

    public float GetPosFromNote(int note)
    {
        //return noteObjects[note].transform.position.x;
        return (note * zoom) + viewerRect.position.x;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Middle)
        {
            return;
        }
        Vector2 diff = eventData.position - lastMousePosition;
        startNote = lastStartNote + (diff.x * (1 / zoom));

        startNote = Mathf.Clamp(startNote, -128, 0);
        //viewerRect.position = last + new Vector3(diff.x, 0, 0);

        //viewerRect.position = new Vector3(Mathf.Clamp(viewerRect.position.x,0, Mathf.Infinity), viewerRect.position.y, viewerRect.position.z);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Middle)
        {
            return;
        }
        lastStartNote = startNote;
        lastMousePosition = eventData.position;
        // throw new System.NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlight = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlight = false;
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (highlight)
        {
            if (eventData.scrollDelta.y < -0.01f)
            {
                float z = zoom;
                zoom *= 0.7f;
                zoom = Mathf.Clamp(zoom, 10, 100);

                if (z != zoom)
                {
                    startNote += (1 / zoom) * 100;

                }
            }

            if (eventData.scrollDelta.y > 0.01f)
            {
                float z = zoom;
                zoom *= 1.3f;
                zoom = Mathf.Clamp(zoom, 10, 100);

                if (z != zoom)
                {
                    startNote -= (1 / zoom) * 100;

                }
            }
        }


    }
}
