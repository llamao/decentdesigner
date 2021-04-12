using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WavformRenderCopier : MonoBehaviour
{
    public WavTexture.WaveformRenderer s;
    public WavTexture.WaveformRenderer t;

    public float startOffset;
    private void LateUpdate()
    {
        t.startTime = s.startTime + ((startOffset/ 200) * s.zoom);
        t.zoom = s.zoom;
        t.selectedSample = s.selectedSample;
        t.xScale = s.xScale;
        t.yScale = s.yScale;
        //t.yScale = s.yScale * yScaleOffset;
    }
}
