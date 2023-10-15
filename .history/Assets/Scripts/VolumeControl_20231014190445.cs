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

    private Sprite maxVolumeImage;

    private Sprite midVolumeImage;

    private Sprite minVolumeImage;

    private Sprite mutedVolumeImage;

    private string volumeIconPathKey = "VolumeIconPath";

    void Start()
    {
        LoadPrefs();
        SetVolumeIconImagePath();
    }

    public void SetVolume(float sliderValue)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(sliderValue) * 20);
        SetVolumeIconImage (sliderValue);
        SavePrefs (sliderValue);
    }

    private void SavePrefs(float volume)
    {
        PlayerPrefs.SetFloat("MasterVolume", volume);

        // Save the volume icon image path
        PlayerPrefs.SetString(volumeIconPathKey, GetVolumeIconImagePath());

        volumeIcon.GetComponent<Image>().sprite = GetVolumeIconSprite();
    }

    private void LoadPrefs()
    {
        var newVolume = PlayerPrefs.GetFloat("MasterVolume");
        audioMixer.SetFloat("MasterVolume", newVolume);
        audioSlider.value = newVolume;

        // Load and set the volume icon image path
        string iconPath = PlayerPrefs.GetString(volumeIconPathKey, "");
        SetVolumeIconImageByPath (iconPath);
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
            volumeIcon.GetComponent<Image>().sprite = mutedVolumeImage;
        }
        else if (sliderValue > 0.0001 && sliderValue < 0.3333f)
        {
            volumeIcon.GetComponent<Image>().sprite = minVolumeImage;
        }
        else if (sliderValue >= 0.3333f && sliderValue < 0.6666f)
        {
            volumeIcon.GetComponent<Image>().sprite = midVolumeImage;
        }
        else if (sliderValue >= 0.6666f)
        {
            volumeIcon.GetComponent<Image>().sprite = maxVolumeImage;
        }
    }

    private string GetVolumeIconImagePath()
    {
        Image imageComponent = volumeIcon.GetComponent<Image>();
        if (imageComponent.sprite == maxVolumeImage)
        {
            return "Volume_Icons/Volume_Max";
        }
        else if (imageComponent.sprite == midVolumeImage)
        {
            return "Volume_Icons/Volume_Mid";
        }
        else if (imageComponent.sprite == minVolumeImage)
        {
            return "Volume_Icons/Volume_Min";
        }
        else
        {
            return "Volume_Icons/Volume_Muted";
        }
    }

    private void SetVolumeIconImageByPath(string imagePath)
    {
        switch (imagePath)
        {
            case "Volume_Icons/Volume_Max":
                volumeIcon.GetComponent<Image>().sprite = maxVolumeImage;
                break;
            case "Volume_Icons/Volume_Mid":
                volumeIcon.GetComponent<Image>().sprite = midVolumeImage;
                break;
            case "Volume_Icons/Volume_Min":
                volumeIcon.GetComponent<Image>().sprite = minVolumeImage;
                break;
            case "Volume_Icons/Volume_Muted":
                volumeIcon.GetComponent<Image>().sprite = mutedVolumeImage;
                break;
        }
    }

    private Sprite GetVolumeIconSprite()
    {
        return volumeIcon.GetComponent<Image>().sprite;
    }
}
