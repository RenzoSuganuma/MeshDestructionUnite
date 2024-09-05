using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GouwangDestruction.Editor
{
    /// <summary>
    /// 編集モードの剛腕クラスをツールバーから呼び出せるように実装しているクラス
    /// </summary>
    public sealed class GouwangDestructionEditor : EditorWindow
    {
        [MenuItem("Window/SmasherDestruction")]
        public static void ShowSmasherWindow()
        {
            var window = GetWindow<GouwangDestructionEditorWindow>();
            window.titleContent = new GUIContent("SmasherDestructionEditor");
        }
    }
}