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

    private string volumeIconKey = "VolumeIconState";

    private float previousVolume;

    void Start()
    {
        SetVolumeIconImagePath();
        LoadPrefs();
    }

    public void SetVolume(float sliderValue)
    {
        if (sliderValue > 0.0001f)
        {
            previousVolume = sliderValue;
        }
        audioSlider.value = sliderValue;
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(sliderValue) * 20);
        Debug.Log("sliderValue " + sliderValue);
        SetVolumeIconImage (sliderValue);
        SavePrefs (sliderValue);
    }

    private void SavePrefs(float volume)
    {
        PlayerPrefs.SetFloat("MasterVolume", volume);

        int volumeIconState = GetVolumeIconState();
        PlayerPrefs.SetInt (volumeIconKey, volumeIconState);
        volumeIcon.GetComponent<Image>().sprite = volumeIconImage;
    }

    private void LoadPrefs()
    {
        var newVolume = PlayerPrefs.GetFloat("MasterVolume");
        audioMixer.SetFloat("MasterVolume", newVolume);
        audioSlider.value = newVolume;

        int volumeIconState = PlayerPrefs.GetInt(volumeIconKey, 0);
        SetVolumeIconByState (volumeIconState);
    }

    private void SetVolumeIconImagePath()
    {
        maxVolumeImage = Resources.Load<Sprite>("Volume_Icons/Volume_Max");
        midVolumeImage = Resources.Load<Sprite>("Volume_Icons/Volume_Mid");
        minVolumeImage = Resources.Load<Sprite>("Volume_Icons/Volume_Min");
        mutedVolumeImage = Resources.Load<Sprite>("Volume_Icons/Volume_Muted");
    }

    private void SetVolumeIconImage(float sliderValue)
    {
        if (sliderValue <= 0.0001)
        {
            volumeIconImage = mutedVolumeImage;
        }
        else if (sliderValue > 0.0001 && sliderValue < 0.3333f)
        {
            volumeIconImage = minVolumeImage;
        }
        else if (sliderValue >= 0.3333f && sliderValue < 0.6666f)
        {
            volumeIconImage = midVolumeImage;
        }
        else if (sliderValue >= 0.6666f)
        {
            volumeIconImage = maxVolumeImage;
        }
    }

    private void SetVolumeIconByState(int state)
    {
        switch (state)
        {
            case 1:
                volumeIconImage = minVolumeImage;
                break;
            case 2:
                volumeIconImage = midVolumeImage;
                break;
            case 3:
                volumeIconImage = maxVolumeImage;
                break;
            case 0:
                volumeIconImage = mutedVolumeImage;
                break;
        }
    }

    private int GetVolumeIconState()
    {
        if (volumeIconImage == minVolumeImage) return 1;
        if (volumeIconImage == midVolumeImage) return 2;
        if (volumeIconImage == maxVolumeImage) return 3;
        return 0; // Default for mutedVolumeImage
    }

    public void OnVolumeIconButton()
    {
        SetVolume(0.0001f);
    }
}
