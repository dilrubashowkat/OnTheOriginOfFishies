using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTheOriginOfFishies
{
    internal class RenderTarget2D : IDisposable
    {
        public readonly RenderBase RenderBase;

        internal Texture2D Texture;
        internal ShaderResourceView SRV;
        internal RenderTargetView RTV;

        internal Texture2D DepthTexture;
        internal DepthStencilView DSV;

        public RenderTarget2D(RenderBase rb, int width, int height, bool depth)
        {
            RenderBase = rb;

            Texture = new Texture2D(RenderBase.Device, new Texture2DDescription()
            {
                Width = width,
                Height = height,
                OptionFlags = ResourceOptionFlags.None,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                Format = Format.R32G32B32A32_Float,
                CpuAccessFlags = CpuAccessFlags.None,

                ArraySize = 1,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default
            });
            SRV = new ShaderResourceView(RenderBase.Device, Texture);
            RTV = new RenderTargetView(RenderBase.Device, Texture);

            if (depth)
            {
                DepthTexture = new Texture2D(RenderBase.Device, new Texture2DDescription()
                {
                    Width = width,
                    Height = height,
                    OptionFlags = ResourceOptionFlags.None,
                    BindFlags = BindFlags.DepthStencil,
                    Format = Format.D24_UNorm_S8_UInt,
                    CpuAccessFlags = CpuAccessFlags.None,

                    ArraySize = 1,
                    MipLevels = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default
                });
                DSV = new DepthStencilView(RenderBase.Device, DepthTexture);
            }
        }

        public void Clear(Color4 color)
        {
            RenderBase.DeviceContext.ClearRenderTargetView(RTV, color);
            if (DepthTexture != null)
                RenderBase.DeviceContext.ClearDepthStencilView(DSV, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
        }

        public void Dispose()
        {
            DSV?.Dispose();
            DepthTexture?.Dispose();
            RTV.Dispose();
            SRV.Dispose();
            Texture.Dispose();
        }
    }
}
