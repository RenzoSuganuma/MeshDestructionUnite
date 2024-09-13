using UnityEditor;
using UnityEngine;

namespace SmasherDestruction.Editor
{
    /// <summary>
    /// GUIレイアウトのプリセット
    /// </summary>
    public static class SmasherDestructionConstantValues
    {
        public static string AssingFragmentationTargetLabel = "Assing Fragmentation Target";
        public static string FragmentMeshesFileNameLabel = "Fragment Meshes File Name";
        public static string SaveToStorageFragmentMeshesFileLabel = "Save To Storage";
        public static string ResetAllOptionsLabel = "Reset All";
        public static string CloseWindowLabel = "Close Window";
        
        public static int SmasherTitleFontSize = 32;
        public static int SmasherLabelFontSize = 16;

        public static GUIStyle GetGUIStyle_LabelTitle()
        {
            var gui = new GUIStyle(GUI.skin.label);
            gui.fontSize = SmasherTitleFontSize;
            gui.fontStyle = FontStyle.Bold;
            gui.alignment = TextAnchor.MiddleCenter;
            gui.richText = false;
            gui.normal.textColor = Color.black;
            gui.normal.background = MakeTex(600, 1, new Color(.75f, .75f, .75f));
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
            var gui = new GUIStyle(GUI.skin.label);
            gui.fontSize = 24;
            gui.fontStyle = FontStyle.Bold;
            gui.alignment = TextAnchor.MiddleCenter;
            gui.hover.background = MakeTex(600,1,new Color(.75f, .75f, .75f));
            gui.hover.textColor = Color.black;
            gui.normal.background = MakeTex(600,1,new Color(.75f, .75f, .75f));
            gui.normal.textColor = Color.black;
            gui.richText = true;
            return gui;
        }

        public static GUIStyle GetGUIStyle_LabelNotice()
        {
            var gui = new GUIStyle(GUI.skin.label);
            gui.fontSize = 24;
            gui.fontStyle = FontStyle.Bold;
            gui.alignment = TextAnchor.MiddleCenter;
            gui.normal.textColor = Color.black;
            gui.normal.background = MakeTex(600, 1, new Color(.75f, .75f, .75f));
            gui.richText = false;
            return gui;
        }

        public static GUIStyle GetGUIStyle_ScaryButton()
        {
            var gui = new GUIStyle(GUI.skin.button);
            gui.fontSize = 12;
            gui.fontStyle = FontStyle.Bold;
            gui.alignment = TextAnchor.MiddleCenter;
            gui.richText = true;
            gui.normal.textColor = Color.white;
            gui.hover.textColor = Color.red;
            return gui;
        }
        
        public static GUIStyle GetGUIStyle_SaveButton()
        {
            var gui = new GUIStyle(GUI.skin.button);
            gui.fontSize = 12;
            gui.fontStyle = FontStyle.Bold;
            gui.alignment = TextAnchor.MiddleCenter;
            gui.richText = true;
            gui.normal.textColor = Color.white;
            gui.hover.textColor = Color.green;
            return gui;
        }
        
        public static GUIStyle GetGUIStyle_ExecuteButton()
        {
            var gui = new GUIStyle(GUI.skin.button);
            gui.fontSize = 12;
            gui.fontStyle = FontStyle.Bold;
            gui.alignment = TextAnchor.MiddleCenter;
            gui.richText = true;
            gui.normal.textColor = Color.white;
            gui.hover.textColor = Color.yellow;
            return gui;
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}