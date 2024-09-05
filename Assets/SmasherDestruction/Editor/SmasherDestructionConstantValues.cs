using UnityEngine;

namespace SmasherDestruction.Editor
{
    /// <summary>
    /// GUIレイアウトのプリセット
    /// </summary>
    public static class SmasherDestructionConstantValues
    {
        public static int SmasherLabelFontSize = 32;

        public static GUIStyle GetGUIStyle_LabelBig()
        {
            var gui = new GUIStyle();
            gui.fontSize = SmasherLabelFontSize;
            gui.fontStyle = FontStyle.Bold;
            gui.alignment = TextAnchor.MiddleCenter;

            return gui;
        }

        public static GUIStyle GetGUIStyle_LabelSmall()
        {
            var gui = new GUIStyle();
            gui.fontSize = 24;
            gui.fontStyle = FontStyle.Bold;
            gui.alignment = TextAnchor.MiddleCenter;
            return gui;
        }
    }
}