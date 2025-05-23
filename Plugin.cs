using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using GrimbartTales;
using System.Reflection;
using GrimbartTales.Platformer2D.CharacterController;
using ItorahDebug.Hitbox;
using GrimbartTales.Platformer2D.DamageSystem;
using System.Collections.Generic;

namespace ItorahDebug {

    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin {
        internal static new ManualLogSource Logger;

        bool customDebugMenuOpen = false;
        bool showDebugInfo = true;
        bool showHitbox = false;
        bool showHitboxTerrain = false;

        DebugMenu dMenu;
        FieldInfo dMenuOpenVar;
        FieldInfo dNormalTimeScaleVar;

        GameObject itorah;
        SkillSet playerSkillSet;

        Vector3 storedPos;

        GameObject hitBoxRenderer = null;
        GameObject hitBoxTerrainRenderer = null;

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

            dropdownOptions = new List<string>();
            foreach (var story in dMenu.StoryProgressDropdown.options) {
                dropdownOptions.Add(story.text);
            }
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

        private void OnDestroy() {
            if (hitBoxRenderer != null) {
                DestroyImmediate(hitBoxRenderer);
                hitBoxRenderer = null;
                DestroyImmediate(hitBoxTerrainRenderer);
                hitBoxTerrainRenderer = null;
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

        private bool showDropdown = false;
        private int selectedDropdownIndex = 0;
        private List<string> dropdownOptions;

        private Vector2 dropdownScrollPosition = Vector2.zero; // Scroll position for the dropdown
        private int maxDropdownHeight = 200; // Maximum height of the dropdown

        private int maxWidth = 400; // Maximum width
        private void DrawCustomDebugMenu() {
            int xPosition = Screen.width - maxWidth - 10;
            int currentY = 10;
            int yIncrement = 22;
            GUI.depth = int.MaxValue;
            GUI.Label(new Rect(xPosition, currentY, 200, 20), "Custom Debug Menu");
            currentY += yIncrement;
            if (GUI.Button(new Rect(xPosition, currentY, maxWidth, 20), "Store position (F1)")) {
                storedPos = itorah.transform.position;
            }
            currentY += yIncrement;
            if (GUI.Button(new Rect(xPosition, currentY, maxWidth, 20), "Teleport to stored position (F2)")) {
                itorah.transform.position = storedPos;
            }
            currentY += yIncrement;
            if (GUI.Button(new Rect(xPosition, currentY, maxWidth, 20), "Restore health")) {
                itorah.GetComponent<LifePoints>().CurrentPoints = itorah.GetComponent<LifePoints>().Maximum;
            }
            currentY += yIncrement;
            if (GUI.Toggle(new Rect(xPosition, currentY, maxWidth, 20), showHitbox, "Hitboxes")) {
                showHitbox = true;
                if (hitBoxRenderer == null) {
                    hitBoxRenderer = new GameObject("HitboxRenderer");
                    hitBoxRenderer.AddComponent<HitboxRender>();
                }
            } else {
                showHitbox = false;
                if (hitBoxRenderer != null) {
                    DestroyImmediate(hitBoxRenderer);
                    hitBoxRenderer = null;
                }
            }
            currentY += yIncrement;
            if (GUI.Toggle(new Rect(xPosition, currentY, maxWidth, 20), showHitboxTerrain, "Terrain Hitboxes")) {
                showHitboxTerrain = true;
                if (hitBoxTerrainRenderer == null) {
                    hitBoxTerrainRenderer = new GameObject("HitboxRendererTerrain");
                    hitBoxTerrainRenderer.AddComponent<HitboxTerrainRender>();
                }
            } else {
                showHitboxTerrain = false;
                if (hitBoxTerrainRenderer != null) {
                    DestroyImmediate(hitBoxTerrainRenderer);
                    hitBoxTerrainRenderer = null;
                }
            }
            currentY += yIncrement;

            // Toggle DebugInfo
            if (GUI.Toggle(new Rect(xPosition, currentY, maxWidth, 20), showDebugInfo, "Debug Info")) {
                showDebugInfo = true;
            } else {
                showDebugInfo = false;
            }
            currentY += yIncrement;
            
            // Skill Toggles
            if (GUI.Toggle(new Rect(xPosition, currentY, maxWidth/2, 20), playerSkillSet.skills[0].learned, "WallJump")) {
                playerSkillSet.skills[0].learned = true;
            } else {
                playerSkillSet.skills[0].learned = false;
            }
            if (GUI.Toggle(new Rect(xPosition+maxWidth/2, currentY, maxWidth/2, 20), playerSkillSet.skills[1].learned, "Stomp")) {
                playerSkillSet.skills[1].learned = true;
            } else {
                playerSkillSet.skills[1].learned = false;
            }
            currentY += yIncrement;
            if (GUI.Toggle(new Rect(xPosition, currentY, maxWidth/2, 20), playerSkillSet.skills[2].learned, "DoubleJump")) {
                playerSkillSet.skills[2].learned = true;
            } else {
                playerSkillSet.skills[2].learned = false;
            }
            if (GUI.Toggle(new Rect(xPosition+maxWidth/2, currentY, maxWidth/2, 20), playerSkillSet.skills[3].learned, "UpperCut")) {
                playerSkillSet.skills[3].learned = true;
            } else {
                playerSkillSet.skills[3].learned = false;
            }
            currentY += yIncrement;
            if (GUI.Toggle(new Rect(xPosition, currentY, maxWidth/2, 20), playerSkillSet.skills[4].learned, "Dash")) {
                playerSkillSet.skills[4].learned = true;
            } else {
                playerSkillSet.skills[4].learned = false;
            }
            if (GUI.Toggle(new Rect(xPosition+maxWidth/2, currentY, maxWidth/2, 20), playerSkillSet.skills[5].learned, "Heal")) {
                playerSkillSet.skills[5].learned = true;
            } else {
                playerSkillSet.skills[5].learned = false;
            }
            currentY += yIncrement;
            if (GUI.Toggle(new Rect(xPosition, currentY, maxWidth/2, 20), playerSkillSet.skills[6].learned, "Throw")) {
                playerSkillSet.skills[6].learned = true;
            } else {
                playerSkillSet.skills[6].learned = false;
            }
            if (GUI.Toggle(new Rect(xPosition+maxWidth/2, currentY, maxWidth/2, 20), playerSkillSet.skills[7].learned, "ChargeAttack")) {
                playerSkillSet.skills[7].learned = true;
            } else {
                playerSkillSet.skills[7].learned = false;
            }
            currentY += yIncrement;

            // Story Progression
            GUI.Label(new Rect(xPosition, currentY, maxWidth, 20), "Story Progression:");
            currentY += yIncrement;

            selectedDropdownIndex = dMenu.StoryProgressDropdown.value;
            if (GUI.Button(new Rect(xPosition, currentY, maxWidth, 20), dropdownOptions[selectedDropdownIndex])) {
                showDropdown = !showDropdown;
            }
            if (showDropdown) {
                
                int totalDropdownHeight = dropdownOptions.Count * 20;
                Rect scrollViewRect = new Rect(xPosition, currentY + 20, maxWidth, Mathf.Min(maxDropdownHeight, totalDropdownHeight));
                Rect contentRect = new Rect(0, 0, maxWidth-20, totalDropdownHeight);

                dropdownScrollPosition = GUI.BeginScrollView(scrollViewRect, dropdownScrollPosition, contentRect);

                for (int i = 0; i < dropdownOptions.Count; i++) {
                    if (GUI.Button(new Rect(0, i * 20, maxWidth-20, 20), dropdownOptions[i])) {
                        selectedDropdownIndex = i;
                        showDropdown = false;
                        dMenu.StoryProgressDropdown.value = i;
                        dMenu.SetSelectedStoryProgress();   
                    }
                }

                GUI.EndScrollView();
            }
            currentY += yIncrement + (showDropdown ? Mathf.Min(maxDropdownHeight, dropdownOptions.Count * 20) : 0);
        }
    }
}
