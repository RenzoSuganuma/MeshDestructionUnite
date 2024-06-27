using UnityEditor;
using UnityEngine;

/// <summary> MachomanUserのインスペクタを拡張する。ボタンの追加をする </summary>
[CustomEditor(typeof(ArmStrongUser_Proto))]
public class ArmStrongEditor : Editor // MachoManクラスを拡張する
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ArmStrongUser_Proto userProto = target as ArmStrongUser_Proto;

        if (GUILayout.Button("Cut"))
        {
            userProto.CutMesh();
        }
        
        if (GUILayout.Button("Cut Randomly"))
        {
            userProto.CutRandomly();
        }

        if (GUILayout.Button("Check Directory"))
        {
            userProto.CheckDirectory();
        }

        if (GUILayout.Button("Save Meshes"))
        {
            userProto.SaveCuttedMeshes();
        }
    }
}