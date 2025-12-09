using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField] private LayerMask holeLayerMask;
    [SerializeField] private Camera mainCamera;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleInput(Input.mousePosition);
        }
    }

    private void HandleInput(Vector2 screenPos)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
        {
            return;
        }

        if (IsPointerOverUIElement())
        {
            Debug.Log("[PlayerInputController] Click is on UI → block world click");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, holeLayerMask))
        {
            Hole hole = hit.collider.GetComponent<Hole>();
            if (hole != null)
            {
                GameManager.Instance.SelectHole(hole);
            }
        }
    }
    
    private bool IsPointerOverUIElement()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
}