using System;
using UnityEngine;
using Array = System.Array;

public class AudioManager : MonoBehaviour
{
    # region Singleton

    public static AudioManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    #endregion

    public Sound[] sounds;
    private static AudioSource _audio;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        _audio = GetComponent<AudioSource>();
    }

    public static void Play(string soundname, Sound[] sounds = default)
    {
        sounds = sounds ?? Instance.sounds;
        var sound = Array.Find(sounds, s => string.Equals(s.name, soundname, StringComparison.CurrentCultureIgnoreCase));
        if (sound == null)
        {
            Debug.LogWarning("Sound: " + soundname + " not found!");
            return;
        }

        _audio.volume = sound.volume * .2f;
        _audio.pitch = sound.pitch;

        _audio.PlayOneShot(sound.clip);
    }
}

[System.Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;

    [Range(0f, 2f)]
    public float volume = 1;
    [Range(0.1f, 3f)]
    public float pitch = 1;
}