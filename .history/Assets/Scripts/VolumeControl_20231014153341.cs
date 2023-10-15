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

    private Image maxVolumeImage;

    private Image midVolumeImage;

    private Image minVolumeImage;

    private Image mutedVolumeImage;

    void Start()
    {
        LoadPrefs();
        SetGameObjects();
        SetVolumeIconImagePath();
    }

    public void SetVolume(float sliderValue)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(sliderValue) * 20);
        SavePrefs (sliderValue);
        switch (sliderValue) {
            case var expression when sliderValue = 0:
            volumeIconImage = mutedVolumeImage;
            break;
            case var expression when sliderValue > 0 && sliderValue < 0.3333:
            volumeIconImage = minVolumeImage;
            break;
            case var expression when sliderValue > 0.3333 && sliderValue < 0.6666:
            volumeIconImage = midVolumeImage;
            break;
            case var expression when sliderValue > 0.6666:
            volumeIconImage = maxVolumeImage;
            break;
        }
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

    private void SetVolumeIconImagePath()
    {
        maxVolumeImage = Resources.Load<Image>("Volume_Max");
        midVolumeImage = Resources.Load<Image>("Volume_Mid");
        minVolumeImage = Resources.Load<Image>("Volume_Min");
        mutedVolumeImage = Resources.Load<Image>("Volume_Muted");
    }
}
