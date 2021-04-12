using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PIanoViewerBlock : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    public PianoViewer viewer;
    public SampleTagObject tagOwner;

    public Text nameText;

    RectTransform rec;

    //public Image boxImage;
    public Text editTextTop;
    public Text editTextBottom;
    public Image topEdit;
    public Image bottomEdit;
    public Image edgeMask;
    public Image[] edges;
    public Image[] corners;
    public Color defaultColour;
    public Color hoverColor;
    public Color highlightColor;
    public Color selectedColor;
    public int lowNote;
    public int hiNote;
    public int lowVel;
    public int hiVel;

    int lowNotePrev;
    int hiNotePrev;
    int lowVelPrev;
    int hiVelPrev;

    public Vector2 mousePos;
    public Vector2 mDiff;
    public Vector2 lastMousePos;

    byte selectedPoints;

    bool highlight;
    bool dragging;
    public string currentEdge;

    public bool selected;

    byte emptyByte = new byte();

    private void Start()
    {
        rec = GetComponent<RectTransform>();
    }
    private void LateUpdate()
    {
        hiNote = Mathf.Clamp(hiNote, lowNote, 127);
        lowNote = Mathf.Clamp(lowNote, 0, hiNote);
        hiVel = Mathf.Clamp(hiVel, 4, 127);
        lowVel = Mathf.Clamp(lowVel, 0, hiVel - 4);
        transform.position = new Vector3(viewer.GetPosFromNote(lowNote), transform.position.y);

        transform.localPosition = new Vector3(transform.localPosition.x, -((transform.parent.GetComponent<RectTransform>().rect.height) * Mathf.InverseLerp(127, 0, hiVel)));

        rec.sizeDelta = new Vector2(viewer.zoom * (( hiNote - lowNote ) + 1), transform.localPosition.y + (transform.parent.GetComponent<RectTransform>().rect.height * Mathf.InverseLerp(127,0,lowVel)));
    }

    private void Update()
    {
        edgeMask.color = Color.white;

        if (!selected)
        {
            Color col = Color.clear;
            if (highlight)
            {
                col = hoverColor;
                if (Input.GetMouseButtonDown(0) && (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftAlt)))
                {
                    transform.SetAsLastSibling();
                    tagOwner.Select();
                }
            }

            foreach (Image item in edges)
            {
                item.color = col;
            }
            return;
        }
        else
        {
            edgeMask.color = selectedColor;
        }



        int tolerance = 8;
        mousePos = Input.mousePosition;

        mDiff = (mousePos - lastMousePos);

        foreach (Image item in edges)
        {
            item.color = defaultColour;
        }

        foreach (Image item in corners)
        {
            item.color = Color.clear;
        }

        // left
        // right
        // top
        // bottom
        bool[] selectedEdges = new bool[4];

        if (highlight && !dragging)
        {
            Vector3[] v = new Vector3[4];

            // 0: bottom left
            // 1: top left
            // 2: top right
            // 3: bottom right
            rec.GetWorldCorners(v);

            //left
            if (mousePos.x > (v[0].x - tolerance) && mousePos.x < (v[0].x + tolerance))
            {
            selectedEdges[0] = true;
            }
            else
            {
                selectedEdges[0] = false;
            }
            //right
            if (mousePos.x > (v[2].x - tolerance) && mousePos.x < (v[2].x + tolerance))
            {
                selectedEdges[1] = true;
            }
            else
            {
                selectedEdges[1] = false;
            }
            //top
            if (mousePos.y > (v[2].y - tolerance) && mousePos.y < (v[2].y + tolerance))
            {
                selectedEdges[2] = true;
            }
            else
            {
                selectedEdges[2] = false;
            }
            //bottom
            if (mousePos.y > (v[3].y - tolerance) && mousePos.y < (v[3].y + tolerance))
            {
                selectedEdges[3] = true;
            }
            else
            {
                selectedEdges[3] = false;
            }

            //center
            if ((mousePos.x > v[1].x + tolerance &&
                 mousePos.x < v[2].x - tolerance &&
                (mousePos.y < v[2].y - tolerance &&
                 mousePos.y > v[3].y + tolerance)))
            //(mousePos.y > (v[2].y + v[3].y) * 0.1f &&
            // mousePos.y < (v[2].y + v[3].y) * 0.9f)))
            {
                for (int i = 0; i < selectedEdges.Length; i++)
                {
                    selectedEdges[i] = true;
                }
            }



            //boxImage.color = selectedColor;
        }
        else
        {
            //boxImage.color = defaultColour;
        }

        if (!dragging && !Input.GetMouseButton(0)) selectedPoints = BoolsToByte(selectedEdges);

        if (!dragging && selectedPoints != emptyByte)
        {
            lastMousePos = mousePos;
            lowNotePrev = lowNote;
            hiNotePrev = hiNote;
            lowVelPrev = lowVel;
            hiVelPrev = hiVel;
        }

        float yMul = 0.3f;
        float xMul = (1 / viewer.zoom);

        switch (selectedPoints)
        {
            case 1:
                currentEdge = "bottom";
                edges[3].color = highlightColor;
                if (Input.GetMouseButton(0)) { lowVel = lowVelPrev + (int)(mDiff.y * yMul); dragging = true; editTextBottom.text = (Mathf.Clamp(lowVel,0,hiVel-4)).ToString(); bottomEdit.enabled = true; }
                else FinishEdit();
                break;
            case 2:
                currentEdge = "top";
                edges[2].color = highlightColor;
                if (Input.GetMouseButton(0)) { hiVel = hiVelPrev + (int)(mDiff.y * yMul); dragging = true; editTextTop.text = (Mathf.Clamp(hiVel, 4, 127)).ToString(); topEdit.enabled = true; }
                else FinishEdit();
                break;
            case 4:
                currentEdge = "right";
                edges[1].color = highlightColor;
                if (Input.GetMouseButton(0)) { hiNote = hiNotePrev + (int)(mDiff.x * xMul); dragging = true; }
                else FinishEdit();
                break;
            case 5:
                currentEdge = "bottom right";
                corners[3].color = highlightColor;
                if (Input.GetMouseButton(0))
                {
                    hiNote = hiNotePrev + (int)(mDiff.x * xMul);
                    lowVel = lowVelPrev + (int)(mDiff.y * yMul);
                    dragging = true;
                }
                else FinishEdit();
                break;
            case 6:
                currentEdge = "top right";
                corners[2].color = highlightColor;
                if (Input.GetMouseButton(0))
                {
                    hiNote = hiNotePrev + (int)(mDiff.x * xMul);
                    hiVel = hiVelPrev + (int)(mDiff.y * yMul);
                    dragging = true;
                }
                else FinishEdit();
                break;
            case 8:
                currentEdge = "left";
                if (Input.GetMouseButton(0)) {lowNote = lowNotePrev + (int)(mDiff.x * xMul); dragging = true;}
                else FinishEdit();
                edges[0].color = highlightColor;
                break;
            case 9:
                currentEdge = "bottom left";
                corners[0].color = highlightColor;
                if (Input.GetMouseButton(0))
                {
                    lowNote = lowNotePrev + (int)(mDiff.x * xMul);
                    lowVel = lowVelPrev + (int)(mDiff.y * yMul);
                    dragging = true;
                }
                else FinishEdit();
                break;
            case 10:
                currentEdge = "top left";
                corners[1].color = highlightColor;
                if (Input.GetMouseButton(0))
                {
                    lowNote = lowNotePrev + (int)(mDiff.x * xMul);
                    hiVel = hiVelPrev + (int)(mDiff.y * yMul);
                    dragging = true;
                }
                else FinishEdit();
                break;
            case 15:

                edges[4].color = highlightColor;
                if (Input.GetMouseButtonDown(0) && highlight)
                {
                    dragging = true;
                }
                if (Input.GetMouseButton(0) && dragging)
                {
                    hiNote = hiNotePrev + (int)(mDiff.x * xMul);
                    lowNote = lowNotePrev + (int)(mDiff.x * xMul);
                    // dont move velocity if you hold shift
                    if (!Input.GetKey(KeyCode.LeftShift))
                    {
                        lowVel = lowVelPrev + (int)(mDiff.y * yMul);
                        hiVel = hiVelPrev + (int)(mDiff.y * yMul);
                    }
                    else
                    {
                        lowVel = lowVelPrev;
                        hiVel = hiVelPrev;
                    }

                    editTextBottom.text = (Mathf.Clamp(lowVel, 0, hiVel - 4)).ToString();
                    editTextTop.text = (Mathf.Clamp(hiVel, 4, 127)).ToString();

                    topEdit.enabled = true;
                    bottomEdit.enabled = true;
                }

                else
                {
                    FinishEdit();
                }

                currentEdge = "center";
                break;
            default:
                currentEdge = "none";

                break;
        }

    }

    void FinishEdit()
    {
        if (dragging)
        {
            dragging = false;
            transform.SetAsLastSibling();
            tagOwner.Select();
            tagOwner.elementReference.GetAttribute("loNote").value = lowNote.ToString();
            tagOwner.elementReference.GetAttribute("hiNote").value = hiNote.ToString();
            tagOwner.elementReference.GetAttribute("loVel").value = lowVel.ToString();
            tagOwner.elementReference.GetAttribute("hiVel").value = hiVel.ToString();

            editTextTop.text = "";
            editTextBottom.text = "";

            topEdit.enabled = false;
            bottomEdit.enabled = false;

            Editor.current.UpdateParameters();

        }
    }

    public byte BoolsToByte(IEnumerable<bool> bools)
    {
        byte result = 0;
        foreach (bool value in bools)
        {
            result *= 2;

            if (value)
                result += 1;
        }

        return result;
    }

    public byte BoolsToByte(params bool[] bools)
    {
        return BoolsToByte(bools.AsEnumerable());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlight = true;
        //throw new System.NotImplementedException();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlight = false;
        // throw new System.NotImplementedException();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            viewer.OnDrag(eventData);
        }
        //throw new System.NotImplementedException();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            viewer.OnBeginDrag(eventData);
        }
        //throw new System.NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            viewer.OnEndDrag(eventData);
        }
        //throw new System.NotImplementedException();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }
}
