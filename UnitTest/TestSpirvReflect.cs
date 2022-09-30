using MafrixEngine.ModelLoaders;
using SPIRVCross;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using ThirdPartyLib;

namespace UnitTest
{
    public class TestSpirvReflect
    {
        public string vertShader = "Asserts/Shaders/base.vert.spv";
        public string fragShader = "Asserts/Shaders/base.frag.spv";

        [Fact]
        public unsafe void TestSpirvReflect_Api()
        {
            byte[] vertBytes = File.ReadAllBytes(vertShader);
            byte[] fragBytes = File.ReadAllBytes(fragShader);

            var module = stackalloc SpvReflectShaderModule[1];
            fixed(void* vertPtr = vertBytes)
            {
                SpirvReflect.spvReflectCreateShaderModule((nuint)vertBytes.Length, vertPtr, module);
            }
            var moduleInfo = module[0];
            Assert.NotEqual(0u, moduleInfo.entry_point_count);
        }
    }
}
