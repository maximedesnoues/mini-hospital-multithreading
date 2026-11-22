using MiniHospital.Config;
using Raylib_cs;
using System.Numerics;

namespace MiniHospital.Rendering
{
    public static class TextureManager
    {
        private static Dictionary<TextureId, Texture2D> textures = new Dictionary<TextureId, Texture2D>();

        public static void LoadDefault()
        {
            if (textures == null)
                textures = new Dictionary<TextureId, Texture2D>();

            textures.Add(TextureId.Desk,
                Load("Assets/foreground_desk.png"));

            textures.Add(TextureId.Chair,
                Load("Assets/foreground_chair.png"));

            textures.Add(TextureId.Admission,
                Load("Assets/foreground_admission.png"));

            textures.Add(TextureId.Bench,
                Load("Assets/foreground_bench.png"));

            textures.Add(TextureId.Plant,
                Load("Assets/foreground_plant.png"));

            textures.Add(TextureId.Planter,
                Load("Assets/foreground_planter.png"));

            textures.Add(TextureId.Table,
                Load("Assets/foreground_table.png"));

            textures.Add(TextureId.Arch,
                Load("Assets/foreground_entrance.png"));

            textures.Add(TextureId.Characters,
                Load("Assets/characters.png"));

            textures.Add(TextureId.Background,
                Load("Assets/background.png"));

            textures.Add(TextureId.Bubbles,
                Load("Assets/bubbles.png"));
        }

        public static Texture2D Get(TextureId id)
        {
            if (textures == null || textures.Count == 0)
                LoadDefault();
            Texture2D ret;
            if (textures == null || !textures.TryGetValue(id, out ret))
                throw new Exception("Texture manquante : " + id);
            return ret;
        }

        public static void Unload()
        {
            foreach (Texture2D texture in textures.Values)
                Raylib.UnloadTexture(texture);

            textures.Clear();
        }

        private static Texture2D Load(string filename)
        {
            string path = Path.Combine(AppContext.BaseDirectory, filename);
            if (!File.Exists(path)) throw new FileNotFoundException("Asset manquant", path);
            return Raylib.LoadTexture(path);
        }
    }
}