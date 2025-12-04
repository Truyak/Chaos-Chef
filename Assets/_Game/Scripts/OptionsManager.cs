using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    public AudioMixer audioMixer;

    [Header("Slider References")]
    public Slider mainVolumeSlider;
    public Slider sfxVolumeSlider;

    private const string MAIN_VOLUME_PARAM = "MainVolume";
    private const string SFX_VOLUME_PARAM = "SFXVolume";

    private void Start()
    {
        mainVolumeSlider.value = PlayerPrefs.GetFloat("MainVolume", 0.8f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.8f);

        mainVolumeSlider.onValueChanged.AddListener(SetMainVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMainVolume(float value)
    {
        float dB = value > 0.001f ? Mathf.Log10(value) * 20f : -80f;
        audioMixer.SetFloat(MAIN_VOLUME_PARAM, dB);
        PlayerPrefs.SetFloat("MainVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        float dB = value > 0.001f ? Mathf.Log10(value) * 20f : -80f;
        audioMixer.SetFloat(SFX_VOLUME_PARAM, dB);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }
}