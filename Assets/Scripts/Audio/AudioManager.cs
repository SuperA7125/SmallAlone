using System.Collections;
using UnityEngine;

/// <summary>
/// Singleton audio manager. Reads all clips and volume settings from
/// a single AudioData ScriptableObject asset — drag that asset into
/// the Data field in the Inspector.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Tooltip("The single AudioData asset that holds every clip and volume setting.")]
    [SerializeField] private AudioData data;
    public AudioData Data => data;

    [SerializeField] private int sfxPoolSize = 8;

    private AudioSource[] sfxPool;
    private int sfxPoolIndex;
    private AudioSource musicSourceA;
    private AudioSource musicSourceB;
    private bool usingSourceA = true;

    private AudioSource ActiveMusicSource => usingSourceA ? musicSourceA : musicSourceB;
    private AudioSource InactiveMusicSource => usingSourceA ? musicSourceB : musicSourceA;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxPool = new AudioSource[sfxPoolSize];
        for (int i = 0; i < sfxPoolSize; i++)
        {
            sfxPool[i] = gameObject.AddComponent<AudioSource>();
            sfxPool[i].playOnAwake = false;
        }

        musicSourceA = gameObject.AddComponent<AudioSource>();
        musicSourceA.loop = true;
        musicSourceA.playOnAwake = false;

        musicSourceB = gameObject.AddComponent<AudioSource>();
        musicSourceB.loop = true;
        musicSourceB.playOnAwake = false;
        musicSourceB.volume = 0f;

        ApplyVolumes();
    }

    private void ApplyVolumes()
    {
        if (data == null) return;
        foreach (var s in sfxPool)
            s.volume = data.SFXVolume * data.MasterVolume;
        musicSourceA.volume = data.MusicVolume * data.MasterVolume;
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f, float pitchVariation = 0f)
    {
        if (clip == null || data == null) return;
        AudioSource source = sfxPool[sfxPoolIndex];
        sfxPoolIndex = (sfxPoolIndex + 1) % sfxPoolSize;
        source.clip = clip;
        source.volume = data.SFXVolume * data.MasterVolume * volumeScale;
        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.Play();
    }

    public void PlaySFXRandom(AudioClip[] clips, float volumeScale = 1f, float pitchVariation = 0f)
    {
        if (clips == null || clips.Length == 0) return;
        PlaySFX(clips[Random.Range(0, clips.Length)], volumeScale, pitchVariation);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || data == null) return;
        if (ActiveMusicSource.clip == clip && ActiveMusicSource.isPlaying) return;
        StopAllCoroutines();
        StartCoroutine(CrossfadeMusic(clip));
    }

    public void StopMusic() => StartCoroutine(FadeOutMusic());

    private IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        float duration = data != null ? data.CrossfadeDuration : 1f;
        float targetVolume = data != null ? data.MusicVolume * data.MasterVolume : 0.5f;

        AudioSource fadeOut = ActiveMusicSource;
        AudioSource fadeIn = InactiveMusicSource;
        fadeIn.clip = newClip;
        fadeIn.volume = 0f;
        fadeIn.Play();
        usingSourceA = !usingSourceA;

        float elapsed = 0f;
        float startVol = fadeOut.volume;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            fadeOut.volume = Mathf.Lerp(startVol, 0f, t);
            fadeIn.volume = Mathf.Lerp(0f, targetVolume, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        fadeOut.volume = 0f;
        fadeOut.Stop();
        fadeIn.volume = targetVolume;
    }

    private IEnumerator FadeOutMusic()
    {
        float duration = data != null ? data.CrossfadeDuration : 1f;
        AudioSource active = ActiveMusicSource;
        float startVol = active.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            active.volume = Mathf.Lerp(startVol, 0f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        active.Stop();
        active.volume = 0f;
    }
}