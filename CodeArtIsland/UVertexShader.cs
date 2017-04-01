using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;

namespace OnTheOriginOfFishies
{
    internal class UVertexShader : IDisposable
    {
        public readonly RenderBase RenderBase;

        public readonly VertexShader VertexShader;
        private ShaderSignature inputSignature;
        private InputLayout inputLayout;

        public UVertexShader(RenderBase rb, string filename, InputElement[] inputElements)
        {
            RenderBase = rb;

            var shaderFlags =
#if DEBUG
                ShaderFlags.Debug;
#else
                ShaderFlags.None;
#endif
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile(filename, "main", "vs_4_0", shaderFlags))
            {
                inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                VertexShader = new VertexShader(rb.Device, vertexShaderByteCode);
            }

            if (inputElements != null)
                inputLayout = new InputLayout(rb.Device, inputSignature, inputElements);
        }

        public void Set()
        {
            RenderBase.DeviceContext.InputAssembler.InputLayout = inputLayout;
            RenderBase.DeviceContext.VertexShader.Set(VertexShader);
        }

        public void Dispose()
        {
            if (inputLayout != null)
                inputLayout.Dispose();
            inputSignature.Dispose();
            VertexShader.Dispose();
        }
    }
}
