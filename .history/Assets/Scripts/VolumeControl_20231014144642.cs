using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [SerializeField]
    public void SetVolume(float sliderValue)
    {
        audioMixer.SetFloat(v "MasterVolume", Mathf.Log10(sliderValue) * 20);
    }
}
