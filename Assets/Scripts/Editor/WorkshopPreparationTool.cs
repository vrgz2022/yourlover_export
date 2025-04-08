using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 创意工坊资源准备工具：用于生成预览图和资源包
/// </summary>
public class WorkshopPreparationTool : EditorWindow
{
    private GameObject uploadTarget;
    private Camera previewCamera;
    private string outputPath = "SteamWorkshopContent";
    private int previewWidth = 512;
    private int previewHeight = 512;
    private string title = "物品标题";
    private string description = "物品描述";
    private Vector2 scrollPosition;

    [MenuItem("Tools/创意工坊资源准备工具")]
    public static void ShowWindow()
    {
        GetWindow<WorkshopPreparationTool>("创意工坊资源准备");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.LabelField("创意工坊资源准备工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 上传物体选择
        EditorGUI.BeginChangeCheck();
        uploadTarget = EditorGUILayout.ObjectField("上传物体", uploadTarget, typeof(GameObject), true) as GameObject;
        if (EditorGUI.EndChangeCheck())
        {
            if (uploadTarget != null && uploadTarget.scene.name == null)
            {
                EditorUtility.DisplayDialog("提示", "请选择场景中的物体，而不是预制体", "确定");
                uploadTarget = null;
            }
        }

        // 预览图设置
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("预览图设置", EditorStyles.boldLabel);
        previewCamera = (Camera)EditorGUILayout.ObjectField("预览相机", previewCamera, typeof(Camera), true);
        previewWidth = EditorGUILayout.IntField("预览图宽度", previewWidth);
        previewHeight = EditorGUILayout.IntField("预览图高度", previewHeight);

        // 资源设置
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("资源设置", EditorStyles.boldLabel);
        outputPath = EditorGUILayout.TextField("输出路径", outputPath);

        // 物品信息
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("物品信息", EditorStyles.boldLabel);
        title = EditorGUILayout.TextField("标题", title);
        EditorGUILayout.LabelField("描述");
        description = EditorGUILayout.TextArea(description, GUILayout.Height(100));

        // 压缩选项
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("压缩选项:", GUILayout.Width(70));
        if (GUILayout.Button("优化资源", GUILayout.Width(120)))
        {
            OptimizeResources();
        }
        EditorGUILayout.EndHorizontal();

        // 操作按钮
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = uploadTarget != null;
        if (GUILayout.Button("生成预览图"))
        {
            CapturePreview();
        }
        
        if (GUILayout.Button("生成资源包"))
        {
            BuildAssetBundle();
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        // 生成所有文件
        EditorGUILayout.Space();
        if (GUILayout.Button("生成所有文件"))
        {
            GenerateAllFiles();
        }

        // 帮助信息
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "使用说明：\n" +
            "1. 选择要上传的物体\n" +
            "2. 设置预览相机\n" +
            "3. 填写标题和描述\n" +
            "4. 点击[生成所有文件]\n" +
            "5. 使用steamcmd上传", 
            MessageType.Info);

        EditorGUILayout.EndScrollView();
    }

    private void CapturePreview()
    {
        if (uploadTarget == null) return;

        try
        {
            // 保存当前场景相机
            Camera sceneCamera = null;
            if (Camera.main != null)
            {
                sceneCamera = Camera.main;
            }

            // 创建预览相机
            GameObject cameraObj = new GameObject("PreviewCamera");
            Camera previewCamera = cameraObj.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = Color.black;

            // 计算物体边界
            Renderer[] renderers = uploadTarget.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                EditorUtility.DisplayDialog("错误", "物体没有可见部分", "确定");
                return;
            }

            // 计算包围盒
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            // 设置相机位置
            float objectSize = bounds.size.magnitude;
            float distance = objectSize * 2.0f;

            // 如果场景中有相机，使用场景相机的角度
            if (sceneCamera != null)
            {
                previewCamera.transform.position = sceneCamera.transform.position;
                previewCamera.transform.rotation = sceneCamera.transform.rotation;
            }
            else
            {
                // 默认45度角俯视
                previewCamera.transform.position = bounds.center + new Vector3(distance * 0.5f, distance * 0.5f, -distance * 0.5f);
                previewCamera.transform.LookAt(bounds.center);
            }

            // 调整相机视野以适应物体
            previewCamera.fieldOfView = 60f;
            float targetDistance = distance / (2.0f * Mathf.Tan(previewCamera.fieldOfView * 0.5f * Mathf.Deg2Rad));
            previewCamera.transform.position = bounds.center + previewCamera.transform.rotation * new Vector3(0, 0, -targetDistance);

            // 确保输出目录存在
            Directory.CreateDirectory(outputPath);

            // 渲染预览图
            RenderTexture rt = new RenderTexture(previewWidth, previewHeight, 24);
            previewCamera.targetTexture = rt;
            previewCamera.Render();
            RenderTexture.active = rt;

            // 保存预览图
            Texture2D preview = new Texture2D(previewWidth, previewHeight);
            preview.ReadPixels(new Rect(0, 0, previewWidth, previewHeight), 0, 0);
            preview.Apply();

            // 保存为PNG
            byte[] bytes = preview.EncodeToPNG();
            string filePath = Path.Combine(outputPath, "preview.jpg");
            File.WriteAllBytes(filePath, bytes);

            // 清理
            RenderTexture.active = null;
            previewCamera.targetTexture = null;
            DestroyImmediate(rt);
            DestroyImmediate(preview);
            DestroyImmediate(cameraObj);

            Debug.Log($"预览图已保存: {filePath}");
            EditorUtility.DisplayDialog("成功", "预览图已生成", "确定");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"生成预览图失败: {ex}");
            EditorUtility.DisplayDialog("错误", $"生成预览图失败: {ex.Message}", "确定");
        }
    }

    private void BuildAssetBundle()
    {
        if (uploadTarget == null) return;

        try
        {
            // 确保目录存在
            Directory.CreateDirectory(outputPath);

            // 创建临时预制体
            string tempPrefabDir = "Assets/Editor/Temp";
            if (!Directory.Exists(tempPrefabDir))
            {
                Directory.CreateDirectory(tempPrefabDir);
            }

            string prefabPath = Path.Combine(tempPrefabDir, "UploadTemp.prefab");
            GameObject prefabInstance = PrefabUtility.SaveAsPrefabAsset(uploadTarget, prefabPath);

            // 检查模型和纹理大小
            CheckResourceSize(prefabInstance);

            Debug.Log($"开始构建AssetBundle...");
            Debug.Log($"预制体路径：{prefabPath}");
            Debug.Log($"输出目录：{outputPath}");

            // 构建AssetBundle
            var assetBundleBuild = new AssetBundleBuild
            {
                assetBundleName = "item_content",
                assetNames = new string[] { prefabPath }
            };

            // 使用LZ4HC压缩格式，它有很好的压缩率和解压速度
            BuildAssetBundleOptions bundleOptions = BuildAssetBundleOptions.None;
            // 使用LZ4HC压缩，这是Unity推荐的压缩方式，比默认的LZMA压缩率低但解压速度快
            bundleOptions |= BuildAssetBundleOptions.ChunkBasedCompression;
            // 强制重建以确保干净
            bundleOptions |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            // 严格模式以减少文件大小
            bundleOptions |= BuildAssetBundleOptions.StrictMode;

            // 构建AssetBundle
            BuildPipeline.BuildAssetBundles(
                outputPath,
                new AssetBundleBuild[] { assetBundleBuild },
                bundleOptions,
                BuildTarget.StandaloneWindows64
            );

            // 重命名生成的文件
            string generatedBundle = Path.Combine(outputPath, "item_content");
            string finalBundlePath = Path.Combine(outputPath, "item_content.unity3d");
            if (File.Exists(generatedBundle))
            {
                if (File.Exists(finalBundlePath))
                {
                    File.Delete(finalBundlePath);
                }
                File.Move(generatedBundle, finalBundlePath);

                // 验证Bundle文件的有效性和大小
                using (FileStream fs = File.OpenRead(finalBundlePath))
                {
                    byte[] signature = new byte[7];
                    fs.Read(signature, 0, 7);
                    string header = System.Text.Encoding.ASCII.GetString(signature);

                    if (!header.StartsWith("Unity"))
                    {
                        throw new System.Exception("生成的文件不是有效的Unity AssetBundle，请检查构建设置");
                    }

                    // 检查文件大小
                    float fileSizeMB = fs.Length / (1024f * 1024f);
                    if (fileSizeMB > 100f)
                    {
                        Debug.LogWarning($"警告：AssetBundle文件大小为{fileSizeMB:F2}MB，超过Steam的100MB限制！");
                        EditorUtility.DisplayDialog("警告", 
                            $"AssetBundle文件大小为{fileSizeMB:F2}MB，超过Steam的100MB限制！\n\n" +
                            "请减少模型复杂度或材质/纹理大小后重试。", "确定");
                    }

                    Debug.Log($"AssetBundle已生成：{finalBundlePath}");
                    Debug.Log($"文件大小：{fileSizeMB:F2}MB ({fs.Length} 字节)");
                    Debug.Log($"文件头：{header}");
                }

                // 删除不需要的文件
                string[] filesToDelete = {
                    Path.Combine(outputPath, "item_content.manifest"),
                    Path.Combine(outputPath, "SteamWorkshopContent"),
                    Path.Combine(outputPath, "SteamWorkshopContent.manifest")
                };

                foreach (string file in filesToDelete)
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                        Debug.Log($"已删除不需要的文件: {file}");
                    }
                }

                EditorUtility.DisplayDialog("成功", "资源包已生成", "确定");
            }
            else
            {
                throw new System.Exception($"资源包生成失败，文件不存在：{generatedBundle}");
            }

            // 清理临时文件
            if (File.Exists(prefabPath))
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }
            if (Directory.Exists(tempPrefabDir))
            {
                Directory.Delete(tempPrefabDir, true);
            }
            AssetDatabase.Refresh();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"生成资源包失败: {ex}");
            EditorUtility.DisplayDialog("错误", $"生成资源包失败: {ex.Message}", "确定");
        }
    }

    /// <summary>
    /// 检查资源大小并警告过大的资源
    /// </summary>
    private void CheckResourceSize(GameObject prefab)
    {
        // 检查模型
        MeshFilter[] meshFilters = prefab.GetComponentsInChildren<MeshFilter>(true);
        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh != null)
            {
                int vertexCount = meshFilter.sharedMesh.vertexCount;
                if (vertexCount > 20000)
                {
                    Debug.LogWarning($"高面数模型: {meshFilter.gameObject.name} - {vertexCount} 顶点");
                }
            }
        }

        // 检查纹理
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            if (renderer.sharedMaterials != null)
            {
                foreach (Material mat in renderer.sharedMaterials)
                {
                    if (mat != null)
                    {
                        // 检查常见纹理属性
                        string[] textureProps = new string[] { "_MainTex", "_BumpMap", "_EmissionMap", "_MetallicGlossMap", "_OcclusionMap" };
                        foreach (string prop in textureProps)
                        {
                            if (mat.HasProperty(prop))
                            {
                                Texture tex = mat.GetTexture(prop);
                                if (tex != null)
                                {
                                    // 获取纹理路径
                                    string path = AssetDatabase.GetAssetPath(tex);
                                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                                    if (importer != null)
                                    {
                                        // 检查未压缩的纹理
                                        if (importer.textureCompression == TextureImporterCompression.Uncompressed)
                                        {
                                            Debug.LogWarning($"未压缩纹理: {path} 在 {renderer.gameObject.name} 上");
                                        }

                                        // 检查超大分辨率
                                        if (tex is Texture2D tex2D)
                                        {
                                            if (tex2D.width > 2048 || tex2D.height > 2048)
                                            {
                                                Debug.LogWarning($"高分辨率纹理: {path} ({tex2D.width}x{tex2D.height}) 在 {renderer.gameObject.name} 上");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 优化资源以减小AssetBundle大小
    /// </summary>
    private void OptimizeResources()
    {
        if (uploadTarget == null) return;

        try
        {
            // 收集需要压缩的纹理
            Renderer[] renderers = uploadTarget.GetComponentsInChildren<Renderer>(true);
            var texturesToOptimize = new System.Collections.Generic.List<Texture>();

            foreach (Renderer renderer in renderers)
            {
                if (renderer.sharedMaterials != null)
                {
                    foreach (Material mat in renderer.sharedMaterials)
                    {
                        if (mat != null)
                        {
                            // 检查常见纹理属性
                            string[] textureProps = new string[] { "_MainTex", "_BumpMap", "_EmissionMap", "_MetallicGlossMap", "_OcclusionMap" };
                            foreach (string prop in textureProps)
                            {
                                if (mat.HasProperty(prop))
                                {
                                    Texture tex = mat.GetTexture(prop);
                                    if (tex != null && !texturesToOptimize.Contains(tex))
                                    {
                                        texturesToOptimize.Add(tex);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (texturesToOptimize.Count == 0)
            {
                EditorUtility.DisplayDialog("信息", "未找到可优化的纹理", "确定");
                return;
            }

            int optimizedCount = 0;

            // 压缩纹理
            foreach (Texture tex in texturesToOptimize)
            {
                string path = AssetDatabase.GetAssetPath(tex);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer != null)
                {
                    bool needsApply = false;

                    // 对超大分辨率的纹理进行缩小
                    if (tex is Texture2D tex2D && (tex2D.width > 2048 || tex2D.height > 2048))
                    {
                        importer.maxTextureSize = 2048;
                        needsApply = true;
                    }

                    // 设置压缩格式
                    if (importer.textureCompression != TextureImporterCompression.CompressedHQ)
                    {
                        importer.textureCompression = TextureImporterCompression.CompressedHQ;
                        needsApply = true;
                    }

                    // 优化mipmap设置
                    if (importer.mipmapEnabled)
                    {
                        // 使用更高效的mipmap设置
                        importer.mipmapEnabled = true;
                        importer.borderMipmap = false;
                        importer.mipmapFilter = TextureImporterMipFilter.BoxFilter;
                        needsApply = true;
                    }

                    if (needsApply)
                    {
                        importer.SaveAndReimport();
                        optimizedCount++;
                    }
                }
            }

            if (optimizedCount > 0)
            {
                EditorUtility.DisplayDialog("完成", $"已优化 {optimizedCount} 个纹理资源。\n\n请重新生成AssetBundle以应用优化。", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("完成", "已检查资源，但没有需要优化的内容。\n如果文件仍然过大，可能需要简化模型或手动降低资源质量。", "确定");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"优化资源失败: {ex}");
            EditorUtility.DisplayDialog("错误", $"优化资源失败: {ex.Message}", "确定");
        }
    }

    /// <summary>
    /// 生成所有必要文件
    /// </summary>
    private void GenerateAllFiles()
    {
        if (uploadTarget == null)
        {
            EditorUtility.DisplayDialog("错误", "请选择要上传的物体", "确定");
            return;
        }

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
        {
            EditorUtility.DisplayDialog("错误", "请填写标题和描述", "确定");
            return;
        }

        try
        {
            // 确保输出目录存在
            Directory.CreateDirectory(outputPath);

            // 生成预览图
            CapturePreview();

            // 生成资源包
            BuildAssetBundle();

            // 生成description.txt
            string descriptionPath = Path.Combine(outputPath, "description.txt");
            string descriptionContent = 
                $"Title: {title}\n" +
                $"Description: {description}\n" +
                $"Created: {System.DateTime.Now:yyyy/M/d HH:mm:ss}";
            File.WriteAllText(descriptionPath, descriptionContent);

            // 生成VDF文件
            string vdfPath = Path.Combine(outputPath, "workshop_item.vdf");
            string vdfContent = 
                "\"workshopitem\"\n" +
                "{\n" +
                "\t\"appid\"\t\t\"3584390\"\n" +
                $"\t\"title\"\t\t\"{title}\"\n" +
                $"\t\"description\"\t\t\"{description}\"\n" +
                "\t\"visibility\"\t\t\"0\"\n" +
                "\t\"changenote\"\t\t\"\"\n" +
                $"\t\"contentfolder\"\t\t\"{Path.GetFullPath(outputPath)}\"\n" +
                $"\t\"previewfile\"\t\t\"{Path.GetFullPath(Path.Combine(outputPath, "preview.jpg"))}\"\n" +
                "}\n";
            File.WriteAllText(vdfPath, vdfContent);

            // 生成上传说明
            string uploadGuidePath = Path.Combine(outputPath, "上传说明.txt");
            string uploadGuideContent = 
                "使用steamcmd上传创意工坊内容的步骤：\n\n" +
                "1. 打开命令提示符或PowerShell\n" +
                "2. 切换到steamcmd所在目录\n" +
                "3. 运行以下命令：\n\n" +
                "steamcmd +login 你的Steam用户名 +workshop_build_item \"" + 
                Path.GetFullPath(Path.Combine(outputPath, "workshop_item.vdf")) + 
                "\" +quit\n\n" +
                "注意：\n" +
                "- 首次使用可能需要输入Steam令牌验证码\n" +
                "- 上传成功后会显示物品ID\n" +
                "- 如需更新已有物品，请在vdf文件中添加publishedfileid字段";
            File.WriteAllText(uploadGuidePath, uploadGuideContent);

            // 清理多余文件
            string manifestPath = Path.Combine(outputPath, "SteamWorkshopContent.manifest");
            if (File.Exists(manifestPath))
            {
                File.Delete(manifestPath);
            }
            string extraBundlePath = Path.Combine(outputPath, "SteamWorkshopContent");
            if (File.Exists(extraBundlePath))
            {
                File.Delete(extraBundlePath);
            }

            EditorUtility.DisplayDialog("成功", "所有文件已生成完成", "确定");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"生成文件失败: {ex}");
            EditorUtility.DisplayDialog("错误", $"生成文件失败: {ex.Message}", "确定");
        }
    }
} 