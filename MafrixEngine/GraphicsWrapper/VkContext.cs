using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Assimp;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.SDL;
using static MafrixEngine.GraphicsWrapper.VulkanWrapper;

namespace MafrixEngine.GraphicsWrapper
{
    static class BasicLayerAndExtension
    {
        public static IVkLayer[] Layers = new IVkLayer[] {
#if DEBUG
            new BasicLayer(new string[]
            {
                "VK_LAYER_KHRONOS_validation",
            }),
#endif
        };
        public static IVkExtension[] Extensions = new IVkExtension[] {
#if DEBUG
            new BasicExtension(new string[]
            {
                ExtDebugUtils.ExtensionName,
            }),
#endif
            new BasicExtension(new string[] // surface extension
            {
                "VK_KHR_surface",
                "VK_KHR_win32_surface",
            }),
            //new BasicExtension(new string[] // swapchain extension
            //{
            //    KhrSwapchain.ExtensionName
            //})
        };
    }

    public unsafe class VkInstance : IDisposable
    {
        private Vk vk;
        private List<IVkLayer> vkLayers;
        private List<IVkExtension> vkExtensions;
        public Instance instance;
        public VkInstance(Vk vk)
        {
            this.vk = vk;
            vkLayers = new List<IVkLayer>();
            vkExtensions = new List<IVkExtension>();
            instance = default;
        }

        public void AddLayer(IVkLayer layer)
        {
            vkLayers.Add(layer);
        }
        public void AddDeviceLayer(IVkLayer layer)
        {
            throw new NotSupportedException("NotSupport device extension yet.");
        }
        public void AddExtension(IVkExtension ext)
        {
            vkExtensions.Add(ext);
        }
        public void AddDeviceExtension(IVkExtension ext)
        {
            throw new NotSupportedException("NotSupport device extension yet.");
        }

        private void CheckLayersSupported()
        {
            uint layerCount = 0;
            vk.EnumerateInstanceLayerProperties(ref layerCount, null);
            var availableLayers = stackalloc LayerProperties[(int)layerCount];
            vk.EnumerateInstanceLayerProperties(&layerCount, availableLayers);

            var totalLayers = new HashSet<string>();
            for(int i = 0; i < layerCount; i++)
            {
                totalLayers.Add(Marshal.PtrToStringAnsi((nint)availableLayers[i].LayerName)!);
            }
            var neededLayers = new HashSet<string>();
            foreach(var layer in vkLayers)
            {
                foreach(var name in layer.LayerNames)
                {
                    neededLayers.Add(name);
                }
            }

            neededLayers.RemoveWhere(totalLayers.Contains);
            if (neededLayers.Count != 0)
            {
                var desc = "NotSupport all layers:\n";
                foreach(var layer in neededLayers)
                {
                    desc += "\t" + layer.ToString() + "\n";
                }
                throw new NotSupportedException(desc);
            }
        }

        private void CheckExtensionsSupported()
        {
            uint extensionCount = 0;
            vk.EnumerateInstanceExtensionProperties((byte*)null, &extensionCount, null);
            var extensionsArray = stackalloc ExtensionProperties[(int)extensionCount];
            vk.EnumerateInstanceExtensionProperties((byte*)null, &extensionCount, extensionsArray);

            var totalExtensions = new HashSet<string>();
            for(var i = 0; i < extensionCount; i++)
            {
                totalExtensions.Add(Marshal.PtrToStringAnsi((nint)extensionsArray[i].ExtensionName)!);
            }
            var neededExtensions = new HashSet<string>();
            foreach(var extension in vkExtensions)
            {
                foreach(var name in extension.ExtensionNames)
                {
                    neededExtensions.Add(name);
                }
            }

            neededExtensions.RemoveWhere(totalExtensions.Contains);
            if (neededExtensions.Count != 0)
            {
                var desc = "NotSupport all extensions:\n";
                foreach (var ext in neededExtensions)
                {
                    desc += "\t" + ext.ToString() + "\n";
                }
                throw new NotSupportedException(desc);
            }
        }

        public void Initialize(string appName, uint appVersion)
        {
            foreach(var l in BasicLayerAndExtension.Layers)
            {
                AddLayer(l);
            }
            foreach(var e in BasicLayerAndExtension.Extensions)
            {
                AddExtension(e);
            }
#if DEBUG
            CheckLayersSupported();
            CheckExtensionsSupported();
#endif
            ApplicationInfo applicationInfo = new ApplicationInfo(StructureType.ApplicationInfo);
            applicationInfo.PApplicationName = (byte*)SilkMarshal.StringToPtr(appName);
            applicationInfo.ApplicationVersion = appVersion;
            applicationInfo.PEngineName = (byte*)SilkMarshal.StringToPtr("MafrixEngine");
            applicationInfo.EngineVersion = new Version32(0, 0, 1);
            applicationInfo.ApiVersion = Vk.Version11;

            var createInfo = new InstanceCreateInfo(StructureType.InstanceCreateInfo);
            createInfo.PApplicationInfo = &applicationInfo;

            var layersCount = 0;
            vkLayers.ForEach(layer => layersCount += layer.LayerNames.Length);
            var layers = stackalloc byte*[layersCount];
            var iter = 0;
            // Copy layer names to stack array
            foreach(var layer in vkLayers)
            {
                var names = layer.LayerNames;
                for(int j = 0; j < names.Length; j++)
                {
                    layers[iter] = (byte*)SilkMarshal.StringToPtr(names[j]);
                    iter++;
                }
            }
            createInfo.EnabledLayerCount = (uint)layersCount;
            createInfo.PpEnabledLayerNames = layers;

            var extensionCount = 0;
            vkExtensions.ForEach(ext => extensionCount += ext.ExtensionNames.Length);
            var extensions = stackalloc byte*[extensionCount];
            iter = 0;
            foreach(var exten in vkExtensions)
            {
                var names = exten.ExtensionNames;
                for(var j = 0; j < names.Length; j++)
                {
                    extensions[iter] = (byte*)SilkMarshal.StringToPtr(names[j]);
                    iter++;
                }
            }
            createInfo.EnabledExtensionCount = (uint)extensionCount;
            createInfo.PpEnabledExtensionNames = extensions;

            var result = vk.CreateInstance(createInfo, null, out instance);
            VkContext.DebugCheck(result, "Instance create failed.");
            vk.CurrentInstance = instance;
        }

        public void Dispose()
        {
            vk.DestroyInstance(instance, null);
            // log
        }
    }

    public unsafe class VkPhysicalDevice
    {
        private Vk vk;
        public PhysicalDevice physicalDevice;
        public uint queueFamilyIndex;
        public VkPhysicalDevice(Vk vk)
        {
            this.vk = vk;
        }
        //private unsafe bool CheckDeviceExtensionSupport(PhysicalDevice device)
        //{
        //    return deviceExtensions.All(ext => vk.IsDeviceExtensionPresent(instance, ext));
        //}
        public void Initialize(Instance instance)
        {
            var devices = vk.GetPhysicalDevices(instance);

            if(devices.Count == 1)
            {
                physicalDevice = devices.ElementAt(0);
                queueFamilyIndex = 0;
                return;
            }
        }
    }

    public unsafe class VkLogicalDevice : IDisposable
    {
        private Vk vk;
        private Instance instance;
        private PhysicalDevice physicalDevice;
        public VkLogicalDevice(Vk vk)
        {
            this.vk = vk;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public unsafe class VkContext : IDisposable
    {
        public Vk vk;
        private VkInstance vkInstance;
        public Instance instance { get => vkInstance.instance; }
        public PhysicalDevice physicalDevice { get => vkPhysicalDevice.physicalDevice; }
        private VkPhysicalDevice vkPhysicalDevice;
        public Device device;
        private KhrSwapchain khrSwapchain;
        private KhrSurface khrSurface;

        private List<IVkExtension> vkExtensions;
        private byte** extensions;
        private uint extCount;
        private string[] deviceExtensions = { KhrSwapchain.ExtensionName };
        private string[] validationLayers;

        public VkContext()
        {
            vk = Vk.GetApi();
            vkInstance = new VkInstance(vk);
            //physicalDevice = default;
            vkPhysicalDevice = new VkPhysicalDevice(vk);
            device = default;

            vkExtensions = new List<IVkExtension>();
            extensions = null;
            extCount = 0;
        }

        public void AddExtension(IVkExtension extension)
        {
            vkExtensions.Add(extension);
        }

        public void SetExtensions(byte** ext, uint count)
        {
            extensions = ext;
            extCount = count;
        }

        public void Initialize(string appName, uint appVersion)
        {
            foreach(var extension in vkExtensions)
            {
                vkInstance.AddExtension(extension);
            }
            vkInstance.Initialize(appName, appVersion);

            //vk.TryGetInstanceExtension(instance, out khrSurface);
            //vk.TryGetInstanceExtension(instance, out khrSwapchain);
            vkPhysicalDevice.Initialize(instance);
            CreateLogicalDevice();
        }

        private unsafe QueueFamilyIndices FindQueueFamilies(PhysicalDevice device)
        {
            var indices = new QueueFamilyIndices();

            uint queryFamilyCount = 0;
            vk.GetPhysicalDeviceQueueFamilyProperties(device, &queryFamilyCount, null);

            using var mem = GlobalMemory.Allocate((int)queryFamilyCount * sizeof(QueueFamilyProperties));
            var queueFamilies = (QueueFamilyProperties*)Unsafe.AsPointer(ref mem.GetPinnableReference());

            vk.GetPhysicalDeviceQueueFamilyProperties(device, &queryFamilyCount, queueFamilies);
            for (var i = 0u; i < queryFamilyCount; i++)
            {
                var queueFamily = queueFamilies[i];
                if (queueFamily.QueueFlags.HasFlag(QueueFlags.GraphicsBit))
                {
                    indices.GraphicsFamily = i;
                    indices.PresentFamily = i;
                }

                if (indices.IsComplete())
                {
                    break;
                }
            }
            return indices;
        }

        private unsafe void CreateLogicalDevice()
        {
            validationLayers = GetOptimalValidationLayers();

            var indices = FindQueueFamilies(physicalDevice);
            var uniqueQueueFamilies = indices.GraphicsFamily!.Value == indices.PresentFamily!.Value
                ? new[] { indices.GraphicsFamily.Value }
                : new[] { indices.GraphicsFamily.Value, indices.PresentFamily.Value };

            using var mem = GlobalMemory.Allocate((int)uniqueQueueFamilies.Length * sizeof(DeviceQueueCreateInfo));
            var queueCreateInfos = (DeviceQueueCreateInfo*)Unsafe.AsPointer(ref mem.GetPinnableReference());

            var queuePriority = 1f;
            for (var i = 0; i < uniqueQueueFamilies.Length; i++)
            {
                var queueCreateInfo = new DeviceQueueCreateInfo(StructureType.DeviceQueueCreateInfo);
                queueCreateInfo.QueueFamilyIndex = uniqueQueueFamilies[i];
                queueCreateInfo.QueueCount = 1;
                queueCreateInfo.PQueuePriorities = &queuePriority;
                queueCreateInfos[i] = queueCreateInfo;
            }

            var deviceFeatures = new PhysicalDeviceFeatures();
            deviceFeatures.SamplerAnisotropy = Vk.True;
            var createInfo = new DeviceCreateInfo(StructureType.DeviceCreateInfo);
            createInfo.QueueCreateInfoCount = (uint)uniqueQueueFamilies.Length;
            createInfo.PQueueCreateInfos = queueCreateInfos;
            createInfo.PEnabledFeatures = &deviceFeatures;
            createInfo.EnabledExtensionCount = (uint)deviceExtensions.Length;

            var enabledExtensionNames = SilkMarshal.StringArrayToPtr(deviceExtensions);
            createInfo.PpEnabledExtensionNames = (byte**)enabledExtensionNames;

            if (EnableValidationLayers)
            {
                createInfo.EnabledLayerCount = (uint)validationLayers.Length;
                createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(validationLayers);
            }
            else
            {
                createInfo.EnabledLayerCount = 0;
            }

            var result = vk.CreateDevice(physicalDevice, in createInfo, null, out device);
            DebugCheck(result, "Create device failed.");

            vk.CurrentDevice = device;
        }

        private unsafe string[]? GetOptimalValidationLayers()
        {
            string[][] validationLayerNamesPriorityList =
            {
                new [] {"VK_LAYER_KHRONOS_validation"},
                new [] {"VK_LAYER_LUNARG_standard_validation"},
                new []
                {
                    "VK_LAYER_LUNARG_parameter_validation",
                    "VK_LAYER_LUNARG_object_tracker",
                    "VK_LAYER_LUNARG_core_validation"
                }
            };
            uint layerCount = 0;
            vk.EnumerateInstanceLayerProperties(ref layerCount, null);

            var availableLayers = new LayerProperties[layerCount];
            vk.EnumerateInstanceLayerProperties(&layerCount, availableLayers);

            var availableLayerNames = availableLayers.Select(availableLayer => Marshal.PtrToStringAnsi((nint)availableLayer.LayerName)).ToArray();
            foreach (var validationLayerNameSet in validationLayerNamesPriorityList)
            {
                if (validationLayerNameSet.All(validationLayerName => availableLayerNames.Contains(validationLayerName)))
                {
                    return validationLayerNameSet;
                }
            }
            return null;
        }

        private unsafe void MarshalAssignString(out byte* targ, string s)
        {
            targ = (byte*)Marshal.StringToHGlobalAnsi(s);
        }
        private unsafe void MarshalFreeString(in byte* targ)
        {
            Marshal.FreeHGlobal((IntPtr)targ);
        }

        public static void DebugCheck(Result result, string reason)
        {
#if DEBUG
            if(result != Result.Success)
            {
                throw new Exception(reason + "Error code is: " + result.ToString());
            }
#endif
        }
        public static void DebugCheck(Result result, Exception exception)
        {
#if DEBUG
            if(result != Result.Success)
            {
                throw exception;
            }
#endif
        }

        public void Dispose()
        {
            vk.DestroyDevice(device, null);
            vkInstance.Dispose();
        }
    }
}
