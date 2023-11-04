using System.Reflection;

namespace Fushigi.Bfres
{
    public class BfresFile
    {
        public Dictionary<string, Model> Models = new Dictionary<string, Model>();

        public string Name;

        private BinaryHeader BinHeader; //A header shared between bntx and other formats
        private ResHeader Header; //Bfres header

        public BfresFile(string filePath) {
            Read(File.OpenRead(filePath));
        }

        public BfresFile(Stream stream) {
            Read(stream);
        }

        public void Read(Stream stream)
        {
            var reader = stream.AsBinaryReader();

             stream.Read(Utils.AsSpan(ref BinHeader));
             stream.Read(Utils.AsSpan(ref Header));

            Name = reader.ReadStringOffset(Header.NameOffset);

            reader.Seek(Header.ModelOffset);
            var models = reader.ReadArray<Model>(Header.ModelCount);

            for (int i = 0; i < models.Count; i++)
                Models.Add(models[i].Name, models[i]);
        }
    }
}
