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
            var obj = InstantiateCutterPlane();
            var window = GetWindow<GouwangEditorWindow>();
            window.titleContent = new GUIContent("GouwangWindow");
            window.CutterPlane = obj;
        }

        [MenuItem("Window/SmasherDestruction/Tsujigiri")]
        public static void ShowTsujigiriWindow()
        {
            var obj = InstantiateCutterPlane();
            var window = GetWindow<TsujigiriEditorWindow>();
            window.CutterPlane = obj;
            window.titleContent = new GUIContent("TsujigiriWindow");
        }
        
        [MenuItem("Window/SmasherDestruction/NawabariModoki")]
        public static void ShowNawabariModokiWindow()
        {
            var window = GetWindow<NawabariModokiEditorWindow>();
            window.titleContent = new GUIContent("NawabariModokiWindow");
        }
        
        [MenuItem("Window/SmasherDestruction/Nawabari")]
        public static void ShowNawabariWindow()
        {
            var window = GetWindow<NawabariEditorWindow>();
            window.titleContent = new GUIContent("NawabariWindow");
        }

        static GameObject InstantiateCutterPlane()
        {
            return GameObject.Instantiate(
                PrefabUtility.LoadPrefabContents(SmasherDestructionEditorUtility.CutterPlanePrefabFolderAbsolutePath));
        }
    }
}