using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

[CreateAssetMenu(menuName = "Sound/Sound Repository")]
public class SoundRepository : ScriptableObject
{
    [SerializeField] List<SoundClip> sounds;

    public SoundClip GetSoundClip(string name)
    {
        foreach (SoundClip soundClip in sounds)
        {
            if (soundClip.name == name)
            {
                return soundClip;
            }
        }
        return null;
    }
}
