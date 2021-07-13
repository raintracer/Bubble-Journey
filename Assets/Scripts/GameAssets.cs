using System.Collections.Generic;
using UnityEngine;

static public class GameAssets
{

    private static readonly GameObject GO;

    private static Dictionary<string, Sound> Sounds;
    private static Dictionary<string, Mesh> Meshes;
    private static Dictionary<string, UnityEngine.Material> Materials;
    private static Dictionary<string, UnityEngine.Sprite> Sprites;

    static GameAssets()
    {
        GO = new GameObject("GameAssetObject");
        Object.DontDestroyOnLoad(GO);
        LoadSounds();
        LoadMaterials();
        LoadSprites();
    }

    #region Materials
    private static void LoadMaterials()
    {

        // 

        Materials = new Dictionary<string, UnityEngine.Material>
        {
            ["SpriteDefault"] = Resources.Load<UnityEngine.Material>("Default"),
            ["Offset"] = Resources.Load<UnityEngine.Material>("Offset"),
        };
    }

    private static UnityEngine.Material GetMaterial(string _MaterialName)
    {
        if (!Materials.TryGetValue(_MaterialName, out UnityEngine.Material MaterialTemp)) Debug.LogError("Material was not found: " + _MaterialName);
        return MaterialTemp;
    }

    public static class Material
    {
        // Example - public static UnityEngine.Material ArenaGrid { get => GetMaterial("Arena Grid"); }
        public static UnityEngine.Material SpriteDefault { get => GetMaterial("SpriteDefault"); }

        public static UnityEngine.Material Offset { get => GetMaterial("Offset"); }
    }

    #endregion

    #region Sprites

    private static void LoadSprites()
    {
        Sprites = new Dictionary<string, UnityEngine.Sprite>
        {
            // Example - ["TileBlue"] = Resources.Load<UnityEngine.Sprite>("Tile-Blue"),
        };
    }

    private static UnityEngine.Sprite GetSprite(string _SpriteName)
    {
        if (!Sprites.TryGetValue(_SpriteName, out UnityEngine.Sprite SpriteTemp)) Debug.LogError("Sprite was not found: " + _SpriteName);
        return SpriteTemp;
    }

    public static class Sprite
    {
        // Example - public static UnityEngine.Material ArenaGrid { get => GetMaterial("Arena Grid"); }
    }

    #endregion

    #region Sounds

    private static void LoadSounds()
    {
        Sounds = new Dictionary<string, Sound>
        {
            ["pop1"] = new Sound(GO.AddComponent<AudioSource>(), "pop1", 1f),
            ["pop2"] = new Sound(GO.AddComponent<AudioSource>(), "pop2", 1f),
            ["pop3"] = new Sound(GO.AddComponent<AudioSource>(), "pop3", 1f),
            ["death"] = new Sound(GO.AddComponent<AudioSource>(), "death", 1f),
            ["MenuMusic"] = new Sound(GO.AddComponent<AudioSource>(), "MenuMusic", 1f),
            ["PlaySound"] = new Sound(GO.AddComponent<AudioSource>(), "PlaySound", 0.5f),
            ["Song1"] = new Sound(GO.AddComponent<AudioSource>(), "Song1", 1f, true),
            ["CreditMusic"] = new Sound(GO.AddComponent<AudioSource>(), "CreditMusic", 1f, true)
        };
    }
    public static Sound GetSound(string _SoundName)
    {
        if (!Sounds.TryGetValue(_SoundName, out Sound SoundTemp)) Debug.LogError("Sound was not found: " + _SoundName);
        return SoundTemp;
    }

    public class Sound
    {

        // EXPOSE SOUNDS FOR STRONG TYPING
        public static Sound pop1 { get => GetSound("pop1"); }
        public static Sound pop2 { get => GetSound("pop2"); }
        public static Sound pop3 { get => GetSound("pop3"); }
        public static Sound death { get => GetSound("death"); }
        public static Sound MenuMusic { get => GetSound("MenuMusic"); }
        public static Sound PlaySound { get => GetSound("PlaySound"); }
        public static Sound Song1 { get => GetSound("Song1"); }

        public static Sound CreditMusic { get => GetSound("CreditMusic"); }

        private AudioSource Source;
        public string ClipName { get; private set; }
        public float Volume
        {
            get { return Source.volume; }
            set { Source.volume = value; }
        }
        public float Pitch
        {
            get { return Source.pitch; }
            set { Source.pitch = value; }
        }

        public bool Loop
        {
            get { return Source.loop; }
            set { Source.loop = value; }
        }

        public Sound(AudioSource Source, string ClipName, float Volume, bool Loop = false, float Pitch = 1.00f)
        {
            this.Source = Source;
            this.ClipName = ClipName;
            this.Volume = Volume;
            this.Pitch = Pitch;
            this.Loop = Loop;
            this.Source.clip = Resources.Load<AudioClip>(ClipName);
        }

        public void Play()
        {
            Source.Play();
        }

        public void Stop()
        {
            Source.Stop();
        }

    }

    #endregion

    #region Bubble Journey Methods

    public static string FormatTimeInMS(float MS)
    {
        // Timer

        int Minutes = (int)(MS / 60000f);
        int Seconds = (int)((MS % 60000) / 1000);
        int Milliseconds = (int)(MS % 1000);
        return System.String.Format("{0:00}:{1:00}:{2:00}", Minutes, Seconds, Milliseconds / 10);
        

    }
   
    #endregion

}