using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SmasherDestruction.Editor
{
    public class SmasherDestructionEditor : EditorWindow
    {
        [MenuItem("Window/SmasherDestruction")]
        public static void ShowSmasherWindow()
        {
            var window = CreateWindow<SmasherDestructionEditorWindow>();
            window.titleContent = new GUIContent("SmasherDestructionEditor");
            window.name = "SmasherDestructionEditor:Tab 1";
        }
    }
}