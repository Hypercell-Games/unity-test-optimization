using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIClickDetector : MonoBehaviour
{
    private EventSystem eventSystem;
    private PointerEventData pointerEventData;
    private GraphicRaycaster raycaster;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            raycaster = FindObjectOfType<GraphicRaycaster>();
            eventSystem = FindObjectOfType<EventSystem>();

            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;

            var results = new List<RaycastResult>();
            raycaster.Raycast(pointerEventData, results);

            foreach (var result in results)
            {
                Debug.Log("Hit " + result.gameObject.name, result.gameObject);
            }
        }
    }
}
