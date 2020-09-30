using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
[CreateAssetMenu(menuName = "Sound/SoundClip")]
public class SoundClip : ScriptableObject
{
    public AudioClip file;
    [Range(0,1)]
    public float volume;
    public bool shouldLoop;
}
