using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SequencerData))]
public class SequencerDataInspector : Editor 
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (GUILayout.Button("Open Sequencer"))
        {
            SequencerWindow window = EditorWindow.GetWindow(typeof(SequencerWindow)) as SequencerWindow;
            window.Focus();
        }

        DrawDefaultInspector ();

        serializedObject.ApplyModifiedProperties();
    }
}
