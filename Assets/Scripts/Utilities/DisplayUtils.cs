using System;
using UnityEngine;

public static class DisplayUtils
{
    /// <summary>
    /// Determine wether the mouse position is on the game's screen or not
    /// </summary>
    public static bool IsMouseOnScreen()
    {
        var mousePos = Input.mousePosition;
        return mousePos.x > 0 &&
            mousePos.y > 0 &&
            mousePos.x <= Screen.width &&
            mousePos.y <= Screen.height;
    }

}
