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
            //["SpriteDefault"] = Resources.Load<UnityEngine.Material>("Arena Grid"),
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
            ["CursorClick"] = new Sound(GO.AddComponent<AudioSource>(), "CursorClick", 0.05f),
            ["DefaultBust"] = new Sound(GO.AddComponent<AudioSource>(), "DefaultBust", 0.5f),
            ["Swap"] = new Sound(GO.AddComponent<AudioSource>(), "Swap", 0.5f),
            ["TileLand"] = new Sound(GO.AddComponent<AudioSource>(), "TileLand", 0.5f),
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
        public static Sound CursorClick { get => GetSound("CursorClick"); }
        public static Sound DefaultBust { get => GetSound("DefaultBust"); }
        public static Sound Swap { get => GetSound("Swap"); }
        public static Sound TileLand { get => GetSound("TileLand"); }

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

    #region Glassheart Methods

    public static UnityEngine.Sprite GetSpriteByTileColor(BasicTile.TileColor _Color)
    {
        return _Color switch
        {
            BasicTile.TileColor.Red    => Sprite.TileRed,
            BasicTile.TileColor.Yellow => Sprite.TileYellow,
            BasicTile.TileColor.Green  => Sprite.TileGreen,
            BasicTile.TileColor.Blue   => Sprite.TileBlue,
            BasicTile.TileColor.Indigo => Sprite.TileIndigo,
            BasicTile.TileColor.Purple => Sprite.TilePurple,
            _ => Sprite.TileRed,
        };
    }

    public static BasicTile.TileColor GetRandomTileColor()
    {
        return (BasicTile.TileColor) Random.Range((int)0, (int)6);
    }

    #endregion

}