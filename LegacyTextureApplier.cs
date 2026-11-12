using UnityEngine;

namespace _2023ChestFace;

internal static class LegacyTextureApplier
{
    private static readonly MaterialPropertyBlock PropertyBlock = new MaterialPropertyBlock();
    private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
    private static readonly int BaseMapStId = Shader.PropertyToID("_BaseMap_ST");
    private static readonly int BaseMapWhId = Shader.PropertyToID("_BaseMap_WH");
    private static readonly int AtlasToggleId = Shader.PropertyToID("_USE_TEX_ARRAY_ATLAS");
    private static readonly int AtlasSliceId = Shader.PropertyToID("_BaseMap_AtlasSlice");
    private static readonly int AtlasSliceSourceId = Shader.PropertyToID("_BaseMap_AtlasSliceSource");
    private static bool loggedApplied;

    internal static void ApplyToFace(Renderer? faceRenderer, Texture2D? texture, Vector4 scaleOffset)
    {
        if (faceRenderer == null || texture == null)
            return;

        Material[] materials = faceRenderer.materials;
        for (int slot = 0; slot < materials.Length; slot++)
        {
            Material material = materials[slot];
            if (material == null || !material.HasProperty(BaseMapId))
                continue;

            // Face renderer materials are instances; these settings cannot alter other players or body materials.
            material.SetTexture(BaseMapId, texture);
            material.SetTextureScale("_BaseMap", new Vector2(scaleOffset.x, scaleOffset.y));
            material.SetTextureOffset("_BaseMap", new Vector2(scaleOffset.z, scaleOffset.w));
            material.DisableKeyword("_USE_TEX_ARRAY_ATLAS");
            material.EnableKeyword("_USE_TEXTURE");
            if (material.HasProperty(AtlasToggleId)) material.SetFloat(AtlasToggleId, 0f);
            if (material.HasProperty(AtlasSliceId)) material.SetFloat(AtlasSliceId, 0f);
            if (material.HasProperty(AtlasSliceSourceId)) material.SetFloat(AtlasSliceSourceId, 0f);
            if (material.HasProperty(BaseMapWhId))
                material.SetVector(BaseMapWhId, new Vector4(texture.width, texture.height, 1f / texture.width, 1f / texture.height));

            faceRenderer.GetPropertyBlock(PropertyBlock, slot);
            PropertyBlock.SetTexture(BaseMapId, texture);
            PropertyBlock.SetVector(BaseMapStId, scaleOffset);
            if (material.HasProperty(BaseMapWhId))
                PropertyBlock.SetVector(BaseMapWhId, new Vector4(texture.width, texture.height, 1f / texture.width, 1f / texture.height));
            if (material.HasProperty(AtlasToggleId)) PropertyBlock.SetFloat(AtlasToggleId, 0f);
            if (material.HasProperty(AtlasSliceId)) PropertyBlock.SetFloat(AtlasSliceId, 0f);
            if (material.HasProperty(AtlasSliceSourceId)) PropertyBlock.SetFloat(AtlasSliceSourceId, 0f);
            faceRenderer.SetPropertyBlock(PropertyBlock, slot);
        }
    }
    public static void ApplyToOtherFaces(Renderer? faceRenderer, Texture2D? texture, Vector4 scaleOffset)
    {
        if (faceRenderer == null || texture == null)
            return;

        Material[] materials = faceRenderer.materials;
        bool applied = false;
        for (int slot = 0; slot < materials.Length; slot++)
        {
            Material material = materials[slot];
            if (material == null || !material.HasProperty(BaseMapId))
                continue;

            material.SetTexture(BaseMapId, texture);
            material.SetTextureScale("_BaseMap", new Vector2(scaleOffset.x, scaleOffset.y));
            material.SetTextureOffset("_BaseMap", new Vector2(scaleOffset.z, scaleOffset.w));
            material.DisableKeyword("_USE_TEX_ARRAY_ATLAS");
            material.EnableKeyword("_USE_TEXTURE");
            if (material.HasProperty(AtlasToggleId)) material.SetFloat(AtlasToggleId, 0f);
            if (material.HasProperty(AtlasSliceId)) material.SetFloat(AtlasSliceId, 0f);
            if (material.HasProperty(AtlasSliceSourceId)) material.SetFloat(AtlasSliceSourceId, 0f);
            if (material.HasProperty(BaseMapWhId))
                material.SetVector(BaseMapWhId, new Vector4(texture.width, texture.height, 1f / texture.width, 1f / texture.height));

            faceRenderer.GetPropertyBlock(PropertyBlock, slot);
            PropertyBlock.SetTexture(BaseMapId, texture);
            PropertyBlock.SetVector(BaseMapStId, scaleOffset);
            if (material.HasProperty(BaseMapWhId))
                PropertyBlock.SetVector(BaseMapWhId, new Vector4(texture.width, texture.height, 1f / texture.width, 1f / texture.height));
            if (material.HasProperty(AtlasToggleId)) PropertyBlock.SetFloat(AtlasToggleId, 0f);
            if (material.HasProperty(AtlasSliceId)) PropertyBlock.SetFloat(AtlasSliceId, 0f);
            if (material.HasProperty(AtlasSliceSourceId)) PropertyBlock.SetFloat(AtlasSliceSourceId, 0f);
            faceRenderer.SetPropertyBlock(PropertyBlock, slot);
            applied = true;
        }

        if (applied && !loggedApplied)
        {
            Plugin.Log?.LogInfo("Applied the replacement texture to the local face renderer only.");
            loggedApplied = true;
        }
    }
}


