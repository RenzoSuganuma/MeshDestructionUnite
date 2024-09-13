using System.Collections.Generic;
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

        public static void PaintFragments(int fragmentsCount,out GameObject copiedParent,
            GameObject fragmentsParent, Material insideMaterial)
        {
            copiedParent = null;
            
            // 断片を強調表示
            if (fragmentsCount is not 0)
            {
                // いったん同じものを生成
                copiedParent = GameObject.Instantiate(fragmentsParent);
                copiedParent.name = "_Temporary Object_";
                fragmentsParent.SetActive(false);
                for (int i = 0; i < copiedParent.transform.childCount; i++)
                {
                    var m = new Material(insideMaterial);
                    m.color = new Color(
                        Random.Range(0, 1f),
                        Random.Range(0, 1f),
                        Random.Range(0, 1f));
                    copiedParent.transform.GetChild(i).GetComponent<MeshRenderer>().sharedMaterial = m;
                }
            }
        }
    }
}