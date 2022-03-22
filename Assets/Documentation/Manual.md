Main Menu System Guide
======================

This documentation describes how to use the `SettingsMenu` component in
your project.

Behaviours
----------

-   \[`AudioManager`\]
-   \[`MainMenu`\]
-   \[`SettingsMenu`\]
-   \[`StopSounds`\]

AudioManager
------------

This behaviour allows you to store all your audio files in one
gameobject, adjusting the files properties and being able to reference
them.

### Properties

-   `Sounds` - List that contains all the sounds
-   `Name` - Name of the audio clip to refer to it
-   `Clip` - Location of the audio file
-   `Volume` - How loud the audio is
-   `Pitch` - Adjust the pitch of the audio
-   `Loop` - Decided if the audio clip should loop once it reaches the
    end
-   `Group` - The Audio group that this clip belongs to, in order to
    adjust audio levels as a group.

### Script

In order to use audio files we must reference Unity's Audio system.

    using UnityEngine.Audio;

We create a list that stores all the audio files and instance it so that
it will be present in all of our scenes, so we can use all the sounds in
every single scene, for each audio clip that we add we add some
properties that we can later adjust in the Inspector.

    public Sound[] sounds;
        public static AudioManager instance;
        void Awake()
        {
            if (instance == null)
                instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            foreach (Sound s in sounds)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.outputAudioMixerGroup = s.group;

                s.source.loop = s.loop;
            }
        }

In order to call sounds within other scripts we create a public void
with the string which will be our audioclips `Name`

        public void Play(string name)
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.Play();
        }
    }

MainMenu
--------

This behaviour allows you to load Twitter with preset messages to
promote the game, you can also exit the game and switch to the next
scene using buttons.

### Properties

-   `APP_PC_Link` - Link to the site where your game is located.
-   `Twittername Parameter` - Message included in the tweet.

### Script

In order to switch scenes we will need to use Unity's SceneManagement.

    using UnityEngine.SceneManagement;

We create a few strings that will change what is tweeted for us, we are
able to link the game as well as a brief description of it, in order to
bring up twitters create tweet we link to its url, and you can even
change the language if you or the game are not english.

    [SerializeField] private string APP_PC_LINK = "INSERT LINK HERE";
    [SerializeField] private string twitternameParameter = "Just played GAME TITLE, GAME DESCRIPTION";
    private string twitterdescriptionParam = "";
    private const string TWITTER_ADDRESS = "http://twitter.com/intent/tweet";
    private const string TWITTER_LANGUAGE = "en";

A few voids are made which will be mapped onto Button's OnClick event,
this allows us to quickly transition the scene to the next one in the
build index list, and you can also quit the game at the same time. Using
the strings we created we can reference them when loading up the url
which will take players to a new window.

    public void PlayGame()
    {
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void Promote()
    {
    Application.OpenURL(TWITTER_ADDRESS + "?text=" + WWW.EscapeURL(twitternameParameter + "\n" + twitterdescriptionParam + "\n" + APP_PC_LINK));
    }
    public void QuitGame()
    {
    Debug.Log("QUIT");
    Application.Quit();
    }
    }

SettingsMenu
------------

This behaviour allows you to change the volume for every single sound as
well as specifically the Music and SFX. It also allows you to adjust the
games resolution, quality setting and if you want the game to be
fullscreened or not.

### Properties

-   `AudioMixer` - The parent mixer object that controls all the games
    sound
-   `MusicMixer` - Controls the volume of sounds under the
    musicmixergroup
-   `SFXMixer` - Controls the volume of sounds under the sfxmixergroup
-   `ResolutionDropdown`- Dropdown UI component that allows you to
    select between all the available resolutions

### Script

In this script we are able to do many things such as load a specific
scene and controlling audio while require the usage of UI systems and
IEnumerables.

    using UnityEngine.Audio;
    using UnityEngine.UI;
    using System.Linq;
    using TMPro;
    using UnityEngine.SceneManagement;

We start by referencing our 3 audio groups, the Audio Mixer which is the
parent of both the Music and SFX Mixer has a different reference
compared to the childs. We are also making use of TMP to improve the
quality of the UI therefore we need to reference the TMPDropdown instead
of the regular Dropdown. In order to display all ouf our resolutions we
will be creating a list.

    public AudioMixer AudioMixer;
    public AudioMixerGroup MusicMixer;
    public AudioMixerGroup SFXMixer;
    public TMP_Dropdown resolutionDropdown;
    Resolution[] resolutions;

Because every computer is different they may not be able to use higher
resolutions therefore it would be pointless including them in the List,
therefore we will check the available resolutions when the game starts
as the user will not be immediately put on the settings menu. In this
start function we also set it so that the framerate is the highest
possible, this means that if the users machine is capable of 60fps then
all the resolutions will be 60fps, this also stops duplicate resolutions
from showing every single possible framerate, making it much easier to
scroll through the dropdown.

    private void Start()
    {
    resolutions = Screen.resolutions.Select(resolution => new Resolution { width = resolution.width, height = resolution.height }).Distinct().ToArray();
    resolutionDropdown.ClearOptions();
    List<string> options = new List<string>();
    int currentResolutionIndex = 0;
    for (int i = 0; i < resolutions.Length; i++)
    {
    string option = resolutions[i].width + " x " + resolutions[i].height;
    options.Add(option);
    if (resolutions[i].width == Screen.width &&
    resolutions[i].height == Screen.height)
    {
    currentResolutionIndex = i;
    }
    }
    resolutionDropdown.AddOptions(options);
    resolutionDropdown.value = currentResolutionIndex;
    resolutionDropdown.RefreshShownValue();
    }

Using the resolution system we just created as well as using Unity's own
settings, we can create multiple voids that we can assign to buttons
using OnValueChanged, this can be used to change the volume as you move
the sliders.

    public void LoadScene(int level)
    {
    SceneManager.LoadScene(level);
    }
    public void SetResolution(int ResolutionIndex)
    {
    Resolution resolution = resolutions[ResolutionIndex];
    Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
    public void SetVolume(float volume)
    {
    AudioMixer.SetFloat("volume", volume);
    }
    public void SetMusicVolume(float MusicVolume)
    {
    AudioMixer.SetFloat("musicvolume", MusicVolume);
    }
    public void SetSFXVolume(float SFXVolume)
    {
    AudioMixer.SetFloat("sfxvolume", SFXVolume);
    }
    public void SetQuality(int qualityIndex)
    {
     QualitySettings.SetQualityLevel(qualityIndex);
    }
    public void SetFullscreen(bool isFullscreen)
    {
    Screen.fullScreen = isFullscreen;
     }
    }

StopSounds
----------

Stops all the sounds in the scene playing.

### Properties

-   `Audio Source` - References all the audio sources present in the
    scene.

### Script

We create a list for AudioSources. In the StopAllAudio void we find all
the audio sources in the scene and stop each one playing.

        private AudioSource[] allAudioSources;
        public void StopAllAudio()
        {
            allAudioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
            foreach (AudioSource audioS in allAudioSources)
            {
                Debug.Log("MUSIC STOPPED");
                audioS.Stop();
            }
        }
    }
