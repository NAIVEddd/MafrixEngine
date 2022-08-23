using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MafrixEngine.GraphicsWrapper;
using Silk.NET.Maths;
using Silk.NET.Assimp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Vulkan;
using Image = SixLabors.ImageSharp.Image;

namespace UnitTest
{
    public class TestGraphicsWrapper
    {
        [Fact]
        public void TestVkInstance()
        {
            var vk = Vk.GetApi();
            using var vkInstance = new VkInstance(vk);
            Assert.Equal(0, vkInstance.instance.Handle);
            vkInstance.Initialize("TestInstance", new Version32(0, 0, 1));
            Assert.NotEqual(0, vkInstance.instance.Handle);
        }

        [Fact]
        public void TestVkContext()
        {
            using var vkContext = new VkContext();
            Assert.NotNull(vkContext.vk);
            Assert.Equal(0, vkContext.instance.Handle);
            Assert.Equal(0, vkContext.physicalDevice.Handle);
            Assert.Equal(0, vkContext.device.Handle);

            vkContext.Initialize("Test", new Version32(0,0,1));
            Assert.NotEqual(0, vkContext.instance.Handle);
            Assert.NotEqual(0, vkContext.physicalDevice.Handle);
            Assert.NotEqual(0, vkContext.device.Handle);
            Assert.ThrowsAny<Exception>(
                () => VkContext.DebugCheck(Result.ErrorDeviceLost, "DeviceLost"));
            Assert.Throws<NotImplementedException>(
                () => VkContext.DebugCheck(Result.ErrorMemoryMapFailed,
                                    new NotImplementedException("Not support MemoryMap!")));
        }

        [Fact]
        public void TestMat4Size()
        {
            var expected = 16 * sizeof(float);
            var actually = Unsafe.SizeOf<Matrix4X4<float>>();

            Assert.Equal(expected, actually);
        }

        [Fact]
        public void TestTime()
        {
            var now = DateTime.Now;
            var later = now.AddMilliseconds(2894);
            var elapsed = later.Subtract(now);
            Assert.Equal(2894, elapsed.TotalMilliseconds);
            Assert.Equal(2.894, elapsed.TotalSeconds);
        }

        [Fact]
        public unsafe void TestSixLaborsLoadImage()
        {
            using var image = Image.Load<Rgba32>("Asserts/lim.PNG");
            var memoryGroup = image.GetPixelMemoryGroup();
            Memory<byte> array = new byte[memoryGroup.TotalLength * sizeof(Rgba32)];

            //Assert.Equal(1394, memoryGroup.Count);
            Assert.Equal(2000, image.Width);
            Assert.Equal(1394, image.Height);
            var block = MemoryMarshal.Cast<byte, Rgba32>(array.Span);
            foreach(var memory in memoryGroup)
            {
                memory.Span.CopyTo(block);
                block = block.Slice(memory.Length);
            }
        }
    }
}
