using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
 #endif
using UnityEngine;

public class SequencerWindow : EditorWindow
{
    private static SequencerWindow thisWindowLive;
    private Vector2 scrollPosTargets;
    private Vector2 scrollPosSections;
    private GameObject dataHolderValGoLastVal = null;
    private static GameObject dataHolderGO = null;
    public static SequencerData sequencerData;
    private static string lastSelectedArea = "targets"; //can be "targets" , "sections"

    private static int lastSelectedSection = -1;
    private static string renameBoxString = "";
    private static int lastSelectedCommand = 0;
    private GUIStyle guiBGAltColorA;
    private Texture2D littleTextureForBG_A;
    private GUIStyle guiBGAltColorB;
    private Texture2D littleTextureForBG_B;
    private static int insertIndex = 0;

   #region Window Stuff
    void OnDisable()
    {
        DestroyImmediate(littleTextureForBG_A);
        DestroyImmediate(littleTextureForBG_B);
    }

    void OnDestroy()
    {
        DestroyImmediate(littleTextureForBG_A);
        DestroyImmediate(littleTextureForBG_B);
    }

    void OnEnable()
    {
        if (dataHolderGO != null)
        {
            testForData();
        }
        
        littleTextureForBG_A = new Texture2D(1, 1);
        littleTextureForBG_A.SetPixel(0, 0, new Color(0f, 1f, 1f, .15f));
        littleTextureForBG_A.Apply();
        littleTextureForBG_A.hideFlags = HideFlags.HideAndDontSave;
        guiBGAltColorA = new GUIStyle();
        guiBGAltColorA.normal.background = littleTextureForBG_A;
        
        littleTextureForBG_B = new Texture2D(1, 1);
        littleTextureForBG_B.SetPixel(0, 0, new Color(1f, 0f, 1f, .15f));
        littleTextureForBG_B.Apply();
        littleTextureForBG_B.hideFlags = HideFlags.HideAndDontSave;
        guiBGAltColorB = new GUIStyle();
        guiBGAltColorB.normal.background = littleTextureForBG_B;
    }

    public static void show()
    {
        thisWindowLive = (SequencerWindow)EditorWindow.GetWindow(typeof(SequencerWindow));
    }
    
    public static void hide()
    {
        show();
        thisWindowLive.Close();
        thisWindowLive = null;
    }
    
    [MenuItem("Window/Sequencer Window")]
    static void startAutoPrefabJobEditor()
    {
        show();
    }
    #endregion

    void OnGUI()
    {
        testForData();

        EditorGUILayout.BeginVertical();
        {
            drawDataHolder();
           
            if (dataHolderGO != null && sequencerData != null)
            {
                //spacer 
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
    
                drawUiAreas();
                //spacer 
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                
                if (lastSelectedArea == "targets")
                    drawDefineTargets();
                else if (lastSelectedArea == "sections")
                    drawSections();
            }
        }
        EditorGUILayout.EndVertical();

    }

    private void testForData()
    {
        if (dataHolderGO != null)
        {
            sequencerData = dataHolderGO.GetComponent<SequencerData>();
            
            if (sequencerData == null)
            {
                sequencerData = dataHolderGO.AddComponent<SequencerData>();
            }
        }
    }

    private void drawDataHolder()
    {
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("Drag & Drop object that holds data here:");
            if (GUILayout.Button("Find"))
                dataHolderGO = findDataHolderGo();
            dataHolderGO = EditorGUILayout.ObjectField(dataHolderGO, typeof(GameObject), true) as GameObject;       
        }   
        EditorGUILayout.EndHorizontal();    
    }

    private GameObject findDataHolderGo()
    {
        SequencerData data = SceneView.FindObjectOfType<SequencerData>();
        if (data != null)
            return data.gameObject;
        return null;
    }

    private void drawUiAreas()
    {
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Define Targets"))
                lastSelectedArea = "targets";  
         
            if (GUILayout.Button("Sections"))
            {
                lastSelectedArea = "sections"; 
                if (lastSelectedSection == -1 && sequencerData.sections != null && sequencerData.sections.Count > 0)
                {
                    lastSelectedSection = 0;
                }
            }
        }   
        EditorGUILayout.EndHorizontal();   
    }

    private void drawDefineTargets()
    {
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("Define Targets");
            if (GUILayout.Button("New Target"))
                doAddNewTarget();
        }   
        EditorGUILayout.EndHorizontal();  
        
        //spacer 
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1)); 
        
        scrollPosTargets = EditorGUILayout.BeginScrollView(scrollPosTargets);
        {
            if (sequencerData != null && sequencerData.targets != null)
            {
                EditorGUILayout.BeginVertical();
                {
                    SequencerTargetModel[] tempTargets = new SequencerTargetModel[sequencerData.targets.Count]; 
                    sequencerData.targets.CopyTo(tempTargets);
                    List<SequencerTargetModel> targets = new List<SequencerTargetModel>(tempTargets);

                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (i % 2 == 0)
                            EditorGUILayout.BeginHorizontal(guiBGAltColorB);
                        else
                            EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("target:");
                            targets [i].target = EditorGUILayout.ObjectField(targets [i].target, typeof(GameObject), true) as GameObject;
                            GUILayout.Label("Nickname:");
                            targets [i].nickname = GUILayout.TextField(targets [i].nickname);  
                            if (GUILayout.Button("Delete this target"))
                                doDeleteTarget(targets [i].target);
                        }  
                        EditorGUILayout.EndHorizontal(); 
                    }
                }
                EditorGUILayout.EndVertical(); 
            }
        }
        EditorGUILayout.EndScrollView(); 
    }

    private void drawSections()
    {
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("Sections");
            if (GUILayout.Button("New Section"))
                doAddNewSection();
            
            if (GUILayout.Button("Duplicate Selected Section"))
            {
                if (sequencerData.sections != null && lastSelectedSection > -1)
                    doDuplicateSection(sequencerData.sections [lastSelectedSection]);
            }

//            if (GUILayout.Button("Fix Commands to have reference to data object"))
//                doFixCommands();
        }   
        EditorGUILayout.EndHorizontal();  

        //spacer 
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1)); 

        if (sequencerData != null && sequencerData.getSectionNames().Length > 0)
        { 
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Select Section:");
                lastSelectedSection = EditorGUILayout.Popup(lastSelectedSection, sequencerData.getSectionNames(), GUILayout.Width(100));
    
                GUILayout.Label("Rename to this:");
                renameBoxString = GUILayout.TextField(renameBoxString);
    
                if (GUILayout.Button("Rename this Section"))
                    doRenameSection(sequencerData.sections [lastSelectedSection], renameBoxString);    
        
                if (GUILayout.Button("Delete this Section"))
                    doDeleteSection(sequencerData.getSectionNames() [lastSelectedSection]); 
            }  
            EditorGUILayout.EndHorizontal(); 
        }  

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("Available Commands:");
            lastSelectedCommand = EditorGUILayout.Popup(lastSelectedCommand, SequencerCommandTypes.getAsStringArray(), GUILayout.Width(100));

            if (GUILayout.Button("Add Command"))
                doAddCommandToSection(lastSelectedSection, lastSelectedCommand);

            insertIndex = EditorGUILayout.IntField(insertIndex);
            if (GUILayout.Button("Add Command At"))
                doAddCommandToSectionAt(lastSelectedSection, lastSelectedCommand, insertIndex);

        }  
        EditorGUILayout.EndHorizontal(); 

        //spacer 
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        
        if (lastSelectedSection > -1 && lastSelectedSection < sequencerData.sections.Count && sequencerData.sections [lastSelectedSection] != null && sequencerData.sections [lastSelectedSection].commandList != null)
        {      
            scrollPosSections = EditorGUILayout.BeginScrollView(scrollPosSections);
            {   
                EditorGUILayout.BeginVertical();
                {
                    SequencerCommandBase[] tempCommands = new SequencerCommandBase[sequencerData.sections [lastSelectedSection].commandList.Count]; 
                    sequencerData.sections [lastSelectedSection].commandList.CopyTo(tempCommands);

                    for (int i = 0; i < tempCommands.Length; i++)
                    {
                        if (i % 2 == 0)
                            EditorGUILayout.BeginHorizontal(guiBGAltColorA);
                        else
                            EditorGUILayout.BeginHorizontal();
                        {
                            tempCommands [i].drawFrontUi();
                            tempCommands [i].drawCustomUi();
                            tempCommands [i].drawBackUi();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }   
                EditorGUILayout.EndVertical();  
            }
            EditorGUILayout.EndScrollView(); 
        } else
        {
            lastSelectedSection -= 1;
        }
    }   
    
    //note this deletes all found with same target ( in general we dont want duplicate targets anyway)
    void doDeleteTarget(GameObject targetObject)
    {
        for (int i = sequencerData.targets.Count-1; i > -1; i--)
        {
            if (targetObject == sequencerData.targets [i].target)
            {
                sequencerData.targets.Remove(sequencerData.targets [i]);
            }
        }
    }

    //note this deletes all found with same name ( in general we dont want duplicates names anyway)
    void doDeleteSection(string sectionName)
    {
        for (int i = sequencerData.sections.Count-1; i > -1; i--)
        {
            if (sectionName == sequencerData.sections [i].name)
            {
                SequencerSectionModel section = sequencerData.sections [i];
                sequencerData.sections.Remove(sequencerData.sections [i]);

                foreach (SequencerCommandBase command in section.commandList)
                {
                    DestroyImmediate(command);
                }
            }
        }
    }

    //note this renames all with same name
    void doRenameSection(SequencerSectionModel sectionModel, string newName)
    {
        foreach (SequencerSectionModel model in sequencerData.sections)
        {
            if (sectionModel != model && model.name == newName)
            {
                Debug.LogWarning("Could not rename to " + newName + " because another section is already named that way !");
                return;
            }
        }

        sectionModel.name = newName;
    }

    void doAddCommandToSection(int sectionIndex, int typeIndex)
    {
        if (sectionIndex == -1 || typeIndex == -1)
        {
            Debug.LogWarning("Make sure a section & command is selected");
            return; 
        }

        Type type = SequencerCommandTypes.commandTypes [typeIndex];
        ScriptableObject temp = ScriptableObject.CreateInstance(type);
        ((SequencerCommandBase)temp).init(sectionIndex, sequencerData);

        sequencerData.sections [sectionIndex].commandList.Add((SequencerCommandBase)temp);

        ((SequencerCommandBase)temp).updateAllIndex();
    }
    
    void doAddCommandToSectionAt(int sectionIndex, int typeIndex, int insertIndex)
    {
        if (sectionIndex == -1 || typeIndex == -1)
        {
            Debug.LogWarning("Make sure a section & command is selected");
            return; 
        }
        
        Type type = SequencerCommandTypes.commandTypes [typeIndex];
        ScriptableObject temp = ScriptableObject.CreateInstance(type);
        ((SequencerCommandBase)temp).init(sectionIndex, sequencerData);
        
        insertIndex = Mathf.Clamp(insertIndex, 0, sequencerData.sections [sectionIndex].commandList.Count);

        sequencerData.sections [sectionIndex].commandList.Insert(insertIndex, (SequencerCommandBase)temp);
    }

    void doAddNewTarget()
    {
        SequencerTargetModel model = new SequencerTargetModel();
        model.nickname = "target_" + UnityEngine.Random.Range(0, int.MaxValue).ToString();
        sequencerData.targets.Add(model);
    }

    void doAddNewSection()
    {
        SequencerSectionModel model = new SequencerSectionModel();
        model.name = "section_" + UnityEngine.Random.Range(0, int.MaxValue).ToString();
        model.commandList = new List<SequencerCommandBase>();
        sequencerData.sections.Add(model);
        lastSelectedSection = sequencerData.sections.Count - 1;
    }

    void doDuplicateSection(SequencerSectionModel model)
    {
        SequencerSectionModel newModel = model.clone();
        newModel.name += "_" + UnityEngine.Random.Range(0, int.MaxValue).ToString();
        //TODO make sure this name is unique before continue

        foreach (SequencerCommandBase cmd in newModel.commandList)
        {
            cmd.init(sequencerData.sections.Count, sequencerData);
        }

        sequencerData.sections.Add(newModel);
        lastSelectedSection = sequencerData.sections.Count - 1;
    }

    void Update()
    {
        if (dataHolderValGoLastVal != dataHolderGO)
        {
            dataHolderValGoLastVal = dataHolderGO;
            testForData();
        }
    }

    void doFixCommands()
    {
        foreach (SequencerSectionModel section in sequencerData.sections)
        {
            foreach (SequencerCommandBase command in section.commandList)
            {
                command.sequencerData = sequencerData;
            } 
        } 
    }
}