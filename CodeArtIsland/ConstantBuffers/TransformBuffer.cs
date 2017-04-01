using SharpDX;
using System.Runtime.InteropServices;

namespace OnTheOriginOfFishies.ConstantBuffers
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TransformBuffer
    {
        public Matrix World;
        public Matrix View;
        public Matrix Projection;
    }
}
