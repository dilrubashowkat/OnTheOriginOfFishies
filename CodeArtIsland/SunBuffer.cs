using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTheOriginOfFishies
{
    public class SunBuffer : IDisposable
    {
        private ConstantBuffers.UConstantBuffer cb;

        internal SunBuffer(RenderBase rb)
        {
            cb = new ConstantBuffers.UConstantBuffer(rb, Utilities.SizeOf<Vector4>() * 2);
        }

        public void Set(Vector3 dir, Vector3 col, float intensity)
        {
            var ds = cb.Map();

            dir.Normalize();
            ds.Write(dir);
            ds.Write(intensity);
            ds.Write(col);
            ds.Write(0.0f);

            ds = null;
            cb.UnMap();

            cb.Set(4);
        }

        public void Dispose()
        {
            cb.Dispose();
        }
    }
}
