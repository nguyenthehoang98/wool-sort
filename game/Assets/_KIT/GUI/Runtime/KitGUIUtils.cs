using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _KIT
{
    public static class KitGUIUtils
    {
        private static LayerMask uiMask = LayerMask.NameToLayer("UI");

        public static bool IsPointerOverUIObject()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new UnityEngine.Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.layer == uiMask) return true;
            }

            return false;
        }

        public static bool IsPointerOverUIObject(GraphicRaycaster raycaster, ref List<RaycastResult> results)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            if (Input.touchCount > 0)
            {
                eventData.position = Input.GetTouch(0).position;
            }
            else
            {
                eventData.position = Input.mousePosition;
            }

            results.Clear();
            raycaster.Raycast(eventData, results);
            return results.Count > 0;
        }
    }
}