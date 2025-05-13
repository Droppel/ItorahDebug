using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using GrimbartTales;
using System.Reflection;
using GrimbartTales.Platformer2D.CharacterController;
using ItorahDebug.Hitbox;
using GrimbartTales.Platformer2D.DamageSystem;

namespace ItorahDebug {

    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin {
        internal static new ManualLogSource Logger;

        DebugMenu dMenu;
        FieldInfo dMenuOpenVar;
        FieldInfo dNormalTimeScaleVar;

        GameObject itorah;

        Vector3 storedPos;

        GameObject hitBoxRenderer = null;

        private void Awake() {
            // Plugin startup logic
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            dMenu = Resources.FindObjectsOfTypeAll<DebugMenu>()[0];
            dMenu.safetyDisableForReleaseBuild = false;

            dMenu.gameObject.SetActive(true);

            dMenuOpenVar = typeof(DebugMenu).GetField("debugMenuOpen", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            dNormalTimeScaleVar = typeof(DebugMenu).GetField("normalTimescale", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);

            itorah = GameObject.Find("itorah");
        }

        private void Update() {

            if ((bool)dMenuOpenVar.GetValue(dMenu)) {
                if (Input.GetKeyDown(KeyCode.Alpha3)) {
                    dNormalTimeScaleVar.SetValue(dMenu, 0.5f);
                }
            }

            if (Input.GetKeyDown(KeyCode.F1)) {
                storedPos = itorah.transform.position;
                Logger.LogInfo($"Stored position: {storedPos}");
            }

            if (Input.GetKeyDown(KeyCode.F2)) {
                itorah.transform.position = storedPos;
            }

            if (Input.GetKeyDown(KeyCode.F5)) {
                if (hitBoxRenderer == null) {
                    hitBoxRenderer = new GameObject();
                    hitBoxRenderer.AddComponent<HitboxRender>();
                } else {
                    DestroyImmediate(hitBoxRenderer);
                    hitBoxRenderer = null;
                }
            }
            if (Input.GetKeyDown(KeyCode.F3)) {
                itorah.GetComponent<LifePoints>().CurrentPoints = itorah.GetComponent<LifePoints>().Maximum;
            }
        }
    }
}
