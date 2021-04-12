using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WavTextureBaker : MonoBehaviour
{
    public WavTexture.WavTexture wavTexture;
    public AudioClip clip;

    public void Bake(AudioClip clip)
    {
        wavTexture.Initialize(clip, WavTexture.WavTexture.BitRate.High);
    }

}
