//#define REMOVE_AUDIO //actually removes Audio files from build by stripping the properties ( this should be added to the global defines for it to be totally effective)
//#define MUSIC_DEBUG // logs a bunch of info
//#define AUDIO_DEBUG // logs a bunch of info
//#define AUDIO_OFF // generally mute audio 
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//invadereriks awesome Soundmanager class
//send bug fixes to invadererik@gmail.com

public enum SoundInterruptType
{
    DontCare, //plays sound regardless if the same sound is playing or not
    Interrupt, //stops instances of the same sound and plays again from beginning
    DontInterrupt, //if an instance of the sound is playing, dont play the sound
    DontInterruptButInterruptOthers, //doesnt interrupt this sound, but does interrupt other sounds
    InterruptAllPlayMe //interrupts all sounds including this one, plays this one from beginning
}

public class SoundManager : MonoBehaviour
{
    static public SoundManager _instance = null;
#if !REMOVE_AUDIO
    public AudioClip edx_defaultClip; //if we couldnt find a clip play this instead

    public AudioClip defaultClip;
    public List<AudioClip> sfxClips = new List<AudioClip>();
    public List<AudioClip> musicClips = new List<AudioClip>();
#endif
    private AudioSource musicAudioSourceA;
    private AudioSource musicAudioSourceB;
    private AudioSource currentMusicAudioSource;
    public float musicVolume = 1f;
    public float fadeTime = 2;
    public float sfxVolume = 1f;
    public bool muteAllSound = false;
    public bool muteMusic = false;
    public bool muteSfx = false;
    public int audioPoolSize = 8;
    private List<AudioSource> audioSourcePool = new List<AudioSource>();
    private Dictionary<string, string[]> sfxSets = new Dictionary<string, string[]>();
    private Tweener musicFadeOutTweener;
    private Tweener musicFadeInTweener;

    private Dictionary<string, string[]> musicPlayLists = new Dictionary<string, string[]>();
    private int lastTrackPlayed = 0;
    private string lastPlaylistName = "na";
    public bool isPlayingMusicPlayList = false;
    private bool lastTrackDidStart = false;
    public float musicTrackFadeAtPercent = .9f;

    public static string nullSoundName = "notReallyAnAudioClip";
    public bool dontDestroyOnLoad = false;

    [Header("Sources")]
    [SerializeField] private GenericFloat soundVolumeFloat;
    [SerializeField] private GenericFloat musicVolumeFloat;
    [SerializeField] private GenericBool muteAllToggleBool;
    [SerializeField] private SOPlay3DAudioAction onPlay3dAudio;
    [SerializeField] private SOPlayMusicAudioAction onPlayMusicAudio;
    [SerializeField] private SOPlaySoundAction onPlaySound;

    void onSoundVolumeChange(float volume)
    {
        setSoundVolume(volume);
    }

    void onMusicVolumeChange(float volume)
    {
        setMusicVolume(volume);
    }

    void onMuteAllToggle(bool value)
    {
        muteAllSound = value;
        if (value)
        {
            pauseMusic();
            stopAllSounds();
        }
        else
        {
            unPauseMusic();
        }

    }

    static public SoundManager Get()
    {
        return _instance;
    }

    void OnDestroy()
    {
        if (musicFadeOutTweener != null)
        {
            musicFadeOutTweener.Kill();
        }

        if (musicFadeInTweener != null)
        {
            musicFadeInTweener.Kill();
        }

        musicFadeOutTweener = null;
        musicFadeInTweener = null;

        soundVolumeFloat.onChange -= onSoundVolumeChange;
        musicVolumeFloat.onChange -= onMusicVolumeChange;
        muteAllToggleBool.onChange -= onMuteAllToggle;
        onPlay3dAudio.action -= playSfx3d;
        onPlayMusicAudio.action -= playMusic;
        onPlaySound.action -= playSfx;
    }

    void Awake()
    {
#if (AUDIO_OFF)
                muteAllSound = true;
#endif

        if (_instance != null)
        {
            Debug.Log("Found a previous SoundManager instance! Destroying this one!");
            Destroy(gameObject);
            return;
        }
        else if (_instance == null)
            _instance = this;

        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

#if !REMOVE_AUDIO
        defaultClip = edx_defaultClip;
#endif
        //Create the music sources (2) for crossFading 
        musicAudioSourceA = gameObject.AddComponent<AudioSource>();
        musicAudioSourceB = gameObject.AddComponent<AudioSource>();

        musicAudioSourceA.volume = musicAudioSourceB.volume = musicVolume;
        musicAudioSourceA.loop = musicAudioSourceB.loop = true;
        musicAudioSourceA.rolloffMode = AudioRolloffMode.Linear;
        musicAudioSourceB.rolloffMode = AudioRolloffMode.Linear;

        currentMusicAudioSource = musicAudioSourceA;

        //initialize the Sfx pool
        int i = 0;
        while (i < audioPoolSize)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.rolloffMode = AudioRolloffMode.Linear;
            audioSourcePool.Add(source);
            ++i;
        }

        //init tweens so I dont have to keep on checking for nulls below
        musicFadeInTweener = DOTween.To(() => musicAudioSourceA.volume, x => musicAudioSourceA.volume = x, 1, 0);
        musicFadeOutTweener = DOTween.To(() => musicAudioSourceB.volume, x => musicAudioSourceB.volume = x, 0, 0);

        musicFadeInTweener.SetUpdate(UpdateType.Normal, true);
        musicFadeOutTweener.SetUpdate(UpdateType.Normal, true);

        soundVolumeFloat.onChange += onSoundVolumeChange;
        musicVolumeFloat.onChange += onMusicVolumeChange;
        muteAllToggleBool.onChange += onMuteAllToggle;
        onPlay3dAudio.action += playSfx3d;
        onPlayMusicAudio.action += playMusic;
        onPlaySound.action += playSfx;

        //initialize with SO values
        onSoundVolumeChange(soundVolumeFloat.Value);
        onMusicVolumeChange(musicVolumeFloat.Value);
        onMuteAllToggle(muteAllToggleBool.Value);
    }

    private AudioSource getSourceFromPool()
    {
        foreach (AudioSource adSource in audioSourcePool)
        {
            if (!adSource.isPlaying)
            {
                return adSource;
            }
        }

        Debug.LogWarning("Audio Source Pool passed limit of " + audioPoolSize + ", stealing sound from first source");
        return audioSourcePool[0];
    }

    #region Play Music  

    public string getCurrentMusicClipName()
    {
        if (currentMusicAudioSource.clip != null && currentMusicAudioSource.volume > .02f && currentMusicAudioSource.isPlaying && (!musicFadeOutTweener.IsActive() || !musicFadeOutTweener.IsPlaying() ))
            return currentMusicAudioSource.clip.name;

        return nullSoundName;
    }

    public float getCurrentMusicClipVolume()
    {
        if (currentMusicAudioSource.clip != null)
            return currentMusicAudioSource.volume;

        return musicVolume;
    }

    public void setMusicVolume(float value)
    {
        musicVolume = value;
        currentMusicAudioSource.volume = value;
    }

    public void setMusicOnOff(bool value) // true = On, false = off
    {
        if (!muteMusic == value)
            return;

        if (value)
        {
            muteMusic = false;
            currentMusicAudioSource.Play();
        }
        else
        {
            pauseMusic();
            muteMusic = true;
        }
    }

    public AudioClip getMusicByName(string name)
    {
#if !REMOVE_AUDIO
        return findAudioInListByName(name, musicClips);
#else
                return null;
#endif
    }

    public int musicListCount
    {
        get
        {
            return musicClips.Count;
        }
    }

    public void pauseMusic()
    {
        if (currentMusicAudioSource != null)
            currentMusicAudioSource.Pause();
    }

    public void unPauseMusic()
    {
        if (muteAllSound || muteMusic)
            return;

        if (currentMusicAudioSource.clip != null)
        {
            currentMusicAudioSource.UnPause();
        }
        else if (isPlayingMusicPlayList)
        {
            if (currentMusicAudioSource.clip != null)
                playLastMusic();
            else
                playMusic(musicPlayLists[lastPlaylistName][lastTrackPlayed]);
        }
    }

    public void playMusic(int musicClipIndex)
    {
        playMusic(musicClipIndex, musicVolume);
    }

    public void playMusic(int musicClipIndex, float volume)
    {
        if (musicClipIndex < 0 || musicClipIndex >= musicClips.Count)
        {
            debugMusic("Tried to play music index : " + musicClipIndex + " but list size is " + musicClips.Count);
        }
        else
        {
            playMusic(musicClips[musicClipIndex].name, volume);
        }
    }

    public void playMusicPlaylist(string playListName, string[] tracks, bool pickRandomStartTrack)
    {
        isPlayingMusicPlayList = true;

        musicAudioSourceA.loop = false;
        musicAudioSourceB.loop = false;

        lastTrackDidStart = false;

        musicPlayLists.Add(playListName, tracks);

        lastPlaylistName = playListName;

        if (pickRandomStartTrack)
            lastTrackPlayed = UnityEngine.Random.Range(0, tracks.Length);
        else
            lastTrackPlayed = 0;

        playMusic(tracks[lastTrackPlayed]);
    }

    public void playNextTrack()
    {
        lastTrackDidStart = false;
        lastTrackPlayed = (lastTrackPlayed + 1) % musicPlayLists[lastPlaylistName].Length;
        playMusic(musicPlayLists[lastPlaylistName][lastTrackPlayed]);
    }

    void Update()
    {
        //Debug.Log(currentMusicAudioSource.time);

        if (isPlayingMusicPlayList)
        {
            if (!lastTrackDidStart && currentMusicAudioSource.time > 0)
                lastTrackDidStart = true;
            else if (lastTrackDidStart && (currentMusicAudioSource.time > currentMusicAudioSource.clip.length * musicTrackFadeAtPercent ||
                currentMusicAudioSource.time == 0))
                playNextTrack();
        }
    }

    public void playMusic(string musicClipName)
    {
        playMusic(musicClipName, musicVolume);
    }

    public void playMusic(AudioClip musicClip)
    {
        actuallyPlayMusic(musicClip, musicVolume);

        if (muteAllSound || muteMusic)
        {
            pauseMusic();
        }
    }

    public void playMusic(string musicClipName, float volume)
    {
        if (!muteAllSound && !muteMusic)
        {
            AudioClip musicClip = getMusicByName(musicClipName);
            actuallyPlayMusic(musicClip, volume);
        }
    }

    void actuallyPlayMusic(AudioClip musicClip, float volume)
    {
        if (musicClip == null)
        {
            debugMusic("Tried to play music but soundManager could not find it!");
            return;
        }

        string musicClipName = musicClip.name;

        //we have music playing already
        if (currentMusicAudioSource.isPlaying)
        {
            debugMusic("music is aleady playing, checking options");

            //check if we are in the middle of fadingIn or Out 
            if ((musicFadeOutTweener.IsActive() && musicFadeOutTweener.IsPlaying()) || (musicFadeInTweener.IsActive() && musicFadeInTweener.IsPlaying()))
            {
                debugMusic("A Fade is happening...");

                //at this point we only care if one of the musics that was fading is the one we want
                //and we want to play it at its current volume back up to 1, the other music we fade out 
                //unless neither of them are the one we want, then do a crossfade

                AudioSource fadeOutTarget = null;
                AudioSource fadeInTarget = null;

                if (musicFadeInTweener.target != null && ((AudioSource)musicFadeInTweener.target).clip.name == musicClipName)
                {
                    fadeInTarget = (AudioSource)musicFadeInTweener.target;

                    if (musicFadeOutTweener.target != null)
                    {
                        fadeOutTarget = (AudioSource)musicFadeOutTweener.target;
                    }
                }

                if (fadeInTarget == null && musicFadeOutTweener.target != null && ((AudioSource)musicFadeOutTweener.target).clip.name == musicClipName)
                {
                    fadeInTarget = (AudioSource)musicFadeOutTweener.target;

                    if (musicFadeInTweener.target != null)
                    {
                        fadeOutTarget = (AudioSource)musicFadeOutTweener.target;
                    }
                }

                //clean up faders 
                musicFadeInTweener.Kill();
                musicFadeOutTweener.Kill();

                if (fadeOutTarget != null)
                {
                    if (fadeOutTarget.clip != null)
                        debugMusic("Fading out:" + fadeOutTarget.clip.name);
                    fadeOutMusic(fadeOutTarget);
                }

                if (fadeInTarget != null)
                {
                    if (fadeInTarget.clip != null)
                        debugMusic("Fading in:" + fadeInTarget.clip.name);
                    doMusicPlay(musicClip, fadeInTarget.volume, volume);
                }

                //make sure the audioSources match up
                if (fadeOutTarget == null)
                {
                    if (fadeInTarget != null)
                    {
                        AudioSource otherSource = (fadeInTarget == musicAudioSourceA) ? musicAudioSourceB : musicAudioSourceA;
                        if (otherSource.clip != null)
                        {
                            debugMusic("Stopping music :" + otherSource.clip.name);
                            otherSource.Stop();
                        }
                    }
                }

                //we want a new clip to play if neither of the fades where for this clip
                if (fadeInTarget == null)
                {
                    doMusicCrossFade(musicClip, volume);
                }
            }
            //check if its the same clip that is already playing
            else if (currentMusicAudioSource.clip.name == musicClipName)
            {
                debugMusic("The same music is playing already, so we will ignore play.");
                return; //this could be some other behavior like restarting it or something 
            }
            //another music is playing, crossfade from one music to another
            else
            {
                debugMusic("Another music is playing, we will do a standard crossfade.");
                doMusicCrossFade(musicClip, volume);
            }
        }
        //nothing was playing fade in new music
        else
        {
            debugMusic("no music was playing, playing: " + musicClip.name);
            doMusicPlay(musicClip, volume);
        }
    }

    private void doMusicCrossFade(AudioClip musicClip, float endVolume)
    {
        doMusicCrossFade(musicClip, 0f, endVolume);
    }

    private void doMusicCrossFade(AudioClip musicClip, float startVolume, float endVolume)
    {
        debugMusic("crossfading from: " + currentMusicAudioSource.clip.name + " to: " + musicClip.name);

        //clean up faders 
        musicFadeInTweener.Kill();
        musicFadeOutTweener.Kill();

        fadeOutMusic(currentMusicAudioSource);

        AudioSource nextMusicAudioSource = (currentMusicAudioSource == musicAudioSourceA) ? musicAudioSourceB : musicAudioSourceA;
        nextMusicAudioSource.clip = musicClip;
        nextMusicAudioSource.volume = startVolume;
        nextMusicAudioSource.Play();
        musicFadeInTweener = DOTween.To(() => nextMusicAudioSource.volume, x => nextMusicAudioSource.volume = x, endVolume, fadeTime).OnComplete(() => musicCrossFadeComplete());
        musicFadeInTweener.SetUpdate(UpdateType.Normal, true);

        currentMusicAudioSource = nextMusicAudioSource;
    }

    private void musicCrossFadeComplete()
    {
        debugMusic("Done with crossfade current music is now: " + currentMusicAudioSource.clip.name + " ,vol:" + currentMusicAudioSource.volume);//((AudioSource)tweenEvent.parms[0]).clip.name );
                                                                                                                                                 /*if ( musicFadeOutTweener.isComplete && musicFadeInTweener.isComplete )
                                                                                                                                                     {   
                                                                                                                                                         currentMusicAudioSource = ((AudioSource)tweenEvent.parms[0]);
                                                                                                                                                     }*/
    }

    private void doMusicPlay(AudioClip musicClip, float startVolume, float endVolume)
    {
        debugMusic("playing music: " + musicClip.name);

        currentMusicAudioSource.clip = musicClip;
        currentMusicAudioSource.volume = startVolume;
        currentMusicAudioSource.Play();

        musicFadeInTweener = DOTween.To(() => currentMusicAudioSource.volume, x => currentMusicAudioSource.volume = x, endVolume, fadeTime).SetEase(Ease.InCubic);
        musicFadeInTweener.SetUpdate(UpdateType.Normal, true);
    }

    private void doMusicPlay(AudioClip musicClip, float volume)
    {
        doMusicPlay(musicClip, 0f, volume);
    }

    public void playLastMusic()
    {
        doMusicPlay(currentMusicAudioSource.GetComponent<AudioSource>().clip, musicVolume);
    }

    public void stopMusic()
    {
        currentMusicAudioSource.Stop();
    }

    public void stopMusicByName(string name)
    {
        if (currentMusicAudioSource != null && currentMusicAudioSource.clip != null && currentMusicAudioSource.clip.name == name)
        {
            currentMusicAudioSource.Stop();
        }
    }

    public void fadeOutMusic()
    {
        fadeOutMusic(currentMusicAudioSource);
    }

    public void fadeOutMusic(AudioSource musicSource)
    {
        if (musicSource == null || musicSource.clip == null)
        {
            return;
        }
        debugMusic("fading out music: " + musicSource.clip.name);

        musicFadeOutTweener = DOTween.To(() => musicSource.volume, x => musicSource.volume = x, 0, fadeTime).OnComplete(() => doneMusicFade(musicSource));
        musicFadeOutTweener.SetUpdate(UpdateType.Normal, true);
    }

    public void doneMusicFade(AudioSource audioSource)
    {
        AudioSource fadedTargetAudioSource = audioSource;
        if (fadedTargetAudioSource.volume < .1f && fadedTargetAudioSource != (musicFadeInTweener.target as AudioSource))
        {
            if (fadedTargetAudioSource.clip != null)
            {
                debugMusic("done music fade stoping music: " + fadedTargetAudioSource.clip.name);
                fadedTargetAudioSource.Stop();
            }
        }
        else
        {
            debugMusic("music Fade is done, but volume is above .1(" + fadedTargetAudioSource.volume + "} so wasnt killed.");
        }
    }

    private void debugMusic(string output)
    {
#if (MUSIC_DEBUG)
            Debug.Log(output);
#endif
    }
    #endregion

    #region Play Sound Effects

    public AudioSource playSoundNow(AudioClip clip)
    {
        // this is a test
        //playSfx(clip);

#if (AUDIO_DEBUG)
            Debug.Log("playing sound fx:" + clip.name);
#endif

        AudioSource source = getSourceFromPool();

        source.volume = sfxVolume;
        source.clip = clip;
        source.Play();
        source.loop = false;

        return source;
    }

    public void setSoundVolume(float value)
    {
        sfxVolume = value;
        foreach (AudioSource clip in audioSourcePool)
        {
            clip.volume = value;
        }
    }

    public void setSoundOnOff(bool value) // true = On, false = off
    {
        if (!muteSfx == value)
            return;

        if (value)
        {
            muteSfx = false;
        }
        else
        {
            stopAllSounds();
            muteSfx = true;
        }
    }

    public void createSet(string setName, string[] clipNames)
    {
        sfxSets.Add(setName, clipNames);
    }

    public void playRandomSfxFromSet(string setName)
    {
        playRandomSfxFromSet(setName, sfxVolume);
    }

    public void playRandomSfxFromSet(string setName, float volume)
    {
        if (!muteAllSound && !muteSfx)
        {
            string soundName = sfxSets[setName][Random.Range(0, sfxSets[setName].Length)];
            playSfx(soundName);
        }
    }

    public void playRandomSfxFromSetInterruptAllSet(string setName)
    {
        if (!muteAllSound && !muteSfx)
            playRandomSfxFromSetInterruptAllSet(setName, sfxVolume);
    }

    public void playRandomSfxFromSetInterruptAllSet(string setName, float volume)
    {
        if (!muteAllSound && !muteSfx)
        {
            string soundName = sfxSets[setName][Random.Range(0, sfxSets[setName].Length)];
            playSfx(soundName, sfxVolume, SoundInterruptType.Interrupt, sfxSets[setName]);
        }
    }

    public void playSfxDelayed(AudioClip clip, float delayTime)
    {
        if (!muteAllSound && !muteSfx)
            StartCoroutine(waitThenPlayClipSfx(clip, delayTime));
    }

    IEnumerator waitThenPlayClipSfx(AudioClip clipName, float time)
    {
        yield return new WaitForSeconds(time);
        playSfx(clipName);
    }

    public void playSfxDelayed(string clipName, float delayTime)
    {
        if (!muteAllSound && !muteSfx)
            StartCoroutine(waitThenPlaySfx(clipName, delayTime));
    }

    IEnumerator waitThenPlaySfx(string clipName, float time)
    {
        yield return new WaitForSeconds(time);
        playSfx(clipName);
    }

    public void playSfx(string clipName)
    {
        playSfx(clipName, sfxVolume);
    }

    public void playSfx(string clipName, bool loop)
    {
        if (!muteAllSound && !muteSfx)
            play(getSfxByName(clipName), sfxVolume, loop);
    }

    public void playSfx(string clipName, float volume)
    {
        playSfx(clipName, volume, SoundInterruptType.DontCare);
    }

    public void playSfx(string clipName, float volume, SoundInterruptType interruptType)
    {
        playSfx(clipName, volume, interruptType, new string[0]);
    }

    public void playSfx(string clipName, float volume, SoundInterruptType interruptType, string[] alsoInterrupt)
    {
#if !REMOVE_AUDIO
        AudioClip theClip = findAudioInListByName(clipName, sfxClips);

        if (theClip == defaultClip)
        { //if we cant find the audio in list by that name, attempt to find the closest possible
            theClip = findClosestAudioInListByName(clipName);
        }

        if (interruptType == SoundInterruptType.DontInterrupt || interruptType == SoundInterruptType.DontInterruptButInterruptOthers)
        {
            playSfx(theClip, volume, interruptType, new List<string>(alsoInterrupt), clipName);
        }
        else
        {
            playSfx(theClip, volume, interruptType, new List<string>(alsoInterrupt));
        }
#endif
    }

    public void playSfx(AudioClip soundClip, float volume)
    {
        playSfx(soundClip, volume, SoundInterruptType.DontCare, new List<string>());
    }

    public void playSfx(AudioClip soundClip, float volume, SoundInterruptType interruptType)
    {
        playSfx(soundClip, volume, interruptType, new List<string>());
    }

    public void playSfx(AudioClip soundClip, float volume, SoundInterruptType interruptType, string[] alsoInterrupt)
    {
        playSfx(soundClip, volume, interruptType, new List<string>(alsoInterrupt));
    }

    public void playSfx(AudioClip soundClip, float volume, SoundInterruptType interruptType, List<string> alsoInterrupt)
    {
        playSfx(soundClip, volume, interruptType, alsoInterrupt, "");
    }

    public void playSfx(AudioClip soundClip, float volume, SoundInterruptType interruptType, List<string> alsoInterrupt, string dontInterrupt)
    {
        if (!muteAllSound && !muteSfx)
        {
            switch (interruptType)
            {
                case SoundInterruptType.DontCare:
                    play(soundClip, volume, false);
                    break;
                case SoundInterruptType.DontInterrupt:
                    playClipOnlyIfSimilarNotPlaying(soundClip, dontInterrupt, volume);
                    break;
                case SoundInterruptType.Interrupt:
                    stopPlayingSoundList(alsoInterrupt, soundClip.name);
                    play(soundClip, volume, false);
                    break;
                case SoundInterruptType.DontInterruptButInterruptOthers:
                    stopPlayingSoundList(alsoInterrupt);
                    playClipOnlyIfSimilarNotPlaying(soundClip, dontInterrupt, volume);
                    break;
                case SoundInterruptType.InterruptAllPlayMe:
                    stopAllSounds();
                    play(soundClip, volume, false);
                    break;
            }
        }
    }

    public void stopAllSounds()
    {
        foreach (AudioSource source in audioSourcePool)
        {
            source.loop = false;
            source.Stop();
        }
    }

    private void playClipOnlyIfSimilarNotPlaying(AudioClip soundClip, string abridgedClipName, float volume)
    {
        if (abridgedClipName.Length > 1)
        {
            bool similarClipPlaying = false;
            List<AudioClip> dontInterruptClipNames = findClosestAudioInListByName(abridgedClipName, true);
            foreach (AudioClip dontInterruptClipName in dontInterruptClipNames)
            {
                if (SfxIsPlaying(dontInterruptClipName))
                {
                    similarClipPlaying = true;
                }
            }
            if (similarClipPlaying == false)
            {
                play(soundClip, volume, false);
            }
        }
        else
        {
            if (!SfxIsPlaying(soundClip))
            {
                play(soundClip, volume, false);
            }
        }
    }

    public void stopPlayingSoundList(List<string> alsoInterrupt)
    {
        stopPlayingSoundList(alsoInterrupt, nullSoundName);
    }

    public void stopPlayingSoundList(AudioClip[] alsoInterrupt)
    {
        List<string> interruptNames = new List<string>();
        foreach (AudioClip clip in alsoInterrupt)
        {
            interruptNames.Add(clip.name);
        }
        stopPlayingSoundList(interruptNames, nullSoundName);
    }

    public void stopPlayingSoundList(List<AudioClip> alsoInterrupt)
    {
        List<string> interruptNames = new List<string>();
        foreach (AudioClip clip in alsoInterrupt)
        {
            interruptNames.Add(clip.name);
        }
        stopPlayingSoundList(interruptNames, nullSoundName);
    }

    public void stopPlayingSoundList(List<string> alsoInterrupt, string callingSoundName)
    {
        alsoInterrupt.Add(callingSoundName);

        foreach (string soundName in alsoInterrupt)
        {
            if (soundName == nullSoundName)
                continue;

#if (AUDIO_DEBUG)
                //Debug.Log ( "calling sound name when stopping sounds:" + callingSoundName + " : " + soundName);
#endif

            //not working that well with non embedded list clips, commenting out for now
            //List<AudioClip> audioClips = findClosestAudioInListByName(soundName, true);

            foreach (AudioSource audioSource in audioSourcePool)
            {
                if (audioSource.isPlaying)
                {
                    //                    foreach (AudioClip clip in audioClips)
                    //                    {
                    //if ( audioSource.clip == clip)
                    if (audioSource.clip.name == soundName)
                    {
                        audioSource.Stop();
#if (AUDIO_DEBUG)
                                Debug.Log("stopping sound fx:" + audioSource.clip.name);
#endif
                    }
                    //                    }
                }
            }
        }
    }

    public bool SfxIsPlaying(AudioClip soundClip)
    {
        foreach (AudioSource audioSource in audioSourcePool)
        {
            if (audioSource.clip == soundClip && audioSource.isPlaying)
            {
                return true;
            }
        }

        return false;
    }

    public AudioSource getPlayingSfxSource(AudioClip soundClip)
    {
        foreach (AudioSource audioSource in audioSourcePool)
        {
            if (audioSource.clip == soundClip)
            {
                return audioSource;
            }
        }

        return null;
    }

    public void playSfx(AudioClip soundClip)
    {
        if (!muteAllSound && !muteSfx)
        {
            playSfx(soundClip, sfxVolume, SoundInterruptType.DontCare, new List<string>());
        }
    }

    public AudioClip getSfxByName(string name)
    {
#if !REMOVE_AUDIO
        return findAudioInListByName(name, sfxClips);
#else
            return null;
#endif
    }

    private AudioClip findAudioInListByName(string name, List<AudioClip> theList)
    {
        foreach (AudioClip clip in theList)
        {
            if (clip && clip.name == name)
            {
                return clip;
            }
        }
#if !REMOVE_AUDIO
        return defaultClip;
#else
            return null;
#endif
    }

    private void play(AudioClip clip, float volume, bool loop)
    {
#if (AUDIO_DEBUG)
            Debug.Log("playing sound fx:" + clip.name);
#endif

        AudioSource source = getSourceFromPool();

        source.volume = volume * sfxVolume;
        source.clip = clip;
        source.Play();
        source.loop = loop;
    }

    private AudioClip findClosestAudioInListByName(string clipName)
    {
        return findClosestAudioInListByName(clipName, false)[0];
    }

    // tries to find a clip thats similar, if it finds one thats clipname_mr or clipName_fr
    // if it finds clipName_01  etc, it will randomly pick one from the set
    // returns a list, most times a list with 1 clip
    private List<AudioClip> findClosestAudioInListByName(string clipName, bool returnCompleteList)
    {
        List<AudioClip> randomSet = new List<AudioClip>();

#if !REMOVE_AUDIO
        AudioClip genderClip = defaultClip;

        List<AudioClip> genderSet = new List<AudioClip>();

        foreach (AudioClip aClip in sfxClips)
        {
            if (aClip != null && aClip.name.IndexOf(clipName) > -1)
            {
                if ((aClip.name.Contains("_mr") || aClip.name.Contains("_male")))
                {
                    genderClip = aClip;
                    genderSet.Add(aClip);
                    continue;
                }

                if ((aClip.name.Contains("_fr") || aClip.name.Contains("_female")))
                {
                    genderClip = aClip;
                    genderSet.Add(aClip);
                    continue;
                }

                randomSet.Add(aClip);
            }
        }

        if (genderSet.Count > 1)
        {
            if (returnCompleteList)
            {
                return genderSet;
            }

            return new List<AudioClip>() { genderSet[UnityEngine.Random.Range(0, genderSet.Count)] };
        }

        if (genderClip != defaultClip)
        {
            return new List<AudioClip>() { genderClip };
        }

        if (randomSet.Count > 0)
        {
            if (returnCompleteList)
            {
                return randomSet;
            }

            return new List<AudioClip>() { randomSet[UnityEngine.Random.Range(0, randomSet.Count)] };
        }

        return new List<AudioClip>() { defaultClip };
#else
            return null;
#endif
    }

    public void playSfx3d(Vector3 targetPos, AudioClip clip, float volumeScale)
    {
        if (muteAllSound || muteSfx) return;
    
        //Debug.Log("Audio target is: " + target.name);
        AudioSource.PlayClipAtPoint(clip, targetPos, sfxVolume * volumeScale);        
    }

    public void playSfx3d(GameObject target, string name, float minDistance, float maxDistance, float volumeScale)
    {
#if !REMOVE_AUDIO
        if (!muteAllSound && !muteSfx)
        {
            AudioSource source = target.GetComponent<AudioSource>();
            if (source == null)
            {
                source = target.AddComponent<AudioSource>();
            }
            else if (source.clip && source.clip.name != name)
            {
                source = target.AddComponent<AudioSource>();
            }

            if (source.clip == null)
            {
                source.clip = findAudioInListByName(name, sfxClips);
            }

            if (source.isPlaying)
            {
                return;
            }

            source.rolloffMode = AudioRolloffMode.Linear;

            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            source.PlayOneShot(source.clip, sfxVolume * volumeScale);
        }
#endif
    }
    #endregion
}