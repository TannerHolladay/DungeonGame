#if UNITY_EDITOR
using UnityEngine;

namespace PSS
{
    public static class EventEx
    {
        public static void Disable(this Event e)
        {
            GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
            e.Use();
            GUIUtility.hotControl = 0;
        }
    }
}
#endif