using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [SerializeField]
    private AudioMixer audioMixer;

    [SerializeField]
    private Slider audioSlider;

    void Start()
    {
        LoadPrefs();
    }

    public void SetVolume(float sliderValue)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(sliderValue) * 20);
    }

    private void SavePrefs(float sliderValue)
    {
        PlayerPrefs.SetFloat("MasterVolume", sliderValue);
    }

    private void LoadPrefs()
    {
        var newVolume = PlayerPrefs.GetFloat("MasterVolume");

        audioMixer.SetFloat("MasterVolume", newVolume);
    }
}
