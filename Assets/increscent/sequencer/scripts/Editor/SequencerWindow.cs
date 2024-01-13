using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SequencerWindow : EditorWindow
{
	static SequencerWindow thisWindowLive;
	Vector2 scrollPosTargets;
	Vector2 scrollPosSections;
	GameObject dataHolderValGoLastVal = null;
	TextAsset fileData;
	GameObject dataHolderGO = null;
	public SequencerData sequencerData;
	string lastSelectedArea = "targets"; //can be "targets" , "sections", "import", "variables", "statistics"

	int lastSelectedSection = -1;
	string renameBoxString = "";
	int lastSelectedCommand = 0;
	int lastSelectedCommandFilter = 0;
	bool doTypeFilter = false;

	GUIStyle guiBGDisabledColor;
	Texture2D littleTextureForDisabledBG;
	GUIStyle guiBGAltColorA;
	Texture2D littleTextureForBG_A;
	GUIStyle guiBGAltColorB;
	Texture2D littleTextureForBG_B;
	GUIStyle guiBGStubColor;
	Texture2D littleTextureForStub;
	GUIStyle guiBGDropColor;
	Texture2D littleTextureForBG_Drop;
	GUIStyle guiBGDragColor;
	Texture2D littleTextureForBG_Drag;

	int insertIndex = 0;

	TextAsset renpyTextAsset;
	string importTextAreaText = "";

	public bool paging = true;
	int pagingSize = 100;
	int page = 0;

	string tempVarName = "variableX";
	string tempVarValue = "42";

	SequencerTargetModel currentSetupTarget = null;

	bool allowCharacterStubs = false;
	string renpyExportFileName = "exportRenpy";

	public static RenameTargetWindow renameTargetWindowLive;
	public static RenameVariableWindow renameVariableWindowLive;

	public static ReplaceTargetWindow replaceTargetWindowLive;
	public static ReplaceSectionWindow replaceSectionWindowLive;
	public static ReplaceVariableWindow replaceVariableWindowLive;

	bool drawMinimized = false;

	public class CustomDragData
	{
		public int originalIndex;
	}

	int drawDragPreview = -1;
	int futureDragPreviewIndex = -1;

	List<int> futureMultiDragIndexes = new List<int>();
	List<int> multiDragIndexes = new List<int>();

	Rect dropRect;
	Rect lastCommandRect;

	#region Window Stuff
	void OnDisable()
	{
		DestroyImmediate(littleTextureForBG_A);
		DestroyImmediate(littleTextureForBG_B);
		DestroyImmediate(littleTextureForDisabledBG);
		DestroyImmediate(littleTextureForStub);
	}

	void OnDestroy()
	{
		DestroyImmediate(littleTextureForBG_A);
		DestroyImmediate(littleTextureForBG_B);
		DestroyImmediate(littleTextureForDisabledBG);
		DestroyImmediate(littleTextureForStub);
	}

	void OnEnable()
	{
		if (dataHolderGO != null)
		{
			testForData();
		}

		setColors();
	}

	void setColors()
	{
		if (sequencerData == null)
			return;

		littleTextureForStub = new Texture2D(1, 1);
		littleTextureForStub.SetPixel(0, 0, sequencerData.stubColor);
		littleTextureForStub.Apply();
		littleTextureForStub.hideFlags = HideFlags.HideAndDontSave;
		guiBGStubColor = new GUIStyle();
		guiBGStubColor.normal.background = littleTextureForStub;

		littleTextureForDisabledBG = new Texture2D(1, 1);
		littleTextureForDisabledBG.SetPixel(0, 0, sequencerData.disabledColor);
		littleTextureForDisabledBG.Apply();
		littleTextureForDisabledBG.hideFlags = HideFlags.HideAndDontSave;
		guiBGDisabledColor = new GUIStyle();
		guiBGDisabledColor.normal.background = littleTextureForDisabledBG;

		littleTextureForBG_A = new Texture2D(1, 1);
		littleTextureForBG_A.SetPixel(0, 0, sequencerData.normalRowColor);
		littleTextureForBG_A.Apply();
		littleTextureForBG_A.hideFlags = HideFlags.HideAndDontSave;
		guiBGAltColorA = new GUIStyle();
		guiBGAltColorA.normal.background = littleTextureForBG_A;

		littleTextureForBG_B = new Texture2D(1, 1);
		littleTextureForBG_B.SetPixel(0, 0, sequencerData.normalAltRowColor);
		littleTextureForBG_B.Apply();
		littleTextureForBG_B.hideFlags = HideFlags.HideAndDontSave;
		guiBGAltColorB = new GUIStyle();
		guiBGAltColorB.normal.background = littleTextureForBG_B;

		littleTextureForBG_Drop = new Texture2D(1, 1);
		littleTextureForBG_Drop.SetPixel(0, 0, sequencerData.dropColor);
		littleTextureForBG_Drop.Apply();
		littleTextureForBG_Drop.hideFlags = HideFlags.HideAndDontSave;
		guiBGDropColor = new GUIStyle();
		guiBGDropColor.normal.background = littleTextureForBG_Drop;

		littleTextureForBG_Drag = new Texture2D(1, 1);
		littleTextureForBG_Drag.SetPixel(0, 0, sequencerData.dragColor);
		littleTextureForBG_Drag.Apply();
		littleTextureForBG_Drag.hideFlags = HideFlags.HideAndDontSave;
		guiBGDragColor = new GUIStyle();
		guiBGDragColor.normal.background = littleTextureForBG_Drag;

		pagingSize = sequencerData.pagingSize;

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
				else if (lastSelectedArea == "settings")
					drawSettings();
			}
		}
		EditorGUILayout.EndVertical();

	}

	void testForData()
	{
		if (dataHolderGO != null && sequencerData == null)
		{
			sequencerData = dataHolderGO.GetComponent<SequencerData>();

			if (sequencerData == null)
			{
				sequencerData = dataHolderGO.AddComponent<SequencerData>();
			}

			if (sequencerData != null)
			{
				setColors();
			}
		}
	}

	void drawDataHolder()
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
			}
			else
			{
				fileData = EditorGUILayout.ObjectField(fileData, typeof(TextAsset), true) as TextAsset;
				if (GUILayout.Button("Import data into scene") && fileData != null)
				{
					doImportSequencerFile();
				}
			}

			if (GUILayout.Button("Force Dirty Scene"))
				makeSceneDirty();
		}
		EditorGUILayout.EndHorizontal();
	}


    public static GameObject[] GetDontDestroyOnLoadObjects()
    {
		//makes a temp object, which we assing to dont destory on load
		//we then get the "DontDestroyOnLoad" scene from temp object
		//we then erase the temp object and get root objects from scene
        GameObject temp = null;
        try
        {
            temp = new GameObject();
            UnityEngine.Object.DontDestroyOnLoad( temp );
            UnityEngine.SceneManagement.Scene dontDestroyOnLoad = temp.scene;
            UnityEngine.Object.DestroyImmediate( temp );
            temp = null;
     
            return dontDestroyOnLoad.GetRootGameObjects();
        }
        finally
        {
            if( temp != null )
                UnityEngine.Object.DestroyImmediate( temp );
        }
    }

	GameObject findDataHolderGo()
	{
		SequencerData data = SceneView.FindObjectOfType<SequencerData>();
		if (data != null)
			return data.gameObject;

		// check even disabled objs, can upgrade this to use above passing true flag, in version 2020
		// get root objects for all open scenes
		List<GameObject[]> rootObjsPerScene = new List<GameObject[]>();
		for ( int i=0; i < EditorSceneManager.sceneCount ; i++){
			rootObjsPerScene.Add(EditorSceneManager.GetSceneAt(i).GetRootGameObjects());
		}
		//add dont destroy on load scene too
		if (Application.isPlaying)
			rootObjsPerScene.Add(GetDontDestroyOnLoadObjects());

		// go through each scenes root objs
		foreach( GameObject[] rootObjs in rootObjsPerScene){
			foreach (GameObject obj in rootObjs)
			{
				foreach (SequencerData child in obj.GetComponentsInChildren<SequencerData>(true)){
					return child.gameObject;
				}
			}
		}

		return null;
	}

	void drawUiAreas()
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

			if (GUILayout.Button("Import & Export"))
			{
				lastSelectedArea = "import";
			}

			if (GUILayout.Button("Statistics"))
			{
				lastSelectedArea = "statistics";
			}

			if (GUILayout.Button("Settings"))
			{
				lastSelectedArea = "settings";
			}
		}
		EditorGUILayout.EndHorizontal();
	}

	bool target_is3d = false;
	bool target_addAnims = false;
	bool target_addAttires = false;
	bool target_addExpressions = false;
	string target_name = "newCharacter_";

	void drawDefineTargets()
	{
		//setup characters view
		if (currentSetupTarget != null)
		{
			target_is3d = GUILayout.Toggle(target_is3d, "3D character");

			if (target_is3d)
			{
				target_addAnims = GUILayout.Toggle(target_addAnims, "Add Selected Animations");
			}
			else
			{
				target_addAttires = GUILayout.Toggle(target_addAttires, "Add Selected as Attires");
				target_addExpressions = GUILayout.Toggle(target_addExpressions, "Add Selected as Expressions ( backgrounds use this one)");
			}

			if (currentSetupTarget.target == null)
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Name of the resulting GameObject:");
					target_name = EditorGUILayout.TextField(target_name);
				}
				GUILayout.EndHorizontal();
			}

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

				if (currentSetupTarget.target.GetComponent<VN_CharBase>() == null)
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
						Animation animComp = currentSetupTarget.target.GetComponent<Animation>();
						if (animComp == null)
							animComp = currentSetupTarget.target.AddComponent<Animation>();

						foreach (object obj in Selection.objects)
						{
							if (obj is AnimationClip)
							{
								AnimationClip clip = obj as AnimationClip;

								bool alreadyHasClip = false;
								for (int i = 0, max = animComp.GetClipCount(); i < max; i++)
								{
									if (animComp.GetClip(clip.name) != null)
									{
										alreadyHasClip = true;
									}
								}

								if (!alreadyHasClip)
									animComp.AddClip(obj as AnimationClip, (obj as AnimationClip).name);
							}
						}
					}
				}
				else
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

		}
		//normal define targets view
		else
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
								targets[i].type = SequencerTargetTypes.targetTypes[EditorGUILayout.Popup(Array.IndexOf(SequencerTargetTypes.targetTypes, targets[i].type), SequencerTargetTypes.targetTypes)];
								GUILayout.Label("target:");
								targets[i].target = EditorGUILayout.ObjectField(targets[i].target, typeof(GameObject), true) as GameObject;
								GUILayout.Label("Nickname:");
								GUILayout.Label(targets[i].nickname);
								if (targets[i].type == SequencerTargetTypes.character)
								{
									if (GUILayout.Button("Setup Character"))
									{
										currentSetupTarget = targets[i];
									}
								}
								if (GUILayout.Button("Rename"))
								{
									doRenameTarget(targets[i]);
								}
								if (GUILayout.Button("Delete/Replace"))
									doDeleteTarget(targets[i]);
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

	void drawSections()
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
					doDuplicateSection(sequencerData.sections[lastSelectedSection]);
			}

			//developer fix tool
			//            if (GUILayout.Button("Fix Commands"))
			//                doFixCommands();
		}
		EditorGUILayout.EndHorizontal();

		if (sequencerData != null && sequencerData.getSectionNames().Length > 0)
		{
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Label("Select Section:");
				lastSelectedSection = EditorGUILayout.Popup(lastSelectedSection, sequencerData.getSectionNames(), GUILayout.Width(100));

				GUILayout.Label("Rename to this:");
				renameBoxString = GUILayout.TextField(renameBoxString);

				if (GUILayout.Button("Rename this Section"))
					doRenameSection(sequencerData.sections[lastSelectedSection], renameBoxString);

				if (GUILayout.Button("Delete this Section"))
					doDeleteSection(sequencerData.getSectionNames()[lastSelectedSection]);
			}
			EditorGUILayout.EndHorizontal();
		}

		//spacer 
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

		if (lastSelectedSection > -1 && lastSelectedSection < sequencerData.sections.Count && sequencerData.sections[lastSelectedSection] != null)
		{
			if (lastSelectedSection > -1)
			{
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Available Commands:");
					lastSelectedCommand = EditorGUILayout.Popup(lastSelectedCommand, SequencerCommandTypes.getAsStringArray(), GUILayout.Width(100));

					if (GUILayout.Button("Add Command"))
						doAddCommandToSection(lastSelectedSection, lastSelectedCommand);

					insertIndex = EditorGUILayout.IntField(insertIndex);
					if (GUILayout.Button("Add Command At"))
						doAddCommandToSectionAt(lastSelectedSection, lastSelectedCommand, insertIndex);

                    if (GUILayout.Button("Delete Selected Command(s)"))
                        doDeleteSelectedCommands(lastSelectedSection, lastSelectedCommand);
                }
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Display Commands options:");
					lastSelectedCommandFilter = EditorGUILayout.Popup(lastSelectedCommandFilter, SequencerCommandTypes.getAsStringArray(), GUILayout.Width(100));
					doTypeFilter = GUILayout.Toggle(doTypeFilter, "Toggle Filter");
					drawMinimized = GUILayout.Toggle(drawMinimized, "Draw Minimized");
				}
				EditorGUILayout.EndHorizontal();

			}
		}

		//spacer 
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

		if (lastSelectedSection > -1 && lastSelectedSection < sequencerData.sections.Count && sequencerData.sections[lastSelectedSection] != null && sequencerData.sections[lastSelectedSection].commandList != null)
		{
			scrollPosSections = EditorGUILayout.BeginScrollView(scrollPosSections);
			{
				int pageStartIndex = 0;
				int pageEndIndex;
				int totalPages = 1;

				SequencerCommandBase[] tempCommands = new SequencerCommandBase[sequencerData.sections[lastSelectedSection].commandList.Count];
				sequencerData.sections[lastSelectedSection].commandList.CopyTo(tempCommands);

				pageEndIndex = tempCommands.Length;

				if (paging)
				{
					if (tempCommands.Length > pagingSize)
					{
						totalPages = Mathf.CeilToInt((float)tempCommands.Length / (float)pagingSize);
					}

					if (totalPages > 1)
					{
						pageStartIndex = page * pagingSize;
						pageEndIndex = Mathf.Min(pageStartIndex + pagingSize, tempCommands.Length);
					}
					else
						page = 0;


					if (totalPages > 1)
					{
						EditorGUILayout.BeginHorizontal();
						{
							if (GUILayout.Button("< previous page"))
								page = Mathf.Max(0, page - 1);

							GUILayout.Label(" Page: " + (page + 1) + " / " + totalPages);

							if (GUILayout.Button(" > next page"))
								page = Mathf.Min(totalPages - 1, page + 1);
						}
						EditorGUILayout.EndHorizontal();
					}
				}

				//Rect commandsArea = 
				EditorGUILayout.BeginVertical();
				{
					if (Event.current.type == EventType.Layout)
						applyLayoutChanges();

					for (int i = pageStartIndex; i < pageEndIndex; i++)
					{
						if (doTypeFilter && tempCommands[i].GetType() != SequencerCommandTypes.commandTypes[lastSelectedCommandFilter])
							continue;

						if (multiDragIndexes.Count > 0 && multiDragIndexes.Contains(i))
							lastCommandRect = EditorGUILayout.BeginHorizontal(guiBGDragColor);
						else if (draggedIndex == i)
							lastCommandRect = EditorGUILayout.BeginHorizontal(guiBGDragColor);
						else if (!tempCommands[i].isEnabled)
							lastCommandRect = EditorGUILayout.BeginHorizontal(guiBGDisabledColor);
						else if (tempCommands[i].commandId == "SC_Stub")
							lastCommandRect = EditorGUILayout.BeginHorizontal(guiBGStubColor);
						else if (i % 2 == 0)
							lastCommandRect = EditorGUILayout.BeginHorizontal(guiBGAltColorA);
						else
							lastCommandRect = EditorGUILayout.BeginHorizontal();

						tempCommands[i].drawFrontUi();
						if ( drawMinimized )
							tempCommands[i].drawMinimizedUi();
						else
							tempCommands[i].drawCustomUi();

						tempCommands[i].drawBackUi();

						EditorGUILayout.EndHorizontal();

						makeDraggable(lastCommandRect, i);

						//Draw drag drop preview below
						if (drawDragPreview == i && draggedIndex != -1 && i != draggedIndex)
						{
							int dropPreviewCount = 1;

							if (multiDragIndexes.Count > 0)
								dropPreviewCount = multiDragIndexes.Count;

							EditorGUILayout.BeginVertical();
							{
								for (int count = 0; count < dropPreviewCount; count++)
								{
									dropRect = EditorGUILayout.BeginHorizontal(guiBGDropColor);
									{
										GUILayout.Box("Will be dropped here! (always dropped below selected)", GUILayout.ExpandWidth(true), GUILayout.Height(40));
									}
									EditorGUILayout.EndHorizontal();

									makeDroppable(dropRect, i);
								}
							}
							EditorGUILayout.EndVertical();
						}
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

							GUILayout.Label(" Page: " + (page + 1) + " / " + totalPages);

							if (GUILayout.Button(" > next page"))
								page = Mathf.Min(totalPages - 1, page + 1);
						}
						EditorGUILayout.EndHorizontal();
					}
				}
			}
			EditorGUILayout.EndScrollView();
		}
		else
		{
			lastSelectedSection -= 1;
		}
	}

	void applyLayoutChanges()
	{
		if (futureDragPreviewIndex != -1)
		{
			drawDragPreview = futureDragPreviewIndex;
			futureDragPreviewIndex = -1;
		}

		multiDragIndexes = futureMultiDragIndexes;
	}

	void drawSettings()
	{
		EditorGUILayout.BeginVertical();
		{
			sequencerData.disabledColor = EditorGUILayout.ColorField("Disabled Color", sequencerData.disabledColor);
			sequencerData.stubColor = EditorGUILayout.ColorField("Stub Color", sequencerData.stubColor);
			sequencerData.normalRowColor = EditorGUILayout.ColorField("Normal Row Color", sequencerData.normalRowColor);
			sequencerData.normalAltRowColor = EditorGUILayout.ColorField("Alt Row Color", sequencerData.normalAltRowColor);
			sequencerData.dragColor = EditorGUILayout.ColorField("Drag Color", sequencerData.dragColor);
			sequencerData.dropColor = EditorGUILayout.ColorField("Drop Color", sequencerData.dropColor);

			sequencerData.pagingSize = EditorGUILayout.IntField("Paging Size", sequencerData.pagingSize);

			EditorGUILayout.Space();

			if (GUILayout.Button("Apply"))
			{
				setColors();
			}

            EditorGUILayout.Space();
            EditorGUILayout.Space();

			if (GUILayout.Button("Update UIDs"))
			{
				updateUIDS();
			}
        }
		EditorGUILayout.EndVertical();
	}

	#region stats
	List<bool> sectionForStatistics = new List<bool>();
	int stat_choices = 0;
	int stat_speechBubbles = 0;
	int stat_words = 0;
	int stat_music = 0;
	int stat_sfx = 0;
	int stat_variables = 0;
	int stat_characters = 0;

	List<string> uniqueMusicList = new List<string>();
	List<string> uniqueAudioList = new List<string>();
	List<string> uniqueVariableList = new List<string>();
	List<string> uniqueCharactersList = new List<string>();

	void drawStatistics()
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
					EditorGUILayout.LabelField(sequencerData.sections[i].name);
					sectionForStatistics[i] = EditorGUILayout.Toggle(sectionForStatistics[i]);
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

	void generateStats()
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
			if (sectionForStatistics[i])
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

	void drawVariables()
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
					GUILayout.Label(variableNames[i]);

					int varIndex = sequencerData.getIndexOfVariable(variableNames[i]);
					if (varIndex > -1)
					{
						GUILayout.Label("Initial value:");
						sequencerData.variables[varIndex].value = EditorGUILayout.TextField(sequencerData.variables[varIndex].value);
						if (GUILayout.Button("Rename this variable"))
						{
							doRenameVariable(variableNames[i]);
						}

						if (GUILayout.Button("Delete this variable"))
						{
							doDeleteVariable(variableNames[i]);
						}

					}
				}
				EditorGUILayout.EndHorizontal();
			}
		}
		EditorGUILayout.EndVertical();
	}

	void drawImport()
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

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Export Dialog file name: ");
                renpyExportFileName = EditorGUILayout.TextField(renpyExportFileName);
                if (GUILayout.Button("Export Dialog & VO file"))
                {
                    doExportDialogAndVOFile(renpyExportFileName);
                }

                fileData = EditorGUILayout.ObjectField(fileData, typeof(TextAsset), true) as TextAsset;
                if (GUILayout.Button("Import data into scene") && fileData != null)
                {
                    doImportDialogVOFile();
                }
            }
            EditorGUILayout.EndHorizontal();



        }
		EditorGUILayout.EndVertical();
	}

	void doExportRenpy(string fileName)
	{
		SequencerRenPy renpyTranslator = new SequencerRenPy();
		renpyTranslator.export(fileName, sequencerData, lastSelectedSection);
	}

	void doImportRenPy(string text)
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

		makeSceneDirty();
	}

	//note this deletes all found with same target ( in general we dont want duplicate targets anyway)
	void doDeleteTarget(SequencerTargetModel targetModel)
	{
		//check if target is being used, if it is, then pop replace dialog window
		int numberOfRefrences = 0;

		//basically we do a rename to the same name, to check if there are any references, 
		//I should probably fix this so it doesnt actually rename, just test by adding a flag to that function ?
		foreach (SequencerSectionModel sectionModel in sequencerData.sections)
		{
			foreach (SequencerCommandBase command in sectionModel.commandList)
			{
				bool wasUpdated = command.updateTargetReference(targetModel.nickname, targetModel.nickname);

				if (wasUpdated)
					numberOfRefrences += 1;
			}
		}

		if (numberOfRefrences > 0)
		{
			replaceTargetWindowLive = (ReplaceTargetWindow)EditorWindow.GetWindow(typeof(ReplaceTargetWindow));
			replaceTargetWindowLive.toReplaceTargetModel = targetModel;
			replaceTargetWindowLive.data = sequencerData;
		}
		else
			sequencerData.targets.Remove(targetModel);

		makeSceneDirty();
	}

	//note this deletes all found with same name ( in general we dont want duplicates names anyway)
	void doDeleteSection(string sectionName)
	{
		//check if there are any references first
		int numberOfRefrences = 0;

		foreach (SequencerSectionModel sectionModel in sequencerData.sections)
		{
			foreach (SequencerCommandBase command in sectionModel.commandList)
			{
				bool wasUpdated = command.updateSectionReference(sectionName, sectionName);

				if (wasUpdated)
					numberOfRefrences += 1;
			}
		}

		//if there are references then show replace window
		if (numberOfRefrences > 0)
		{
			doReplaceSection(sectionName);
		}
		//do delete since no references
		else
		{
			for (int i = sequencerData.sections.Count - 1; i > -1; i--)
			{
				if (sectionName == sequencerData.sections[i].name)
				{
					SequencerSectionModel section = sequencerData.sections[i];
					sequencerData.sections.Remove(sequencerData.sections[i]);

					foreach (SequencerCommandBase command in section.commandList)
					{
						DestroyImmediate(command);
					}
				}
			}
		}

		makeSceneDirty();
	}

	void doRenameSection(SequencerSectionModel renameSectionModel, string newName)
	{
		//cant be blank name
		newName = newName.Trim();
		if (newName == null || newName.Length == 0)
		{
			Debug.Log("Cant rename to blank!");
			return;
		}

		//check for unique section name
		foreach (SequencerSectionModel model in sequencerData.sections)
		{
			if (renameSectionModel != model && model.name == newName)
			{
				Debug.LogWarning("Could not rename to " + newName + " because another section is already named that way !");
				return;
			}
		}

		int numberOfRefrences = 0;
		//make sure references are updated in all sections
		foreach (SequencerSectionModel sectionModel in sequencerData.sections)
		{
			foreach (SequencerCommandBase command in sectionModel.commandList)
			{
				bool wasUpdated = command.updateSectionReference(renameSectionModel.name, newName);

				if (wasUpdated)
					numberOfRefrences += 1;
			}
		}

		Debug.Log("Updated " + numberOfRefrences + " to this sections.");

		//do rename
		renameSectionModel.rename(newName);

		makeSceneDirty();
	}

	void doAddCommandToSection(int sectionIndex, int typeIndex)
	{
		if (sectionIndex == -1 || typeIndex == -1)
		{
			Debug.LogWarning("Make sure a section & command is selected");
			return;
		}

		Type type = SequencerCommandTypes.commandTypes[typeIndex];
		ScriptableObject temp = ScriptableObject.CreateInstance(type);
		((SequencerCommandBase)temp).init(sequencerData.sections[sectionIndex].name, sequencerData);

		sequencerData.sections[sectionIndex].commandList.Add((SequencerCommandBase)temp);

		((SequencerCommandBase)temp).updateAllIndex();

		makeSceneDirty();
	}

	void doDeleteSelectedCommands(int sectionIndex, int lastSelectedIndex)
	{
        if (multiDragIndexes.Count > 0)
		{
			multiDragIndexes.Sort();
			//delete from last index so following index doesnt change
			for(int index= multiDragIndexes.Count-1; index>=0; index--)
			{
				sequencerData.sections[sectionIndex].commandList[multiDragIndexes[index]].deleteThisCommand();
            }

			multiDragIndexes.Clear();
		}else if ( lastSelectedIndex > 0)
		{
			sequencerData.sections[sectionIndex].commandList[lastSelectedIndex].deleteThisCommand();
			lastSelectedCommand = -1;
		}
    }	

	void doAddCommandToSectionAt(int sectionIndex, int typeIndex, int insertIndex)
	{
		if (sectionIndex == -1 || typeIndex == -1)
		{
			Debug.LogWarning("Make sure a section & command is selected");
			return;
		}

		Type type = SequencerCommandTypes.commandTypes[typeIndex];
		ScriptableObject temp = ScriptableObject.CreateInstance(type);
		((SequencerCommandBase)temp).init(sequencerData.sections[sectionIndex].name, sequencerData);

		insertIndex = Mathf.Clamp(insertIndex, 0, sequencerData.sections[sectionIndex].commandList.Count);

		sequencerData.sections[sectionIndex].commandList.Insert(insertIndex, (SequencerCommandBase)temp);

		makeSceneDirty();
	}

	void doAddNewTarget()
	{
		SequencerTargetModel model = new SequencerTargetModel();
		model.nickname = "target_" + UnityEngine.Random.Range(0, int.MaxValue).ToString();
		sequencerData.targets.Add(model);

		makeSceneDirty();
	}

	void doAddNewSection()
	{
		SequencerSectionModel model = new SequencerSectionModel();
		model.name = "section_" + UnityEngine.Random.Range(0, int.MaxValue).ToString();
		model.commandList = new List<SequencerCommandBase>();
		sequencerData.sections.Add(model);
		lastSelectedSection = sequencerData.sections.Count - 1;

		makeSceneDirty();
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

		makeSceneDirty();
	}

	void Update()
	{
		if (dataHolderValGoLastVal != dataHolderGO)
		{
			dataHolderValGoLastVal = dataHolderGO;
			testForData();
		}

		Repaint();
	}

	void doExportDialogAndVOFile( string filename)
	{
        string filePath = Application.dataPath + "/" + filename + "_dialog.txt";
        StreamWriter fileStream = new StreamWriter(filePath);

        foreach (SequencerSectionModel section in sequencerData.sections)
        {
            fileStream.Write("Scene: " + section.name + Environment.NewLine);
            foreach (SequencerCommandBase command in section.commandList)
            {
                Type typeCommand = command.GetType();

				if (typeCommand == typeof(SC_VN_Choice))
				{
					SC_VN_Choice choice = (SC_VN_Choice) command;
					fileStream.Write("Choice: " + choice.size + Environment.NewLine);
					for(int i =0; i < choice.size; i++)
					{
						fileStream.Write(choice.uids[i].ToString() + " " + choice.optionTextList[i] + " ╫ " + Environment.NewLine);			
					}
				}
				else if ( typeCommand == typeof(SC_VN_Dialog))
				{
                    SC_VN_Dialog dia = (SC_VN_Dialog)command;
                    fileStream.Write("Dialog: " + dia.speakerTargetName + Environment.NewLine);
					fileStream.Write(dia.uid + " ╫ " + dia.audioClipName + " ╫ " + dia.text + " ╫ " + Environment.NewLine);
                }
            }

            fileStream.Write(Environment.NewLine);
        }

        fileStream.Close();

        Debug.LogWarning("Done, file at: " + filePath);
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

    public Stream GenerateStreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    struct DialogSwitchTarget
    {
        public string sceneName;
        public uint uid;
        public string text;
        public string audioName;
    }
    public string RemoveWhitespace(string str)
    {
        return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
    }

    void doImportDialogVOFile()
	{
		Dictionary<uint, DialogSwitchTarget> uidToData = new();

		using (var stream = GenerateStreamFromString(fileData.text))
        {
			StreamReader reader = new StreamReader(stream);

            string line;
			string currSceneName = "";
			// Read line by line
			while ((line = reader.ReadLine()) != null)
			{
                //Choice: size, Dialog: target, Scene: name
                if (line.Contains("Scene:"))
                {
                    string[] split = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);

                    currSceneName = split[1];
                }
                else if (line.Contains("Choice:"))
                {
					// parsing this:
                    // fileStream.Write(choice.uids[i].ToString() + " " + choice.optionTextList[i] + " ╫ " + Environment.NewLine);
                    
					string[] split = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);

                    uint size = uint.Parse(split[1]);

					for(int i=0; i < size; i++)
					{
						int index = 0;
						int spaceIndex = 0;
                        List<char> uidChars = new List<char>();
						List<char> choiceTextChars = new List<char>();
                        while (reader.Peek() >= 0)
                        {
                            char c = (char)reader.Read();

                            if (c == '╫')
                            {
                                break;
                            }
							else if ( c == ' ')
							{
								spaceIndex = index;
								continue;
							}

							if ( spaceIndex == 0)
							{
								uidChars.Add(c);
							}
							else
							{
								choiceTextChars.Add(c);
							}
							
							index++;
                        }

						DialogSwitchTarget switchTar = new();
                        switchTar.text = new string(choiceTextChars.ToArray());
						switchTar.sceneName = currSceneName;
						switchTar.uid = uint.Parse(uidChars.ToArray());
						uidToData[switchTar.uid] = switchTar;
                    }
				}
				else if (line.Contains("Dialog:"))
				{
					// parsing this:
                    // fileStream.Write(dia.uid + " ╫ " + dia.audioClipName + " ╫ " + dia.text + " ╫ " + Environment.NewLine);

                    int delimeterCount = 0;
                    List<char> uidChars = new List<char>();
					List<char> audioChars = new List<char>();
                    List<char> textChars = new List<char>();
                    while (reader.Peek() >= 0)
                    {
                        char c = (char)reader.Read();

                        if (c == '╫')
                        {
							delimeterCount++;
							if ( delimeterCount == 3)
							{
								break;
							}
							reader.Read();
                            continue;
                        }
                       
                        if (delimeterCount == 0)
                        {
                            uidChars.Add(c);
                        }
                        else if (delimeterCount == 1)
                        {
                            audioChars.Add(c);
                        }else if (delimeterCount == 2)
						{
                            textChars.Add(c);
						}
                    }

					DialogSwitchTarget switchTar = new();
					switchTar.text = new string(textChars.ToArray());
					switchTar.audioName = RemoveWhitespace(new string(audioChars.ToArray()));
					switchTar.sceneName = currSceneName;
					switchTar.uid = uint.Parse(uidChars.ToArray());

                    uidToData[switchTar.uid] = switchTar;
                }
            }
        }

        //actually replace dialogs + VO 
        foreach (SequencerSectionModel section in sequencerData.sections)
        {
            foreach (SequencerCommandBase command in section.commandList)
            {
                Type typeCommand = command.GetType();

                if (typeCommand == typeof(SC_VN_Choice))
                {
                    SC_VN_Choice choice = (SC_VN_Choice)command;
                    for (int i = 0; i < choice.size; i++)
                    {
						choice.optionTextList[i] = uidToData[choice.uids[i]].text;
                    }
                }
                else if (typeCommand == typeof(SC_VN_Dialog))
                {
                    SC_VN_Dialog dia = (SC_VN_Dialog)command;
					dia.text = uidToData[dia.uid].text;
					dia.audioClipName = uidToData[dia.uid].audioName;
                }
            }
        }

        Debug.Log("Finished import of Dialog file!");
	}

    void doImportSequencerFile()
	{
		string[] splitFile = fileData.text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

		GameObject newDataHolder = new GameObject("sequencerDataHolderObject");
		SequencerData data = newDataHolder.AddComponent<SequencerData>();

		string currentType = "";
		SequencerSectionModel lastSectionModel = null;
		for (int i = 0; i < splitFile.Length; i++)
		{
			if (splitFile[i].Contains("TARGETS:"))
			{
				currentType = "targets";
				continue;
			}
			else if (splitFile[i].Contains("VARIABLES:"))
			{
				currentType = "variables";
				continue;
			}
			else if (splitFile[i].Contains("SECTIONS:"))
			{
				currentType = "sections";
				continue;
			}

			string[] splitLine = splitFile[i].Split(new char[] { '╫' });

			if (currentType == "targets")
			{
				SequencerTargetModel targetModel = new SequencerTargetModel();
				targetModel.nickname = splitLine[0];
				targetModel.type = splitLine[2];

				GameObject goalTarget = GameObject.Find(splitLine[1]);
				if (goalTarget != null)
					targetModel.target = goalTarget;

				data.targets.Add(targetModel);
			}
			else if (currentType == "variables")
			{
				SequencerVariableModel variableModel = new SequencerVariableModel();
				variableModel.name = splitLine[0];
				variableModel.value = splitLine[1];

				data.variables.Add(variableModel);
			}
			else if (currentType == "sections")
			{
				if (splitLine[0] == "section:")
				{
					lastSectionModel = new SequencerSectionModel();
					lastSectionModel.commandList = new List<SequencerCommandBase>();
					lastSectionModel.name = splitLine[1];
					data.sections.Add(lastSectionModel);
					continue;
				}
				else
				{
					SequencerCommandBase command = ScriptableObject.CreateInstance(splitLine[0]) as SequencerCommandBase;
					command.init(lastSectionModel.name, data);
					command.initFromSequncerSerializedString(splitLine);
					lastSectionModel.commandList.Add(command);
				}
			}
		}

		Debug.LogWarning("Done, importing file");

		makeSceneDirty();
	}

	void doRenameTarget(SequencerTargetModel toRenameTargetModel)
	{
		renameTargetWindowLive = (RenameTargetWindow)EditorWindow.GetWindow(typeof(RenameTargetWindow));
		renameTargetWindowLive.toRenameTargetModel = toRenameTargetModel;
		renameTargetWindowLive.data = sequencerData;

		makeSceneDirty();
	}

	void doReplaceSection(string sectionToReplace)
	{
		replaceSectionWindowLive = (ReplaceSectionWindow)EditorWindow.GetWindow(typeof(ReplaceSectionWindow));
		replaceSectionWindowLive.toReplaceSectionName = sectionToReplace;
		replaceSectionWindowLive.data = sequencerData;

		makeSceneDirty();
	}

	void doRenameVariable(string variableName)
	{
		renameVariableWindowLive = (RenameVariableWindow)EditorWindow.GetWindow(typeof(RenameVariableWindow));
		renameVariableWindowLive.variableToRename = variableName;
		renameVariableWindowLive.data = sequencerData;

		makeSceneDirty();
	}

	void doDeleteVariable(string variableName)
	{
		//check if variable is not referenced
		int numberOfRefrences = 0;

		//rename all command references
		foreach (SequencerSectionModel sectionModel in sequencerData.sections)
		{
			foreach (SequencerCommandBase command in sectionModel.commandList)
			{
				bool wasUpdated = command.updateVariableReference(variableName, variableName);

				if (wasUpdated)
					numberOfRefrences += 1;
			}
		}

		if (numberOfRefrences > 0)
		{
			doReplaceVariable(variableName);
		}
		else
		{
			sequencerData.variables.RemoveAt(sequencerData.getIndexOfVariable(variableName));
		}

		makeSceneDirty();
	}

	void doReplaceVariable(string variableToReplace)
	{
		replaceVariableWindowLive = (ReplaceVariableWindow)EditorWindow.GetWindow(typeof(ReplaceVariableWindow));
		replaceVariableWindowLive.variableToReplace = variableToReplace;
		replaceVariableWindowLive.data = sequencerData;

		makeSceneDirty();
	}

	void makeSceneDirty()
	{
		EditorSceneManager.MarkSceneDirty(sequencerData.gameObject.scene);
	}

	int draggedIndex = -1;

	void makeDroppable(Rect dropArea, int targetIndex)
	{
		currentEvent = Event.current;
		currentEventType = currentEvent.type;

		if (!dropArea.Contains(currentEvent.mousePosition))
			return;

		if (currentEventType == EventType.MouseUp)
		{
			if (currentEvent.shift)
			{
				//Debug.Log("multi select detect.");
				if (!futureMultiDragIndexes.Contains(draggedIndex))
					futureMultiDragIndexes.Add(draggedIndex);
				else if(futureMultiDragIndexes.Contains(draggedIndex))
					futureMultiDragIndexes.Remove(draggedIndex);
			}
			else
				futureMultiDragIndexes.Clear();

			if (draggedIndex != -1 && draggedIndex != targetIndex)
				doDragDrop(draggedIndex, targetIndex);

			draggedIndex = -1;
			currentEvent.Use();
		}
	}

	Event currentEvent;
	EventType currentEventType;

	protected void makeDraggable(Rect dropArea, int targetIndex)
	{
		// Cache References:
		currentEvent = Event.current;
		currentEventType = currentEvent.type;

		if (!dropArea.Contains(currentEvent.mousePosition))
			return;

		futureDragPreviewIndex = targetIndex;

		switch (currentEventType)
		{
			case EventType.MouseDown:
				if (!currentEvent.shift)
					futureMultiDragIndexes.Clear();

				draggedIndex = targetIndex;
				currentEvent.Use();

				//Debug.Log("Mouse Down: " + currentEvent.button);
				break;
			case EventType.Repaint:
				if (draggedIndex != -1)
				{
					if (draggedIndex != targetIndex)
						EditorGUI.DrawRect(dropArea, sequencerData.dropColor);
					else if (draggedIndex == targetIndex)
						EditorGUI.DrawRect(dropArea, sequencerData.dragColor);
				}
				//Debug.Log("Repaint");
				break;
			case EventType.MouseUp:
				//check for //shift key being pressed
				if (currentEvent.shift)
				{
					//Debug.Log("multi select detect.");
					if (!futureMultiDragIndexes.Contains(draggedIndex))
						futureMultiDragIndexes.Add(draggedIndex);
					else if(futureMultiDragIndexes.Contains(draggedIndex))
						futureMultiDragIndexes.Remove(draggedIndex);
				}
				else
					futureMultiDragIndexes.Clear();

				//drag drop below 
				if (draggedIndex != -1 && draggedIndex != targetIndex)
				{
					doDragDrop(draggedIndex, targetIndex);
				}

				draggedIndex = -1;
				currentEvent.Use();
				break;
				//            default:
				//                Debug.Log ( "defaulted: "  + currentEventType.ToString());
				//                break;
		}
	}

	void doDragDrop(int fromIndex, int toIndex)
	{
		if (futureMultiDragIndexes.Count > 0)
		{
			//order 
			futureMultiDragIndexes.Sort();

			//save instance id's since indexes are not "reliable"
			int[] instanceIds = new int[futureMultiDragIndexes.Count];
			for (int i = 0; i < futureMultiDragIndexes.Count; i++)
			{
				instanceIds[i] = sequencerData.sections[lastSelectedSection].commandList[futureMultiDragIndexes[i]].GetInstanceID();
			}

			int nextIndex = toIndex;

			for (int c = 0; c < futureMultiDragIndexes.Count; c++)
			{
				nextIndex = sequencerData.sections[lastSelectedSection].commandList[futureMultiDragIndexes[c]].changeIndex(nextIndex);

				//update remaining indexes after change
				for (int d = c + 1; d < futureMultiDragIndexes.Count; d++)
				{
					//search through commmand list : capped at prev index +- size of futureMultiDragIndexes.count (optimization)
					for (int b = Math.Max(0, futureMultiDragIndexes[d] - futureMultiDragIndexes.Count); b < Math.Min(sequencerData.sections[lastSelectedSection].commandList.Count, futureMultiDragIndexes[d] + futureMultiDragIndexes.Count); b++)
					{
						if (sequencerData.sections[lastSelectedSection].commandList[b].GetInstanceID() == instanceIds[d])
						{
							futureMultiDragIndexes[d] = sequencerData.sections[lastSelectedSection].commandList[b].currIndex;
							break;
						}
					}
				}
			}

			futureMultiDragIndexes.Clear();
		}
		else
		{
			//Debug.Log("X-Recieved drop from original spot: " + draggedIndex + " to: " + startIndex);
			sequencerData.sections[lastSelectedSection].commandList[fromIndex].changeIndex(toIndex);
		}
	}

	//Update unique ids, only updates id's that have 0
	void updateUIDS()
	{
        //first we should count up if any
        int length = sequencerData.sections.Count;
		for (int i = 0; i < length; i++)
		{
			foreach (SequencerCommandBase seqCommand in sequencerData.sections[i].commandList)
			{
                Type typeCommand = seqCommand.GetType();

				if (typeCommand == typeof(SC_VN_Choice))
				{
					SC_VN_Choice choice = seqCommand as SC_VN_Choice;
					for (int j = 0; j < choice.uids.Count; j++)
					{
						if (choice.uids[j] > sequencerData.last_uid)
						{
							sequencerData.last_uid = choice.uids[j];
						}
					}
				}

                if (typeCommand == typeof(SC_VN_Dialog))
                {
                    SC_VN_Dialog dia = seqCommand as SC_VN_Dialog;
                    if (dia.uid != 0)
                    {
                        if ( dia.uid > sequencerData.last_uid)
						{
							sequencerData.last_uid = dia.uid;	
						}
                    }
                }
            }
		}

        // replace the ones that are missing
        for (int i = 0; i < length; i++)
        {
            foreach (SequencerCommandBase seqCommand in sequencerData.sections[i].commandList)
            {
                Type typeCommand = seqCommand.GetType();

				if (typeCommand == typeof(SC_VN_Choice))
				{
					SC_VN_Choice choice = seqCommand as SC_VN_Choice;
					//assign existing
					for (int j = 0; j < choice.uids.Count; j++)
					{
						if (choice.uids[j] == 0)
						{
							choice.uids[j] = sequencerData.NewUID();
						}
					}

                    //check size matches, update it if not
                    if (choice.uids.Count < choice.size)
					{
                        for (int z = choice.uids.Count; z < choice.size; z++)
                        {
                            choice.uids.Add(sequencerData.NewUID());
                        }
                    }
                }

				if (typeCommand == typeof(SC_VN_Dialog))
				{
					SC_VN_Dialog dia = seqCommand as SC_VN_Dialog;
                    if ( dia.uid == 0)
					{
						dia.uid = sequencerData.NewUID();
					}
                }
            }
        }
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