using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [SerializeField]
    public private AudioMixer audioMixer;

    public Slider volumeSlider;

    public string volumeParameterName = "MasterVolume";

    private void Start()
    {
        float volume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        volumeSlider.value = volume;
        audioMixer.SetFloat(volumeParameterName, Mathf.Log10(volume) * 20);
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat(volumeParameterName, Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }
}
