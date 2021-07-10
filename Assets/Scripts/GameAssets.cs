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
            ["SpriteDefault"] = Resources.Load<GameObject>("Wall").GetComponent<SpriteRenderer>().sharedMaterial
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
            ["pop1"] = new Sound(GO.AddComponent<AudioSource>(), "pop1", 0.5f),
            ["pop2"] = new Sound(GO.AddComponent<AudioSource>(), "pop2", 0.5f),
            ["pop3"] = new Sound(GO.AddComponent<AudioSource>(), "pop3", 0.5f),
            ["death"] = new Sound(GO.AddComponent<AudioSource>(), "death", 0.5f)
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

   
    #endregion

}