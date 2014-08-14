using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

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
    private TextAsset fileData;
    private static GameObject dataHolderGO = null;
    public static SequencerData sequencerData;
    private static string lastSelectedArea = "targets"; //can be "targets" , "sections", "import", "variables", "statistics"

    private static int lastSelectedSection = -1;
    private static string renameBoxString = "";
    private static int lastSelectedCommand = 0;
    private int lastSelectedCommandFilter = 0;
    private bool doTypeFilter = false;
    private GUIStyle guiBGAltColorA;
    private Texture2D littleTextureForBG_A;
    private GUIStyle guiBGAltColorB;
    private Texture2D littleTextureForBG_B;
    private static int insertIndex = 0;

    private TextAsset renpyTextAsset;
    private string importTextAreaText = "";

    public static bool paging = true;
    private int pagingSize = 100;
    private int page = 0;

    private string tempVarName = "variableX";
    private string tempVarValue = "42";

    private SequencerTargetModel currentSetupTarget = null;

    private bool allowCharacterStubs = false;
    private string renpyExportFileName = "exportRenpy";

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

        currentSetupTarget = null;
    }

    [MenuItem("Window/Sequencer Window")]
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
                else if (lastSelectedArea == "import")
                    drawImport();
                else if (lastSelectedArea == "variables")
                    drawVariables();
                else if (lastSelectedArea == "statistics")
                    drawStatistics();
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

            GUILayout.Label("Click Find if you have a data holder in the scene, otherwise Drag & Drop object that will hold the data here:");
            if (GUILayout.Button("Find"))
                dataHolderGO = findDataHolderGo();
            dataHolderGO = EditorGUILayout.ObjectField(dataHolderGO, typeof(GameObject), true) as GameObject;       
        
            if (dataHolderGO != null)
            {
                if (GUILayout.Button("Export data to file"))
                {
                    doExportSequencerFile();
                }
            } else
            {
                fileData = EditorGUILayout.ObjectField(fileData, typeof(TextAsset), true) as TextAsset; 
                if (GUILayout.Button("Import data into scene") && fileData != null)
                {
                    doImportSequencerFile();
                }
            }

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

            if (GUILayout.Button("Variables"))
            {
                lastSelectedArea = "variables";
            }

            if (GUILayout.Button("Import"))
            {   
                lastSelectedArea = "import";
            }

            if (GUILayout.Button("Statistics"))
            {   
                lastSelectedArea = "statistics";
            }
        }   
        EditorGUILayout.EndHorizontal();   
    }

    private bool target_is3d = false;
    private bool target_addAnims = false;
    private bool target_addAttires = false;
    private bool target_addExpressions = false;
    private string target_name = "newCharacter_" + UnityEngine.Random.Range(0, int.MaxValue);

    private void drawDefineTargets()
    {
        if (currentSetupTarget != null)
        {
            target_is3d = GUILayout.Toggle(target_is3d, "3D character");

            if (target_is3d)
            {
                target_addAnims = GUILayout.Toggle(target_addAnims, "Add Selected Animations");
            } else
            {
                target_addAttires = GUILayout.Toggle(target_addAttires, "Add Selected as Attires");
                target_addExpressions = GUILayout.Toggle(target_addExpressions, "Add Selected as Expressions");
            } 

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Name of the resulting GameObject:");
                target_name = EditorGUILayout.TextField(target_name);
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Create GameObject or Add Sprites/Animations to existing"))
            {
                //check if go exists, and if it has a VN_CharBase component
                //check to add which type 
                //check & add selected stuff if matches type 

                if (currentSetupTarget.target == null)
                {
                    currentSetupTarget.target = new GameObject(target_name);
                    currentSetupTarget.target.transform.parent = currentSetupTarget.target.transform.root;
                }

                if (currentSetupTarget.target.GetComponent< VN_CharBase>() == null)
                {
                    if (target_is3d)
                        currentSetupTarget.target.AddComponent<VN_3D_Character>();
                    else 
                        currentSetupTarget.target.AddComponent<VN_Character>();
                }

                if (target_is3d)
                {
                    currentSetupTarget.target.GetComponent<VN_3D_Character>();
                    if (target_addAnims)
                    {
                        Animation animComp = currentSetupTarget.target.AddComponent<Animation>();
                        foreach (object obj in Selection.objects)
                        {
                            if (obj is AnimationClip)
                            {
                                animComp.AddClip(obj as AnimationClip, (obj as AnimationClip).name);
                            }
                        }
                    }
                } else
                {
                    VN_Character char2d = currentSetupTarget.target.GetComponent<VN_Character>();

                    List<Sprite> spriteSelection = new List<Sprite>();
                    foreach (object obj in Selection.objects)
                    {
                        if (obj is Sprite)
                        {
                            spriteSelection.Add(obj as Sprite);
                        }
                    }
                    List<GameObject> instantiatedSprites = new List<GameObject>();
                    if (spriteSelection != null)
                    {
                        foreach (Sprite aSprite in spriteSelection)
                        {    
                            GameObject newSprite = new GameObject(aSprite.name);
                            SpriteRenderer spriteRen = newSprite.AddComponent<SpriteRenderer>();
                            spriteRen.sprite = aSprite;
                            newSprite.transform.parent = currentSetupTarget.target.transform;
                            instantiatedSprites.Add(newSprite);
                        }

                        if (target_addAttires)
                            char2d.attires = instantiatedSprites.ToArray();
                        if (target_addExpressions)
                            char2d.expressions = instantiatedSprites.ToArray();
                    }
                }

                currentSetupTarget = null;
            }

        } else
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
                                GUILayout.Label("type:");
                                targets [i].type = SequencerTargetTypes.targetTypes [EditorGUILayout.Popup(Array.IndexOf(SequencerTargetTypes.targetTypes, targets [i].type), SequencerTargetTypes.targetTypes)]; 
                                GUILayout.Label("target:");
                                targets [i].target = EditorGUILayout.ObjectField(targets [i].target, typeof(GameObject), true) as GameObject;
                                GUILayout.Label("Nickname:");
                                targets [i].nickname = GUILayout.TextField(targets [i].nickname);  
                                if (targets [i].type == SequencerTargetTypes.character)
                                {
                                    if (GUILayout.Button("Setup Character"))
                                    {
                                        currentSetupTarget = targets [i];
                                    }
                                }
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
    }

    private void drawSections()
    {
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("Sections");

            paging = GUILayout.Toggle(paging, "Use Paging:");

            if (GUILayout.Button("New Section"))
                doAddNewSection();
            
            if (GUILayout.Button("Duplicate Selected Section"))
            {
                if (sequencerData.sections != null && lastSelectedSection > -1)
                    doDuplicateSection(sequencerData.sections [lastSelectedSection]);
            }

            //developer fix tool
//            if (GUILayout.Button("Fix Commands"))
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

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("Filter by Command type:");
            lastSelectedCommandFilter = EditorGUILayout.Popup(lastSelectedCommandFilter, SequencerCommandTypes.getAsStringArray(), GUILayout.Width(100));
            
            doTypeFilter = GUILayout.Toggle(doTypeFilter, "Toggle Filter");
        }  
        EditorGUILayout.EndHorizontal(); 

        //spacer 
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        
        if (lastSelectedSection > -1 && lastSelectedSection < sequencerData.sections.Count && sequencerData.sections [lastSelectedSection] != null && sequencerData.sections [lastSelectedSection].commandList != null)
        {      
            scrollPosSections = EditorGUILayout.BeginScrollView(scrollPosSections);
            {                       
                int pageStartIndex = 0;
                int pageEndIndex;
                int totalPages = 1;

                SequencerCommandBase[] tempCommands = new SequencerCommandBase[sequencerData.sections [lastSelectedSection].commandList.Count]; 
                sequencerData.sections [lastSelectedSection].commandList.CopyTo(tempCommands);
                
                pageEndIndex = tempCommands.Length;
                
                if (paging)
                {
                    if (tempCommands.Length > pagingSize)
                    {
                        totalPages = Mathf.FloorToInt((float)tempCommands.Length / (float)pagingSize);
                    }
                    
                    if (totalPages > 1)
                    {
                        pageStartIndex = page * pagingSize;
                        pageEndIndex = Mathf.Min(pageStartIndex + pagingSize, tempCommands.Length); 
                    } else
                        page = 0;                    


                    if (totalPages > 1)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {   
                            if (GUILayout.Button("< previous page"))
                                page = Mathf.Max(0, page - 1);                                           
                            
                            GUILayout.Label(" Page: " + page + " out of " + totalPages);
                            
                            if (GUILayout.Button(" > next page"))
                                page = Mathf.Min(totalPages, page + 1);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.BeginVertical();
                {
                    for (int i = pageStartIndex; i < pageEndIndex; i++)
                    {
                        if (doTypeFilter && tempCommands [i].GetType() != SequencerCommandTypes.commandTypes [lastSelectedCommandFilter])
                            continue;

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

                if (paging)
                {
                    if (totalPages > 1)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {   
                            if (GUILayout.Button("< previous page"))
                                page = Mathf.Max(0, page - 1);                                           
                        
                            GUILayout.Label(" Page: " + page + " out of " + totalPages);
                        
                            if (GUILayout.Button(" > next page"))
                                page = Mathf.Min(totalPages, page + 1);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                } 
            }
            EditorGUILayout.EndScrollView(); 
        } else
        {
            lastSelectedSection -= 1;
        }
    }   

    #region stats
    private List<bool> sectionForStatistics = new List<bool>();
    private int stat_choices = 0;
    private int stat_speechBubbles = 0;
    private int stat_words = 0;
    private int stat_music = 0;
    private int stat_sfx = 0;
    private int stat_variables = 0;
    private int stat_characters = 0;

    private List<string> uniqueMusicList = new List<string>();
    private List<string> uniqueAudioList = new List<string>();
    private List<string> uniqueVariableList = new List<string>();
    private List<string> uniqueCharactersList = new List<string>();

    private void drawStatistics()
    {
        EditorGUILayout.BeginVertical();
        { 
            EditorGUILayout.LabelField("Number of sections: " + sequencerData.sections.Count);

            int length = sequencerData.sections.Count;
            for (int i = 0; i < length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (sectionForStatistics.Count == i)
                        sectionForStatistics.Add(true);
                    EditorGUILayout.LabelField(sequencerData.sections [i].name);
                    sectionForStatistics [i] = EditorGUILayout.Toggle(sectionForStatistics [i]);
                }
                EditorGUILayout.EndHorizontal(); 
            }

            if (GUILayout.Button("Generate Stats for selected sections"))
                generateStats();

            EditorGUILayout.LabelField("Branches/Choices: " + stat_choices.ToString());
            EditorGUILayout.LabelField("Speech Bubbles: " + stat_speechBubbles.ToString());
            EditorGUILayout.LabelField("Total Words: " + stat_words.ToString());
            EditorGUILayout.LabelField("Music Played: " + stat_music.ToString());
            EditorGUILayout.LabelField("SFX Played: " + stat_sfx.ToString());
            EditorGUILayout.LabelField("Variables Used: " + stat_variables.ToString());
            EditorGUILayout.LabelField("Characters Used: " + stat_characters.ToString());

            if (GUILayout.Button("Print stats to console for copy/paste"))
            {
                int selectedSections = 0;
                foreach (bool enabledSection in sectionForStatistics)
                    selectedSections += (enabledSection) ? 1 : 0;
                Debug.LogWarning("Selected Sections: " + selectedSections + "\n" +
                    "Branches/Choices: " + stat_choices.ToString() + "\n" +
                    "Speech Bubbles: " + stat_speechBubbles.ToString() + "\n" +
                    "Total Words: " + stat_words.ToString() + "\n" +
                    "Music Played: " + stat_music.ToString() + "\n" +
                    "SFX Played: " + stat_sfx.ToString() + "\n" +
                    "Variables Used: " + stat_variables.ToString() + "\n" +
                    "Characters Used: " + stat_characters.ToString()
                );
            }
        }   
        EditorGUILayout.EndVertical();  
    }

    private void generateStats()
    {
        //reset
        stat_choices = 0;
        stat_speechBubbles = 0;
        stat_words = 0;
        stat_music = 0;
        stat_sfx = 0;
        stat_variables = 0;
        stat_characters = 0;

        uniqueMusicList.Clear();
        uniqueAudioList.Clear();
        uniqueVariableList.Clear();
        uniqueCharactersList.Clear();

        int length = sequencerData.sections.Count;
        for (int i = 0; i < length; i++)
        {
            if (sectionForStatistics [i])
            {
                foreach (SequencerCommandBase seqCommand in sequencerData.sections[i].commandList)
                {
                    Type typeCommand = seqCommand.GetType();

                    if (typeCommand == typeof(SC_VN_Choice) || typeCommand == typeof(SC_VN_ExpressionJump))
                        stat_choices += 1;
                    if (typeCommand == typeof(SC_VN_Dialog))
                        stat_speechBubbles += 1;
                    if (typeCommand == typeof(SC_VN_Dialog))
                        stat_words += wordCount(((SC_VN_Dialog)seqCommand).text);
                    if (typeCommand == typeof(SC_PlayMusic))
                    {
                        string name = ((SC_PlayMusic)seqCommand).audioClipName;
                        if (name == null || name.Length == 0 || name == SoundManager.nullSoundName)
                            name = ((SC_PlayMusic)seqCommand).audioClip.name;

                        if (!uniqueMusicList.Contains(name))
                        {
                            uniqueMusicList.Add(name);
                            stat_music += 1;
                        }
                    }
                    if (typeCommand == typeof(SC_PlaySfx))
                    {
                        string name = ((SC_PlaySfx)seqCommand).audioClipName;
                        if (name == null || name.Length == 0 || name == SoundManager.nullSoundName)
                            name = ((SC_PlaySfx)seqCommand).audioClip.name;
                        
                        if (!uniqueAudioList.Contains(name))
                        {
                            uniqueAudioList.Add(name);
                            stat_sfx += 1;
                        }
                    }
                    if (typeCommand == typeof(SC_InputVariable) || typeCommand == typeof(SC_SetVariable))
                    {
                        string name = "";
                        if (typeCommand == typeof(SC_InputVariable))
                            name = ((SC_InputVariable)seqCommand).variableName;
                        else if (typeCommand == typeof(SC_SetVariable))
                            name = ((SC_SetVariable)seqCommand).variableName;

                        if (!uniqueVariableList.Contains(name))
                        {
                            uniqueVariableList.Add(name);
                            stat_variables += 1;
                        }
                    }
                    if (typeCommand == typeof(SC_VN_Show)) //TODO:  wouldnt really give you a better idea/number, would need a subtype -> // && sequencerData.getTargetModel(((SC_VN_Show)command).lastSelectedWho) == SequencerTargetTypes.character
                    {
                        string name = ((SC_VN_Show)seqCommand).lastSelectedWho;
                        
                        if (!uniqueCharactersList.Contains(name))
                        {
                            uniqueCharactersList.Add(name);
                            stat_characters += 1;
                        }
                    }   
                }
            }
        }
    }
    
    public int wordCount(string txtToCount)
    {
        string pattern = "\\w+";
        Regex regex = new Regex(pattern);
        
        int countedWords = regex.Matches(txtToCount).Count;
        
        return countedWords;
    }
    #endregion

    private void drawVariables()
    {
        EditorGUILayout.BeginVertical();
        {            
            EditorGUILayout.BeginHorizontal();
            { 
                GUILayout.Label("Variables");

                GUILayout.Label("Variable Name:");
                tempVarName = EditorGUILayout.TextField(tempVarName);
                GUILayout.Label("Value:");
                tempVarValue = EditorGUILayout.TextField(tempVarValue);

                if (GUILayout.Button("Add Variable"))
                {
                    if (sequencerData.getIndexOfVariable(tempVarName) == -1)
                    {
                        sequencerData.variables.Add(new SequencerVariableModel(tempVarName, tempVarValue));
                    }
                }
            }
            EditorGUILayout.EndHorizontal();    
   
            string[] variableNames = sequencerData.getNamesOfVariables();

            for (int i = 0; i < variableNames.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {  
                    GUILayout.Label("Name of Variable:");
                    GUILayout.Label(variableNames [i]);
                    
                    int varIndex = sequencerData.getIndexOfVariable(variableNames [i]);
                    if (varIndex > -1)
                    {
                        GUILayout.Label("Initial value:");
                        sequencerData.variables [varIndex].value = EditorGUILayout.TextField(sequencerData.variables [varIndex].value);
                        if (GUILayout.Button("Delete this variable"))
                        {
                            sequencerData.variables.RemoveAt(varIndex);
                        }
                    }  
                }
                EditorGUILayout.EndHorizontal();        
            }
        }   
        EditorGUILayout.EndVertical();  
    }

    private void drawImport()
    {
        EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Allow Creation of Character Target Stubs ");
                allowCharacterStubs = EditorGUILayout.Toggle(allowCharacterStubs); 
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Import");
        
                renpyTextAsset = EditorGUILayout.ObjectField(renpyTextAsset, typeof(TextAsset), true) as TextAsset;   

                if (GUILayout.Button("Import Ren'Py script"))
                    doImportRenPy(null);
            }   
            EditorGUILayout.EndHorizontal();

            GUILayout.Label("Paste a Ren'Py snippet (!If problems make sure it's Tab delimited!)");

            importTextAreaText = EditorGUILayout.TextArea(importTextAreaText, GUILayout.ExpandWidth(true), GUILayout.Height(100));

            if (GUILayout.Button("Import Ren'Py snippet into current selected section!"))
            {
                if (importTextAreaText.Length > 3)
                    doImportRenPy(importTextAreaText);
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Export file name: ");
                renpyExportFileName = EditorGUILayout.TextField(renpyExportFileName);
                if (GUILayout.Button("(Alpha Feature: Use at own risk!) Export to Ren'py txt file"))
                {
                    doExportRenpy(renpyExportFileName);
                }
            }
            EditorGUILayout.EndHorizontal();

        }
        EditorGUILayout.EndVertical();
    }

    private void doExportRenpy(string fileName)
    {
        SequencerRenPy renpyTranslator = new SequencerRenPy();
        renpyTranslator.export(fileName, sequencerData, lastSelectedSection);
    }

    private void doImportRenPy(string text)
    {
        if (renpyTextAsset != null || text != null)
        {
            SequencerRenPy renpyTranslator = new SequencerRenPy();
            if (renpyTextAsset == null)
                renpyTranslator.renpyToSequencer(text, sequencerData, lastSelectedSection, allowCharacterStubs);
            else
                renpyTranslator.renpyToSequencer(renpyTextAsset.text, sequencerData, allowCharacterStubs);
            Debug.LogWarning("Done!");
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

        sectionModel.rename(newName);
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
        ((SequencerCommandBase)temp).init(sequencerData.sections [sectionIndex].name, sequencerData);

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
        ((SequencerCommandBase)temp).init(sequencerData.sections [sectionIndex].name, sequencerData);
        
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
            cmd.init(newModel.name, sequencerData);
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

    void doExportSequencerFile()
    {
        string filePath = Application.dataPath + "/" + sequencerData.gameObject.name + ".txt";
        StreamWriter fileStream = new StreamWriter(filePath);

        fileStream.Write("TARGETS:\n");

        foreach (SequencerTargetModel target in sequencerData.targets)
        {
            fileStream.Write(target.nickname + "╫" + target.target.name + "╫" + target.type + "\n");
        }

        fileStream.Write("VARIABLES:\n");
        
        foreach (SequencerVariableModel variable in sequencerData.variables)
        {
            fileStream.Write(variable.name + "╫" + variable.value + "\n");
        }

        fileStream.Write("SECTIONS:\n");
        
        foreach (SequencerSectionModel section in sequencerData.sections)
        {
            fileStream.Write("section:╫" + section.name + "╫" + section.commandList.Count.ToString() + "\n");
            foreach (SequencerCommandBase command in section.commandList)
            {
                fileStream.Write(command.toSequncerSerializedString());
            }
        }

        fileStream.Close();
        
        Debug.LogWarning("Done, file at: " + filePath);
    }

    void doImportSequencerFile()
    {
        string[] splitFile = fileData.text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        GameObject newDataHolder = new GameObject("sequencerDataHolderObject");
        SequencerData data = newDataHolder.AddComponent<SequencerData>();

        string currentType = "";
        SequencerSectionModel lastSectionModel = null;
        for (int i=0; i < splitFile.Length; i++)
        {
            if (splitFile [i].Contains("TARGETS:"))
            {
                currentType = "targets";
                continue;
            } else if (splitFile [i].Contains("VARIABLES:"))
            {
                currentType = "variables";
                continue;
            } else if (splitFile [i].Contains("SECTIONS:"))
            {
                currentType = "sections";
                continue;
            }

            string[] splitLine = splitFile [i].Split(new char[] {'╫'});
             
            if (currentType == "targets")
            {
                SequencerTargetModel targetModel = new SequencerTargetModel();
                targetModel.nickname = splitLine [0];
                targetModel.type = splitLine [2];

                GameObject goalTarget = GameObject.Find(splitLine [1]);
                if (goalTarget != null)
                    targetModel.target = goalTarget;

                data.targets.Add(targetModel);
            } else if (currentType == "variables")
            {
                SequencerVariableModel variableModel = new SequencerVariableModel();
                variableModel.name = splitLine [0];
                variableModel.value = splitLine [1];

                data.variables.Add(variableModel);
            } else if (currentType == "sections")
            {
                if (splitLine [0] == "section:")
                {
                    lastSectionModel = new SequencerSectionModel();
                    lastSectionModel.commandList = new List<SequencerCommandBase>();
                    lastSectionModel.name = splitLine [1];
                    data.sections.Add(lastSectionModel);
                    continue;
                } else
                {
                    SequencerCommandBase command = ScriptableObject.CreateInstance(splitLine [0]) as SequencerCommandBase;
                    command.init(lastSectionModel.name, data);
                    command.initFromSequncerSerializedString(splitLine);
                    lastSectionModel.commandList.Add(command);
                }
            }
        }

        Debug.LogWarning("Done, importing file");
    }

    //developer function to force fix commands 
//    void doFixCommands()
//    {
//        foreach (SequencerSectionModel section in sequencerData.sections)
//        {
//            foreach (SequencerCommandBase command in section.commandList)
//            {
//                command.sequencerData = sequencerData;
//                command.sectionName = section.name;
//            } 
//        } 
//    }
}