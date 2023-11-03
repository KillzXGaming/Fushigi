using Fushigi.Byml;
using Fushigi.gl;
using Fushigi.util;
using ImGuiNET;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using System.Numerics;

namespace Fushigi.ui.widgets
{
    public class ActorDB
    {
        public static Dictionary<string, Actor> Actors = new Dictionary<string, Actor>();

        private static Dictionary<string, int> _icons = new Dictionary<string, int>();

        public static void Init()
        {
            if (Actors.Count > 0)
                return;

            byte[] actorDBBytes = RomFS.GetFileBytes($"RSDB/ActorInfo.Product.100.rstbl.byml.zs");
            /* grab actor information file */
            Byml.Byml actorDB = new Byml.Byml(new MemoryStream(FileUtil.DecompressData(actorDBBytes)));

            var list = ((BymlArrayNode)actorDB.Root);
            foreach (BymlHashTable node in list.Array.Cast<BymlHashTable>())
            {
                string category = ((BymlNode<string>)node["Category"]).Data;
                string id = ((BymlNode<string>)node["__RowId"]).Data;
                string modelfile = ((BymlNode<string>)node["ModelProjectName"]).Data;
                string modelname = ((BymlNode<string>)node["FmdbName"]).Data;

                Actors.Add(id, new Actor()
                {
                    Name = id,
                    Category = category,
                    FileName = modelfile,
                    ModelName = modelname,
                });
            }
        }

        public class Actor
        {
            public string Name { get; set; }
            public string Category { get; set; }
            public string FileName { get; set; }
            public string ModelName { get; set; }

            public int GetIcon(GL gl)
            {
                string icon = $"res/ActorIcons/{FileName}.bfres_{ModelName}.png";
                if (!_icons.ContainsKey(icon))
                {
                    if (File.Exists(icon))
                    {
                        GLTexture2D tex = new GLTexture2D(gl);
                        _icons.Add(icon, (int)tex.ID);
                        tex.Load(icon);
                    }
                    return -1;
                }
                else
                    return _icons[icon];
            }
        }
    }
}
