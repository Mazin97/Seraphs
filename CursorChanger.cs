using UnityEngine;

public class CursorChanger : MonoBehaviour
{
    public Texture2D customCursor;
    public Vector2 cursorHotspot = Vector2.zero;

    private void Start()
    {
        Cursor.SetCursor(customCursor, cursorHotspot, CursorMode.Auto);
    }
}
