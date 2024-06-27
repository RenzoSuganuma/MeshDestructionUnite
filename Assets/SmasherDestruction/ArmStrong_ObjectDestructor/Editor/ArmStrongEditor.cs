using UnityEditor;
using UnityEngine;

/// <summary> MachomanUserのインスペクタを拡張する。ボタンの追加をする </summary>
[CustomEditor(typeof(ArmStrongUser_RunTime))]
public class ArmStrongEditor : Editor // MachoManクラスを拡張する
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ArmStrongUser_RunTime userRunTime = target as ArmStrongUser_RunTime;

        if (GUILayout.Button("Cut"))
        {
            userRunTime.CutMesh();
        }
        
        if (GUILayout.Button("Cut Randomly"))
        {
            userRunTime.CutRandomly();
        }

        if (GUILayout.Button("Check Directory"))
        {
            userRunTime.CheckDirectory();
        }

        if (GUILayout.Button("Save Meshes"))
        {
            userRunTime.SaveCuttedMeshes();
        }
    }
}