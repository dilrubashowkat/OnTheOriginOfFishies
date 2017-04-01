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
    internal class FishRenderer : IDisposable
    {
        public readonly RenderBase RenderBase;

        public FishManager Manager;

        private UVertexShader vertexShader;
        private PixelShader pixelShader;

        private int[] vertexCount;
        private Buffer[] vertexBuffer;
        private Buffer coneVertexBuffer;

        private UConstantBuffer paramsBuffer;

        public FishRenderer(RenderBase rb, FishManager manager)
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

            vertexShader = new UVertexShader(RenderBase, "shaders/Vertex_PC_Explode.hlsl", VertexPositionNormal.InputElements);
            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("shaders/Pixel_PC_FishPointLights.hlsl", "main", "ps_4_0", shaderFlags))
                pixelShader = new PixelShader(RenderBase.Device, pixelShaderByteCode);

            vertexBuffer = new Buffer[3];
            vertexCount = new int[vertexBuffer.Length];
            vertexBuffer[0] = Buffer.Create(RenderBase.Device, BindFlags.VertexBuffer, VertexPositionNormal.LoadSTL("models/fish.stl", out vertexCount[0]));
            vertexBuffer[1] = Buffer.Create(RenderBase.Device, BindFlags.VertexBuffer, VertexPositionNormal.LoadSTL("models/fish4.stl", out vertexCount[1]));
            vertexBuffer[2] = Buffer.Create(RenderBase.Device, BindFlags.VertexBuffer, VertexPositionNormal.LoadSTL("models/fish5.stl", out vertexCount[2]));

            coneVertexBuffer = Buffer.Create(RenderBase.Device, BindFlags.VertexBuffer,
                new VertexPositionColor[] { new VertexPositionColor(new Vector3(0.0f, 0.0f, 0.0f), Color.White), new VertexPositionColor(new Vector3(1.0f, 1.0f, 0.0f), Color.White), new VertexPositionColor(new Vector3(1.0f, -1.0f, 0.0f), Color.White) });
        }

        public void Update()
        {
            var dt = 1.0f / 60.0f;
            foreach (var fish in Manager.DeadFish)
            {
                fish.FoodLevel -= dt * 0.2f;
                fish.Position += fish.Velocity * dt * new Vector2((float)Math.Cos(fish.Rotation), (float)Math.Sin(fish.Rotation));
            }

            if (Manager.DeadFish.Count > 0)
                if (Manager.DeadFish[0].FoodLevel < -3)
                    Manager.DeadFish.RemoveAt(0);
        }

        private float et;
        public void Render()
        {
            //RenderBase.DeviceContext.Rasterizer.State = RenderBase.RasterizerStates.None;

            var dt = 1.0f / 60.0f;
            et += dt;
            //RenderBase.SetWorld(Matrix.Identity);

            vertexShader.Set();
            RenderBase.DeviceContext.PixelShader.Set(pixelShader);
            paramsBuffer.Set(2);

            RenderBase.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            RenderBase.DeviceContext.OutputMerger.BlendState = RenderBase.BlendStates.Additive;
            if ((Input.XInputInput.State[0].Buttons & SharpDX.XInput.GamepadButtonFlags.Y) == SharpDX.XInput.GamepadButtonFlags.Y)
            {
                //Draw Sight
                RenderBase.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(coneVertexBuffer, Utilities.SizeOf<VertexPositionColor>(), 0));

                for (int i = 0; i < Manager.FishCount && i == 0; i++)
                {
                    var fish = Manager.FishList[i];
                    for (int j = 0; j < fish.Sights.Length; j++)
                    {
                        var ds = paramsBuffer.Map();
                        if (fish.Sights[j].Color.W == 0.0f)
                            ds.Write(new Vector4(1, 1, 0, 0.05f));
                        else
                            ds.Write(fish.Sights[j].Color);
                        ds.Write(0.0f);
                        paramsBuffer.UnMap();
                        RenderBase.SetWorld(Matrix.Scaling((float)Math.Cos(fish.Sights[j].WidthAngle / 2.0f) * fish.Sights[j].Distance, (float)Math.Sin(fish.Sights[j].WidthAngle / 2.0f) * fish.Sights[j].Distance, 1.0f) * Matrix.RotationZ(fish.Sights[j].MidAngle) * Matrix.Translation(fish.Sights[j].Start.X, fish.Sights[j].Start.Y, 0.0f) * Matrix.RotationZ(fish.Rotation) * Matrix.Translation(fish.Position.X, fish.Position.Y, 0.0f));//Matrix.Scaling(0.05f));
                        RenderBase.DeviceContext.Draw(3, 0);
                    }
                }
            }

            for (int mi = 0; mi < vertexBuffer.Length; mi++)
            {
                RenderBase.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer[mi], Utilities.SizeOf<VertexPositionNormal>(), 0));

                foreach (var fish in Manager.DeadFish)
                {
                    if (fish.MeshIndex != mi)
                        continue;
                    var ds = paramsBuffer.Map();
                    ds.Write(fish.Color.X);
                    ds.Write(fish.Color.Y);
                    ds.Write(fish.Color.Z);
                    ds.Write(fish.Color.W * (1.0f + fish.FoodLevel / 3.0f));
                    ds.Write(fish.FoodLevel * 10.0f);
                    ds.Write(fish.Seed);
                    paramsBuffer.UnMap();
                    RenderBase.SetWorld(Matrix.Scaling(fish.Size.X, fish.Size.Y, (fish.Size.X + fish.Size.Y) / 2.0f) * Matrix.RotationX(fish.Roll) * Matrix.RotationZ(fish.Rotation) * Matrix.Translation(fish.Position.X, fish.Position.Y, 0.0f));//Matrix.Scaling(0.05f));
                    RenderBase.DeviceContext.Draw(vertexCount[mi], 0);
                }

                //Draw Fish
                RenderBase.DeviceContext.OutputMerger.BlendState = RenderBase.BlendStates.Default;

                for (int i = 0; i < Manager.FishCount; i++)
                {
                    var fish = Manager.FishList[i];
                    if (fish.MeshIndex != mi)
                        continue;
                    var ds = paramsBuffer.Map();
                    ds.Write(Manager.FishList[i].Color);
                    ds.Write(0);
                    ds.Write(0);
                    paramsBuffer.UnMap();
                    RenderBase.SetWorld(Matrix.Scaling(fish.Size.X, fish.Size.Y, (fish.Size.X + fish.Size.Y) / 2.0f) * Matrix.RotationX(fish.Roll) * Matrix.RotationZ(fish.Rotation) * Matrix.Translation(fish.Position.X, fish.Position.Y, 0.0f));//Matrix.Scaling(0.05f));
                    RenderBase.DeviceContext.Draw(vertexCount[mi], 0);
                }
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
            coneVertexBuffer.Dispose();
            for (int i = 0; i < vertexBuffer.Length; i++)
                vertexBuffer[i].Dispose();
            pixelShader.Dispose();
            vertexShader.Dispose();
        }
    }
}
