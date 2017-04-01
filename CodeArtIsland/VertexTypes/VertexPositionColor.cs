using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Runtime.InteropServices;
using System;
using System.IO;

namespace OnTheOriginOfFishies.VertexTypes
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionColor
    {
        public static readonly InputElement[] InputElements = new InputElement[]
        {
            new InputElement("SV_POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
            new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0, InputClassification.PerVertexData, 0)
        };

        public readonly Vector3 Position;
        public readonly Color4 Color;

        public VertexPositionColor(Vector3 position, Color4 color)
        {
            Position = position;
            Color = color;
        }

        internal static VertexPositionColor[] LoadSTL(string filename)
        {
            VertexPositionColor[] verts;

            using (var fs = File.OpenRead(filename))
            using (var r = new BinaryReader(fs))
            {
                for (int i = 0; i < 80; i++)
                    r.ReadByte();
                verts = new VertexPositionColor[r.ReadUInt32() * 3];

                for (int i = 0; i < verts.Length; i++)
                {
                    if (i % 3 == 0)
                    {
                        r.ReadSingle();
                        r.ReadSingle();
                        r.ReadSingle();
                    }

                    verts[i] = new VertexPositionColor(new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()), Color4.White);
                    if (i % 3 == 2)
                        r.ReadUInt16();
                }
            }

            return verts;
        }

        internal static VertexPositionColor[] LoadSTL(string filename, out int vertCount)
        {
            var r = LoadSTL(filename);
            vertCount = r.Length;
            return r;
        }
    }
}
