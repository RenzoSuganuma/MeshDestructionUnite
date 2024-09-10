using UnityEngine;

namespace SmasherDestruction.Editor
{
    /// <summary>
    /// GUIレイアウトのプリセット
    /// </summary>
    public static class SmasherDestructionConstantValues
    {
        public static int SmasherTitleFontSize = 32;
        public static int SmasherLabelFontSize = 16;

        public static GUIStyle GetGUIStyle_LabelTitle()
        {
            var gui = new GUIStyle();
            gui.fontSize = SmasherTitleFontSize;
            gui.fontStyle = FontStyle.Bold;
            gui.alignment = TextAnchor.MiddleCenter;
            gui.richText = true;
            return gui;
        }
        
        public static GUIStyle GetGUIStyle_LabelBig()
        {
            var gui = new GUIStyle();
            gui.fontSize = SmasherLabelFontSize;
            gui.fontStyle = FontStyle.Bold;
            gui.alignment = TextAnchor.MiddleCenter;
            gui.richText = true;

            return gui;
        }

        public static GUIStyle GetGUIStyle_LabelSmall()
        {
            var gui = new GUIStyle();
            gui.fontSize = 24;
            gui.fontStyle = FontStyle.Bold;
            gui.alignment = TextAnchor.MiddleCenter;
            gui.richText = true;
            return gui;
        }
    }
}