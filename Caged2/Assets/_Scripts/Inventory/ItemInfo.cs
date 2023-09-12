using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemInfo : NetworkBehaviour
{
    public bool isLocked;
    public readonly bool isValueable;
    public readonly int ValueableWorthMin;
    public readonly int ValueableWorthMax;
    public Sprite sprite;
    public AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;
    //private void OnCollisionEnter(Collision collision){
      //  if (isLocked || isValueable) {return;}
        //int clipNum = Random.Range(0, audioClips.Length);
    //    audioSource.clip = audioClips[clipNum];
    //    audioSource.Play();
    //}
}
