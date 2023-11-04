using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Fushigi.Bfres
{
    public class Model : IResData, INamed
    {
        public string Name { get; set; }

        public void Read(BinaryReader reader)
        {
            var header = new ModelHeader();
            reader.BaseStream.Read(Utils.AsSpan(ref header));

            long pos = reader.BaseStream.Position;

            Name = reader.ReadStringOffset(header.NameOffset);
            Console.WriteLine($"Model - {Name} -");

            reader.ReadArray<VertexBuffer>(header.VertexArrayOffset, header.VertexBufferCount);
            reader.ReadArray<Shape>(header.ShapeArrayOffset, header.ShapeCount);

            //return
            reader.Seek(pos);
        }
    }
}
