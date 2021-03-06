namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using System;
    using System.Runtime.CompilerServices;

    public class RenderTextureArray : IDisposable
    {
        private bool disposedValue;

        public int Width { get; }

        public int Height { get; }

        public Viewport Viewport => new(Width, Height);

        public int Count { get; }

        public readonly IShaderResourceView[] SRVs;
        public readonly IRenderTargetView[] RTVs;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RenderTextureArray(IGraphicsDevice device, int width, int height, int count = 1, Format format = Format.RGBA32Float)
        {
            Count = count;
            Width = width;
            Height = height;
            RTVs = new IRenderTargetView[count];
            SRVs = new IShaderResourceView[count];
            for (int i = 0; i < count; i++)
            {
                ITexture2D tex = device.CreateTexture2D(format, Width, Height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceMiscFlag.None);
                SRVs[i] = device.CreateShaderResourceView(tex);
                RTVs[i] = device.CreateRenderTargetView(tex, new(Width, Height));
                tex.Dispose();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                foreach (IRenderTargetView rtv in RTVs)
                    rtv.Dispose();
                foreach (IShaderResourceView srv in SRVs)
                    srv.Dispose();

                disposedValue = true;
            }
        }

        ~RenderTextureArray()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}