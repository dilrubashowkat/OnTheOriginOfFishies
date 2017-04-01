using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;

namespace OnTheOriginOfFishies
{
    internal class FishGlow : IDisposable
    {
        public readonly RenderBase RenderBase;

        public FishManager Manager;

        private UVertexShader vertexShader;
        private PixelShader pixelShader;

        private ConstantBuffers.UConstantBuffer cb;

        public float Intensity = 0.75f;

        public FishGlow(RenderBase rb, FishManager manager)
        {
            RenderBase = rb;
            Manager = manager;

            var shaderFlags =
#if DEBUG
                ShaderFlags.Debug;
#else
                ShaderFlags.None;
#endif

            vertexShader = new UVertexShader(RenderBase, "shaders/Vertex_FS.hlsl", null);
            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("shaders/Pixel_FishGlow.hlsl", "main", "ps_4_0", shaderFlags))
                pixelShader = new PixelShader(RenderBase.Device, pixelShaderByteCode);

            cb = new ConstantBuffers.UConstantBuffer(RenderBase, 4 * 4 + FishManager.MAX_FISH_COUNT * 2 * 4 * 4);
        }

        private float et;
        public void Render()
        {
            RenderBase.DeviceContext.InputAssembler.SetVertexBuffers(0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            RenderBase.DeviceContext.InputAssembler.SetIndexBuffer(null, (SharpDX.DXGI.Format)0, 0);

            vertexShader.Set();
            RenderBase.DeviceContext.PixelShader.Set(pixelShader);

            RenderBase.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            RenderBase.DeviceContext.OutputMerger.BlendState = RenderBase.BlendStates.Additive;

            var ds = cb.Map();
            ds.Write(Manager.FishCount);
            ds.Write(150.0f * (1 + Input.XInputInput.State[0].LeftTrigger / 255.0f * 5) * (1 - Input.XInputInput.State[0].RightTrigger / 255.0f));
            ds.Write(Intensity);//(Input.XInputInput.State[0].LeftThumbY / (float)short.MaxValue + 1.0f) * 0.75f);
            ds.Write((et += (1.0f / 60.0f)) * 100.0f);

            for (int i = 0; i < FishManager.MAX_FISH_COUNT; i++)
            {
                if (Manager.FishList[i] != null)
                {
                    ds.Write(Manager.FishList[i].Position);
                    ds.Write(0.0f);
                    ds.Write(1.0f);
                }
                else
                    ds.Write(Vector4.Zero);
            }
            for (int i = 0; i < FishManager.MAX_FISH_COUNT; i++)
            {
                if (Manager.FishList[i] != null)
                {
                    ds.Write(Manager.FishList[i].Color);
                }
                else
                    ds.Write(Vector4.Zero);
            }
            ds = null;

            cb.UnMap();

            cb.Set(1);

            RenderBase.DeviceContext.Draw(3, 0);

            RenderBase.DeviceContext.OutputMerger.BlendState = RenderBase.BlendStates.Default;
        }

        public void Dispose()
        {
            vertexShader.Dispose();
            pixelShader.Dispose();
            cb.Dispose();
        }
    }
}
