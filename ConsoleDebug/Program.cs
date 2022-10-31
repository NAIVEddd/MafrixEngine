// See https://aka.ms/new-console-template for more information
using MafrixEngine.GraphicsWrapper;
using Silk.NET.Windowing;
using Silk.NET.GLFW;
using Silk.NET.Maths;
using MafrixEngine.Source;
using ConsoleDebug;

var wrapper = new VulkanWrapper();
wrapper.InitVulkan();
//wrapper.InitVulkanShadowMap();

wrapper.MainLoop();

wrapper.window!.Dispose();
wrapper.Cleanup();

//IWindow InitWindow()
//{
//    var opts = WindowOptions.DefaultVulkan with
//    {
//        Size = new Vector2D<int>(1920, 1080),
//        Title = "Debug"
//    };
//    var window = Window.Create(opts);
//    window.Initialize();

//    return window;
//}

//var engine = new Engine();
//engine.window = InitWindow();
//engine.renderSys = new BasicRender();
//engine.staticScene = new StaticSceneProvider(new ValueTuple<string, string, int>("Asserts/sponza", "Sponza.gltf", -1));
//engine.Init("");
//engine.Run();