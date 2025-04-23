using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickDetectorDebug : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = Input.mousePosition;

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            if (results.Count > 0)
            {
                var clickedObject = results[0].gameObject;
                Debug.Log("Было нажатие на объект: " + clickedObject.name, clickedObject);
            }
        }
    }
}
