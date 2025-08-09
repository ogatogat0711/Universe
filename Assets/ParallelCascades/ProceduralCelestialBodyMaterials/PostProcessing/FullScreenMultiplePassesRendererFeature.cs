// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Experimental.Rendering;
// using UnityEngine.Rendering;
// using UnityEngine.Rendering.RenderGraphModule;
// using UnityEngine.Rendering.Universal;
//
// namespace ParallelCascades.ProceduralCelestialBodyMaterials.PostProcessing
// {
//     public class FullScreenMultiplePassRendererFeature : ScriptableRendererFeature
//     {
//         /// <summary>
//         /// An injection point for the full screen pass. This is similar to the RenderPassEvent enum but limited to only supported events.
//         /// </summary>
//         public enum InjectionPoint
//         {
//             BeforeRenderingTransparents = RenderPassEvent.BeforeRenderingTransparents,
//         }
//
//         /// <summary>
//         /// Specifies at which injection point the pass will be rendered.
//         /// </summary>
//         public FullScreenMultiplePassRendererFeature.InjectionPoint injectionPoint = FullScreenMultiplePassRendererFeature.InjectionPoint.BeforeRenderingTransparents;
//
//         /// <summary>
//         /// Specifies whether the assigned material will need to use the current screen contents as an input texture.
//         /// Disable this to optimize away an extra color copy pass when you know that the assigned material will only need
//         /// to write on top of or hardware blend with the contents of the active color target.
//         /// </summary>
//         public bool fetchColorBuffer = true;
//
//         /// <summary>
//         /// A mask of URP textures that the assigned material will need access to. Requesting unused requirements can degrade
//         /// performance unnecessarily as URP might need to run additional rendering passes to generate them.
//         /// </summary>
//         public ScriptableRenderPassInput requirements = ScriptableRenderPassInput.None;
//
//         [SerializeField]
//         [Tooltip("Materials used for blitting. They will be blit in the same order they have in the list starting from index 0. ")]
//         private List<Material> m_Materials;
//
//         /// <summary>
//         /// Specifies if the active camera's depth-stencil buffer should be bound when rendering the full screen pass.
//         /// Disabling this will ensure that the material's depth and stencil commands will have no effect (this could also have a slight performance benefit).
//         /// </summary>
//         public bool bindDepthStencilAttachment = false;
//
//         private FullScreenMultiplePassRendererFeature.FullScreenMultiplePasses m_FullScreenPass;
//
//         public override void Create()
//         {
//             m_FullScreenPass = new FullScreenMultiplePasses(name);
//         }
//         
//         // Here you can inject one or multiple render passes in the renderer.
//         // This method is called when setting up the renderer once per-camera.
//         public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
//         {
//             if (renderingData.cameraData.cameraType == CameraType.Preview
//                 || renderingData.cameraData.cameraType == CameraType.Reflection
//                 || UniversalRenderer.IsOffscreenDepthTexture(ref renderingData.cameraData))
//                 return;
//
//             // Early return if there is no texture to blit.
//             if (m_Materials == null || m_Materials.Count == 0) return;
//
//             m_FullScreenPass.renderPassEvent = (RenderPassEvent)injectionPoint;
//             m_FullScreenPass.ConfigureInput(requirements);
//             m_FullScreenPass.SetupMembers(m_Materials, fetchColorBuffer, bindDepthStencilAttachment);
//
//             m_FullScreenPass.requiresIntermediateTexture = fetchColorBuffer;
//         
//             renderer.EnqueuePass(m_FullScreenPass);
//         }
//
//         /// <inheritdoc/>
//         protected override void Dispose(bool disposing)
//         {
//             m_FullScreenPass.Dispose();
//         }
//
//         internal class FullScreenMultiplePasses : ScriptableRenderPass
//         {
//             private List<Material> m_Materials;
//             private int m_PassIndex;
//             private bool m_FetchActiveColor;
//             private bool m_BindDepthStencilAttachment;
//             private RTHandle m_CopiedColor;
//
//             private static MaterialPropertyBlock s_SharedPropertyBlock = new MaterialPropertyBlock();
//
//             public FullScreenMultiplePasses(string passName)
//             {
//                 profilingSampler = new ProfilingSampler(passName);
//             }
//
//             public void SetupMembers(List<Material> materials, bool fetchActiveColor, bool bindDepthStencilAttachment)
//             {
//                 m_Materials = materials;
//                 m_FetchActiveColor = fetchActiveColor;
//                 m_BindDepthStencilAttachment = bindDepthStencilAttachment;
//             }
//
//             public void Dispose()
//             {
//                 m_CopiedColor?.Release();
//             }
//
//              private static void ExecuteCopyColorPass(RasterCommandBuffer cmd, RTHandle sourceTexture)
//              {
//                  Blitter.BlitTexture(cmd, sourceTexture, new Vector4(1, 1, 0, 0), 0.0f, false);
//              }
//
//             private static void ExecuteMainPass(RasterCommandBuffer cmd, RTHandle sourceTexture, Material material, int passIndex)
//             {
//                 s_SharedPropertyBlock.Clear();
//                 if (sourceTexture != null)
//                     s_SharedPropertyBlock.SetTexture(ShaderPropertyId.blitTexture, sourceTexture);
//
//                 // We need to set the "_BlitScaleBias" uniform for user materials with shaders relying on core Blit.hlsl to work
//                 s_SharedPropertyBlock.SetVector(ShaderPropertyId.blitScaleBias, new Vector4(1, 1, 0, 0));
//
//                 cmd.DrawProcedural(Matrix4x4.identity, material, passIndex, MeshTopology.Triangles, 3, 1, s_SharedPropertyBlock);
//             }
//
//             public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
//             {
//                 UniversalResourceData resourcesData = frameData.Get<UniversalResourceData>();
//                 UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
//
//                 TextureHandle source, destination;
//
//                 Debug.Assert(resourcesData.cameraColor.IsValid());
//
//                 if (m_FetchActiveColor)
//                 {
//                     var targetDesc = renderGraph.GetTextureDesc(resourcesData.cameraColor);
//                     targetDesc.name = "_CameraColorFullScreenPass";
//                     targetDesc.clearBuffer = false;
//
//                     source = resourcesData.activeColorTexture;
//                     destination = renderGraph.CreateTexture(targetDesc);
//                 
//                     using (var builder = renderGraph.AddRasterRenderPass<CopyPassData>("Copy Color Full Screen", out var passData, profilingSampler))
//                     {
//                         passData.inputTexture = source;
//                         builder.UseTexture(passData.inputTexture, AccessFlags.Read);
//
//                         builder.SetRenderAttachment(destination, 0, AccessFlags.Write);
//
//                         builder.SetRenderFunc((CopyPassData data, RasterGraphContext rgContext) =>
//                         {
//                             ExecuteCopyColorPass(rgContext.cmd, data.inputTexture);
//                         });
//                     }
//
//                     //Swap for next pass;
//                     source = destination;                
//                 }
//                 else
//                 {
//                     source = TextureHandle.nullHandle;
//                 }
//
//                 destination = resourcesData.activeColorTexture;
//
//                 foreach (var material in m_Materials)
//                 {
//                     
//                     using (var builder = renderGraph.AddRasterRenderPass<MainPassData>(passName, out var passData, profilingSampler))
//                     {
//                         passData.material = material;
//                         passData.passIndex = m_PassIndex;
//
//                         passData.inputTexture = source;
//
//                         if(passData.inputTexture.IsValid())
//                             builder.UseTexture(passData.inputTexture, AccessFlags.Read);
//
//                         bool needsColor = (input & ScriptableRenderPassInput.Color) != ScriptableRenderPassInput.None;
//                         bool needsDepth = (input & ScriptableRenderPassInput.Depth) != ScriptableRenderPassInput.None;
//                         bool needsMotion = (input & ScriptableRenderPassInput.Motion) != ScriptableRenderPassInput.None;
//                         bool needsNormal = (input & ScriptableRenderPassInput.Normal) != ScriptableRenderPassInput.None;
//
//                         if (needsColor)
//                         {
//                             Debug.Assert(resourcesData.cameraOpaqueTexture.IsValid());
//                             builder.UseTexture(resourcesData.cameraOpaqueTexture);
//                         }
//
//                         if (needsDepth)
//                         {
//                             Debug.Assert(resourcesData.cameraDepthTexture.IsValid());
//                             builder.UseTexture(resourcesData.cameraDepthTexture);
//                         }
//
//                         if (needsMotion)
//                         {
//                             Debug.Assert(resourcesData.motionVectorColor.IsValid());
//                             builder.UseTexture(resourcesData.motionVectorColor);
//                             Debug.Assert(resourcesData.motionVectorDepth.IsValid());
//                             builder.UseTexture(resourcesData.motionVectorDepth);
//                         }
//
//                         if (needsNormal)
//                         {
//                             Debug.Assert(resourcesData.cameraNormalsTexture.IsValid());
//                             builder.UseTexture(resourcesData.cameraNormalsTexture);
//                         }
//                     
//                         builder.SetRenderAttachment(destination, 0, AccessFlags.Write);
//
//                         if (m_BindDepthStencilAttachment)
//                             builder.SetRenderAttachmentDepth(resourcesData.activeDepthTexture, AccessFlags.Write);
//
//                         builder.SetRenderFunc((global::UnityEngine.Rendering.Universal.FullScreenPassRendererFeature.FullScreenRenderPass.MainPassData data, RasterGraphContext rgContext) =>
//                         {
//                             ExecuteMainPass(rgContext.cmd, data.inputTexture, data.material, data.passIndex);
//                         });                
//                     }
//                 }
//             }
//
//             private class CopyPassData
//             {
//                 internal TextureHandle inputTexture;
//             }
//
//             private class MainPassData
//             {
//                 internal Material material;
//                 internal TextureHandle inputTexture;
//             }
//         }
//     }
// }