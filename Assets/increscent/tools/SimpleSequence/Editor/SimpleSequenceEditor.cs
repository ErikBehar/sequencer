using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;

[CustomEditor(typeof(SimpleSequence))]
public class SimpleSequenceEditor : Editor
{
    SimpleSequence tar;
    void OnEnable()
    {
        tar = (SimpleSequence)target;
        refreshTypes();
    }

    private void refreshTypes()
    {
        tar.stepTypes.Clear();
        string[] guids = AssetDatabase.FindAssets("SimpleSequenceStep");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string filename = Path.GetFileName(path);
            string[] split = filename.Split('.');
            tar.stepTypes.Add(split[0]);
        }
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        tar.pickedTypeIndex = EditorGUILayout.Popup( tar.pickedTypeIndex, tar.stepTypes.ToArray());

        if (GUILayout.Button("Add Step Type"))
        {
            Type assetType = GetType(tar.stepTypes[tar.pickedTypeIndex]);
            tar.addStepWithType(assetType);
        }
    }

    // note that this only works in Editor not in runtime
    // if you want it in runtime, try something with System.AppDomain.CurrentDomain.GetAssemblies() instead of Assembly.Load
    private Type GetType(string TypeName)
    {
        // Try Type.GetType() first. This will work with types defined
        // by the Mono runtime, in the same assembly as the caller, etc.
        var type = Type.GetType(TypeName);

        // If it worked, then we're done here
        if (type != null)
            return type;

        // If the TypeName is a full name, then we can try loading the defining assembly directly
        if (TypeName.Contains("."))
        {
            // Get the name of the assembly (Assumption is that we are using 
            // fully-qualified type names)
            var assemblyName = TypeName.Substring(0, TypeName.IndexOf('.'));

            // Attempt to load the indicated Assembly
            var assembly = Assembly.Load(assemblyName);
            if (assembly == null)
                return null;

            // Ask that assembly to return the proper Type
            type = assembly.GetType(TypeName);
            if (type != null)
                return type;

        }


        // If we still haven't found the proper type, we can enumerate all of the 
        // loaded assemblies and see if any of them define the type
        var currentAssembly = Assembly.GetExecutingAssembly();
        var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
        foreach (var assemblyName in referencedAssemblies)
        {
            // Load the referenced assembly
            var assembly = Assembly.Load(assemblyName);
            if (assembly != null)
            {
                // See if that assembly defines the named type
                type = assembly.GetType(TypeName);
                if (type != null)
                    return type;
            }
        }

        // The type just couldn't be found...
        return null;

    }
}