using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Runtime.InteropServices;
using System;
using System.IO;

namespace OnTheOriginOfFishies.VertexTypes
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionNormal
    {
        public static readonly InputElement[] InputElements = new InputElement[]
        {
            new InputElement("SV_POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
            new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0)
        };

        public static readonly InputElement[] InputElementsInstanced = new InputElement[]
        {
            new InputElement("SV_POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
            new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),
            new InputElement("TEXCOORD", 0, Format.R32G32B32A32_Float, 0, 1, InputClassification.PerInstanceData, 1),
            new InputElement("TEXCOORD", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
            new InputElement("TEXCOORD", 2, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
            new InputElement("TEXCOORD", 3, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
            new InputElement("COLOR", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
        };

        public readonly Vector3 Position;
        public readonly Vector3 Normal;

        public VertexPositionNormal(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }

        internal static VertexPositionNormal[] LoadSTL(string filename)
        {
            VertexPositionNormal[] verts;

            Vector3 normal = Vector3.Zero;

            using (var fs = File.OpenRead(filename))
            using (var r = new BinaryReader(fs))
            {
                for (int i = 0; i < 80; i++)
                    r.ReadByte();
                verts = new VertexPositionNormal[r.ReadUInt32() * 3];

                for (int i = 0; i < verts.Length; i++)
                {
                    if (i % 3 == 0)
                        normal = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

                    verts[i] = new VertexPositionNormal(new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()), normal);
                    if (i % 3 == 2)
                        r.ReadUInt16();
                }
            }

            return verts;
        }

        internal static VertexPositionNormal[] LoadSTL(string filename, out int vertCount)
        {
            var r = LoadSTL(filename);
            vertCount = r.Length;
            return r;
        }
    }
}
