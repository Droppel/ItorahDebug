using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using System.Reflection;
using GrimbartTales.Platformer2D.CharacterController;
using ItorahDebug.Hitbox;
using GrimbartTales.Platformer2D.DamageSystem;
using System.Collections.Generic;
using GrimbartTales.Platformer2D;
using GrimbartTales.Base.SaveSystem;
using System.IO;
using GrimbartTales.Platformer2D.Level;
using HarmonyLib;
using System;
using UnityEngine.EventSystems;

namespace ItorahDebug {

    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin {
        Harmony harmony;
        internal static new ManualLogSource Logger;

        bool customDebugMenuOpen = false;
        bool showDebugInfo = true;
        float highestY;
        float highestYVel;
        bool showHealthbars = false;
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

        string lastLoadedSave = "";

        private void Awake() {
            // Plugin startup logic
            Logger = base.Logger;
            harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        public void InitMod() {
            dMenu = Resources.FindObjectsOfTypeAll<DebugMenu>()[0];
            dMenu.safetyDisableForReleaseBuild = false;

            dMenu.gameObject.SetActive(true);

            dMenuOpenVar = typeof(DebugMenu).GetField("debugMenuOpen", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            dNormalTimeScaleVar = typeof(DebugMenu).GetField("normalTimescale", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);

            GetItorahReference();

            dropdownOptionsStorProg = new List<string>();
            foreach (var story in dMenu.StoryProgressDropdown.options) {
                dropdownOptionsStorProg.Add(story.text);
            }
            dropdownOptionsCheckpoint = new List<string>();
            foreach (var checkpoint in dMenu.BonfireDropdown.options) {
                dropdownOptionsCheckpoint.Add(checkpoint.text);
            }
            dropdownOptionsSaves = new List<string>();
            string saveLocation = Path.Combine(Application.persistentDataPath, "Itorah", "gamestates", "customSaves");
            if (Directory.Exists(saveLocation)) {
                string[] saveFiles = Directory.GetFiles(saveLocation, "*.sav");
                foreach (string saveFile in saveFiles) {
                    string fileName = Path.GetFileNameWithoutExtension(saveFile);
                    dropdownOptionsSaves.Add(fileName);
                }
            } else {
                Directory.CreateDirectory(saveLocation);
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

            if (Input.GetKeyDown(KeyCode.F3)) {
                if (!string.IsNullOrEmpty(lastLoadedSave)) {
                    LoadGame(lastLoadedSave);
                } else {
                    Logger.LogWarning("No last loaded save found.");
                }
            }

            if (Input.GetKeyDown(KeyCode.F4)) {
                RespawnPoint.lastActivatedRespawnPoint = null;
            }

            if (Input.GetKeyDown(KeyCode.Alpha8)) {
                GetItorahReference();
                this.customDebugMenuOpen = !this.customDebugMenuOpen;
                Cursor.visible = this.customDebugMenuOpen;
            }

            highestY = Mathf.Max(highestY, itorah.transform.position.y);
            highestYVel = Mathf.Max(highestYVel, itorah.GetComponent<Rigidbody2D>().velocity.y);
        }

        private void OnDestroy() {
            harmony.UnpatchSelf();

            if (hitBoxRenderer != null) {
                DestroyImmediate(hitBoxRenderer);
                hitBoxRenderer = null;
            }

            if (hitBoxTerrainRenderer != null) {
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

            if (showHealthbars) {
                DrawHealthBars();
            }
        }

        private void SaveGame(string saveName) {
            if (itorah == null) {
                GetItorahReference();
            }

            SaveSystem saveSystem = itorah.GetComponent<ItorahSessionData>().SaveSystem;
            string saveLocation = Path.Combine(Application.persistentDataPath, "Itorah", "gamestates", "customSaves");
            if (!Directory.Exists(saveLocation)) {
                Directory.CreateDirectory(saveLocation);
            }

            Checkpoint[] checkpoints = GameObject.FindObjectsOfType<Checkpoint>();
            if (checkpoints.Length == 0) {
                Logger.LogWarning("No checkpoints found in the scene. Saving without checkpoints.");
            } else {
                Checkpoint.lastActivatedCheckpoint = checkpoints[0].Data;
            }
            Checkpoint.lastActivatedCheckpoint.position = itorah.transform.position;

            saveSystem.SaveCurrentSession("customSaves/" + saveName);
            Logger.LogInfo($"Saved game as {saveName}");
        }

        private void LoadGame(string saveName) {
            SaveSystem saveSystem = itorah.GetComponent<ItorahSessionData>().SaveSystem;
            saveSystem.LoadFromSaveGame(saveName);
            Logger.LogInfo($"Loaded save game: {saveName}");
        }

        private void DrawHealthBars() {
            // Create a black background texture
            Texture2D redTexture = new Texture2D(1, 1);
            redTexture.SetPixel(0, 0, Color.red);
            redTexture.Apply();
            Texture2D greenTexture = new Texture2D(1, 1);
            greenTexture.SetPixel(0, 0, Color.green);
            greenTexture.Apply();

            // Create a GUIStyle with the backgrounds
            GUIStyle redBackgroundStyle = new GUIStyle();
            redBackgroundStyle.normal.background = redTexture;
            GUIStyle greenBackgroundStyle = new GUIStyle();
            greenBackgroundStyle.normal.background = greenTexture;


            LifePoints[] lifePoints = GameObject.FindObjectsOfType<LifePoints>();
            for (int i = 0; i < lifePoints.Length; i++) {
                if (!lifePoints[i].isActiveAndEnabled) {
                    return;
                }

                Transform transf = lifePoints[i].transform;

                // Calculate the position and size of the health bar
                string healthText = $"{lifePoints[i].CurrentPoints}/{lifePoints[i].Maximum}";
                Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(healthText));
                Vector3 screenPos = Camera.main.WorldToScreenPoint(transf.position);
                float xText = screenPos.x - textSize.x / 2; // Center the health bar
                float yText = Screen.height - screenPos.y;

                float barSize = 100; // Width of the health bar
                float xBar = screenPos.x - barSize / 2;
                float yBar = Screen.height - screenPos.y;

                float healthPercentage = (float)lifePoints[i].CurrentPoints / lifePoints[i].Maximum;
                float healthBarWidth = barSize * healthPercentage;
                GUI.Box(new Rect(xBar, yBar, healthBarWidth, textSize.y), GUIContent.none, greenBackgroundStyle);
                // Draw the red background box
                GUI.Box(new Rect(xBar + healthBarWidth, yBar, barSize - healthBarWidth, textSize.y), GUIContent.none, redBackgroundStyle);

                GUIStyle blackTextStyle = new GUIStyle(GUI.skin.label);
                blackTextStyle.normal.textColor = Color.black;
                GUI.Label(new Rect(xText, yText, textSize.x, textSize.y), healthText, blackTextStyle);
            }
        }

        private void DrawDebugInfo() {
            if (showDebugInfo) {
                int xPosition = Screen.width - 210;
                int currentY = Screen.height - 150;
                int yIncrement = 22;
                GUI.Label(new Rect(10, currentY, 200, 20), "Debug Info");
                currentY += yIncrement;
                GUI.Label(new Rect(10, currentY, 200, 20), $"Position: {itorah.transform.position}");
                currentY += yIncrement;
                GUI.Label(new Rect(10, currentY, 200, 20), $"Velocity: {itorah.GetComponent<Rigidbody2D>().velocity}");
                currentY += yIncrement;
                GUI.Label(new Rect(10, currentY, 200, 20), $"Highest Y: {highestY}");
                currentY += yIncrement;
                GUI.Label(new Rect(10, currentY, 200, 20), $"Highest Y Velocity: {highestYVel}");
            }
        }

        private bool showDropdownStorProg = false;
        private int selectedDropdownIndexStorProg = 0;
        private List<string> dropdownOptionsStorProg;
        private Vector2 dropdownScrollPositionStorProg = Vector2.zero; // Scroll position for the dropdown

        private bool showDropdownCheckpoint = false;
        private int selectedDropdownIndexCheckpoint = 0;
        private List<string> dropdownOptionsCheckpoint;
        private Vector2 dropdownScrollPositionCheckpoint = Vector2.zero; // Scroll position for the dropdown

        private bool showDropdownSaves = false;
        private int selectedDropdownIndexSaves = 0;
        private List<string> dropdownOptionsSaves;
        private Vector2 dropdownScrollPositionSaves = Vector2.zero; // Scroll position for the dropdown

        private int maxDropdownHeight = 200; // Maximum height of the dropdown
        private int maxWidth = 400; // Maximum width

        public string saveName = "";

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

            if (GUI.Button(new Rect(xPosition, currentY, maxWidth, 20), "Reset Height Tracker")) {
                highestY = itorah.transform.position.y;
                highestYVel = itorah.GetComponent<Rigidbody2D>().velocity.y;
            }
            currentY += yIncrement;

            // Healthbars
            if (GUI.Toggle(new Rect(xPosition, currentY, maxWidth, 20), showHealthbars, "Healthbars")) {
                showHealthbars = true;
            } else {
                showHealthbars = false;
            }
            currentY += yIncrement;

            // Skill Toggles
            if (GUI.Toggle(new Rect(xPosition, currentY, maxWidth / 2, 20), playerSkillSet.skills[0].learned, "WallJump")) {
                playerSkillSet.skills[0].learned = true;
            } else {
                playerSkillSet.skills[0].learned = false;
            }
            if (GUI.Toggle(new Rect(xPosition + maxWidth / 2, currentY, maxWidth / 2, 20), playerSkillSet.skills[1].learned, "Stomp")) {
                playerSkillSet.skills[1].learned = true;
            } else {
                playerSkillSet.skills[1].learned = false;
            }
            currentY += yIncrement;
            if (GUI.Toggle(new Rect(xPosition, currentY, maxWidth / 2, 20), playerSkillSet.skills[2].learned, "DoubleJump")) {
                playerSkillSet.skills[2].learned = true;
            } else {
                playerSkillSet.skills[2].learned = false;
            }
            if (GUI.Toggle(new Rect(xPosition + maxWidth / 2, currentY, maxWidth / 2, 20), playerSkillSet.skills[3].learned, "UpperCut")) {
                playerSkillSet.skills[3].learned = true;
            } else {
                playerSkillSet.skills[3].learned = false;
            }
            currentY += yIncrement;
            if (GUI.Toggle(new Rect(xPosition, currentY, maxWidth / 2, 20), playerSkillSet.skills[4].learned, "Dash")) {
                playerSkillSet.skills[4].learned = true;
            } else {
                playerSkillSet.skills[4].learned = false;
            }
            if (GUI.Toggle(new Rect(xPosition + maxWidth / 2, currentY, maxWidth / 2, 20), playerSkillSet.skills[5].learned, "Heal")) {
                playerSkillSet.skills[5].learned = true;
            } else {
                playerSkillSet.skills[5].learned = false;
            }
            currentY += yIncrement;
            if (GUI.Toggle(new Rect(xPosition, currentY, maxWidth / 2, 20), playerSkillSet.skills[6].learned, "Throw")) {
                playerSkillSet.skills[6].learned = true;
            } else {
                playerSkillSet.skills[6].learned = false;
            }
            if (GUI.Toggle(new Rect(xPosition + maxWidth / 2, currentY, maxWidth / 2, 20), playerSkillSet.skills[7].learned, "ChargeAttack")) {
                playerSkillSet.skills[7].learned = true;
            } else {
                playerSkillSet.skills[7].learned = false;
            }
            currentY += yIncrement;

            // Story Progression
            GUI.Label(new Rect(xPosition, currentY, maxWidth, 20), "Story Progression:");
            currentY += yIncrement;

            selectedDropdownIndexStorProg = dMenu.StoryProgressDropdown.value;
            if (GUI.Button(new Rect(xPosition, currentY, maxWidth, 20), dropdownOptionsStorProg[selectedDropdownIndexStorProg])) {
                showDropdownStorProg = !showDropdownStorProg;
            }
            if (showDropdownStorProg) {

                int totalDropdownHeight = dropdownOptionsStorProg.Count * 20;
                Rect scrollViewRect = new Rect(xPosition, currentY + 20, maxWidth, Mathf.Min(maxDropdownHeight, totalDropdownHeight));
                Rect contentRect = new Rect(0, 0, maxWidth - 20, totalDropdownHeight);

                dropdownScrollPositionStorProg = GUI.BeginScrollView(scrollViewRect, dropdownScrollPositionStorProg, contentRect);

                for (int i = 0; i < dropdownOptionsStorProg.Count; i++) {
                    if (GUI.Button(new Rect(0, i * 20, maxWidth - 20, 20), dropdownOptionsStorProg[i])) {
                        selectedDropdownIndexStorProg = i;
                        showDropdownStorProg = false;
                        dMenu.StoryProgressDropdown.value = i;
                        dMenu.SetSelectedStoryProgress();
                    }
                }

                GUI.EndScrollView();
            }
            currentY += yIncrement + (showDropdownStorProg ? Mathf.Min(maxDropdownHeight, dropdownOptionsStorProg.Count * 20) : 0);
            // Checkpoint
            GUI.Label(new Rect(xPosition, currentY, maxWidth, 20), "Checkpoint:");
            currentY += yIncrement;
            selectedDropdownIndexCheckpoint = dMenu.BonfireDropdown.value;
            if (GUI.Button(new Rect(xPosition, currentY, maxWidth, 20), dropdownOptionsCheckpoint[selectedDropdownIndexCheckpoint])) {
                showDropdownCheckpoint = !showDropdownCheckpoint;
            }
            if (showDropdownCheckpoint) {
                int totalDropdownHeight = dropdownOptionsCheckpoint.Count * 20;
                Rect scrollViewRect = new Rect(xPosition, currentY + 20, maxWidth, Mathf.Min(maxDropdownHeight, totalDropdownHeight));
                Rect contentRect = new Rect(0, 0, maxWidth - 20, totalDropdownHeight);

                dropdownScrollPositionCheckpoint = GUI.BeginScrollView(scrollViewRect, dropdownScrollPositionCheckpoint, contentRect);

                for (int i = 0; i < dropdownOptionsCheckpoint.Count; i++) {
                    if (GUI.Button(new Rect(0, i * 20, maxWidth - 20, 20), dropdownOptionsCheckpoint[i])) {
                        selectedDropdownIndexCheckpoint = i;
                        showDropdownCheckpoint = false;
                        dMenu.BonfireDropdown.value = i;
                        dMenu.LoadSelectedCheckpoint();
                    }
                }

                GUI.EndScrollView();
            }
            currentY += yIncrement + (showDropdownCheckpoint ? Mathf.Min(maxDropdownHeight, dropdownOptionsCheckpoint.Count * 20) : 0);
            // Save Game
            saveName = GUI.TextField(new Rect(xPosition, currentY, maxWidth, 20), saveName, 30);
            currentY += yIncrement;
            if (GUI.Button(new Rect(xPosition, currentY, maxWidth, 20), "Save Game")) {
                SaveGame(saveName);
                dropdownOptionsSaves.Add(saveName);
            }
            currentY += yIncrement;
            // Checkpoint
            GUI.Label(new Rect(xPosition, currentY, maxWidth, 20), "Custom Saves:");
            currentY += yIncrement;
            selectedDropdownIndexSaves = dMenu.BonfireDropdown.value;
            if (GUI.Button(new Rect(xPosition, currentY, maxWidth, 20), dropdownOptionsSaves[selectedDropdownIndexSaves])) {
                showDropdownSaves = !showDropdownSaves;
            }
            if (showDropdownSaves) {
                int totalDropdownHeight = dropdownOptionsSaves.Count * 20;
                Rect scrollViewRect = new Rect(xPosition, currentY + 20, maxWidth, Mathf.Min(maxDropdownHeight, totalDropdownHeight));
                Rect contentRect = new Rect(0, 0, maxWidth - 20, totalDropdownHeight);

                dropdownScrollPositionSaves = GUI.BeginScrollView(scrollViewRect, dropdownScrollPositionSaves, contentRect);

                for (int i = 0; i < dropdownOptionsSaves.Count; i++) {
                    if (GUI.Button(new Rect(0, i * 20, maxWidth - 20, 20), dropdownOptionsSaves[i])) {
                        selectedDropdownIndexSaves = i;
                        showDropdownSaves = false;
                        string saveName = "customSaves/" + dropdownOptionsSaves[i];
                        lastLoadedSave = saveName;
                        LoadGame(saveName);
                    }
                }

                GUI.EndScrollView();
            }
            currentY += yIncrement + (showDropdownSaves ? Mathf.Min(maxDropdownHeight, dropdownOptionsSaves.Count * 20) : 0);

            if (GUI.Button(new Rect(xPosition, currentY, maxWidth, 20), "Load Last Save (F3)")) {
                if (!string.IsNullOrEmpty(lastLoadedSave)) {
                    LoadGame(lastLoadedSave);
                } else {
                    Logger.LogWarning("No last loaded save found.");
                }
            }
        }
    }



    [HarmonyPatch(typeof(ItorahSessionData))]
    public class ItorahSessionDataPatch {
        [HarmonyPatch("Awake", new Type[] { })]
        private static bool Prefix() {
            Plugin plugin = GameObject.Find("BepInEx_Manager").GetComponent<Plugin>();
            if (plugin == null) {
                ScriptEngine.ScriptEngine sEngine = GameObject.Find("BepInEx_Manager").GetComponent<ScriptEngine.ScriptEngine>();
                var scriptManager = typeof(ScriptEngine.ScriptEngine).GetField("scriptManager", BindingFlags.NonPublic | BindingFlags.Instance);
                GameObject scriptManagerObj = (GameObject)scriptManager.GetValue(sEngine);
                plugin = scriptManagerObj.GetComponent<Plugin>();
            }
            plugin.enabled = true;
            plugin.InitMod();
            Plugin.Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            return true;
        }
    }
    [HarmonyPatch(typeof(InputOptionsScreen))]
    public class InputOptionsScreenPatch {
        [HarmonyPatch("ShowPromptToConfirmChangesOrErrorPrompt", new Type[] { })]
        private static bool Prefix(InputOptionsScreen __instance) {
            EventSystem.current.SetSelectedGameObject(null);
            __instance.ConfirmChangesPrompt.gameObject.SetActive(true);
            return false;
        }
    }
}
