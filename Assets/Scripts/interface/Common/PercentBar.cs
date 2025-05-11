using UnityEngine;
using UnityEngine.UI;

public class PercentBar : MonoBehaviour
{
    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    [Header("UI References")]
    public RectTransform targetImageRect;

    [Range(0f, 1f)]
    public float value = 1.0f;
    public float Value
    {
        get { return value; }
        set
        {
            this.value = Mathf.Clamp01(value);
            UpdateBar();
        }
    }

    [Header("Settings")]
    public Orientation orientation = Orientation.Horizontal;
    public bool updateInRuntime = true;

    void Start()
    {
        if (targetImageRect == null)
        {
            Debug.LogError("PercentBar: No targetImageRect assigned.");
            enabled = false;
            return;
        }

        // Initial update
        UpdateBar();
    }

    void Update()
    {
        if (updateInRuntime)
        {
            UpdateBar();
        }
    }

    public void SetOrientation(Orientation newOrientation)
    {
        orientation = newOrientation;
        UpdateBar();
    }

    private void UpdateBar()
    {
        if (targetImageRect == null) return;
        
        Vector2 newSize = new Vector2();

        if (orientation == Orientation.Horizontal)
        {
            newSize.x = value;
            newSize.y = 1;
        }
        else
        {
            newSize.x = 1;
            newSize.y = value;
        }

        targetImageRect.anchorMax = newSize;
    }
}