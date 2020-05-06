using System.Reflection;
using BepInEx;
using UnityEngine;

namespace RForRotate
{
    [BepInPlugin("org.d20armyknife.plugins.rforrotate", "Press R to Rotate Plug-In", "1.0.0.0")]
    [BepInProcess("TaleSpire.exe")]
    public class RForRotatePlugin: BaseUnityPlugin
    {

        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            UnityEngine.Debug.Log("R For Rotate Plug-in loaded");
        }
        
        private void RotateSelected()
        {
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
            if (SingletonBehaviour<BoardToolManager>.HasInstance && (SingletonBehaviour<BoardToolManager>.Instance.IsCurrentTool<SingleBuilderBoardTool>())) {
                var btm = (SingleBuilderBoardTool)SingletonBehaviour<SingleBuilderBoardTool>.Instance;
                var angle = (float)btm.GetType().GetField("_angle", flags).GetValue(btm);
                angle = (float)(((double)angle + 90.0) % 360.0);
                // After adjusting don't forget to push it back to TaleSpire so they can keep track of it.
                btm.GetType().GetField("_angle", flags).SetValue(btm, angle);
                var selectedAsset = (TilePreviewBoardAsset)btm.GetType().GetField("_selectedTileBoardAsset", flags).GetValue(btm);
                selectedAsset.Rotate(angle);
            }
        }
       

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.R))
            {
                try
                {
                    RotateSelected();
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
        }

    }
}
