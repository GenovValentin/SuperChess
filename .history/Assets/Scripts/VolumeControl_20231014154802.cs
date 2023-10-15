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

    private Sprite volumeIconImage;

    private Sprite maxVolumeImage;

    private Sprite midVolumeImage;

    private Sprite minVolumeImage;

    private Sprite mutedVolumeImage;

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
        if (sliderValue == 0)
        {
            volumeIconImage.sprite = mutedVolumeImage.sprite;
        }
        else if (sliderValue > 0 && sliderValue < 0.3333)
        {
            volumeIconImage.sprite = minVolumeImage.sprite;
        }
        else if (sliderValue > 0.3333 && sliderValue < 0.6666)
        {
            volumeIconImage.sprite = midVolumeImage.sprite;
        }
        else if (sliderValue > 0.6666)
        {
            volumeIconImage.sprite = maxVolumeImage.sprite;
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
        maxVolumeImage = Resources.Load<Sprite>("Volume_Icons/Volume_Max");
        midVolumeImage = Resources.Load<Sprite>("Volume_Icons/Volume_Mid");
        minVolumeImage = Resources.Load<Sprite>("Volume_Icons/Volume_Min");
        mutedVolumeImage = Resources.Load<Sprite>("Volume_Icons/Volume_Muted");
    }
}
