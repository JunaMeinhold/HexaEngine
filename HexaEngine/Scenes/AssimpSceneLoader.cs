namespace HexaEngine.Scenes
{
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using Silk.NET.Assimp;
    using System.Diagnostics;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Text;

    public class AssimpSceneLoader
    {
        public static Assimp Assimp = Assimp.GetApi();

        public static Task ImportAsync(string path, Scene scene)
        {
            return Task.Run(() => Import(path, scene));
        }

        public static Task ImportAsync(string path)
        {
            return Task.Run(() => Import(path, SceneManager.Current));
        }

        public static unsafe void Import(string path, Scene sceneTarget)
        {
            var scene = Assimp.ImportFile(path, (uint)(ImporterFlags.ImporterFlagsSupportBinaryFlavour | ImporterFlags.ImporterFlagsSupportTextFlavour | ImporterFlags.ImporterFlagsSupportCompressedFlavour));
            var scene1 = Assimp.ApplyPostProcessing(scene, (uint)(PostProcessSteps.CalculateTangentSpace | PostProcessSteps.MakeLeftHanded | PostProcessSteps.Triangulate));

            if (scene1 != scene)
            {
                Trace.Fail("");
            }
            AssimpMaterial[] materials = new AssimpMaterial[scene->MNumMaterials];
            for (int i = 0; i < scene->MNumMaterials; i++)
            {
                Material* mat = scene->MMaterials[i];
                Dictionary<(string, object), object> props = new();
                AssimpMaterialTexture[] texs = new AssimpMaterialTexture[(int)TextureType.TextureTypeUnknown + 1];
                for (int j = 0; j < texs.Length; j++)
                {
                    texs[j].Type = (TextureType)j;
                }
                AssimpMaterial material = new();

                for (int j = 0; j < mat->MNumProperties; j++)
                {
                    MaterialProperty* prop = mat->MProperties[j];
                    if (prop == null) continue;
                    Span<byte> buffer = new(prop->MData, (int)prop->MDataLength);
                    string key = prop->MKey;
                    int semantic = (int)prop->MSemantic;
                    switch (key)
                    {
                        case Assimp.MatkeyName:
                            material.Name = Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1));
                            break;

                        case Assimp.MatkeyTwosided:
                            material.Twosided = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyShadingModel:
                            material.ShadingModel = (ShadingMode)MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyEnableWireframe:
                            material.EnableWireframe = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyBlendFunc:
                            material.BlendFunc = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyOpacity:
                            material.Opacity = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyTransparencyfactor:
                            material.Transparencyfactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyBumpscaling:
                            material.Bumpscaling = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyShininess:
                            material.Shininess = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyReflectivity:
                            material.Reflectivity = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyShininessStrength:
                            material.ShininessStrength = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyRefracti:
                            material.Refracti = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyColorDiffuse:
                            material.ColorDiffuse = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyColorAmbient:
                            material.ColorAmbient = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyColorSpecular:
                            material.ColorSpecular = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyColorEmissive:
                            material.ColorEmissive = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyColorTransparent:
                            material.ColorTransparent = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyColorReflective:
                            material.ColorReflective = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyUseColorMap:
                            material.UseColorMap = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyBaseColor:
                            material.BaseColor = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyUseMetallicMap:
                            material.UseMetallicMap = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyMetallicFactor:
                            material.MetallicFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyUseRoughnessMap:
                            material.UseRoughnessMap = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyRoughnessFactor:
                            material.RoughnessFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyAnisotropyFactor:
                            material.AnisotropyFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeySpecularFactor:
                            material.SpecularFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyGlossinessFactor:
                            material.GlossinessFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeySheenColorFactor:
                            material.SheenColorFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeySheenRoughnessFactor:
                            material.SheenRoughnessFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyClearcoatFactor:
                            material.ClearcoatFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyClearcoatRoughnessFactor:
                            material.ClearcoatRoughnessFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyTransmissionFactor:
                            material.TransmissionFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyVolumeThicknessFactor:
                            material.VolumeThicknessFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyVolumeAttenuationDistance:
                            material.VolumeAttenuationDistance = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyVolumeAttenuationColor:
                            material.VolumeAttenuationColor = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyUseEmissiveMap:
                            material.UseEmissiveMap = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyEmissiveIntensity:
                            material.EmissiveIntensity = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyUseAOMap:
                            material.UseAOMap = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyTextureBase:
                            texs[semantic].File = Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1));
                            break;

                        case Assimp.MatkeyUvwsrcBase:
                            texs[semantic].UVWSrc = MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyTexopBase:
                            texs[semantic].Op = (TextureOp)MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyMappingBase:
                            texs[semantic].Mapping = MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyTexblendBase:
                            texs[semantic].Blend = (BlendMode)MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyMappingmodeUBase:
                            texs[semantic].U = (TextureMapMode)MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyMappingmodeVBase:
                            texs[semantic].V = (TextureMapMode)MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyTexmapAxisBase:
                            break;

                        case Assimp.MatkeyUvtransformBase:
                            break;

                        case Assimp.MatkeyTexflagsBase:
                            texs[semantic].Flags = (TextureFlags)MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;
                    }
                }
                if (material.Name == string.Empty)
                    material.Name = i.ToString();
                material.Textures = texs;
                materials[i] = material;
                sceneTarget.AddMaterial(new Objects.Material()
                {
                    Albedo = material.BaseColor,
                    Ao = 1,
                    Emissivness = material.ColorEmissive,
                    Metalness = material.MetallicFactor,
                    Roughness = material.RoughnessFactor,
                    Opacity = material.Opacity,
                    Name = material.Name,
                    AlbedoTextureMap = material.Textures[(int)TextureType.TextureTypeBaseColor].File ?? string.Empty,
                    AoTextureMap = material.Textures[(int)TextureType.TextureTypeAmbientOcclusion].File ?? string.Empty,
                    DisplacementTextureMap = material.Textures[(int)TextureType.TextureTypeDisplacement].File ?? string.Empty,
                    EmissiveTextureMap = material.Textures[(int)TextureType.TextureTypeEmissionColor].File ?? string.Empty,
                    MetalnessTextureMap = material.Textures[(int)TextureType.TextureTypeMetalness].File ?? string.Empty,
                    NormalTextureMap = material.Textures[(int)TextureType.TextureTypeNormals].File ?? string.Empty,
                    RoughnessTextureMap = material.Textures[(int)TextureType.TextureTypeDiffuseRoughness].File ?? string.Empty,
                    RoughnessMetalnessTextureMap = material.Textures[(int)TextureType.TextureTypeUnknown].File ?? string.Empty,
                });
            }

            Objects.Mesh[] meshes = new Objects.Mesh[scene->MNumMeshes];
            for (int i = 0; i < scene->MNumMeshes; i++)
            {
                Mesh* msh = scene->MMeshes[i];
                AssimpMesh mesh = new();
                MeshVertex[] vertices = new MeshVertex[msh->MNumVertices];
                int[] indices = new int[msh->MNumFaces * 3];
                for (int j = 0; j < msh->MNumFaces; j++)
                {
                    var face = msh->MFaces[j];
                    for (int k = 0; k < 3; k++)
                    {
                        indices[j * 3 + k] = (int)face.MIndices[k];
                    }
                }

                for (int j = 0; j < msh->MNumVertices; j++)
                {
                    Vector3 pos = msh->MVertices[j];
                    Vector3 nor = msh->MNormals[j];
                    Vector3 tex = msh->MTextureCoords[0][j];
                    Vector3 tan = msh->MTangents[j];

                    MeshVertex vertex = new(pos, new(tex.X, tex.Y), nor, tan);
                    vertices[j] = vertex;
                }

                MeshBone[] bones = new MeshBone[msh->MNumBones];
                Objects.Skeleton skeleton = new();
                for (int j = 0; j < msh->MNumBones; j++)
                {
                    var bone = msh->MBones[j];
                    MeshWeight[] weights = new MeshWeight[bone->MNumWeights];
                    for (int x = 0; x < bone->MNumWeights; x++)
                    {
                        var weight = bone->MWeights[x];
                        weights[x] = new MeshWeight() { VertexId = weight.MVertexId, Weight = weight.MWeight };
                    }
                    bones[j] = new(bone->MName, weights, bone->MOffsetMatrix);
                    skeleton.AddRelation(bone->MName, FindParent(scene, bone->MName));
                    skeleton.AddBone(bones[j]);
                }
                meshes[i] = new Objects.Mesh() { Indices = indices, Vertices = vertices, Bones = bones, Skeleton = skeleton, MaterialName = materials[msh->MMaterialIndex].Name };
                sceneTarget.AddMesh(meshes[i]);
            }

            AssimpTexture[] textures = new AssimpTexture[scene->MNumTextures];
            for (int i = 0; i < scene->MNumTextures; i++)
            {
                var tex = scene->MTextures[i];
                textures[i] = new()
                {
                    Data = tex->PcData,
                    Format = new Span<int>(tex->AchFormatHint, 1)[0],
                    Height = (int)tex->MHeight,
                    Width = (int)tex->MWidth
                };
            }

            AssimpCamera[] cameras = new AssimpCamera[scene->MNumCameras];
            for (int i = 0; i < scene->MNumCameras; i++)
            {
                var cam = scene->MCameras[i];
                cameras[i] = new()
                {
                    Name = cam->MName,
                    Aspect = cam->MAspect,
                    Far = cam->MClipPlaneFar,
                    Near = cam->MClipPlaneNear,
                    Fov = cam->MHorizontalFOV,
                    Width = cam->MOrthographicWidth,
                };
            }

            AssimpLight[] lights = new AssimpLight[scene->MNumLights];
            for (int i = 0; i < scene->MNumLights; i++)
            {
                var lig = scene->MLights[i];
                lights[i] = new()
                {
                    Name = lig->MName,
                    Type = lig->MType,
                    AngleInner = lig->MAngleInnerCone,
                    AngleOuter = lig->MAngleOuterCone,
                    Color = lig->MColorDiffuse
                };
            }

            SceneNode WalkNode(Node* node, SceneNode parent)
            {
                string name = node->MName;
                SceneNode sceneNode = new();

                if (cameras.Any(x => x.Name == name))
                {
                    var camera = new Cameras.Camera();
                    var cam = cameras.First(x => x.Name == name);
                    camera.Transform.Fov = cam.Fov.ToDeg();
                    camera.Transform.Width = cam.Width;
                    camera.Transform.Height = 1f / cam.Width * cam.Aspect;
                    camera.Transform.Far = cam.Far;
                    camera.Transform.Near = cam.Near;
                    sceneNode = camera;
                }

                if (lights.Any(x => x.Name == name))
                {
                    var lig = lights.First(x => x.Name == name);
                    switch (lig.Type)
                    {
                        case LightSourceType.LightSourceUndefined:
                            throw new NotSupportedException();

                        case LightSourceType.LightSourceDirectional:
                            var dir = new DirectionalLight();
                            dir.Color = new Vector4(lig.Color, 1);
                            sceneNode = dir;
                            break;

                        case LightSourceType.LightSourcePoint:
                            var point = new PointLight();
                            point.Color = new Vector4(lig.Color, 1);
                            point.Strength = 1;
                            sceneNode = point;
                            break;

                        case LightSourceType.LightSourceSpot:
                            var spot = new Spotlight();
                            spot.Color = new Vector4(lig.Color, 1);
                            spot.Strength = 1;
                            spot.ConeAngle = lig.AngleOuter.ToDeg();
                            spot.Blend = lig.AngleInner.ToDeg() / spot.ConeAngle;
                            sceneNode = spot;
                            break;

                        case LightSourceType.LightSourceAmbient:
                            throw new NotSupportedException();

                        case LightSourceType.LightSourceArea:
                            throw new NotSupportedException();
                    }
                }

                sceneNode.Name = name;
                Matrix4x4.Decompose(Matrix4x4.Transpose(node->MTransformation), out var scale, out var orientation, out var position);
                sceneNode.Transform.PositionRotationScale = (position, orientation, scale);
                for (int i = 0; i < node->MNumMeshes; i++)
                {
                    sceneNode.AddMesh(meshes[node->MMeshes[i]]);
                }

                for (int i = 0; i < node->MNumChildren; i++)
                {
                    var child = WalkNode(node->MChildren[i], sceneNode);
                    sceneNode.AddChild(child);
                }

                return sceneNode;
            }

            var root = WalkNode(scene->MRootNode, null);
            if (root.Name == "ROOT")
                sceneTarget.Root.Merge(root);
            else
                sceneTarget.Root.AddChild(root);
        }

        public static unsafe void Open(string path)
        {
            Scene scene = new();
            Import(path, scene);
            SceneManager.Load(scene);
        }

        public static async Task OpenAsync(string path)
        {
            Scene scene = new();
            await SceneManager.AsyncLoad(scene);
            await ImportAsync(path, scene);
        }

        public struct AssimpMaterial
        {
            public string Name = string.Empty;
            public bool Twosided = false;
            public ShadingMode ShadingModel = ShadingMode.ShadingModeGouraud;
            public bool EnableWireframe = false;
            public bool BlendFunc = false;
            public float Opacity = 1;
            public float Transparencyfactor = 1;
            public float Bumpscaling = 1;
            public float Shininess = 0;
            public float Reflectivity = 0;
            public float ShininessStrength = 1;
            public float Refracti = 1;
            public Vector3 ColorDiffuse = Vector3.Zero;
            public Vector3 ColorAmbient = Vector3.Zero;
            public Vector3 ColorSpecular = Vector3.Zero;
            public Vector3 ColorEmissive = Vector3.Zero;
            public Vector3 ColorTransparent = Vector3.Zero;
            public Vector3 ColorReflective = Vector3.Zero;

            public bool UseColorMap = false;
            public Vector3 BaseColor = Vector3.Zero;
            public bool UseMetallicMap = false;
            public float MetallicFactor = 0f;
            public bool UseRoughnessMap = false;
            public float RoughnessFactor = 0.5f;
            public float AnisotropyFactor = 0f;
            public float SpecularFactor = 0.5f;
            public float GlossinessFactor = 0f;
            public float SheenColorFactor = 0f;
            public float SheenRoughnessFactor = 0.5f;
            public float ClearcoatFactor = 0f;
            public float ClearcoatRoughnessFactor = 0.03f;
            public float TransmissionFactor = 0f;
            public float VolumeThicknessFactor = 0;
            public float VolumeAttenuationDistance = 0;
            public Vector3 VolumeAttenuationColor = Vector3.Zero;
            public bool UseEmissiveMap = false;
            public float EmissiveIntensity = 1;
            public bool UseAOMap = false;
            public AssimpMaterialTexture[] Textures = Array.Empty<AssimpMaterialTexture>();

            public AssimpMaterial()
            {
            }
        }

        public struct AssimpMaterialTexture
        {
            public TextureType Type;
            public string File;
            public BlendMode Blend;
            public TextureOp Op;
            public int Mapping;
            public int UVWSrc;
            public TextureMapMode U;
            public TextureMapMode V;
            public TextureFlags Flags;
        }

        public unsafe struct AssimpTexture
        {
            public int Format;
            public int Width;
            public int Height;
            public void* Data;
        }

        public struct AssimpMesh
        {
            public string Name;
            public MeshVertex[] Vertices;
            public int[] Indices;
            public uint Material;
            public AssimpBone[] Bones;
        }

        public unsafe struct AssimpBone
        {
            public string Name;
            public MeshWeight[] Weights;
            public Matrix4x4 Offset;
            public Node* Node;

            public AssimpBone(string name, MeshWeight[] weights, Matrix4x4 offset, Node* node)
            {
                Name = name;
                Weights = weights;
                Offset = offset;
                Node = node;
            }
        }

        public struct AssimpCamera
        {
            public string Name;
            public float Aspect;
            public float Far;
            public float Near;
            public float Fov;
            public float Width;
        }

        public struct AssimpLight
        {
            public string Name;
            public LightSourceType Type;
            public Vector3 Color;
            public float AngleInner;
            public float AngleOuter;
            public float Strength;
        }

        public static unsafe string FindParent(Silk.NET.Assimp.Scene* scene, string name)
        {
            static (bool, string?) Find(Node* node, string name)
            {
                if (node->MName == name)
                    return (true, node->MParent->MName);
                for (int i = 0; i < node->MNumChildren; i++)
                {
                    var result = Find(node->MChildren[i], name);
                    if (result.Item1)
                        return result;
                }
                return (false, default);
            }
            return Find(scene->MRootNode, name).Item2 ?? string.Empty;
        }
    }
}