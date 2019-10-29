using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXMangaer : MonoBehaviour
{
    public AudioSource step;
    public AudioSource woosh;
    public AudioSource slash;
    public AudioSource chest;
    public AudioSource coins;
    public AudioSource collectCoins;
    public AudioSource death;

    private float last;

    public void Death()
    {
        death.Play();
    }

    private void Start()
    {
        last = Time.fixedTime;
    }

    public void Step()
    {
        step.pitch = Random.Range(9, 11) / 10f;
        step.Play();
    }

    public void Woosh()
    {
        woosh.PlayOneShot(woosh.clip);
    }

    public void Slash()
    {
        slash.PlayOneShot(slash.clip);
    }

    public void Chest()
    {
        chest.time = .3f;
        chest.Play();
    }

    public void Coins()
    {
        coins.Play();
    }

    public void CollectCoins()
    {
        if (Time.fixedTime - last > .1f)
        {
            collectCoins.Play();
            last = Time.fixedTime;
        }
    }
}
