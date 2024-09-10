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
        [MenuItem("Window/SmasherDestruction/Gouwang")]
        public static void ShowGouwangWindow()
        {
            GameObject.CreatePrimitive(PrimitiveType.Plane);
            var window = GetWindow<GouwangEditorWindow>();
            window.titleContent = new GUIContent("GouwangWindow");
        }

        [MenuItem("Window/SmasherDestruction/Tsujigiri")]
        public static void ShowTsujigiriWindow()
        {
            GameObject.CreatePrimitive(PrimitiveType.Plane);
            var window = GetWindow<TsujigiriEditorWindow>();
            window.titleContent = new GUIContent("TsujigiriWindow");
        }
    }
}