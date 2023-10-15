using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    public AudioMixer audioMixer; // Reference to the Audio Mixer

    public Slider volumeSlider; // Reference to the Slider UI component

    public string volumeParameterName = "Volume"; // The name of the exposed parameter in the Audio Mixer

    private void Start()
    {
        // Set the initial value of the Slider to match the Audio Mixer parameter
        float volume = PlayerPrefs.GetFloat("Volume", 1.0f); // You can also store the volume setting in PlayerPrefs
        volumeSlider.value = volume;
        audioMixer.SetFloat(volumeParameterName, Mathf.Log10(volume) * 20);
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat(volumeParameterName, Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("Volume", volume); // Save the volume setting
    }
}
