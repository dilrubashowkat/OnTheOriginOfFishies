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
    internal class DecorRenderer : IDisposable
    {
        public readonly RenderBase RenderBase;

        private UVertexShader vertexShaderSway;
        private PixelShader plantPixelShader;
        private PixelShader pixelShader;
        private UVertexShader vertexShader;

        private DynamicVertexBuffer plantInstanceBuffer;

        private Buffer plantVertexBuffer;
        private int plantVertCount;

        private Buffer floorVertexBuffer;
        private int floorVertCount;

        private UConstantBuffer paramsBuffer;

        private Vector4[] plantData;
        private float[] plantWaveSpeed;

        public DecorRenderer(RenderBase rb)
        {
            RenderBase = rb;

            paramsBuffer = new UConstantBuffer(RenderBase, Utilities.SizeOf<Vector4>() * 2);

            var shaderFlags =
#if DEBUG
                ShaderFlags.Debug;
#else
                ShaderFlags.None;
#endif

            vertexShaderSway = new UVertexShader(RenderBase, "shaders/Vertex_PNI_Wave.hlsl", VertexPositionNormal.InputElementsInstanced);
            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("shaders/Pixel_PNI_FishPointLightsAbs.hlsl", "main", "ps_4_0", shaderFlags))
                plantPixelShader = new PixelShader(RenderBase.Device, pixelShaderByteCode);
            vertexShader = new UVertexShader(RenderBase, "shaders/Vertex_PN_Standard.hlsl", VertexPositionNormal.InputElements);
            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("shaders/Pixel_PC_FishPointLightsAbs.hlsl", "main", "ps_4_0", shaderFlags))
                pixelShader = new PixelShader(RenderBase.Device, pixelShaderByteCode);

            plantVertexBuffer = Buffer.Create(RenderBase.Device, BindFlags.VertexBuffer,
                VertexPositionNormal.LoadSTL("models/plant.stl", out plantVertCount));

            floorVertexBuffer = Buffer.Create(RenderBase.Device, BindFlags.VertexBuffer,
                VertexPositionNormal.LoadSTL("models/tankFloor.stl", out floorVertCount));

            /*plantData = new Vector4[]
            {
                new Vector4(-0.8f, .07f, 0, 0.1f),
                new Vector4(0.9f, .04f, 0, 0.2f),
                new Vector4(-0.95f, .06f, 0, 0.15f),
            };
            plantWaveSpeed = new float[]
            {
                1.0f,
                1.2f,
                0.6f,
            };*/

            Random rnd = new Random();
            plantData = new Vector4[500];
            plantWaveSpeed = new float[plantData.Length];
            for (int i = 0; i < plantData.Length; i++)
            {
                var sizeMul = 0.333f;
                if (rnd.Next(0, 25) == 0)
                    sizeMul = 0.5f;
                plantData[i] = new Vector4(rnd.NextFloat(-16.0f / 9.0f * 1.5f, 16.0f / 9.0f * 1.5f), rnd.NextFloat(0.01f, 0.1f) * sizeMul, 0, rnd.NextFloat(0.05f, 0.3f));
                plantWaveSpeed[i] = rnd.NextFloat(0.01f, 2.0f);
            }

            plantInstanceBuffer = new DynamicVertexBuffer(rb, (Utilities.SizeOf<Matrix>() + Utilities.SizeOf<Vector4>()) * plantData.Length);
        }

        private float ET;
        public void Render()
        {
            ET += 1.0f / 60.0f;

            RenderBase.SetWorld(Matrix.Identity);

            vertexShaderSway.Set();
            RenderBase.DeviceContext.PixelShader.Set(plantPixelShader);
            paramsBuffer.Set(2);

            RenderBase.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            //Draw Fish
            RenderBase.DeviceContext.OutputMerger.BlendState = RenderBase.BlendStates.Default;

            //Console.WriteLine("A");
            var di = plantInstanceBuffer.Map();
            for (int i = 0; i < plantData.Length; i++)
            {
                //Console.WriteLine("B");
                ////////var ds = paramsBuffer.Map();
                ////////ds.Write(new Vector4(0.0f, plantData[i].W, 0.0f, 1.0f));
                ////////ds.Write(ET * plantWaveSpeed[i] + (i % 100) * (i % 100));
                ////////paramsBuffer.UnMap();
                //RenderBase.SetWorld(Matrix.Scaling(plantData[i].Y) * Matrix.RotationZ(plantData[i].Z) * Matrix.Translation(plantData[i].X, -1.1f, -0.001f * (150 - plantData.Length + i)));
                di.Write(Matrix.Scaling(plantData[i].Y) * Matrix.RotationZ(plantData[i].Z) * Matrix.Translation(plantData[i].X, -1.1f, -0.001f * (i - plantData.Length * 7 / 10)));
                di.Write(new Vector4(0.0f, plantData[i].W, 0.0f, ET * plantWaveSpeed[i] + (i % 100) * (i % 100)));
                //RenderBase.DeviceContext.Draw(plantVertCount, 0);
            }
            plantInstanceBuffer.UnMap();
            //Console.WriteLine("C");

            RenderBase.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(plantVertexBuffer, Utilities.SizeOf<VertexPositionNormal>(), 0), new VertexBufferBinding(plantInstanceBuffer.Buffer, Utilities.SizeOf<Matrix>() + Utilities.SizeOf<Vector4>(), 0));
            RenderBase.DeviceContext.DrawInstanced(plantVertCount, plantData.Length * 7 / 10, 0, 0);

            {
                vertexShader.Set();
                RenderBase.DeviceContext.PixelShader.Set(pixelShader);

                RenderBase.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(floorVertexBuffer, Utilities.SizeOf<VertexPositionNormal>(), 0));

                var ds = paramsBuffer.Map();
                ds.Write(new Vector4(0.7f, 0.7f, 0.7f, 0.0f));
                ds.Write(0);
                paramsBuffer.UnMap();
                //paramsBuffer.Set(2);
                RenderBase.SetWorld(Matrix.Scaling((16.0f / 9.0f) / 4.0f) * Matrix.Translation(0, -1.0f, 0.0f));
                //System.Threading.Thread.Sleep(250);
                RenderBase.DeviceContext.Draw(floorVertCount, 0);
            }

            /*{
                var ds = paramsBuffer.Map();
                ds.Write(new Vector4(1, 1, 1, 1));
                paramsBuffer.UnMap();
                RenderBase.SetWorld(Matrix.Scaling(0.15f) * Matrix.Translation(Input.XInputInput.State[0].RightThumbX / (float)short.MaxValue, Input.XInputInput.State[0].RightThumbY / (float)short.MaxValue, 0));//Matrix.Scaling(0.05f));
                RenderBase.DeviceContext.Draw(3 * 3, 0);
            }*/
        }

        public void Render2()
        {
            /*RenderBase.SetWorld(Matrix.Identity);

            vertexShaderSway.Set();
            RenderBase.DeviceContext.PixelShader.Set(pixelShader);
            paramsBuffer.Set(2);

            RenderBase.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            //Draw Fish
            RenderBase.DeviceContext.OutputMerger.BlendState = RenderBase.BlendStates.Default;
            RenderBase.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(plantVertexBuffer, Utilities.SizeOf<VertexPositionNormal>(), 0));

            for (int i = plantData.Length - 150; i < plantData.Length; i++)
            {
                var ds = paramsBuffer.Map();
                ds.Write(new Vector4(0.0f, plantData[i].W, 0.0f, 1.0f));
                ds.Write(ET * plantWaveSpeed[i] + (i % 100) * (i % 100));
                paramsBuffer.UnMap();
                RenderBase.SetWorld(Matrix.Scaling(plantData[i].Y) * Matrix.RotationZ(plantData[i].Z) * Matrix.Translation(plantData[i].X, -1.1f, -0.001f * (150 - plantData.Length + i)));
                RenderBase.DeviceContext.Draw(plantVertCount, 0);
            }

            /*{
                var ds = paramsBuffer.Map();
                ds.Write(new Vector4(1, 1, 1, 1));
                paramsBuffer.UnMap();
                RenderBase.SetWorld(Matrix.Scaling(0.15f) * Matrix.Translation(Input.XInputInput.State[0].RightThumbX / (float)short.MaxValue, Input.XInputInput.State[0].RightThumbY / (float)short.MaxValue, 0));//Matrix.Scaling(0.05f));
                RenderBase.DeviceContext.Draw(3 * 3, 0);
            }*/

            //if (plantData.Length > 150)
            {
                vertexShaderSway.Set();
                RenderBase.DeviceContext.PixelShader.Set(plantPixelShader);
                //paramsBuffer.Set(2);

                RenderBase.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(plantVertexBuffer, Utilities.SizeOf<VertexPositionNormal>(), 0), new VertexBufferBinding(plantInstanceBuffer.Buffer, Utilities.SizeOf<Matrix>() + Utilities.SizeOf<Vector4>(), 0));
                RenderBase.DeviceContext.DrawInstanced(plantVertCount, plantData.Length * 3 / 10, 0, plantData.Length * 7 / 10);
            }
        }

        public void Dispose()
        {
            paramsBuffer.Dispose();
            plantVertexBuffer.Dispose();
            floorVertexBuffer.Dispose();
            pixelShader.Dispose();
            vertexShaderSway.Dispose();
            vertexShader.Dispose();
        }
    }
}
