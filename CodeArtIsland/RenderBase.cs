using SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;

using SharpDX.Windows;
using System;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;
using SharpDX;
using OnTheOriginOfFishies.ConstantBuffers;
using OnTheOriginOfFishies.Entities;
using System.Collections.Generic;
using SharpDX.Diagnostics;

namespace OnTheOriginOfFishies
{
    internal class RenderBase : IDisposable
    {
        private RenderForm renderForm;

        public int RenderWidth { get; private set; } = 1920;//1280;//
        public int RenderHeight { get; private set; } = 1080;//720;//

        public D3D.Device Device { get; private set; }
        public D3D.DeviceContext DeviceContext { get; private set; }
        private SwapChain swapChain;

        private D3D.RenderTargetView renderTargetView;

        private RenderTarget2D colorTarget;

        public struct BlendStatesStruct
        {
            public D3D.BlendState Default { get; internal set; }
            public D3D.BlendState Additive { get; internal set; }

            public void Dispose()
            {
                Default.Dispose();
                Additive.Dispose();
            }
        }
        public BlendStatesStruct BlendStates;

        public struct DepthStencilStatesStruct
        {
            public D3D.DepthStencilState Enabled;
            public D3D.DepthStencilState Disabled;

            public DepthStencilStatesStruct(D3D.Device device)
            {
                D3D.DepthStencilStateDescription desc = new D3D.DepthStencilStateDescription()
                {
                    IsDepthEnabled = true,
                    DepthWriteMask = D3D.DepthWriteMask.All,
                    DepthComparison = D3D.Comparison.Less,

                    IsStencilEnabled = false,
                    StencilReadMask = 0xFF,
                    StencilWriteMask = 0xFF,

                    FrontFace = new D3D.DepthStencilOperationDescription()
                    {
                        FailOperation = D3D.StencilOperation.Keep,
                        DepthFailOperation = D3D.StencilOperation.Increment,
                        PassOperation = D3D.StencilOperation.Keep,
                        Comparison = D3D.Comparison.Always
                    },

                    BackFace = new D3D.DepthStencilOperationDescription()
                    {
                        FailOperation = D3D.StencilOperation.Keep,
                        DepthFailOperation = D3D.StencilOperation.Decrement,
                        PassOperation = D3D.StencilOperation.Keep,
                        Comparison = D3D.Comparison.Always
                    }
                };

                Enabled = new D3D.DepthStencilState(device, desc);
                desc.IsDepthEnabled = false;
                desc.IsStencilEnabled = false;
                Disabled = new D3D.DepthStencilState(device, desc);
            }

            public void Dispose()
            {
                Enabled.Dispose();
                Disabled.Dispose();
            }
        }
        public DepthStencilStatesStruct DepthStencilStates;

        public struct RasterizerStatesStruct
        {
            public D3D.RasterizerState Back;
            public D3D.RasterizerState None;
            public D3D.RasterizerState Wire;

            public RasterizerStatesStruct(D3D.Device device)
            {
                var desc = new D3D.RasterizerStateDescription()
                {
                    CullMode = D3D.CullMode.Back,
                    DepthBias = 0,
                    DepthBiasClamp = 0,
                    FillMode = D3D.FillMode.Solid,
                    IsAntialiasedLineEnabled = false,
                    IsDepthClipEnabled = true,
                    IsFrontCounterClockwise = false,
                    IsMultisampleEnabled = true,
                    IsScissorEnabled = false,
                    SlopeScaledDepthBias = 0
                };
                Back = new D3D.RasterizerState(device, desc);

                desc.CullMode = D3D.CullMode.None;
                None = new D3D.RasterizerState(device, desc);

                desc.FillMode = D3D.FillMode.Wireframe;
                Wire = new D3D.RasterizerState(device, desc);
            }

            public void Dispose()
            {
                Wire.Dispose();
                None.Dispose();
                Back.Dispose();
            }
        }
        public RasterizerStatesStruct RasterizerStates;

        private UVertexShader vertexShader;
        private D3D.PixelShader pixelShader;

        private D3D.Buffer triangleVertexBuffer;

        private D3D.InputElement[] inputElements = new D3D.InputElement[]
        {
            new D3D.InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, D3D.InputClassification.PerVertexData, 0),
        };
        //private ShaderSignature inputSignature;
        //private D3D.InputLayout inputLayout;

        private Viewport viewport;

        private UConstantBuffer WVP;
        private Matrix World, View, Projection, Normal;

        private SunBuffer Sun;

        private FishManager fishManager;
        private FishRenderer fishRenderer;
        private RaysRenderer raysRenderer;
        private FishGlow fishGlow;

        private FoodManager foodManager;
        private FoodRenderer foodRenderer;

        private DecorRenderer decorRenderer;

        private float theta = 0.0f;

        public bool Wireframe;

        public RenderBase(int adapterIndex)
        {
            //Only enable when specifically using to debug, eats up CPU time...
            //ObjectTracker.StackTraceProvider = () => Environment.StackTrace;

            SetupWindow();
            Input.XInputInput.Init();
            InitDeviceRes(adapterIndex);
            InitDebugShaders();
            InitTriangle();

            WVP = new UConstantBuffer(this, Utilities.SizeOf<Matrix>() * 4);
            WVP.Set(0);

            Sun = new SunBuffer(this);

            var entities = new List<Entity>();
            var tank = new Tank();
            fishManager = new FishManager(entities, tank);
            fishRenderer = new FishRenderer(this, fishManager);
            fishGlow = new FishGlow(this, fishManager);
            raysRenderer = new RaysRenderer(this);

            foodManager = new FoodManager(entities, tank);
            foodRenderer = new FoodRenderer(this, foodManager);

            decorRenderer = new DecorRenderer(this);

            //swapChain.SetFullscreenState(true, null);
        }

        public void SetWorld(Matrix world)
        {
            World = world;
            World.Transpose();

            Normal = World;
            Normal.Invert();
            Normal.Transpose();

            WriteWVP();
        }

        public void SetCamera(Matrix view, Matrix proj)
        {
            View = view;
            Projection = proj;

            View.Transpose();
            Projection.Transpose();

            WriteWVP();
        }

        private void WriteWVP()
        {
            var ds = WVP.Map();
            
            ds.Write(World);
            ds.Write(View);
            ds.Write(Projection);
            ds.Write(Normal);

            WVP.UnMap();
        }

        private void SetupWindow()
        {
            renderForm = new RenderForm("Fishy Fishy")
            {
                ClientSize = new System.Drawing.Size(RenderWidth, RenderHeight),
                AllowUserResizing = false
            };
        }

        private void InitDeviceRes(int adapterIndex)
        {
            SharpDX.Configuration.EnableObjectTracking = true;

            ModeDescription backBufferDesc = new ModeDescription(RenderWidth, RenderHeight, new Rational(60, 1), Format.R8G8B8A8_UNorm);

            SwapChainDescription swapChainDesc = new SwapChainDescription()
            {
                ModeDescription = backBufferDesc,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = renderForm.Handle,
                IsWindowed = true
            };

            var factory = new Factory1();
            var adapters = factory.Adapters;
            var aid = adapterIndex == -1 ? 0 : adapterIndex;
            if (adapterIndex == -1)
                for (int i = 0; i < adapters.Length; i++)
                    if (adapters[i].Description.VendorId == 0x10DE)
                        aid = i;

            Console.WriteLine("Using adapter: " + adapters[aid].Description.Description);

            D3D.Device.CreateWithSwapChain(adapters[aid],//DriverType.Hardware,
#if DEBUG
                D3D.DeviceCreationFlags.Debug,
#else
                D3D.DeviceCreationFlags.None,
#endif
                swapChainDesc, out D3D.Device device, out swapChain);
            Device = device;
            DeviceContext = Device.ImmediateContext;

            using (D3D.Texture2D backBuffer = swapChain.GetBackBuffer<D3D.Texture2D>(0))
                renderTargetView = new D3D.RenderTargetView(Device, backBuffer);

            colorTarget = new RenderTarget2D(this, RenderWidth, RenderHeight, true);

            viewport = new Viewport(0, 0, RenderWidth, RenderHeight);
            DeviceContext.Rasterizer.SetViewport(viewport);

            BlendStates = new BlendStatesStruct();
            var blendStateDesc = D3D.BlendStateDescription.Default();
            BlendStates.Default = new D3D.BlendState(Device, blendStateDesc);
            blendStateDesc.RenderTarget[0] = new SharpDX.Direct3D11.RenderTargetBlendDescription(true, D3D.BlendOption.SourceAlpha, D3D.BlendOption.One, D3D.BlendOperation.Add, D3D.BlendOption.SourceAlpha, D3D.BlendOption.One, D3D.BlendOperation.Add, D3D.ColorWriteMaskFlags.All);
            BlendStates.Additive = new D3D.BlendState(Device, blendStateDesc);

            DepthStencilStates = new DepthStencilStatesStruct(Device);

            RasterizerStates = new RasterizerStatesStruct(device);
        }

        private void InitTriangle()
        {
            triangleVertexBuffer = D3D.Buffer.Create<Vector3>(Device, D3D.BindFlags.VertexBuffer,
                new Vector3[] { new Vector3(-0.5f, 0.5f, 0.0f), new Vector3(0.5f, 0.5f, 0.0f), new Vector3(0.0f, -0.5f, 0.0f) });
        }

        private void InitDebugShaders()
        {
            var shaderFlags =
#if DEBUG
                ShaderFlags.Debug;
#else
                ShaderFlags.None;
#endif
            vertexShader = new UVertexShader(this, "shaders/Vertex_P_PassThrough.hlsl", inputElements);
            /*using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile("shaders/Vertex_P_PassThrough.hlsl", "main", "vs_4_0", shaderFlags))
            {
                inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                vertexShader = new D3D.VertexShader(Device, vertexShaderByteCode);
            }*/
            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("shaders/Pixel_P_RedDebug.hlsl", "main", "ps_4_0", shaderFlags))
                pixelShader = new D3D.PixelShader(Device, pixelShaderByteCode);

            //inputLayout = new D3D.InputLayout(Device, inputSignature, inputElements);
            //DeviceContext.InputAssembler.InputLayout = inputLayout;
        }

        public void Run()
        {
            RenderLoop.Run(renderForm, RenderFrame);
        }

        bool camState;
        ulong frameNumber;
        int frameSkip = 1;
        private void RenderFrame()
        {
            Input.XInputInput.Update();
            if ((Input.XInputInput.State[0].Buttons & SharpDX.XInput.GamepadButtonFlags.Back) == SharpDX.XInput.GamepadButtonFlags.Back)
                renderForm.Close();
            if (((Input.XInputInput.State[0].Buttons & SharpDX.XInput.GamepadButtonFlags.Start) == SharpDX.XInput.GamepadButtonFlags.Start) && ((Input.XInputInput.PrevState[0].Buttons & SharpDX.XInput.GamepadButtonFlags.Start) != SharpDX.XInput.GamepadButtonFlags.Start))
            {
                swapChain.GetFullscreenState(out SharpDX.Mathematics.Interop.RawBool fs, out Output o);
                o?.Dispose();
                swapChain.SetFullscreenState(!fs, null);
            }
            fishManager.Update();
            fishRenderer.Update();
            foodManager.Update();

            //theta = 0.0f;
            
            if (((Input.XInputInput.State[0].Buttons & SharpDX.XInput.GamepadButtonFlags.X) == SharpDX.XInput.GamepadButtonFlags.X) && ((Input.XInputInput.PrevState[0].Buttons & SharpDX.XInput.GamepadButtonFlags.X) != SharpDX.XInput.GamepadButtonFlags.X))
                camState = !camState;

            if (((Input.XInputInput.State[0].Buttons & SharpDX.XInput.GamepadButtonFlags.DPadUp) == SharpDX.XInput.GamepadButtonFlags.DPadUp) && ((Input.XInputInput.PrevState[0].Buttons & SharpDX.XInput.GamepadButtonFlags.DPadUp) != SharpDX.XInput.GamepadButtonFlags.DPadUp))
                Wireframe = !Wireframe;
            if (((Input.XInputInput.State[0].Buttons & SharpDX.XInput.GamepadButtonFlags.DPadLeft) == SharpDX.XInput.GamepadButtonFlags.DPadLeft) && ((Input.XInputInput.PrevState[0].Buttons & SharpDX.XInput.GamepadButtonFlags.DPadLeft) != SharpDX.XInput.GamepadButtonFlags.DPadLeft))
                frameSkip = frameSkip > 1 ? frameSkip - 1 : 1;
            if (((Input.XInputInput.State[0].Buttons & SharpDX.XInput.GamepadButtonFlags.DPadRight) == SharpDX.XInput.GamepadButtonFlags.DPadRight) && ((Input.XInputInput.PrevState[0].Buttons & SharpDX.XInput.GamepadButtonFlags.DPadRight) != SharpDX.XInput.GamepadButtonFlags.DPadRight))
                frameSkip++;

            if (!camState)
                SetCamera(Matrix.LookAtLH(new Vector3((float)Math.Sin(Math.Sin(theta) * 0.1f), 0, (float)Math.Cos(Math.Sin(theta) * 0.1f)) * -1.3333f, Vector3.Zero, Vector3.Up),//Matrix.LookAtLH(Vector3.BackwardLH * 5, Vector3.Zero, Vector3.Up),
                                                                                                                                                                                 //Matrix.OrthoLH(16.0f / 9.0f * 2.0f, 2.0f, 0, 10.0f));
                    Matrix.PerspectiveFovLH(MathUtil.DegreesToRadians(75.0f), RenderWidth / (float)RenderHeight, 0.1f, 20.0f));
            else
                SetCamera(Matrix.LookAtLH(new Vector3(fishManager.FishList[0].Position * 0.75f, -1.33333f), new Vector3(fishManager.FishList[0].Position * 0.75f, 0.0f), Vector3.Up),
                    Matrix.PerspectiveFovLH(MathUtil.DegreesToRadians(45.0f), RenderWidth / (float)RenderHeight, 0.1f, 20.0f));

            theta += (float)Math.PI / 30.0f / 60.0f * (Input.XInputInput.State[0].LeftThumbY / (float)short.MaxValue + 0.25f) * 4.0f;

            if (frameNumber % (ulong)frameSkip == 0 && (Input.XInputInput.State[0].Buttons & SharpDX.XInput.GamepadButtonFlags.LeftShoulder) != SharpDX.XInput.GamepadButtonFlags.LeftShoulder)
            {
                DeviceContext.Rasterizer.State = Wireframe ? RasterizerStates.Wire : RasterizerStates.Back;

                //DeviceContext.OutputMerger.SetRenderTargets(renderTargetView);
                DeviceContext.PixelShader.SetShaderResource(0, null);
                DeviceContext.OutputMerger.SetRenderTargets(colorTarget.DSV, colorTarget.RTV);
                DeviceContext.OutputMerger.DepthStencilState = DepthStencilStates.Enabled;
                DeviceContext.ClearRenderTargetView(renderTargetView, new Color(0.0f, 0.0f, 0.0f));
                colorTarget.Clear(Color4.Lerp(new Color4(0, 0, 0.04f, 0.0f), new Color4(0.08f, 0.08f, 0.2f, 0.0f), Math.Max(0, -(float)Math.Cos(theta))));

                //RenderDebugTri();

                var intensity = Math.Max(0, -(float)Math.Cos(theta));
                var sunDir = new Vector3((float)Math.Sin(theta), (float)Math.Cos(theta), 1.0f);
                Sun.Set(sunDir, new Vector3(1.0f, 1.0f, 1.0f), intensity);
                fishGlow.Intensity = Math.Max(0, ((float)Math.Cos(intensity * MathUtil.Pi) + 0.2f) / 1.2f);
                raysRenderer.Intensity = 0.02f * (1 - fishGlow.Intensity);
                fishGlow.Intensity *= 0.75f;

                decorRenderer.Render();

                fishRenderer.Render();
                DeviceContext.OutputMerger.SetRenderTargets(colorTarget.DSV, renderTargetView);

                //var pos = new Vector3(fishManager.FishList[0].Position, 0);
                //var np = Vector3.Transform(pos, Projection * View);
                var np = Vector3.Transform(sunDir * new Vector3(1, 1, 0) * -10, Projection * View);

                DeviceContext.OutputMerger.DepthStencilState = DepthStencilStates.Disabled;
                DeviceContext.Rasterizer.State = RasterizerStates.Back;
                raysRenderer.CentrePoint = new Vector2(-sunDir.X * RenderWidth * 10.0f, sunDir.Y * RenderHeight * 10.0f);//new Vector2((np.X / np.W / 2.0f + 0.5f) * RenderWidth, (-np.Y / np.W / 2.0f + 0.5f) * RenderHeight);
                raysRenderer.Render(colorTarget.SRV, 1.0f / 60.0f);
                fishGlow.Render();
                DeviceContext.Rasterizer.State = Wireframe ? RasterizerStates.Wire : RasterizerStates.Back;

                DeviceContext.OutputMerger.DepthStencilState = DepthStencilStates.Enabled;
                foodRenderer.Render();

                decorRenderer.Render2();

                try
                {
                    if ((Input.XInputInput.State[0].Buttons & SharpDX.XInput.GamepadButtonFlags.RightShoulder) != SharpDX.XInput.GamepadButtonFlags.RightShoulder)
                        swapChain.Present(1, PresentFlags.None);
                    else
                        swapChain.Present(0, PresentFlags.None);
                }
                catch (Exception e)
                {
                    Console.WriteLine(Device.DeviceRemovedReason.ToString());
                    Console.ReadLine();
                    renderForm.Close();
                }
            }

            frameNumber++;
        }

        private void RenderDebugTri()
        {
            //DeviceContext.VertexShader.Set(vertexShader);
            vertexShader.Set();
            DeviceContext.PixelShader.Set(pixelShader);

            DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            

            DeviceContext.InputAssembler.SetVertexBuffers(0, new D3D.VertexBufferBinding(triangleVertexBuffer, Utilities.SizeOf<Vector3>(), 0));
            DeviceContext.Draw(3, 0);
        }

        //private void RenderFS()
        //{
        //    DeviceContext.InputAssembler.SetVertexBuffers(0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        //    DeviceContext.InputAssembler.SetIndexBuffer(null, (Format)0, 0);

        //    DeviceContext.VertexShader.Set(fsVertexShader);
        //    DeviceContext.PixelShader.Set(fishGlowPixelShader);

        //    DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

        //    DeviceContext.Draw(3, 0);
        //}

        public void Dispose()
        {
            swapChain.SetFullscreenState(false, null);

            //inputLayout.Dispose();
            //inputSignature.Dispose();

            WVP.Dispose();
            Sun.Dispose();

            decorRenderer.Dispose();

            fishGlow.Dispose();
            fishRenderer.Dispose();
            foodRenderer.Dispose();
            raysRenderer.Dispose();

            RasterizerStates.Dispose();
            DepthStencilStates.Dispose();
            BlendStates.Dispose();

            triangleVertexBuffer.Dispose();
            vertexShader.Dispose();
            pixelShader.Dispose();

            colorTarget.Dispose();

            renderTargetView.Dispose();
            swapChain.Dispose();
            Device.Dispose();
            DeviceContext.Dispose();
            renderForm.Dispose();
        }
    }
}
