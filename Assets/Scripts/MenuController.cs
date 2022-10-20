using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MenuController : MonoBehaviour
{
    [SerializeField] private UnityEvent OnStart;

    private void Start()
    {
        OnStart?.Invoke();
    }
}
