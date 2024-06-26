using UnityEditor;
using UnityEngine;

/// <summary> MachomanUserのインスペクタを拡張する。ボタンの追加をする </summary>
[CustomEditor(typeof(ArmStrongUser))]
public class ArmStrongEditor : Editor // MachoManクラスを拡張する
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ArmStrongUser user = target as ArmStrongUser;

        if (GUILayout.Button("Cut"))
        {
            user.CutMesh();
        }
        
        if (GUILayout.Button("Cut Randomly"))
        {
            user.CutRandomly();
        }

        if (GUILayout.Button("Check Directory"))
        {
            user.CheckDirectory();
        }

        if (GUILayout.Button("Save Meshes"))
        {
            user.SaveCuttedMeshes();
        }
    }
}