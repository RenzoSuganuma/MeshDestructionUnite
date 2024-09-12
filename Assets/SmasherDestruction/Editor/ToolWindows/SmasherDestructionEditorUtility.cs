using System.IO;
using UnityEditor;
using UnityEngine;

namespace GouwangDestruction.Editor
{
    /// <summary>
    /// スマッシャーデストラクション の ユーティリティクラス
    /// </summary>
    public class SmasherDestructionEditorUtility
    {
        public static string CuttedMeshesFolderAbsolutePath =
            "Assets/Resources/SmasherDestruction/CuttedMeshes/";

        public static string CuttedMeshesPrefabFolderAbsolutePath =
            "Assets/Resources/SmasherDestruction/Prefabs/";

        public static string CutterPlanePrefabFolderAbsolutePath =
            "Assets/Resources/SmasherDestruction/Util/CutterPlane.prefab";

        public static void CreateAndSaveToAsset(Mesh mesh, string meshName, int num)
        {
            AssetDatabase.CreateAsset(mesh,
                CuttedMeshesFolderAbsolutePath + $"{meshName}/{meshName}_Mesh_{num}.asset");
        }

        public static void SaveAsPrefab(GameObject fragmentsParent, string meshName)
        {
            PrefabUtility.SaveAsPrefabAsset(fragmentsParent,
                SmasherDestructionEditorUtility.CuttedMeshesPrefabFolderAbsolutePath + $"{meshName}.prefab");
        }
        
        /// <summary>
        /// 保存先ディレクトリを探す。
        /// </summary>
        /// <param name="filePath"></param>
        public static void FindSaveTargetDirectory(string filePath)
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
        }
    }
}