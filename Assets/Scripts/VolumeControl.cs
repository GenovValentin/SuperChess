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

    private string volumeKey = "MasterVolume";

    private float previousVolume;

    private const float MUTED_VOLUME_VALUE = 0.0001f;

    private const float MIN_VOLUME_VALUE = 0.3333f;

    private const float MID_VOLUME_VALUE = 0.6666f;

    private const float DEFAULT_VOLUME_VALUE = 0.5f;

    private bool isVolumeMuted = false;

    private bool isUserSignedIn = false;

    AccountHandler accountHandler;

    private void Start()
    {
        accountHandler = AccountHandler.GetInstance();
        RegisterEvents();

        SetVolumeIconImagePath();
        SetVolume (DEFAULT_VOLUME_VALUE);
    }

    public void SetVolume(float sliderValue)
    {
        isVolumeMuted = sliderValue <= MUTED_VOLUME_VALUE;

        if (!isVolumeMuted)
        {
            previousVolume = sliderValue;
        }
        audioSlider.value = sliderValue;
        audioMixer.SetFloat(volumeKey, Mathf.Log10(sliderValue) * 20);
        SetVolumeIconImage (sliderValue);
        volumeIcon.GetComponent<Image>().sprite = volumeIconImage;

        if (isUserSignedIn)
        {
            accountHandler.SetVolume (sliderValue);
        }
    }

    private Settings GetVolume()
    {
        Settings userSettings = accountHandler.GetUserSettings();
        return userSettings;
    }

    private void RegisterEvents()
    {
        EventBus.SIGN_IN += HandleSignIn;
        EventBus.SIGN_OUT += HandleSignOut;
    }

    private void UnregisterEvents()
    {
        EventBus.SIGN_IN -= HandleSignIn;
        EventBus.SIGN_OUT -= HandleSignOut;
    }

    private void HandleSignIn()
    {
        isUserSignedIn = true;
        SetVolume(GetVolume().volume);
    }

    private void HandleSignOut()
    {
        isUserSignedIn = false;
        SetVolume (DEFAULT_VOLUME_VALUE);
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
        if (sliderValue <= MUTED_VOLUME_VALUE)
        {
            volumeIconImage = mutedVolumeImage;
        }
        else if (
            sliderValue > MUTED_VOLUME_VALUE && sliderValue < MIN_VOLUME_VALUE
        )
        {
            volumeIconImage = minVolumeImage;
        }
        else if (
            sliderValue >= MIN_VOLUME_VALUE && sliderValue < MID_VOLUME_VALUE
        )
        {
            volumeIconImage = midVolumeImage;
        }
        else if (sliderValue >= MID_VOLUME_VALUE)
        {
            volumeIconImage = maxVolumeImage;
        }
    }

    public void OnVolumeIconButton()
    {
        if (isVolumeMuted)
        {
            SetVolume (previousVolume);
        }
        else
        {
            SetVolume (MUTED_VOLUME_VALUE);
        }
    }
}
