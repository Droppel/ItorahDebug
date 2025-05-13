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

        bool customDebugMenuOpen = false;
        bool showDebugInfo = true;

        DebugMenu dMenu;
        FieldInfo dMenuOpenVar;
        FieldInfo dNormalTimeScaleVar;

        GameObject itorah;
        SkillSet playerSkillSet;

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

            GetItorahReference();
        }

        private void GetItorahReference() {
            if (itorah == null) {
                itorah = GameObject.Find("itorah");
                playerSkillSet = itorah.GetComponent<SkillSet>();
            }
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

            if (Input.GetKeyDown(KeyCode.Alpha8)) {

                GetItorahReference();
                this.customDebugMenuOpen = !this.customDebugMenuOpen;
                Cursor.visible = this.customDebugMenuOpen;
            }
        }

        private void OnGUI() {
            if (customDebugMenuOpen) {
                DrawCustomDebugMenu();
            }

            if (showDebugInfo) {
                DrawDebugInfo();
            }
        }

        private void DrawDebugInfo() {
            if (showDebugInfo) {
                int xPosition = Screen.width - 210;
                int currentY = Screen.height - 90;
                int yIncrement = 22;
                GUI.Label(new Rect(10, currentY, 200, 20), "Debug Info");
                currentY += yIncrement;
                GUI.Label(new Rect(10, currentY, 200, 20), $"Position: {itorah.transform.position}");
                currentY += yIncrement;
                GUI.Label(new Rect(10, currentY, 200, 20), $"Velocity: {itorah.GetComponent<Rigidbody2D>().velocity}");
                currentY += yIncrement;
            }
        }

        private void DrawCustomDebugMenu() {
            int xPosition = Screen.width - 210;
            int currentY = 10;
            int yIncrement = 22;
            GUI.depth = int.MaxValue;
            GUI.Label(new Rect(xPosition, currentY, 200, 20), "Custom Debug Menu");
            currentY += yIncrement;
            if (GUI.Button(new Rect(xPosition, currentY, 200, 20), "Store position (F1)")) {
                storedPos = itorah.transform.position;
            }
            currentY += yIncrement;
            if (GUI.Button(new Rect(xPosition, currentY, 200, 20), "Teleport to stored position (F2)")) {
                itorah.transform.position = storedPos;
            }
            currentY += yIncrement;
            if (GUI.Button(new Rect(xPosition, currentY, 200, 20), "Restore health")) {
                itorah.GetComponent<LifePoints>().CurrentPoints = itorah.GetComponent<LifePoints>().Maximum;
            }
            currentY += yIncrement;
            if (GUI.Button(new Rect(xPosition, currentY, 200, 20), "Toggle Hitbox Renderer")) {
                if (hitBoxRenderer == null) {
                    hitBoxRenderer = new GameObject();
                    hitBoxRenderer.AddComponent<HitboxRender>();
                } else {
                    DestroyImmediate(hitBoxRenderer);
                    hitBoxRenderer = null;
                }
            }
            currentY += yIncrement;

            // Toggle DebugInfo
            if (GUI.Toggle(new Rect(xPosition, currentY, 90, 20), showDebugInfo, "Debug Info")) {
                showDebugInfo = true;
            } else {
                showDebugInfo = false;
            }
            currentY += yIncrement;
            
            // Skill Toggles
            if (GUI.Toggle(new Rect(xPosition, currentY, 90, 20), playerSkillSet.skills[0].learned, "WallJump")) {
                playerSkillSet.skills[0].learned = true;
            } else {
                playerSkillSet.skills[0].learned = false;
            }
            if (GUI.Toggle(new Rect(xPosition+100, currentY, 90, 20), playerSkillSet.skills[1].learned, "Stomp")) {
                playerSkillSet.skills[1].learned = true;
            } else {
                playerSkillSet.skills[1].learned = false;
            }
            currentY += yIncrement;
            if (GUI.Toggle(new Rect(xPosition, currentY, 90, 20), playerSkillSet.skills[2].learned, "DoubleJump")) {
                playerSkillSet.skills[2].learned = true;
            } else {
                playerSkillSet.skills[2].learned = false;
            }
            if (GUI.Toggle(new Rect(xPosition+100, currentY, 90, 20), playerSkillSet.skills[3].learned, "UpperCut")) {
                playerSkillSet.skills[3].learned = true;
            } else {
                playerSkillSet.skills[3].learned = false;
            }
            currentY += yIncrement;
            if (GUI.Toggle(new Rect(xPosition, currentY, 90, 20), playerSkillSet.skills[4].learned, "Dash")) {
                playerSkillSet.skills[4].learned = true;
            } else {
                playerSkillSet.skills[4].learned = false;
            }
            if (GUI.Toggle(new Rect(xPosition+100, currentY, 90, 20), playerSkillSet.skills[5].learned, "Heal")) {
                playerSkillSet.skills[5].learned = true;
            } else {
                playerSkillSet.skills[5].learned = false;
            }
            currentY += yIncrement;
            if (GUI.Toggle(new Rect(xPosition, currentY, 90, 20), playerSkillSet.skills[6].learned, "Throw")) {
                playerSkillSet.skills[6].learned = true;
            } else {
                playerSkillSet.skills[6].learned = false;
            }
            if (GUI.Toggle(new Rect(xPosition+100, currentY, 90, 20), playerSkillSet.skills[7].learned, "ChargeAttack")) {
                playerSkillSet.skills[7].learned = true;
            } else {
                playerSkillSet.skills[7].learned = false;
            }
        }
    }
}
