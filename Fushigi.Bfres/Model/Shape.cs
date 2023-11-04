using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Fushigi.Bfres
{
    public class VertexBuffer : IResData
    {


        public void Read(BinaryReader reader)
        {
            var header = new VertexBufferHeader();
            reader.BaseStream.Read(Utils.AsSpan(ref header));

            var bufferSizes = reader.ReadArray<VertexBufferInfo>(header.VertexBufferInfoArray, header.VertexBufferCount);
            var bufferStrides = reader.ReadArray<VertexBufferInfo>(header.VertexBufferStrideArray, header.VertexBufferCount);
            var attributes = reader.ReadArray<VertexAttribute>(header.AttributeArrayOffset, header.VertexAttributeCount);

            for (int buff = 0; buff < bufferSizes.Count; buff++)
            {
                var sizeInBytes = bufferSizes[buff];
                var stride = bufferStrides[buff];
            }
        }
    }

    public class VertexBufferInfo : IResData
    {
        public uint Size; //Size in bytes of buffer

        public void Read(BinaryReader reader)
        {
            Size = reader.ReadUInt32();
            reader.BaseStream.Seek(12, SeekOrigin.Current);
        }
    }

    public class VertexStrideInfo : IResData
    {
        public uint Stride;

        public void Read(BinaryReader reader)
        {
            Stride = reader.ReadUInt32();
            reader.BaseStream.Seek(12, SeekOrigin.Current);
        }
    }

    public class VertexAttribute : IResData, INamed
    {
        public string Name { get; set; }

        public ushort Format { get; set; }
        public ushort Offset { get; set; }
        public ushort BufferIndex { get; set; }

        public void Read(BinaryReader reader)
        {
            Name = reader.ReadStringOffset(reader.ReadUInt64());
            Format = reader.ReadUInt16();
            reader.ReadUInt16();
            Offset = reader.ReadUInt16();
            BufferIndex = reader.ReadUInt16();
        }
    }

    public class Shape : IResData, INamed
    {
        public string Name { get; set; }

        public void Read(BinaryReader reader)
        {
            var header = new ShapeHeader();
            reader.BaseStream.Read(Utils.AsSpan(ref header));

            Name = reader.ReadStringOffset(header.NameOffset);
            Console.WriteLine($"Shape - {Name} -");
        }
    }
    public class Mesh : IResData
    {
        public void Read(BinaryReader reader)
        {
            var header = new MeshHeader();
            reader.BaseStream.Read(Utils.AsSpan(ref header));
        }
    }
}
