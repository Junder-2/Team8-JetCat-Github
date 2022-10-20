using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class UIAudioManager : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip hoverSound;
    [SerializeField] AudioClip pressedSound;

    public void HoverButton()
    {
        audioSource.PlayOneShot(hoverSound);
    }
    public void ClickButton()
    {
        audioSource.PlayOneShot(pressedSound);
    }
}
