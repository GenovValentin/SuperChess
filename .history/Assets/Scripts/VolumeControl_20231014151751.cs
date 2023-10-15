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

    [SerializeField]
    private GameObject volumeIcon;

    private Image volumeIconImage;

    void Start()
    {
        LoadPrefs();
        SetGameObjects();
    }

    public void SetVolume(float sliderValue)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(sliderValue) * 20);
        SavePrefs (sliderValue);
    }

    private void SavePrefs(float volume)
    {
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    private void LoadPrefs()
    {
        var newVolume = PlayerPrefs.GetFloat("MasterVolume");

        audioMixer.SetFloat("MasterVolume", newVolume);

        audioSlider.value = newVolume;
    }

    private void SetGameObjects()
    {
        volumeIconImage = volumeIcon.GetComponent<Image>();
    }
}
