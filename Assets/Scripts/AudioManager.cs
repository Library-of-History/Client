using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public event Action<float> OnBgmVolumeChanged;
    public event Action<float> OnSfxVolumeChanged;
    public event Action<float> OnDocentVolumeChanged;
    public event Action<float> OnCharaVolumeChanged;

    private float masterBgmVolume;
    public float MasterBgmVolume
    {
        get => masterBgmVolume;
        set
        {
            if (Mathf.Approximately(masterBgmVolume, value))
            {
                return;
            }

            masterBgmVolume = value;
            OnBgmVolumeChanged?.Invoke(masterBgmVolume);
        }
    }

    private float masterSfxVolume;
    public float MasterSfxVolume
    {
        get => masterSfxVolume;
        set
        {
            if (Mathf.Approximately(masterSfxVolume, value))
            {
                return;
            }

            masterSfxVolume = value;
            OnSfxVolumeChanged?.Invoke(masterSfxVolume);
        }
    }

    private float masterDocentVolume;
    public float MasterDocentVolume
    {
        get => masterDocentVolume;
        set
        {
            if (Mathf.Approximately(masterDocentVolume, value))
            {
                return;
            }

            masterDocentVolume = value;
            OnDocentVolumeChanged?.Invoke(masterDocentVolume);
        }
    }

    private float masterCharaVolume;
    public float MasterCharaVolume
    {
        get => masterCharaVolume;
        set
        {
            if (Mathf.Approximately(masterCharaVolume, value))
            {
                return;
            }

            masterCharaVolume = value;
            OnCharaVolumeChanged?.Invoke(masterCharaVolume);
        }
    }
}
