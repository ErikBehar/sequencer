using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif
using System.Collections.Generic;
using System;

/// <summary>
/// Sometimes when changing scenes you want a quick way to clear everything on the screen
/// on the other hand when you rewind you want to go back to "same" state as previously
/// this command attempts to do that ( probably not going to be perfect )
/// </summary>

[Serializable]
public class SC_ClearAll : SequencerCommandBase
{
    public override string commandId{ get { return "clearAll"; } }

    public override string commandType{ get { return "base"; } }

    string musicClipName = "";
    float musicClipVolume = 1;
    List<GameObject> targets;
    List<Vector3> positions;
    List<bool> visibilitys;
    List<int> attires;
    List<string> expressions;

    public bool clearMusic = false;

    override public void initChild()
    {
    }

    override public SequencerCommandBase clone()
    {       
        SC_ClearAll newCmd = ScriptableObject.CreateInstance(typeof(SC_ClearAll)) as SC_ClearAll;
        newCmd.clearMusic = clearMusic;
        return base.clone(newCmd);        
    }

    override public void execute(SequencePlayer player)
    {
        myPlayer = player;
        
        if (player.inRewindMode)
        {
            undo();
        } else
        {
            //clear previous
            positions = new List<Vector3>();
            visibilitys = new List<bool>();
            attires = new List<int>();
            expressions = new List<string>();

            //attempt save state:
            
            //music 
            if (SoundManager.Get() != null)
            {
                musicClipName = SoundManager.Get().getCurrentMusicClipName();
                musicClipVolume = SoundManager.Get().getCurrentMusicClipVolume(); 

                if ( clearMusic )
                    SoundManager.Get().pauseMusic();
            }

            //characters (visible, pos, attire, expression)
            foreach (SequencerTargetModel model in myPlayer.sequencerData.targets)
            {
                if (model.target == null)
                {
                    positions.Add(Vector3.zero);
                    visibilitys.Add(false);
                    attires.Add(0);
                    expressions.Add("none");
                    continue;
                }    

                positions.Add(model.target.transform.position);
                visibilitys.Add(model.target.activeInHierarchy);
                VN_CharBase hasCharComp = model.target.GetComponent<VN_CharBase>(); 

                if (hasCharComp != null)
                {
                    attires.Add(hasCharComp.getCurrentAttire());
                    expressions.Add(hasCharComp.getCurrentExpressionName()); 
                } else
                {
                    attires.Add(-1);
                    expressions.Add("");
                }

                //TODO: in future use type instead? backgrounds are characters too ?
                if (hasCharComp != null)
                    model.target.SetActive(false);
            }

            //clear dialogs
            myPlayer.dialogController.hideDialog();

            //TODO: future: vars, flip
        }
        
        myPlayer.callBackFromCommand(); 
    }
    
    override public void undo()
    {
        //music 
        if (clearMusic && SoundManager.Get() != null && musicClipName != SoundManager.nullSoundName)
            SoundManager.Get().unPauseMusic();
        
        //characters (visible, pos, attire, expression)
        int count = myPlayer.sequencerData.targets.Count;
        for (int i = 0; i < count; i++)
        {
            if (myPlayer.sequencerData.targets [i].target == null)
                continue;
        
            SequencerTargetModel model = myPlayer.sequencerData.targets [i];
            
            VN_CharBase hasCharComp = model.target.GetComponent<VN_CharBase>(); 
            model.target.transform.position = positions [i];
           
            if (hasCharComp != null)
            {
                model.target.SetActive(visibilitys [i]);
                hasCharComp.setAttire(attires [i]);
                hasCharComp.setExpression(expressions [i], true);
            }
        }
        
        //future: vars
    }

    override public void forward(SequencePlayer player)
    {
    }
    
    override public void backward(SequencePlayer player)
    {
    }   
    
#if UNITY_EDITOR
    override public void drawCustomUi()
    {
        clearMusic = GUILayout.Toggle(clearMusic, "Clear Music Also?");
    }
#endif

    override public string toSequncerSerializedString()
    {
        return GetType().Name + "╫" + clearMusic.ToString() + "╫\n";
    }

    override public void initFromSequncerSerializedString(string[] splitString)
    {
        clearMusic = bool.Parse(splitString [1]);
    }
}