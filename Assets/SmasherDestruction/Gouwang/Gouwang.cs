using Random = UnityEngine.Random;
using System.Collections.Generic;
using SmasherDestruction.Editor;
using UnityEngine;

namespace GouwangDestruction.Core
{
    /// <summary>
    /// 編集モードで実行されるAPIを提供している剛腕クラス
    /// </summary>
    public static class Gouwang
    {
        /// <summary>
        /// メッシュの断片化処理を実行する
        /// </summary>
        /// <param name="sourceObject"></param>
        /// <param name="fragments"></param>
        /// <param name="planeObject"></param>
        /// <param name="capMaterial"></param>
        /// <param name="makeGap"></param>
        public static void ExecuteFragmentation(
            GameObject sourceObject,
            List<GameObject> fragments,
            Transform planeObject,
            Material capMaterial,
            bool makeGap)
        {
            MakeFragments(sourceObject, fragments, planeObject, capMaterial, makeGap);
        }

        /// <summary>
        /// 断片を生成する
        /// </summary>
        /// <param name="sourceObject"></param>
        /// <param name="fragments"></param>
        /// <param name="planeObject"></param>
        /// <param name="capMaterial"></param>
        /// <param name="makeGap"></param>
        private static void MakeFragments(
            GameObject sourceObject,
            List<GameObject> fragments,
            Transform planeObject,
            Material capMaterial,
            bool makeGap)
        {
            if (sourceObject is null) return;

            var loopCount = Random.Range(1, 10);
            var bounds = sourceObject.GetComponent<MeshFilter>().sharedMesh.bounds;
            var minX = bounds.min.x;
            var minY = bounds.min.y;
            var minZ = bounds.min.z;
            var maxX = bounds.max.x;
            var maxY = bounds.max.y;
            var maxZ = bounds.max.z;

            for (int i = 0; i < loopCount; i++)
            {
                var rotX = Random.Range(-180f, 180f);
                var rotY = Random.Range(-180f, 180f);
                var rotZ = Random.Range(-180f, 180f);
                var posX = Random.Range(minX, maxX);
                var posY = Random.Range(minY, maxY);
                var posZ = Random.Range(minZ, maxZ);

                planeObject.transform.rotation = Quaternion.Euler(rotX, rotY, rotZ);
                planeObject.transform.position = new Vector3(posX, posY, posZ);

                if (sourceObject is null) return;

                TsujigiriUtility.CutTheMesh(
                    sourceObject,
                    fragments,
                    planeObject.transform.position,
                    planeObject.transform.up,
                    capMaterial,
                    makeGap);
            }
        }
    }
}