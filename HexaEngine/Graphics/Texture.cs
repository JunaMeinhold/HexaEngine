﻿namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Specialized;
    using HexaEngine.Mathematics;
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class Texture
    {
        #region Fields

        private readonly IResource resource;
        private readonly IShaderResourceView resourceView;
        private readonly IRenderTargetView renderTargetView;
        private readonly IResource depthStencil;
        public IDepthStencilView DepthStencilView;
        private readonly List<ShaderBinding> bindings = new();
        private bool disposedValue;
        private readonly bool overrwittenDSV;

        public readonly TextureDescription Description;

        #endregion Fields

        #region Properties

        public IResource Resource => resource;

        public IShaderResourceView ResourceView => resourceView;

        public IRenderTargetView RenderTargetView => renderTargetView;

        public IResource DepthStencil => depthStencil;

        #endregion Properties

        #region Constructors

        public Texture(IGraphicsDevice device, IDepthStencilView depthStencil, IResource resource)
        {
            Description = new(resource);

            this.resource = resource;

            if (Description.BindFlags.HasFlag(BindFlags.ShaderResource))
                resourceView = device.CreateShaderResourceView(resource);
            if (Description.BindFlags.HasFlag(BindFlags.RenderTarget))
                renderTargetView = device.CreateRenderTargetView(resource, Description.GetViewport());
            DepthStencilView = depthStencil;
            overrwittenDSV = true;
        }

        public Texture(IGraphicsDevice device, IResource resource, DepthStencilDesc desc)
        {
            Description = new(resource);
            DepthStencilViewDescription depthStencilViewDesc = new()
            {
                Flags = desc.ViewFlags,
                Format = desc.Format,
                ViewDimension = Description.GetDepthStencilViewDimension()
            };

            this.resource = resource;

            switch (Description.Dimension)
            {
                case TextureDimension.Texture1D:
                    {
                        Texture1DDescription depthStencilDesc = new(
                            desc.Format,
                            Description.Width,
                            Description.ArraySize,
                            1,
                            desc.BindFlags,
                            desc.Usage,
                            desc.CPUAccessFlags,
                            ResourceMiscFlag.None);
                        depthStencil = device.CreateTexture1D(depthStencilDesc);
                    }
                    break;

                case TextureDimension.Texture2D:
                    {
                        Texture2DDescription depthStencilDesc = new(
                            desc.Format,
                            Description.Width,
                            Description.Height,
                            Description.ArraySize,
                            1,
                            desc.BindFlags,
                            desc.Usage,
                            desc.CPUAccessFlags,
                            desc.SampleDescription.Count,
                            desc.SampleDescription.Quality,
                            ResourceMiscFlag.None);
                        depthStencil = device.CreateTexture2D(depthStencilDesc);
                    }
                    break;

                case TextureDimension.Texture3D:
                    {
                        Texture2DDescription depthStencilDesc = new(
                            desc.Format,
                            Description.Width,
                            Description.Height,
                            Description.Depth,
                            1,
                            desc.BindFlags,
                            desc.Usage,
                            desc.CPUAccessFlags,
                            desc.SampleDescription.Count,
                            desc.SampleDescription.Quality,
                            ResourceMiscFlag.None);
                        depthStencil = device.CreateTexture2D(depthStencilDesc);
                    }
                    break;

                case TextureDimension.TextureCube:
                    {
                        Texture2DDescription depthStencilDesc = new(
                            desc.Format,
                            Description.Width,
                            Description.Height,
                            Description.ArraySize,
                            1,
                            desc.BindFlags,
                            desc.Usage,
                            desc.CPUAccessFlags,
                            desc.SampleDescription.Count,
                            desc.SampleDescription.Quality,
                            ResourceMiscFlag.TextureCube);
                        depthStencil = device.CreateTexture2D(depthStencilDesc);
                    }
                    break;
            }

            switch (depthStencilViewDesc.ViewDimension)
            {
                case DepthStencilViewDimension.Texture1D:
                    depthStencilViewDesc.Texture1D = new();
                    break;

                case DepthStencilViewDimension.Texture1DArray:
                    depthStencilViewDesc.Texture1DArray = new() { ArraySize = Description.ArraySize };
                    break;

                case DepthStencilViewDimension.Texture2D:
                    depthStencilViewDesc.Texture2D = new();
                    break;

                case DepthStencilViewDimension.Texture2DArray:
                    depthStencilViewDesc.Texture2DArray = new() { ArraySize = Description.ArraySize };
                    break;

                case DepthStencilViewDimension.Texture2DMultisampled:
                    depthStencilViewDesc.Texture2DMS = new();
                    break;

                case DepthStencilViewDimension.Texture2DMultisampledArray:
                    depthStencilViewDesc.Texture2DMSArray = new() { ArraySize = Description.ArraySize };
                    break;
            }

            if (Description.BindFlags.HasFlag(BindFlags.ShaderResource))
                resourceView = device.CreateShaderResourceView(resource);
            if (Description.BindFlags.HasFlag(BindFlags.RenderTarget))
                renderTargetView = device.CreateRenderTargetView(resource, Description.GetViewport());

            DepthStencilView = device.CreateDepthStencilView(depthStencil, depthStencilViewDesc);
            Viewport = Description.GetViewport();
        }

        public Texture(IGraphicsDevice device, IResource resource)
        {
            Description = new(resource);

            this.resource = resource;

            if (Description.BindFlags.HasFlag(BindFlags.ShaderResource))
                resourceView = device.CreateShaderResourceView(resource);
            if (Description.BindFlags.HasFlag(BindFlags.RenderTarget))
                renderTargetView = device.CreateRenderTargetView(resource, Description.GetViewport());
        }

        public Texture(IGraphicsDevice device, IDepthStencilView depthStencil, TextureDescription description)
        {
            Description = description;

            switch (description.Dimension)
            {
                case TextureDimension.Texture1D:
                    resource = device.CreateTexture1D(description.Format, description.Width, description.ArraySize, description.MipLevels, null, description.BindFlags, ResourceMiscFlag.None);
                    break;

                case TextureDimension.Texture2D:
                    resource = device.CreateTexture2D(description.Format, description.Width, description.Height, description.ArraySize, description.MipLevels, null, description.BindFlags, ResourceMiscFlag.None);
                    break;

                case TextureDimension.Texture3D:
                    resource = device.CreateTexture3D(description.Format, description.Width, description.Height, description.Depth, description.MipLevels, null, description.BindFlags, ResourceMiscFlag.None);
                    break;

                case TextureDimension.TextureCube:
                    resource = device.CreateTexture2D(description.Format, description.Width, description.Height, description.ArraySize, description.MipLevels, null, description.BindFlags, ResourceMiscFlag.TextureCube);
                    break;
            }

            if (description.BindFlags.HasFlag(BindFlags.ShaderResource))
                resourceView = device.CreateShaderResourceView(resource);
            if (description.BindFlags.HasFlag(BindFlags.RenderTarget))
                renderTargetView = device.CreateRenderTargetView(resource, Description.GetViewport());

            Viewport = Description.GetViewport();
            overrwittenDSV = true;
            DepthStencilView = depthStencil;
        }

        public Texture(IGraphicsDevice device, TextureDescription description, DepthStencilDesc desc)
        {
            Description = description;
            DepthStencilViewDescription depthStencilViewDesc = new()
            {
                Flags = desc.ViewFlags,
                Format = desc.Format,
                ViewDimension = description.GetDepthStencilViewDimension()
            };

            switch (description.Dimension)
            {
                case TextureDimension.Texture1D:
                    resource = device.CreateTexture1D(description.Format, description.Width, description.ArraySize, description.MipLevels, null, description.BindFlags, ResourceMiscFlag.None);
                    {
                        Texture1DDescription depthStencilDesc = new(
                            desc.Format,
                            description.Width,
                            description.ArraySize,
                            1,
                            desc.BindFlags,
                            desc.Usage,
                            desc.CPUAccessFlags,
                            ResourceMiscFlag.None);
                        depthStencil = device.CreateTexture1D(depthStencilDesc);
                    }
                    break;

                case TextureDimension.Texture2D:
                    resource = device.CreateTexture2D(description.Format, description.Width, description.Height, description.ArraySize, description.MipLevels, null, description.BindFlags, ResourceMiscFlag.None);
                    {
                        Texture2DDescription depthStencilDesc = new(
                            desc.Format,
                            description.Width,
                            description.Height,
                            description.ArraySize,
                            1,
                            desc.BindFlags,
                            desc.Usage,
                            desc.CPUAccessFlags,
                            desc.SampleDescription.Count,
                            desc.SampleDescription.Quality,
                            ResourceMiscFlag.None);
                        depthStencil = device.CreateTexture2D(depthStencilDesc);
                    }
                    break;

                case TextureDimension.Texture3D:
                    resource = device.CreateTexture3D(description.Format, description.Width, description.Height, description.Depth, description.MipLevels, null, description.BindFlags, ResourceMiscFlag.None);
                    {
                        Texture2DDescription depthStencilDesc = new(
                            desc.Format,
                            description.Width,
                            description.Height,
                            description.Depth,
                            1,
                            desc.BindFlags,
                            desc.Usage,
                            desc.CPUAccessFlags,
                            desc.SampleDescription.Count,
                            desc.SampleDescription.Quality,
                            ResourceMiscFlag.None);
                        depthStencil = device.CreateTexture2D(depthStencilDesc);
                    }
                    break;

                case TextureDimension.TextureCube:
                    resource = device.CreateTexture2D(description.Format, description.Width, description.Height, description.ArraySize, description.MipLevels, null, description.BindFlags, ResourceMiscFlag.TextureCube);
                    {
                        Texture2DDescription depthStencilDesc = new(
                            desc.Format,
                            description.Width,
                            description.Height,
                            description.ArraySize,
                            1,
                            desc.BindFlags,
                            desc.Usage,
                            desc.CPUAccessFlags,
                            desc.SampleDescription.Count,
                            desc.SampleDescription.Quality,
                            ResourceMiscFlag.TextureCube);
                        depthStencil = device.CreateTexture2D(depthStencilDesc);
                    }
                    break;
            }

            switch (depthStencilViewDesc.ViewDimension)
            {
                case DepthStencilViewDimension.Texture1D:
                    depthStencilViewDesc.Texture1D = new();
                    break;

                case DepthStencilViewDimension.Texture1DArray:
                    depthStencilViewDesc.Texture1DArray = new() { ArraySize = description.ArraySize };
                    break;

                case DepthStencilViewDimension.Texture2D:
                    depthStencilViewDesc.Texture2D = new();
                    break;

                case DepthStencilViewDimension.Texture2DArray:
                    depthStencilViewDesc.Texture2DArray = new() { ArraySize = description.ArraySize };
                    break;

                case DepthStencilViewDimension.Texture2DMultisampled:
                    depthStencilViewDesc.Texture2DMS = new();
                    break;

                case DepthStencilViewDimension.Texture2DMultisampledArray:
                    depthStencilViewDesc.Texture2DMSArray = new() { ArraySize = description.ArraySize };
                    break;
            }

            if (description.BindFlags.HasFlag(BindFlags.ShaderResource))
                resourceView = device.CreateShaderResourceView(resource);
            if (description.BindFlags.HasFlag(BindFlags.RenderTarget))
                renderTargetView = device.CreateRenderTargetView(resource, Description.GetViewport());

            DepthStencilView = device.CreateDepthStencilView(depthStencil, depthStencilViewDesc);
            Viewport = Description.GetViewport();
        }

        public Texture(IGraphicsDevice device, TextureDescription description)
        {
            Description = description;

            switch (description.Dimension)
            {
                case TextureDimension.Texture1D:
                    resource = device.CreateTexture1D(description.Format, description.Width, description.ArraySize, description.MipLevels, null, description.BindFlags, ResourceMiscFlag.None);
                    break;

                case TextureDimension.Texture2D:
                    resource = device.CreateTexture2D(description.Format, description.Width, description.Height, description.ArraySize, description.MipLevels, null, description.BindFlags, ResourceMiscFlag.None);
                    break;

                case TextureDimension.Texture3D:
                    resource = device.CreateTexture3D(description.Format, description.Width, description.Height, description.Depth, description.MipLevels, null, description.BindFlags, ResourceMiscFlag.None);
                    break;

                case TextureDimension.TextureCube:
                    resource = device.CreateTexture2D(description.Format, description.Width, description.Height, description.ArraySize, description.MipLevels, null, description.BindFlags, ResourceMiscFlag.TextureCube);
                    break;
            }

            if (description.BindFlags.HasFlag(BindFlags.ShaderResource))
                resourceView = device.CreateShaderResourceView(resource);
            if (description.BindFlags.HasFlag(BindFlags.RenderTarget))
                renderTargetView = device.CreateRenderTargetView(resource, Description.GetViewport());

            Viewport = Description.GetViewport();
        }

        public unsafe Texture(IGraphicsDevice device, TextureFileDescription description)
        {
            switch (description.Dimension)
            {
                case TextureDimension.Texture1D:
                    {
                        ITexture1D texture;
                        resource = texture = device.LoadTexture1D(description.Path);
                        Description = new()
                        {
                            Width = texture.Description.Width,
                            Height = 0,
                            Depth = 0,
                            BindFlags = texture.Description.BindFlags,
                            CPUAccessFlags = texture.Description.CPUAccessFlags,
                            Dimension = TextureDimension.Texture1D,
                            Format = texture.Description.Format,
                            MipLevels = texture.Description.MipLevels,
                            MiscFlags = texture.Description.MiscFlags,
                            SampleDescription = SampleDescription.Default,
                            Usage = texture.Description.Usage
                        };
                    }
                    break;

                case TextureDimension.TextureCube:
                case TextureDimension.Texture2D:
                    {
                        ITexture2D texture;
                        resource = texture = device.LoadTexture2D(description.Path);
                        Description = new()
                        {
                            Width = texture.Description.Width,
                            Height = texture.Description.Height,
                            Depth = texture.Description.ArraySize,
                            BindFlags = texture.Description.BindFlags,
                            CPUAccessFlags = texture.Description.CPUAccessFlags,
                            Dimension = TextureDimension.Texture1D,
                            Format = texture.Description.Format,
                            MipLevels = texture.Description.MipLevels,
                            MiscFlags = texture.Description.MiscFlags,
                            SampleDescription = SampleDescription.Default,
                            Usage = texture.Description.Usage
                        };
                    }
                    break;

                case TextureDimension.Texture3D:
                    {
                        ITexture3D texture;
                        resource = texture = device.LoadTexture3D(description.Path);
                        Description = new()
                        {
                            Width = texture.Description.Width,
                            Height = texture.Description.Height,
                            Depth = texture.Description.Depth,
                            BindFlags = texture.Description.BindFlags,
                            CPUAccessFlags = texture.Description.CPUAccessFlags,
                            Dimension = TextureDimension.Texture1D,
                            Format = texture.Description.Format,
                            MipLevels = texture.Description.MipLevels,
                            MiscFlags = texture.Description.MiscFlags,
                            SampleDescription = SampleDescription.Default,
                            Usage = texture.Description.Usage
                        };
                    }
                    break;
            }

            if (description.BindFlags.HasFlag(BindFlags.ShaderResource))
                resourceView = device.CreateShaderResourceView(resource);
            if (description.BindFlags.HasFlag(BindFlags.RenderTarget))
                renderTargetView = device.CreateRenderTargetView(resource, Description.GetViewport());

            Viewport = Description.GetViewport();
        }

        public unsafe Texture(IGraphicsDevice device, Span<byte> rawPixelData, int rowPitch, int slicePitch, TextureDescription description)
        {
            SubresourceData subresourceData;
            fixed (byte* ptr = rawPixelData)
            {
                subresourceData = new(ptr, rowPitch, slicePitch);
            }
            SubresourceData[] subresources = new SubresourceData[] { subresourceData };

            Description = description;

            switch (description.Dimension)
            {
                case TextureDimension.Texture1D:
                    resource = device.CreateTexture1D(description.Format, description.Width, description.ArraySize, description.MipLevels, subresources, description.BindFlags, ResourceMiscFlag.None);
                    break;

                case TextureDimension.Texture2D:
                    resource = device.CreateTexture2D(description.Format, description.Width, description.Height, description.ArraySize, description.MipLevels, subresources, description.BindFlags, ResourceMiscFlag.None);
                    break;

                case TextureDimension.Texture3D:
                    resource = device.CreateTexture3D(description.Format, description.Width, description.Height, description.Depth, description.MipLevels, subresources, description.BindFlags, ResourceMiscFlag.None);
                    break;

                case TextureDimension.TextureCube:
                    resource = device.CreateTexture2D(description.Format, description.Width, description.Height, description.ArraySize, description.MipLevels, subresources, description.BindFlags, ResourceMiscFlag.TextureCube);
                    break;
            }

            if (description.BindFlags.HasFlag(BindFlags.ShaderResource))
                resourceView = device.CreateShaderResourceView(resource);
            if (description.BindFlags.HasFlag(BindFlags.RenderTarget))
                renderTargetView = device.CreateRenderTargetView(resource, Description.GetViewport());

            Viewport = Description.GetViewport();
        }

        public unsafe Texture(IGraphicsDevice device, Span<byte> rawPixelData, int rowPitch, TextureDescription description)
        {
            SubresourceData subresourceData;
            fixed (byte* ptr = rawPixelData)
            {
                subresourceData = new(ptr, rowPitch, 0);
            }
            SubresourceData[] subresources = new SubresourceData[] { subresourceData };

            Description = description;

            switch (description.Dimension)
            {
                case TextureDimension.Texture1D:
                    resource = device.CreateTexture1D(description.Format, description.Width, description.ArraySize, description.MipLevels, subresources, description.BindFlags, ResourceMiscFlag.None);
                    break;

                case TextureDimension.Texture2D:
                    resource = device.CreateTexture2D(description.Format, description.Width, description.Height, description.ArraySize, description.MipLevels, subresources, description.BindFlags, ResourceMiscFlag.None);
                    break;

                case TextureDimension.Texture3D:
                case TextureDimension.TextureCube:
                    throw new ArgumentOutOfRangeException(nameof(description));
            }

            if (description.BindFlags.HasFlag(BindFlags.ShaderResource))
                resourceView = device.CreateShaderResourceView(resource);
            if (description.BindFlags.HasFlag(BindFlags.RenderTarget))
                renderTargetView = device.CreateRenderTargetView(resource, Description.GetViewport());

            Viewport = Description.GetViewport();
        }

        public unsafe Texture(IGraphicsDevice device, byte[][] rawPixelData, int rowPitch, TextureDescription description)
        {
            SubresourceData[] subresources = new SubresourceData[rawPixelData.Length];

            for (int i = 0; i < rawPixelData.Length; i++)
            {
                byte[] data = rawPixelData[i];
                fixed (byte* ptr = data)
                {
                    subresources[i] = new(ptr, rowPitch, 0);
                }
            }

            Description = description;

            switch (description.Dimension)
            {
                case TextureDimension.Texture1D:
                    resource = device.CreateTexture1D(description.Format, description.Width, description.ArraySize, description.MipLevels, subresources, description.BindFlags, ResourceMiscFlag.None);
                    break;

                case TextureDimension.Texture2D:
                    resource = device.CreateTexture2D(description.Format, description.Width, description.Height, description.ArraySize, description.MipLevels, subresources, description.BindFlags, ResourceMiscFlag.None);
                    break;

                case TextureDimension.TextureCube:
                    resource = device.CreateTexture2D(description.Format, description.Width, description.Height, description.ArraySize, description.MipLevels, subresources, description.BindFlags, ResourceMiscFlag.TextureCube);
                    break;

                case TextureDimension.Texture3D:
                    throw new ArgumentOutOfRangeException(nameof(description));
            }

            if (description.BindFlags.HasFlag(BindFlags.ShaderResource))
                resourceView = device.CreateShaderResourceView(resource);
            if (description.BindFlags.HasFlag(BindFlags.RenderTarget))
                renderTargetView = device.CreateRenderTargetView(resource, Description.GetViewport());

            Viewport = Description.GetViewport();
        }

        private unsafe IResource InitFallback(IGraphicsDevice device, TextureFileDescription description)
        {
            SubresourceData fallback;
            Vector4[] values = new Vector4[16];
            for (int i = 0; i < 16; i++)
            {
                values[i] = new(1f, 0.752941176f, 0.796078431f, 1f);
            }
            fixed (byte* ptr = MemoryMarshal.AsBytes(values.AsSpan()))
            {
                fallback = new(ptr, sizeof(Vector4));
            }

            if (description.Dimension == TextureDimension.Texture1D)
            {
                return device.CreateTexture1D(Format.RGBA32Float, 16, 1, 1, new SubresourceData[] { fallback }, BindFlags.ShaderResource);
            }
            if (description.Dimension == TextureDimension.Texture2D)
            {
                return device.CreateTexture2D(Format.RGBA32Float, 4, 4, 1, 1, new SubresourceData[] { fallback }, BindFlags.ShaderResource);
            }
            if (description.Dimension == TextureDimension.TextureCube)
            {
                return device.CreateTexture2D(Format.RGBA32Float, 4, 4, 6, 1, new SubresourceData[] { fallback, fallback, fallback, fallback, fallback, fallback }, BindFlags.ShaderResource, ResourceMiscFlag.TextureCube);
            }
            return default;
        }

        #endregion Constructors

        #region IShaderResource

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBinding(ShaderStage stage, int slot)
        {
            AddBinding(new(stage, slot));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveBinding(ShaderStage stage, int slot)
        {
            RemoveBinding(new(stage, slot));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBinding(ShaderBinding binding)
        {
            bindings.Add(binding);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveBinding(ShaderBinding binding)
        {
            bindings.Remove(binding);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearBindings()
        {
            bindings.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(IGraphicsContext context)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                context.SetShaderResource(resourceView, bindings[i].Stage, bindings[i].Slot);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(IGraphicsContext context, int slot)
        {
            context.SetShaderResource(resourceView, ShaderStage.Pixel, slot);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(IGraphicsContext context, int slot, ShaderStage stage)
        {
            context.SetShaderResource(resourceView, stage, slot);
        }

        #endregion IShaderResource

        #region IRenderTargetView

        public readonly Viewport Viewport;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearAndSetTarget(IGraphicsContext context, Vector4 color, DepthStencilClearFlags flags = DepthStencilClearFlags.None, float depth = 1, byte stencil = 0)
        {
            context.ClearRenderTargetView(renderTargetView, color);
            ClearDepthStencil(context, flags, depth, stencil);
            context.SetRenderTargets(renderTargetView, DepthStencilView);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearTarget(IGraphicsContext context, Vector4 color, DepthStencilClearFlags flags = DepthStencilClearFlags.None, float depth = 1, byte stencil = 0)
        {
            ClearDepthStencil(context, flags, depth, stencil);
            context.ClearRenderTargetView(renderTargetView, new(color.X, color.Y, color.Z, color.W));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTarget(IGraphicsContext context)
        {
            context.SetRenderTargets(renderTargetView, DepthStencilView);
        }

        #endregion IRenderTargetView

        #region IDepthStencil

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearDepthStencil(IGraphicsContext context, DepthStencilClearFlags flags = DepthStencilClearFlags.None, float depth = 1, byte stencil = 0)
        {
            if (depthStencil != null && flags != DepthStencilClearFlags.None)
                context.ClearDepthStencilView(DepthStencilView, flags, depth, stencil);
        }

        #endregion IDepthStencil

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                resource.Dispose();
                resourceView?.Dispose();
                renderTargetView?.Dispose();
                depthStencil?.Dispose();
                if (!overrwittenDSV)
                    DepthStencilView?.Dispose();
                disposedValue = true;
            }
        }

        ~Texture()
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

        #endregion IDisposable
    }
}