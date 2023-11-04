﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Fushigi.Bfres
{
    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public struct BinaryHeader //A header shared between bntx and other formats
    {
        public ulong Magic; //MAGIC + padding

        public byte VersionMicro;
        public byte VersionMinor;
        public ushort VersionMajor;

        public ushort ByteOrder;
        public byte Alignment;
        public byte TargetAddressSize;
        public uint NameOffset;
        public ushort Flag;
        public ushort BlockOffset;
        public uint RelocationTableOffset;
        public uint FileSize;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public struct ResHeader
    {
        public ulong NameOffset;

        public ulong ModelOffset;
        public ulong ModelDictionaryOffset;

        public ulong Reserved0;
        public ulong Reserved1;
        public ulong Reserved2;
        public ulong Reserved3;

        public ulong SkeletalAnimOffset;
        public ulong SkeletalAnimDictionaryOffset;
        public ulong MaterialAnimOffset;
        public ulong MaterialAnimDictionarymOffset;
        public ulong BoneVisAnimOffset;
        public ulong BoneVisAnimDictionarymOffset;
        public ulong ShapeAnimOffset;
        public ulong ShapeAnimDictionarymOffset;
        public ulong SceneAnimOffset;
        public ulong SceneAnimDictionarymOffset;

        public ulong MemoryPoolOffset;
        public ulong MemoryPoolInfoOffset;

        public ulong EmbeddedFilesOffset;
        public ulong EmbeddedFilesDictionaryOffset;

        public ulong UserPointer;

        public ulong StringTableOffset;
        public uint StringTableSize;

        public ushort ModelCount;

        public ushort Reserved4;
        public ushort Reserved5;

        public ushort SkeletalAnimCount;
        public ushort MaterialAnimCount;
        public ushort BoneVisAnimCount;
        public ushort ShapeAnimCount;
        public ushort SceneAnimCount;
        public ushort EmbeddedFileCount;

        public byte ExternalFlags;
        public byte Reserved6;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public struct ModelHeader
    {
        public uint Magic;
        public uint Reserved;
        public ulong NameOffset;
        public ulong PathOffset;

        public ulong SkeletonOffset;
        public ulong VertexArrayOffset;
        public ulong ShapeArrayOffset;
        public ulong ShapeDictionaryOffset;
        public ulong MaterialArrayOffset;
        public ulong MaterialDictionaryOffset;
        public ulong ShaderAssignArrayOffset;

        public ulong UserDataArrayOffset;
        public ulong UserDataDictionaryOffset;

        public ulong UserPointer;

        public ushort VertexBufferCount;
        public ushort ShapeCount;
        public ushort MaterialCount;
        public ushort ShaderAssignCount;
        public ushort UserDataCount;

        public ushort Reserved1;
        public uint Reserved2;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public struct VertexBufferHeader
    {
        public uint Magic;
        public uint Reserved;

        public ulong AttributeArrayOffset;
        public ulong AttributeArrayDictionary;

        public ulong MemoryPoolPointer;

        public ulong RuntimeBufferArray;
        public ulong UserBufferArray;

        public ulong VertexBufferInfoArray;
        public ulong VertexBufferStrideArray;
        public ulong UserPointer;

        public uint BaseMemoryOffset;
        public byte VertexAttributeCount;
        public byte VertexBufferCount;

        public ushort Index;
        public uint VertexCount;

        public ushort Reserved1;
        public ushort VertexBufferAlignment;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public struct ShapeHeader
    {
        public uint Magic;
        public uint Flags;
        public ulong NameOffset;
        public ulong PathOffset;

        public ulong MeshArrayOffset;
        public ulong SkinBoneIndicesOffset;

        public ulong KeyShapeArrayOffset;
        public ulong KeyShapeDictionaryOffset;

        public ulong BoundingBoxOffset;
        public ulong BoundingSphereOffset;

        public ulong UserPointer;

        public ushort Index;
        public ushort MaterialIndex;
        public ushort BoneIndex;
        public ushort VertexBufferIndex;
        public ushort SkinBoneIndex;

        public byte MaxSkinInfluence;
        public byte MeshCount;
        public byte KeyShapeCount;
        public byte KeyAttributeCount;

        public ushort Reserved;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public struct ShapeBounding
    {
        public float CenterX;
        public float CenterY;
        public float CenterZ;

        public float ExtentX;
        public float ExtentY;
        public float ExtentZ;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public struct ShapeRadius
    {
        public float CenterX;
        public float CenterY;
        public float CenterZ;

        public float Radius;
    }


    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public struct MeshHeader
    {
        public ulong SubMeshArrayOffset;
        public ulong IndexBufferInfoOffset;

        public uint IndexBufferMemoryOffset;
        public uint PrimType;
        public uint IndexFormat;
        public uint IndexCount;
        public uint BaseIndex;
        public ushort SubMeshCount;
        public ushort Reserved;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public struct SubMesh
    {
        public uint Offset;
        public uint Count;
    }
}
