using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using SmasherDestruction.Editor;

namespace GouwangDestruction.Core
{
    /// <summary>
    /// 編集モードで実行されるAPIを提供している剛腕クラス
    /// </summary>
    public static class GouwangUtility
    {
        /// <summary>
        /// メッシュの断片化処理を実行する
        /// </summary>
        /// <param name="victimObject"></param>
        /// <param name="fragments"></param>
        /// <param name="planeObject"></param>
        /// <param name="capMaterial"></param>
        /// <param name="makeGap"></param>
        public static void DoFragmentation(
            GameObject victimObject,
            List<GameObject> fragments,
            Transform planeObject,
            Material capMaterial,
            bool makeGap)
        {
            MakeFragments(victimObject, fragments, planeObject, capMaterial, makeGap);
        }

        /// <summary>
        /// 断片を生成する
        /// </summary>
        /// <param name="victimObject"></param>
        /// <param name="fragments"></param>
        /// <param name="planeObject"></param>
        /// <param name="capMaterial"></param>
        /// <param name="makeGap"></param>
        private static void MakeFragments(
            GameObject victimObject,
            List<GameObject> fragments,
            Transform planeObject,
            Material capMaterial,
            bool makeGap)
        {
            if (victimObject is null) return;

            var loopCount = Random.Range(1, 10);
            var bounds = victimObject.GetComponent<MeshFilter>().sharedMesh.bounds;
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

                CutMesh(victimObject, fragments, planeObject, capMaterial, makeGap);
            }
        }

        /// <summary>
        /// メッシュの切断をする。辻斬りの処理を呼んでいる
        /// </summary>
        /// <param name="victimObject"></param>
        /// <param name="fragments"></param>
        /// <param name="planeObject"></param>
        /// <param name="capMaterial"></param>
        /// <param name="makeGap"></param>
        private static void CutMesh(
            GameObject victimObject,
            List<GameObject> fragments,
            Transform planeObject,
            Material capMaterial,
            bool makeGap)
        {
            if (victimObject is null) return;

            TsujigiriUtility.CutTheMesh(
                victimObject,
                fragments,
                planeObject.transform.position,
                planeObject.transform.up,
                capMaterial,
                makeGap);
        }
    }
}