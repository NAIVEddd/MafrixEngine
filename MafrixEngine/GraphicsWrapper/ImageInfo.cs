using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Assimp;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Image = Silk.NET.Vulkan.Image;
using Semaphore = Silk.NET.Vulkan.Semaphore;
using SlImage = SixLabors.ImageSharp.Image;

namespace MafrixEngine.GraphicsWrapper
{
    public class ImageInfo : IDisposable
    {
        private Vk vk;
        private Device device;
        public UInt32 mipLevels;
        public Image image;
        public DeviceMemory imageMemory;
        public ImageView imageView;
        public uint width;
        public uint height;

        public ImageInfo(Vk _vk, Device dev)
        {
            vk = _vk;
            device = dev;
        }

        public unsafe void CreateImage(string name, bool isMipmaps = false)
        {
            using var targImage = SlImage.Load<Rgba32>(name);
            var memoryGroup = targImage.GetPixelMemoryGroup();
            var imageSize = memoryGroup.TotalLength * sizeof(Rgba32);
            Memory<byte> array = new byte[imageSize];

            // copy data from imageFile to memory
            var block = MemoryMarshal.Cast<byte, Rgba32>(array.Span);
            foreach (var memory in memoryGroup)
            {
                memory.Span.CopyTo(block);
                block = block.Slice(memory.Length);
            }
            width = (uint)targImage.Width;
            height = (uint)targImage.Height;
            mipLevels = isMipmaps ? (UInt32)Math.Floor(Math.Log2(Math.Max(width, height))) + 1 : 1;

        }

        public unsafe void Dispose()
        {
            vk.DestroyImageView(device, imageView, null);
            vk.DestroyImage(device, image, null);
            vk.FreeMemory(device, imageMemory, null);
        }
    }
}
