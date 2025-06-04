using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource BGM;
    [SerializeField] private float bgmVol = 0.5f;

    public bool enableBGM = true;
    public bool enableSFX = true;

    private void Awake()
    {
        BGM.ignoreListenerVolume = true;
    }

    public void ToogleBgm()
    {
        enableBGM = !enableBGM;
        EnableBGM();
    }

    void EnableBGM()
    {
        BGM.volume = enableBGM ? bgmVol : 0;
    }

    public void ToogleSfx()
    {
        enableSFX = !enableSFX;
        EnableSFX();
    }

    void EnableSFX()
    {
        AudioListener.volume = enableSFX ? 1 : 0;
    }
}
