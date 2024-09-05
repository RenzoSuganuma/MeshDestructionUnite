using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GouwangDestruction.Editor
{
    public class GouwangDestructionEditor : EditorWindow
    {
        [MenuItem("Window/SmasherDestruction")]
        public static void ShowSmasherWindow()
        {
            var window = GetWindow<GouwangDestructionEditorWindow>();
            window.titleContent = new GUIContent("SmasherDestructionEditor");
        }
    }
}