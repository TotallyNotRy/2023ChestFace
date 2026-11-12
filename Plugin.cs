using System;
using System.Collections;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;

namespace _2023ChestFace;

[BepInPlugin("com.notry.gtag.2023chestface", "2023ChestFace", "1.1.0")]
public sealed class Plugin : BaseUnityPlugin
{
    private const string FaceResourceName = "2023FaceTex.gorillatexture.png";
    private const string FaceSection = "Face";

    private Texture2D? faceTexture;
    private ConfigEntry<float> faceScaleX = null!;
    private ConfigEntry<float> faceScaleY = null!;
    private ConfigEntry<float> faceOffsetX = null!;
    private ConfigEntry<float> faceOffsetY = null!;
    private ConfigEntry<bool> pointFilter = null!;
    private Coroutine? applyLoop;

    internal static Plugin? Instance { get; private set; }
    internal static ManualLogSource? Log { get; private set; }
    internal Texture2D? FaceTexture => faceTexture;
    internal Vector4 FaceST => new Vector4(faceScaleX.Value, faceScaleY.Value, faceOffsetX.Value, faceOffsetY.Value);

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        // The original texture occupies one quadrant in the current face UV layout.
        faceScaleX = Config.Bind(FaceSection, "ScaleX", 2f, "Face texture X scale.");
        faceScaleY = Config.Bind(FaceSection, "ScaleY", 2f, "Face texture Y scale.");
        faceOffsetX = Config.Bind(FaceSection, "OffsetX", 0f, "Face texture X offset.");
        faceOffsetY = Config.Bind(FaceSection, "OffsetY", -1f, "Face texture Y offset.");
        pointFilter = Config.Bind("General", "PointFilter", true, "Use nearest-neighbor filtering for the pixel-art texture.");

        faceTexture = LoadEmbeddedTexture(FaceResourceName);
        if (faceTexture == null)
        {
            Logger.LogError("Face texture was not loaded; face replacement is disabled.");
            enabled = false;
            return;
        }

        applyLoop = StartCoroutine(ApplyLocalFaceLoop());
        applyLoop = StartCoroutine(ApplyGlobalFaceLoop());
        Logger.LogInfo($"Loaded local face texture: {faceTexture.width}x{faceTexture.height}.");
    }

    private void OnEnable()
    {
        if (faceTexture != null && applyLoop == null)
            applyLoop = StartCoroutine(ApplyLocalFaceLoop());
            applyLoop = StartCoroutine(ApplyGlobalFaceLoop());
    }

    private void OnDisable()
    {
        if (applyLoop != null)
        {
            StopCoroutine(applyLoop);
            applyLoop = null;
        }
    }

    private IEnumerator ApplyLocalFaceLoop()
    {
        var wait = new WaitForSeconds(0.5f);
        while (enabled)
        {
            ApplyToLocalFace();
            yield return wait;
        }
        applyLoop = null;
    }
    private IEnumerator ApplyGlobalFaceLoop()
    {
        var wait = new WaitForSeconds(0.5f);
        while (enabled)
        {
            ApplyToOtherFaces();
            yield return wait;
        }
        applyLoop = null;
    }
    private void ApplyToLocalFace()
    {
        try
        {
            VRRig? localRig = GorillaTagger.Instance != null ? GorillaTagger.Instance.offlineVRRig : null;
            if (localRig == null || localRig.faceSkin == null || faceTexture == null)
                return;

            LegacyTextureApplier.ApplyToFace(localRig.faceSkin, faceTexture, FaceST);
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to apply the face texture to the local rig: " + ex);
        }
    }

    public void ApplyToOtherFaces()
    {
        try
        {
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (!rig.isOfflineVRRig)
                {
                    LegacyTextureApplier.ApplyToOtherFaces(rig.faceSkin, faceTexture, FaceST);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to apply the face texture to the rig: " + ex);
        }
    }


    private Texture2D? LoadEmbeddedTexture(string resourceName)
    {
        using (Stream? resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
        {
            if (resource == null)
            {
                Logger.LogError("Missing embedded resource: " + resourceName);
                Logger.LogInfo("Embedded resources: " + string.Join(", ", Assembly.GetExecutingAssembly().GetManifestResourceNames()));
                return null;
            }

            using (var buffer = new MemoryStream())
            {
                resource.CopyTo(buffer);
                byte[] imageBytes = buffer.ToArray();
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!ImageConversion.LoadImage(texture, imageBytes, false))
                {
                    Logger.LogError("Embedded resource is not a readable image: " + resourceName);
                    Destroy(texture);
                    return null;
                }

                texture.name = "2023ChestFace.2023Face";
                texture.filterMode = pointFilter.Value ? FilterMode.Point : FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.anisoLevel = 1;
                return texture;
            }
        }
    }
}


