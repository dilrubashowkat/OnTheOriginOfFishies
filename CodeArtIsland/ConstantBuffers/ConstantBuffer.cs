using Buffer = SharpDX.Direct3D11.Buffer;

using System;
using SharpDX.Direct3D11;
using SharpDX;

namespace OnTheOriginOfFishies.ConstantBuffers
{
    internal class UConstantBuffer : IDisposable
    {
        public readonly RenderBase RenderBase;
        public Buffer Buffer;

        private DataStream stream;

        public UConstantBuffer(RenderBase rb, int size)
        {
            this.RenderBase = rb;

            Buffer = new Buffer(rb.Device, new BufferDescription()
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = size,
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
            });
        }

        public DataStream Map()
        {
            if (stream != null)
                throw new Exception();
            RenderBase.DeviceContext.MapSubresource(Buffer, MapMode.WriteDiscard, MapFlags.None, out stream);
            return stream;
        }

        public void UnMap()
        {
            if (stream == null)
                throw new Exception();
            RenderBase.DeviceContext.UnmapSubresource(Buffer, 0);
            stream.Dispose();
            stream = null;
        }

        public void Set(int buffer)
        {
            RenderBase.DeviceContext.VertexShader.SetConstantBuffer(buffer, Buffer);
            RenderBase.DeviceContext.PixelShader.SetConstantBuffer(buffer, Buffer);
        }

        public void Dispose()
        {
            if (stream != null)
                UnMap();
            Buffer.Dispose();
            Buffer = null;
        }
    }
}
