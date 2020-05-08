using System.Reflection;
using BepInEx;
using UnityEngine;

namespace RForRotate
{
    [BepInPlugin("org.d20armyknife.plugins.rforrotate", "Press R to Rotate Plug-In", "1.1.0.0")]
    [BepInProcess("TaleSpire.exe")]
    public class RForRotatePlugin: BaseUnityPlugin
    {

        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            UnityEngine.Debug.Log("R For Rotate Plug-in loaded");
            // Set the UsingCodeInjection so we don't anger the @Baggers
            AppStateManager.UsingCodeInjection = true;
        }
        
        private void RotateSelected(double amount)
        {
            try
            {
                // We have to first grab the CameraController._targetZoomLerpValue to disable the movement when our rotate is happening. This is the only way to prevent zooming while we are rotating
                var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
                if (SingletonBehaviour<BoardToolManager>.HasInstance && (SingletonBehaviour<BoardToolManager>.Instance.IsCurrentTool<SingleBuilderBoardTool>()))
                {
                    var btm = (SingleBuilderBoardTool)SingletonBehaviour<SingleBuilderBoardTool>.Instance;
                    float angle = (float)btm.GetType().GetField("_angle", flags).GetValue(btm);
                    angle = (float)(((double)angle + amount) % 360.0);
                    
                    // After adjusting don't forget to push it back to TaleSpire so they can keep track of it.
                    btm.GetType().GetField("_angle", flags).SetValue(btm, angle);
                    var selectedAsset = (TilePreviewBoardAsset)btm.GetType().GetField("_selectedTileBoardAsset", flags).GetValue(btm);
                    selectedAsset.Rotate(angle);

                    // Next we need to counter the heightPlaneOffset that is set in the CallUpdate when left shift is pressed
                    float heightPlaneOffset = (float)btm.GetType().GetField("heightPlaneOffset", flags).GetValue(btm);
                    heightPlaneOffset -= (float)((double)Input.mouseScrollDelta.y * (double)Time.deltaTime * 8.0);
                    btm.GetType().GetField("heightPlaneOffset", flags).SetValue(btm, heightPlaneOffset);
                }
                if (SingletonBehaviour<BoardToolManager>.HasInstance && (SingletonBehaviour<BoardToolManager>.Instance.IsCurrentTool<SlabBuilderBoardTool>()))
                {
                    var sbbt = (SlabBuilderBoardTool)SingletonBehaviour<SlabBuilderBoardTool>.Instance;
                    Slab slab = (Slab)sbbt.GetType().GetField("_slab", flags).GetValue(sbbt);
                    slab.Rotate90();
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.Log("Crash in r for rotate Plugin");
                UnityEngine.Debug.Log(ex.Message);
                UnityEngine.Debug.Log(ex.StackTrace);
                UnityEngine.Debug.Log(ex.InnerException);
                UnityEngine.Debug.Log(ex.Source);
            }
        }


        void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                    RotateSelected(90.0);
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                RotateSelected(-90.0);
            }
            if (Input.GetKeyUp(KeyCode.R))
            {
                RotateSelected(90.0);
            }
        }

    }
}
