using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GouwangDestruction.Editor
{
    /// <summary>
    /// 編集モードの剛腕クラスをツールバーから呼び出せるように実装しているクラス
    /// </summary>
    public sealed class SmasherDestructionEditor : EditorWindow
    {
        [MenuItem("Window/SmasherDestruction")]
        public static void ShowSmasherWindow()
        {
            var window = GetWindow<SmasherDestructionEditorWindow>();
            window.titleContent = new GUIContent("SmasherDestructionWindow");
        }
    }
}