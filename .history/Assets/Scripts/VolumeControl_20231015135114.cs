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

    private string volumeKey = "MasterVolume";

    private string isVolumeMutedKey = "IsVolumeMutedState";

    private string previousVolumeKey = "PreviousVolume";

    private float previousVolume;

    private const float mutedVolumeValue = 0.0001f;

    private const float minVolumeValue = 0.3333f;

    private const float midVolumeValue = 0.6666f;

    private bool isVolumeMuted = false;

    void Start()
    {
        SetVolumeIconImagePath();
        LoadPrefs();
    }

    public void SetVolume(float sliderValue)
    {
        if (sliderValue > mutedVolumeValue)
        {
            previousVolume = sliderValue;
        }
        audioSlider.value = sliderValue;
        audioMixer.SetFloat(volumeKey, Mathf.Log10(sliderValue) * 20);
        SetVolumeIconImage (sliderValue);
        SavePrefs (sliderValue);
    }

    private void SavePrefs(float volume)
    {
        PlayerPrefs.SetFloat (volumeKey, volume);
        if (previousVolume != 0)
        {
            PlayerPrefs.SetFloat (previousVolumeKey, previousVolume);
        }

        int isVolumeMutedState = GetIsVolumeMutedState();
        PlayerPrefs.SetInt (isVolumeMutedKey, isVolumeMutedState);

        int volumeIconState = GetVolumeIconState();
        PlayerPrefs.SetInt (volumeIconKey, volumeIconState);
        volumeIcon.GetComponent<Image>().sprite = volumeIconImage;
    }

    private void LoadPrefs()
    {
        var newVolume = PlayerPrefs.GetFloat(volumeKey);
        audioMixer.SetFloat (volumeKey, newVolume);
        audioSlider.value = newVolume;

        previousVolume = PlayerPrefs.GetFloat(previousVolumeKey);

        int isVolumeMutedState = PlayerPrefs.GetInt(isVolumeMutedKey, 0);
        SetIsVolumeMutedState (isVolumeMutedState);

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
        if (sliderValue <= mutedVolumeValue)
        {
            volumeIconImage = mutedVolumeImage;
        }
        else if (sliderValue > mutedVolumeValue && sliderValue < minVolumeValue)
        {
            volumeIconImage = minVolumeImage;
        }
        else if (sliderValue >= minVolumeValue && sliderValue < midVolumeValue)
        {
            volumeIconImage = midVolumeImage;
        }
        else if (sliderValue >= midVolumeValue)
        {
            volumeIconImage = maxVolumeImage;
        }
    }

    private void SetIsVolumeMutedState(int state)
    {
        if (state == 0)
        {
            isVolumeMuted = true;
            return;
        }
        isVolumeMuted = false;
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

    private int GetIsVolumeMutedState()
    {
        if (isVolumeMuted) return 0;
        return 1;
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
        if (isVolumeMuted)
        {
            SetVolume (previousVolume);
        }
        else
        {
            SetVolume (mutedVolumeValue);
        }
        isVolumeMuted = !isVolumeMuted;
    }
}
