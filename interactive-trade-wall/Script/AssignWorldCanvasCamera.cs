using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class AssignWorldCanvasCamera : MonoBehaviour
{
    void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();

        // Only assign if it's World Space canvas
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            if (canvas.worldCamera == null && Camera.main != null)
            {
                canvas.worldCamera = Camera.main;
            }
        }
    }
}
