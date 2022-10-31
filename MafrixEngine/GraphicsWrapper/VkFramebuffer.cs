using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;

namespace MafrixEngine.GraphicsWrapper
{
    public class VkFramebuffer : IDisposable
    {
        private VkContext vkContext;
        public Framebuffer[] framebuffers;
        private RenderPass renderPass;
        private List<(int, int, ImageView)> attachments;
        public Extent2D frameExtent;
        public VkFramebuffer(VkContext ctx, int frames)
        {
            vkContext = ctx;
            framebuffers = new Framebuffer[frames];
            attachments = new List<(int, int, ImageView)>();
        }

        public void SetRenderpass(RenderPass pass)
        {
            renderPass = pass;
        }
        public void SetExtent(Extent2D extent)
        {
            frameExtent = extent;
        }
        int maxAttachIndex = 0;
        public void AddAttachment(int frameIndex, int attachIndex, ImageView attachment)
        {
            attachments.Add((frameIndex, attachIndex, attachment));
            if(attachIndex > maxAttachIndex)
            {
                maxAttachIndex = attachIndex;
            }
        }

        public unsafe void Build()
        {
#if DEBUG
            Debug.Assert(attachments.Count == (maxAttachIndex + 1) * framebuffers.Length);
#endif
            var attachs = stackalloc ImageView[maxAttachIndex+1];
            //var attachments = stackalloc ImageView[2];
            for (var i = 0; i < framebuffers.Length; i++)
            {
                var vl = attachments.Where((a) => a.Item1 == i);
                Debug.Assert(vl.Count() == (maxAttachIndex + 1));
                foreach (var v in vl)
                {
                    attachs[v.Item2] = v.Item3;
                }
                var framebufferInfo = new FramebufferCreateInfo(StructureType.FramebufferCreateInfo);
                framebufferInfo.RenderPass = renderPass;
                framebufferInfo.AttachmentCount = (uint)maxAttachIndex+1;
                framebufferInfo.PAttachments = attachs;
                framebufferInfo.Width = frameExtent.Width;
                framebufferInfo.Height = frameExtent.Height;
                framebufferInfo.Layers = 1;

                Framebuffer framebuffer;
                if (vkContext.vk.CreateFramebuffer(vkContext.device, framebufferInfo, null, out framebuffer) != Result.Success)
                {
                    throw new Exception("failed to create framebuffer!");
                }
                framebuffers[i] = framebuffer;
            }
        }

        public unsafe void Dispose()
        {
            foreach (var framebuffer in framebuffers)
            {
                vkContext.vk.DestroyFramebuffer(vkContext.device, framebuffer, null);
            }
        }
    }
}
