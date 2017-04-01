using Buffer = SharpDX.Direct3D11.Buffer;

using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;
using SharpDX;
using SharpDX.Direct3D;
using OnTheOriginOfFishies.VertexTypes;
using OnTheOriginOfFishies.ConstantBuffers;

namespace OnTheOriginOfFishies
{
    internal class FoodRenderer : IDisposable
    {
        public readonly RenderBase RenderBase;

        public FoodManager Manager;

        private UVertexShader vertexShader;
        private PixelShader pixelShader;

        private Buffer vertexBuffer;

        private UConstantBuffer paramsBuffer;

        private int vertCount;

        public FoodRenderer(RenderBase rb, FoodManager manager)
        {
            RenderBase = rb;
            Manager = manager;

            paramsBuffer = new UConstantBuffer(RenderBase, Utilities.SizeOf<Vector4>() * 2);

            var shaderFlags =
#if DEBUG
                ShaderFlags.Debug;
#else
                ShaderFlags.None;
#endif

            vertexShader = new UVertexShader(RenderBase, "shaders/Vertex_PN_Standard.hlsl", VertexPositionNormal.InputElements);
            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("shaders/Pixel_PC_FishPointLights.hlsl", "main", "ps_4_0", shaderFlags))
                pixelShader = new PixelShader(RenderBase.Device, pixelShaderByteCode);

            vertexBuffer = Buffer.Create(RenderBase.Device, BindFlags.VertexBuffer,
                //new VertexPositionColor[] { new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0.0f), Color.White), new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.0f), Color.White), new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0.0f), Color.White),
                //                            new VertexPositionColor(new Vector3(0.5f, -0.5f, 0.0f), Color.White), new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0.0f), Color.White), new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.0f), Color.White)});
                VertexPositionNormal.LoadSTL("models/food.stl", out vertCount));
        }

        public void Render()
        {
            RenderBase.SetWorld(Matrix.Identity);

            vertexShader.Set();
            RenderBase.DeviceContext.PixelShader.Set(pixelShader);
            paramsBuffer.Set(2);

            RenderBase.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            //Draw Fish
            RenderBase.DeviceContext.OutputMerger.BlendState = RenderBase.BlendStates.Default;
            RenderBase.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<VertexPositionNormal>(), 0));

            foreach (var f in Manager.FoodList)
            {
                var ds = paramsBuffer.Map();
                ds.Write(f.Color);
                paramsBuffer.UnMap();
                RenderBase.SetWorld(Matrix.Scaling(f.Size.X, f.Size.Y, (f.Size.X + f.Size.Y) / 2.0f) * Matrix.RotationZ(f.Rotation) * Matrix.Translation(f.Position.X, f.Position.Y, 0.0f));
                RenderBase.DeviceContext.Draw(vertCount, 0);
            }

            /*{
                var ds = paramsBuffer.Map();
                ds.Write(new Vector4(1, 1, 1, 1));
                paramsBuffer.UnMap();
                RenderBase.SetWorld(Matrix.Scaling(0.15f) * Matrix.Translation(Input.XInputInput.State[0].RightThumbX / (float)short.MaxValue, Input.XInputInput.State[0].RightThumbY / (float)short.MaxValue, 0));//Matrix.Scaling(0.05f));
                RenderBase.DeviceContext.Draw(3 * 3, 0);
            }*/
        }

        public void Dispose()
        {
            paramsBuffer.Dispose();
            vertexBuffer.Dispose();
            pixelShader.Dispose();
            vertexShader.Dispose();
        }
    }
}
