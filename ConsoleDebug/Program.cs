// See https://aka.ms/new-console-template for more information
using MafrixEngine.GraphicsWrapper;
using Silk.NET.Windowing;
using Silk.NET.GLFW;
using Silk.NET.Maths;

var wrapper = new VulkanWrapper();
wrapper.InitVulkan();

wrapper.MainLoop();

wrapper.window!.Dispose();
wrapper.Cleanup();