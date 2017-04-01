using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;

namespace OnTheOriginOfFishies
{
    internal class RaysRenderer : IDisposable
    {
        public readonly RenderBase RenderBase;

        private UVertexShader vertexShader;
        private PixelShader pixelShader;

        private ConstantBuffers.UConstantBuffer cb;

        public Vector2 CentrePoint;
        public float CutOff = 0.1f;
        public float Intensity = 0.02f;
        private float ET;

        public RaysRenderer(RenderBase rb)
        {
            RenderBase = rb;

            var shaderFlags =
#if DEBUG
                ShaderFlags.Debug;
#else
                ShaderFlags.None;
#endif

            vertexShader = new UVertexShader(RenderBase, "shaders/Vertex_FS.hlsl", null);
            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("shaders/Pixel_GodRays.hlsl", "main", "ps_4_0", shaderFlags))
                pixelShader = new PixelShader(RenderBase.Device, pixelShaderByteCode);

            cb = new ConstantBuffers.UConstantBuffer(RenderBase, 4 * 4 * 2);
        }

        public void Render(ShaderResourceView tex, float dt)
        {
            ET += dt * 0.5f;

            //CentrePoint = new Vector2((float)Math.Cos(ET * 3.14f) * 1000 + RenderBase.RenderWidth / 2, (float)Math.Sin(ET) * 1000 + RenderBase.RenderHeight / 2);

            RenderBase.DeviceContext.InputAssembler.SetVertexBuffers(0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            RenderBase.DeviceContext.InputAssembler.SetIndexBuffer(null, (SharpDX.DXGI.Format)0, 0);

            vertexShader.Set();
            RenderBase.DeviceContext.PixelShader.Set(pixelShader);
            RenderBase.DeviceContext.PixelShader.SetShaderResource(0, tex);

            RenderBase.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            //RenderBase.DeviceContext.OutputMerger.BlendState = RenderBase.Additive;

            var ds = cb.Map();
            ds.Write(CentrePoint);
            ds.Write(CutOff);
            //ds.Write((float)Math.Tan(ET * 0.1f * Math.Tan(ET * 2418.241f)) * 1.0f);
            ds.Write(Intensity);
            ds.Write(0);

            ds = null;

            cb.UnMap();

            cb.Set(3);

            RenderBase.DeviceContext.Draw(3, 0);

            //RenderBase.DeviceContext.OutputMerger.BlendState = RenderBase.Default;
        }

        public void Dispose()
        {
            vertexShader.Dispose();
            pixelShader.Dispose();
            cb.Dispose();
        }
    }
}
