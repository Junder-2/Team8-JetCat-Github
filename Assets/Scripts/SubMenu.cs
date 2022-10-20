using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SubMenu : MonoBehaviour
{
    [SerializeField] private GameObject firstSelect;

    private void OnEnable()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        
        eventSystem.SetSelectedGameObject(firstSelect);
    }
}