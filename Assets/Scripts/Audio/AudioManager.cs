
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instanse;
    private void Awake()
    {
        Instanse = this;
    }

    [Header("---Audio Source---")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource SFXSource;
    [SerializeField] private AudioMixer audioMixer;

    [Header("---Audio Clip---")]
    public AudioClip background;
    public AudioClip klick;
    public AudioClip drop_coin;
    public AudioClip deploy_item;
    public AudioClip pick_up_coin;
    public AudioClip marge;
    public AudioClip feed_dino;

    // ================= MUSIC =================

    public void StartMusic()
    {
        if (background == null || audioSource == null) return;

        audioSource.clip = background;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void PauseMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Pause();
    }

    public void ResumeMusic()
    {
        if (audioSource != null)
            audioSource.UnPause();
    }

    public void StopMusic()
    {
        if (audioSource != null)
            audioSource.Stop();
    }

    // ================= VOLUME =================

    public void SetMusicVolume(float value)
    {
        if (audioMixer == null) return;

        audioMixer.SetFloat("MusicVolume", GetVolumeDb(value));
    }

    public void SetSFXVolume(float value)
    {
        if (audioMixer == null) return;

        audioMixer.SetFloat("SFXVolume", GetVolumeDb(value));
    }

    private float GetVolumeDb(float value)
    {
        float normalizedValue = Mathf.Clamp(value, 0f, 100f) / 100f;
        return Mathf.Log10(Mathf.Clamp(normalizedValue, 0.0001f, 1f)) * 20f;
    }

    // ================= SFX =================

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || SFXSource == null) return;

        SFXSource.pitch = 1f;
        SFXSource.PlayOneShot(clip);
    }

    public void PlayClick()
    {
        PlaySFX(klick);
    }

    public void PlayDeployItem()
    {
        PlaySFX(deploy_item);
    }

    public void PlayDropCoin()
    {
        PlaySFX(drop_coin);
    }

    public void PlayPickUpCoin()
    {
        PlaySFX(pick_up_coin);
    }

    public void PlayMerge()
    {
        PlaySFX(marge);
    }

    public void PlayFeedDino()
    {
        PlaySFX(feed_dino);
    }

}
