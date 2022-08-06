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

namespace UnitTest
{
    public class TestGraphicsWrapper
    {
        [Fact]
        public void TestCreateInstance()
        {
            var wrapper = new VulkanWrapper();

            wrapper.CreateInstance();
            Assert.NotNull(wrapper);
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
