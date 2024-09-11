using System.IO;
using SmasherDestruction.Editor;
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
            InstantiateCutterPlane();
            var window = GetWindow<GouwangEditorWindow>();
            window.titleContent = new GUIContent("GouwangWindow");
        }

        [MenuItem("Window/SmasherDestruction/Tsujigiri")]
        public static void ShowTsujigiriWindow()
        {
            InstantiateCutterPlane();
            var window = GetWindow<TsujigiriEditorWindow>();
            window.titleContent = new GUIContent("TsujigiriWindow");
        }

        static void InstantiateCutterPlane()
        {
            GameObject.Instantiate(PrefabUtility.LoadPrefabContents(Gouwang.CutterPlanePrefabFolderAbsolutePath));
        }
    }
}