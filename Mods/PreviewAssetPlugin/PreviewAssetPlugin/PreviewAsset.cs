using BepInEx;
using UnityEngine;
using System.Reflection;
using UnityEngine.Networking;
using Dummiesman;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using ModdingTales;

namespace PreviewAssetPlugin
{
    [BepInPlugin("org.d20armyknife.plugins.previewasset", "Preview Asset Plugin", "1.0.0.0")]
    [BepInProcess("TaleSpire.exe")]
    class PreviewAssetPlugin : BaseUnityPlugin
    {
        //private ModdingUtils mu = new ModdingUtils();
        void Awake()
        {
            UnityEngine.Debug.Log("SetInjectionFlag Plug-in loaded");
            ModdingUtils.Initialize(this);
        }

        public static Texture2D LoadPNG(string filePath)
        {

            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                UnityEngine.Debug.Log("tex len: " + fileData.Length);
                tex = new Texture2D(1, 1);
                tex.LoadImage(fileData);
                //bool result = ImageConversion.LoadImage(tex, fileData);
                tex.Apply();
                UnityEngine.Debug.Log(" Tex: " + tex.width + " : " + tex.height);
            }
            return tex;
        }
        private void AssetFlip()
        {
            var origCursor = UnityEngine.Cursor.visible;
            try
            {
                var test = (SingleBuilderBoardTool)SingletonBehaviour<SingleBuilderBoardTool>.Instance;
            }
            catch
            {
                // pass
            }
            try
            {
                UnityEngine.Cursor.visible = true;
                OpenFileName ofn = new OpenFileName();
                ofn.structSize = Marshal.SizeOf(ofn);
                ofn.filter = "Obj Files\0*.obj\0\0";
                ofn.file = new string(new char[256]);
                ofn.maxFile = ofn.file.Length;
                ofn.fileTitle = new string(new char[64]);
                ofn.maxFileTitle = ofn.fileTitle.Length;
                ofn.initialDir = UnityEngine.Application.dataPath;
                ofn.title = "Select OBJ File";
                ofn.defExt = "OBJ";
                ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;//OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR
                if (DllTest.GetOpenFileName(ofn))
                {
                    var catPath = ofn.file;
                    // We have to first grab the CameraController._targetZoomLerpValue to disable the movement when our rotate is happening. This is the only way to prevent zooming while we are rotating
                    var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
                    if (SingletonBehaviour<BoardToolManager>.HasInstance && (SingletonBehaviour<BoardToolManager>.Instance.IsCurrentTool<SingleBuilderBoardTool>()))
                    {
                        var btm = (SingleBuilderBoardTool)SingletonBehaviour<SingleBuilderBoardTool>.Instance;
                        TilePreviewBoardAsset selectedAsset = (TilePreviewBoardAsset)btm.GetType().GetField("_selectedTileBoardAsset", flags).GetValue(btm);
                        UnityEngine.Debug.Log("Selected Asset: " + selectedAsset);

                        AssetLoader[] assetLoaders = (AssetLoader[])selectedAsset.GetType().GetField("_assetLoaders", flags).GetValue(selectedAsset);
                        UnityEngine.Debug.Log("Asset Loaders: " + assetLoaders);
                        Mesh newMesh = FastObjImporter.Instance.ImportFile(catPath);
                        for (int i = 0; i < assetLoaders.Length; i++)
                        {
                            UnityEngine.Debug.Log("Asset Loader: " + assetLoaders[i].Guid + " : " + assetLoaders[i].assetName + " : " + assetLoaders[i].assetBundlePath);
                            //var w = new WWW("file://Users/Paulo/Desktop/img_2013_008_original.jpg");
                            UnityEngine.Debug.Log("New Mesh: " + newMesh.name + " : " + newMesh.vertices.Length);
                            //Mesh holderMesh = new Mesh();
                            //ObjImporter objImport = new ObjImporter();
                            //holderMesh = objImport.ImportFile("E:\\Projects\\Talespirehax\\TestImport\\12222_Cat_v1_l3.obj");


                            //Material mat = new Material(Shader.Find("Standard"));
                            GameObject loadedAsset = (GameObject)assetLoaders[i].GetType().GetField("_loadedAsset", flags).GetValue(assetLoaders[i]);
                            //var newMesh = (new OBJLoader().Load(catPath)).GetComponent<MeshFilter>().mesh;
                            UnityEngine.Debug.Log("New Mesh: " + newMesh.name + " : " + newMesh.vertices.Length);
                            loadedAsset.GetComponent<MeshFilter>().mesh = newMesh;

                        }
                        var catMtl = Path.ChangeExtension(ofn.file, ".mtl");
                        // This is silly, it replaces every single mesh with the new mesh.
                        //MeshFilter[] mfs = FindObjectsOfType<MeshFilter>();
                        //for (int i = 0; i < mfs.Length; i++)
                        //{
                        //    try
                        //    {
                        //        mfs[i].mesh = newMesh;
                        //    }
                        //    catch
                        //    {
                        //        // 
                        //    }
                        //}
                        for (int i = 0; i < selectedAsset.Renderers.Length; i++)
                        {
                            try
                            {
                                UnityEngine.Debug.Log("Renderer Name: " + selectedAsset.Renderers[i].name);

                                //Color[] colors = selectedAsset.Renderers[i].material
                                //Dictionary<string, Material> materials = new MTLLoader().Load(catMtl, selectedAsset.Renderers[i].material);
                                //foreach (string key in materials.Keys)
                                //{
                                //    UnityEngine.Debug.Log("Mat: " + key);
                                //}
                                //selectedAsset.Renderers[i].material = materials.GetEnumerator().Current.Value;
                                var catTexturePath = "E:/Projects/Talespirehax/TestImport/Cat_diffuse.jpg";
                                //selectedAsset.Renderers[i].material.doubleSidedGI = true;
                                //Texture2D catTexture = new MTLLoader().TryLoadTexture(catTexturePath);
                                //Texture2D tex = new Texture2D(16, 16, TextureFormat.PVRTC_RGBA4, false);
                                //byte[] pvrtcBytes = new byte[]
                                //{
                                //    0x30, 0x32, 0x32, 0x32, 0xe7, 0x30, 0xaa, 0x7f, 0x32, 0x32, 0x32, 0x32, 0xf9, 0x40, 0xbc, 0x7f,
                                //    0x03, 0x03, 0x03, 0x03, 0xf6, 0x30, 0x02, 0x05, 0x03, 0x03, 0x03, 0x03, 0xf4, 0x30, 0x03, 0x06,
                                //    0x32, 0x32, 0x32, 0x32, 0xf7, 0x40, 0xaa, 0x7f, 0x32, 0xf2, 0x02, 0xa8, 0xe7, 0x30, 0xff, 0xff,
                                //    0x03, 0x03, 0x03, 0xff, 0xe6, 0x40, 0x00, 0x0f, 0x00, 0xff, 0x00, 0xaa, 0xe9, 0x40, 0x9f, 0xff,
                                //    0x5b, 0x03, 0x03, 0x03, 0xca, 0x6a, 0x0f, 0x30, 0x03, 0x03, 0x03, 0xff, 0xca, 0x68, 0x0f, 0x30,
                                //    0xaa, 0x94, 0x90, 0x40, 0xba, 0x5b, 0xaf, 0x68, 0x40, 0x00, 0x00, 0xff, 0xca, 0x58, 0x0f, 0x20,
                                //    0x00, 0x00, 0x00, 0xff, 0xe6, 0x40, 0x01, 0x2c, 0x00, 0xff, 0x00, 0xaa, 0xdb, 0x41, 0xff, 0xff,
                                //    0x00, 0x00, 0x00, 0xff, 0xe8, 0x40, 0x01, 0x1c, 0x00, 0xff, 0x00, 0xaa, 0xbb, 0x40, 0xff, 0xff,
                                //};
                                //// Load data into the texture and upload it to the GPU.
                                //tex.LoadRawTextureData(pvrtcBytes);
                                //tex.Apply();

                                UnityEngine.Debug.Log("Shader: " + selectedAsset.Renderers[i].material.shader.name);

                                Material newMat = new Material(Shader.Find("Standard"));
                                newMat.CopyPropertiesFromMaterial(selectedAsset.Renderers[i].material);
                                selectedAsset.Renderers[i].material = newMat;
                                

                                Texture2D catTexture = LoadPNG(catTexturePath);
                                selectedAsset.Renderers[i].material.mainTexture = catTexture;

                                var catBumpTexturePath = "E:/Projects/Talespirehax/TestImport/Cat_bump.jpg";
                                Texture2D catBumpTexture = LoadPNG(catBumpTexturePath);
                                //Texture2D catBumpTexture = new MTLLoader().TryLoadTexture(catBumpTexturePath);
                                selectedAsset.Renderers[i].material.SetTexture("_BumpMap", catBumpTexture);
                                //Color cl = selectedAsset.Renderers[i].material.color;
                                
                                //float scale = selectedAsset.Renderers[i].material.shader = 
                                //UnityEngine.Debug.Log("Bump Scale: " + scale);
                                //selectedAsset.Renderers[i].material.SetFloat("_BumpScale", 0f);

                                //selectedAsset.Renderers[i].material.SetColor("_BumpMap", new Color(cl.r, cl.g, cl.b, 0.5f));// .a = 0;
                            }
                            catch
                            {
                                // 
                            }
                            // After adjusting don't forget to push it back to TaleSpire so they can keep track of it.
                            // btm.GetType().GetField("_angle", flags).SetValue(btm, angle);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.Log("Crash in Asset Preview Plugin");
                UnityEngine.Debug.Log(ex.Message);
                UnityEngine.Debug.Log(ex.StackTrace);
                UnityEngine.Debug.Log(ex.InnerException);
                UnityEngine.Debug.Log(ex.Source);
            }
            UnityEngine.Cursor.visible = origCursor;
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.U))
            {
                AssetFlip();
            }
            if (Input.GetKeyUp(KeyCode.T))
            {
                TextMeshProUGUI[] texts = FindObjectsOfType<TextMeshProUGUI>();
                UnityEngine.Debug.Log("Texts found: " + texts.Length);
                for (int i = 0; i < texts.Length; i++)
                {
                    if (texts[i].name == "BETA")
                    {
                        texts[i].text = "CAT BUILD - beta";
                    }
                    UnityEngine.Debug.Log("Name: " + texts[i].name + "Text: " + texts[i].text);
                }
            }
        }
    }
}
