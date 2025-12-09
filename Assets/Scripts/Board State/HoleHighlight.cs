using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class HoleHighlight : MonoBehaviour
{
    private Renderer rend;
    private Color originalColor;
    [SerializeField] private Color highlightColor = Color.yellow;

    private Hole hole;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
        hole = GetComponent<Hole>();
    }

    private void OnMouseEnter()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsValidMove(hole))
            rend.material.color = highlightColor;
    }

    private void OnMouseExit()
    {
        rend.material.color = originalColor;
    }
}