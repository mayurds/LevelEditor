using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SweetSugar.Scripts.Blocks;
using SweetSugar.Scripts.Core;
using SweetSugar.Scripts.GUI;
using SweetSugar.Scripts.Items;
using SweetSugar.Scripts.Level;
using SweetSugar.Scripts.Level.ItemsPerLevel.Editor;
using SweetSugar.Scripts.System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SweetSugar.Scripts.Editor
{
    [InitializeOnLoad]
    public class LevelMakerEditor : EditorWindow
    {
        private bool groupEnabled;
        private bool myBool = true;
        private float myFloat = 1.23f;
        private int levelNumber = 1;
        private int subLevelNumber = 1;

        public int SubLevelNumber
        {
            get { return subLevelNumber; }
            set
            {
                subLevelNumber = value;
                levelData.currentSublevelIndex = subLevelNumber - 1;
            }
        }


        private int subLevelNumberTotal = 1;

        private SquareTypes squareType;
        private string FileName = "1_1.txt";
        private Vector2 scrollViewVector;
        private bool update;
        private static int selected;
        private string[] toolbarStrings = { "Editor", "Settings", "Shop", "In-apps", "Ads", "GUI", "Rate", "About" };
        private string[] sectionsString = { "Blocks", "Items", "Directions", "Teleports" };

        private static LevelMakerEditor window;
        private bool life_settings_show;
        private bool score_settings;
        private bool boost_show;
        private bool failed_settings_show;
        private bool gems_shop_show;
        private bool target_description_show;
        string levelPath = "Assets/SweetSugar/Resources/Levels/";

        private bool enableGoogleAdsProcessing;
        private bool enableChartboostAdsProcessing;
        private LevelData levelData;
        private Texture[] arrows = new Texture[6];
        private Texture[] teleports = new Texture[2];
        private Texture[] arrows_enter = new Texture[4];
        private LevelScriptable levelScriptable;

        [MenuItem("Sweet Sugar/Game editor and settings")]
        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            window = (LevelMakerEditor)GetWindow(typeof(LevelMakerEditor),false, "Game editor");
            window.Show();
        }
     
     
        public static void ShowWindow()
        {
            GetWindow(typeof(LevelMakerEditor));
        }

        private void OnFocus()
        {
            levelScriptable = Resources.Load("Levels/LevelScriptable") as LevelScriptable;
            LoadLevel(levelNumber);

            if (levelData != null)
            {
                if (levelData.GetField(subLevelNumber - 1).maxRows <= 0)
                    levelData.GetField(subLevelNumber - 1).maxRows = 9;
                if (levelData.GetField(subLevelNumber - 1).maxCols <= 0)
                    levelData.GetField(subLevelNumber - 1).maxCols = 9;
                Initialize();
            }
            arrows[0] = (Texture)AssetDatabase.LoadAssetAtPath("Assets/SweetSugar/Textures_png/EditorSprites/arrow.png",typeof(Texture));
            arrows[1] = (Texture)AssetDatabase.LoadAssetAtPath("Assets/SweetSugar/Textures_png/EditorSprites/arrow_left.png",typeof(Texture));
            arrows[2] = (Texture)AssetDatabase.LoadAssetAtPath("Assets/SweetSugar/Textures_png/EditorSprites/arrow_right.png",typeof(Texture));
            arrows[3] = (Texture)AssetDatabase.LoadAssetAtPath("Assets/SweetSugar/Textures_png/EditorSprites/arrow_up.png",typeof(Texture));
            arrows[4] = (Texture)AssetDatabase.LoadAssetAtPath("Assets/SweetSugar/Textures_png/EditorSprites/circle arrow.png",typeof(Texture));
            arrows[5] = (Texture)AssetDatabase.LoadAssetAtPath("Assets/SweetSugar/Textures_png/EditorSprites/circle arrows.png",typeof(Texture));

            teleports[0] =
                (Texture)AssetDatabase.LoadAssetAtPath("Assets/SweetSugar/Textures_png/EditorSprites/teleport_icon1.png",typeof(Texture));
            teleports[1] =(Texture)AssetDatabase.LoadAssetAtPath("Assets/SweetSugar/Textures_png/EditorSprites/teleport_icon2.png",typeof(Texture));

            arrows_enter[0] = (Texture)AssetDatabase.LoadAssetAtPath("Assets/SweetSugar/Textures_png/EditorSprites/arrow_red.png",typeof(Texture));
            arrows_enter[1] = (Texture)AssetDatabase.LoadAssetAtPath("Assets/SweetSugar/Textures_png/EditorSprites/arrow_red_left.png",typeof(Texture));
            arrows_enter[2] = (Texture)AssetDatabase.LoadAssetAtPath("Assets/SweetSugar/Textures_png/EditorSprites/arrow_red_right.png",typeof(Texture));
            arrows_enter[3] = (Texture)AssetDatabase.LoadAssetAtPath("Assets/SweetSugar/Textures_png/EditorSprites/arrow_red_up.png",typeof(Texture));


        }

        private void OnLostFocus()
        {
            dirtyLevel = true;
            SaveLevel(levelNumber);
        }

        private void Initialize()
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            subLevelNumberTotal = GetSubLevelsCount();
            if (levelNumber < 1)
                levelNumber = 1;
            life_settings_show = true;
            score_settings = true;
            boost_show = true;
            failed_settings_show = true;
            gems_shop_show = true;
            target_description_show = true;
            var num = 0;
            var simpleItem = Resources.Load<GameObject>("Items/Item").GetComponent<ItemSimple>();
            Resources.LoadAll("Items");
        }

        private void InitializeSublevel()
        {
            levelData.GetField(subLevelNumber - 1).levelSquares =
                new SquareBlocks[levelData.GetField(subLevelNumber - 1).maxCols *
                                 levelData.GetField(subLevelNumber - 1).maxRows];
            for (int i = 0; i < levelData.GetField(subLevelNumber - 1).levelSquares.Length; i++)
            {
                SquareBlocks sqBlocks = new SquareBlocks();
                sqBlocks.obstacle = SquareTypes.NONE;

                levelData.GetField(subLevelNumber - 1).levelSquares[i] = sqBlocks;
            }
            ResetDirection();
        }

        private void OnGUI()
        {
//        GUI.skin = customSkin;
            if (!levelData.fields.Any())
            {
                OnFocus();
                return;
            }

            //		if (!gotFocus) return;
            UnityEngine.GUI.changed = false;

     
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
//        GUILayout.Space(30);
            int oldSelected = selected;
            selected = GUILayout.Toolbar(selected, toolbarStrings, GUILayout.Width(450));
            GUILayout.EndHorizontal();

            scrollViewVector = UnityEngine.GUI.BeginScrollView(new Rect(0, 45, position.width , position.height), scrollViewVector,
                new Rect(0, 0, 500, 1600));
            GUILayout.Space(-30);

            if (oldSelected != selected)
                scrollViewVector = Vector2.zero;

            if (selected == 0 && levelData.fields.Any())
            {
                if (levelData != null)
                {
                    //                if (EditorSceneManager.GetActiveScene().name == "game")
                    {
                        GUILevelSelector();
                        GUILayout.Space(10);


                        GUILimit();
                        GUILayout.Space(10);

                        GUIColorLimit();
                        GUILayout.Space(10);
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Bomb timer",GUILayout.Width(80));
                        GUILayout.Space(70);
                        levelData.GetField(subLevelNumber - 1).bombTimer = EditorGUILayout.IntField("",
                            levelData.GetField(subLevelNumber - 1).bombTimer, GUILayout.Width(50));
                        GUILayout.EndHorizontal();
                        GUILayout.Space(10);

                        GUIStars();
                        GUILayout.Space(10);

                        GUILayout.BeginHorizontal();
                        {
                            GUINoRegen();
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(10);

                        GUILevelSize();
                        GUILayout.Space(10);

                        GUILayout.Space(10);

                            GUIBlocks();
                            GUILayout.Space(10);

                   


                        GUIGameField();
                    }
                    //                else
                    //                    GUIShowWarning();
                }
            }
            else if (selected == 1)
            {
                    GUISettings();
            
                GUILayout.Space(10);
                CheckSeparateLevels();
            }
            else if (selected == 2)
            {
                if (EditorSceneManager.GetActiveScene().name == "game")
                    GUIShops();
            }

            UnityEngine.GUI.EndScrollView();
            if (UnityEngine.GUI.changed && !EditorApplication.isPlaying)
                EditorSceneManager.MarkAllScenesDirty();

        }
        private void CheckSeparateLevels()
        {
            if (GUILayout.Button("Re-generate separate levels", GUILayout.Width(300)))
            {
                var ls = Resources.Load("Levels/LevelScriptable") as LevelScriptable;
                for (int i = 0; i < ls.levels.Count(); i++)
                {
                    ScriptableLevelManager.CreateFileLevel( i+1, ls.levels[i]);
                }
            }
        }

  

        #region GUIDialogs

        private void GUIDialogs()
        {
            GUILayout.Label("GUI elements:", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Space(10);
            ShowMenuButton("Menu Play", "MenuPlay");
            ShowMenuButton("Menu Complete", "MenuComplete");
            ShowMenuButton("Menu Failed", "MenuFailed");
            ShowMenuButton("PreFailed", "PreFailed");
//        ShowMenuButton("Pause", "MenuPause");
            ShowMenuButton("Boost Shop", "BoostShop");
            ShowMenuButton("Live Shop", "LiveShop");
            ShowMenuButton("Gems Shop", "GemsShop");
//        ShowMenuButton("Boost Info", "BoostInfo");
            ShowMenuButton("Settings", "Settings");
            ShowMenuButton("Reward", "Reward");
//        ShowMenuButton("Tutorial", "Tutorial");
        }

        private void ShowMenuButton(string label, string name)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, EditorStyles.label, GUILayout.Width(100));

            GUILayout.EndHorizontal();
        }

        public static void SetSearchFilter(string filter, int filterMode)
        {
            SearchableEditorWindow[] windows =
                (SearchableEditorWindow[])Resources.FindObjectsOfTypeAll(typeof(SearchableEditorWindow));
            SearchableEditorWindow hierarchy = null;
            foreach (SearchableEditorWindow window in windows)
            {
                if (window.GetType().ToString() == "UnityEditor.SceneHierarchyWindow")
                {
                    hierarchy = window;
                    break;
                }
            }

            if (hierarchy == null)
                return;

            MethodInfo setSearchType =
                typeof(SearchableEditorWindow).GetMethod("SetSearchFilter", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = { filter, filterMode, false };

            setSearchType.Invoke(hierarchy, parameters);
        }

        #endregion

        #region settings


        private void GUISettings()
        {
            LevelManager lm = Camera.main.GetComponent<LevelManager>();//TODO: move all game settings to scriptabble
            GUILayout.Label("Game settings:", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset to default", GUILayout.Width(150)))
            {
                ResetSettings();
            }

            if (GUILayout.Button("Clear player prefs", GUILayout.Width(150)))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("Player prefs cleared");
            }
            if (GUILayout.Button("Open all levels", new GUILayoutOption[] { GUILayout.Width(150) }))
            {
                for (int i = 1; i < 1000; i++)
                {
                    SaveLevelStarsCount(i, 3);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Facebook", EditorStyles.boldLabel, GUILayout.Width(150)); //1.6.1
            if (GUILayout.Button("Install", GUILayout.Width(70)))
            {
                Application.OpenURL("https://developers.facebook.com/docs/unity/downloads");
            }

            if (GUILayout.Button("Account", GUILayout.Width(70)))
            {
                Application.OpenURL("https://developers.facebook.com");
            }

            if (GUILayout.Button("How to setup", GUILayout.Width(120)))
            {
                Application.OpenURL(
                    "https://docs.google.com/document/d/1bTNdM3VSg8qu9nWwO7o7WeywMPhVLVl8E_O0gMIVIw0/edit?usp=sharing");
            }

            GUILayout.EndHorizontal();

#if GAMESPARKS
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(150);
//                if (GUILayout.Button("Create game", GUILayout.Width(100)))
//                {
//                    GamesparksConfiguration window = CreateInstance<GamesparksConfiguration>();
//                    window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 200);
//                    window.ShowPopup();
//                }

            }
            GUILayout.EndHorizontal();
#endif
//#if FACEBOOK
//        share_settings = EditorGUILayout.Foldout(share_settings, "Share settings:");
//        if (share_settings)
//        {
//            GUILayout.BeginHorizontal();
//            GUILayout.Space(30);
//            GUILayout.BeginVertical();
//            {
//                lm.androidSharingPath =
// EditorGUILayout.TextField("Android path", lm.androidSharingPath, GUILayout.MaxWidth(500));
//                lm.iosSharingPath =
// EditorGUILayout.TextField("iOS path", lm.iosSharingPath, GUILayout.MaxWidth(500));
//            }
//            GUILayout.EndVertical();
//            GUILayout.EndHorizontal();
//
//            GUILayout.Space(10);
//        }
//#endif
            GUILayout.BeginHorizontal();
            GUILayout.Label("Leadboard Gamesparks", EditorStyles.boldLabel, GUILayout.Width(150)); //1.6.1
            if (GUILayout.Button("Install", GUILayout.Width(70)))
            {
                Application.OpenURL("https://docs.gamesparks.com/sdk-center/unity.html");
            }

            if (GUILayout.Button("Account", GUILayout.Width(70)))
            {
                Application.OpenURL("https://portal.gamesparks.net");
            }

            if (GUILayout.Button("How to setup", GUILayout.Width(120)))
            {
                Application.OpenURL("https://docs.google.com/document/d/1JcQfiiD2ALz6v_i9UIcG93INWZKC7z6FHXH_u6w9A8E");
            }

            GUILayout.EndHorizontal();


            //		if (oldFacebookEnable != lm.FacebookEnable) {//1.6.1
            //			SetScriptingDefineSymbols ();
            //		}

            GUILayout.Space(30);
            var lastRect = GUILayoutUtility.GetLastRect();
            lm.fallingCurve = EditorGUI.CurveField(new Rect(lastRect.x + 3, lastRect.y + 30, position.width - 50, 25), "Falling curve", lm.fallingCurve);
            GUILayout.Space(30);

            lm.waitAfterFall = EditorGUILayout.FloatField("Delay after fall", lm.waitAfterFall, GUILayout.Width(50),
                GUILayout.MaxWidth(200));
            GUILayout.Space(10);

//        score_settings = EditorGUILayout.Foldout(score_settings, "Score settings:");
//        if (score_settings)
//        {
//            GUILayout.Space(10);
//            GUILayout.BeginHorizontal();
//            GUILayout.Space(30);
//            GUILayout.BeginVertical();
//            lm.scoreForItem = EditorGUILayout.IntField("Score for item", lm.scoreForItem, GUILayout.Width(50),
//                GUILayout.MaxWidth(200));
//            lm.scoreForBlock = EditorGUILayout.IntField("Score for block", lm.scoreForBlock, GUILayout.Width(50),
//                GUILayout.MaxWidth(200));
//            lm.scoreForWireBlock = EditorGUILayout.IntField("Score for wire block", lm.scoreForWireBlock,
//                GUILayout.Width(50), GUILayout.MaxWidth(200));
//            lm.scoreForSolidBlock = EditorGUILayout.IntField("Score for solid block", lm.scoreForSolidBlock,
//                GUILayout.Width(50), GUILayout.MaxWidth(200));
//            lm.scoreForThrivingBlock = EditorGUILayout.IntField("Score for thriving block", lm.scoreForThrivingBlock,
//                GUILayout.Width(50), GUILayout.MaxWidth(200));
//            GUILayout.Space(10);
//
//            lm.showPopupScores = EditorGUILayout.Toggle("Show popup scores", lm.showPopupScores, GUILayout.Width(50),
//                GUILayout.MaxWidth(200));
//            GUILayout.Space(10);
//
//            lm.scoresColors[0] = EditorGUILayout.ColorField("Score color item 1", lm.scoresColors[0],
//                GUILayout.Width(200), GUILayout.MaxWidth(200));
//            lm.scoresColors[1] = EditorGUILayout.ColorField("Score color item 2", lm.scoresColors[1],
//                GUILayout.Width(200), GUILayout.MaxWidth(200));
//            lm.scoresColors[2] = EditorGUILayout.ColorField("Score color item 3", lm.scoresColors[2],
//                GUILayout.Width(200), GUILayout.MaxWidth(200));
//            lm.scoresColors[3] = EditorGUILayout.ColorField("Score color item 4", lm.scoresColors[3],
//                GUILayout.Width(200), GUILayout.MaxWidth(200));
//            lm.scoresColors[4] = EditorGUILayout.ColorField("Score color item 5", lm.scoresColors[4],
//                GUILayout.Width(200), GUILayout.MaxWidth(200));
//            lm.scoresColors[5] = EditorGUILayout.ColorField("Score color item 6", lm.scoresColors[5],
//                GUILayout.Width(200), GUILayout.MaxWidth(200));
//            GUILayout.Space(10);
//
//            lm.scoresColorsOutline[0] = EditorGUILayout.ColorField("Score color outline item 1",
//                lm.scoresColorsOutline[0], GUILayout.Width(200), GUILayout.MaxWidth(200));
//            lm.scoresColorsOutline[1] = EditorGUILayout.ColorField("Score color outline item 2",
//                lm.scoresColorsOutline[1], GUILayout.Width(200), GUILayout.MaxWidth(200));
//            lm.scoresColorsOutline[2] = EditorGUILayout.ColorField("Score color outline item 3",
//                lm.scoresColorsOutline[2], GUILayout.Width(200), GUILayout.MaxWidth(200));
//            lm.scoresColorsOutline[3] = EditorGUILayout.ColorField("Score color outline item 4",
//                lm.scoresColorsOutline[3], GUILayout.Width(200), GUILayout.MaxWidth(200));
//            lm.scoresColorsOutline[4] = EditorGUILayout.ColorField("Score color outline item 5",
//                lm.scoresColorsOutline[4], GUILayout.Width(200), GUILayout.MaxWidth(200));
//            lm.scoresColorsOutline[5] = EditorGUILayout.ColorField("Score color outline item 6",
//                lm.scoresColorsOutline[5], GUILayout.Width(200), GUILayout.MaxWidth(200));
//            GUILayout.EndVertical();
//            GUILayout.EndHorizontal();
//        }
//
//        GUILayout.Space(20);

            life_settings_show = EditorGUILayout.Foldout(life_settings_show, "Lifes settings:");
            if (life_settings_show)
            {
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                GUILayout.BeginVertical();


                GUILayout.Space(10);

                GUILayout.Label("Total time for refill lifes:", EditorStyles.label);
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                GUILayout.Label("Hour", EditorStyles.label, GUILayout.Width(50));
                GUILayout.Label("Min", EditorStyles.label, GUILayout.Width(50));
                GUILayout.Label("Sec", EditorStyles.label, GUILayout.Width(50));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                GUILayout.EndHorizontal();
                GUILayout.Space(10);


                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(20);

            GUILayout.Space(20);

            GUILayout.Space(20);


            failed_settings_show = EditorGUILayout.Foldout(failed_settings_show, "Failed settings:");
            if (failed_settings_show)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                GUILayout.BeginVertical();

                lm.FailedCost =
                    EditorGUILayout.IntField(new GUIContent("Cost of continue", "Cost of continue after failed"),
                        lm.FailedCost, GUILayout.Width(200), GUILayout.MaxWidth(200));
                lm.ExtraFailedMoves = EditorGUILayout.IntField(new GUIContent("Extra moves", "Extra moves after continue"),
                    lm.ExtraFailedMoves, GUILayout.Width(200), GUILayout.MaxWidth(200));
                lm.ExtraFailedSecs =
                    EditorGUILayout.IntField(new GUIContent("Extra seconds", "Extra seconds after continue"),
                        lm.ExtraFailedSecs, GUILayout.Width(200), GUILayout.MaxWidth(200));
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(20);

            //  EditorUtility.SetDirty(lm);
        }

        private void ResetSettings()
        {
            LevelManager lm = Camera.main.GetComponent<LevelManager>();
            lm.showPopupScores = false;

            lm.FailedCost = 12;
            lm.ExtraFailedMoves = 5;
            lm.ExtraFailedSecs = 30;
            EditorUtility.SetDirty(lm);
        }

        #endregion
        public void SaveLevelStarsCount(int level, int starsCount)
        {
            Debug.Log(string.Format("Stars count {0} of level {1} saved.", starsCount, level));
            PlayerPrefs.SetInt(GetLevelKey(level), starsCount);
        }
    
        private string GetLevelKey(int number)
        {
            return string.Format("Level.{0:000}.StarsCount", number);
        }
        #region shop

        private void GUIShops()
        {
            LevelManager lm = Camera.main.GetComponent<LevelManager>();

            GUILayout.Label("Shop settings:", EditorStyles.boldLabel, GUILayout.Width(150));


            GUILayout.Space(10);
            gems_shop_show = EditorGUILayout.Foldout(gems_shop_show, "Gems shop settings:");
           

            GUILayout.Space(10);
            boost_show = EditorGUILayout.Foldout(boost_show, "Boosts shop settings:");
        }

  

        #endregion

        #region leveleditor

        private void TestLevel(bool playNow = true, bool testByPlay = true)
        {
            dirtyLevel = true;
            SaveLevel(levelNumber);
            if (EditorSceneManager.GetActiveScene().name != "game") EditorSceneManager.OpenScene("Assets/SweetSugar/Scenes/game.unity");
            LevelManager lm = Camera.main.GetComponent<LevelManager>();
            PlayerPrefs.SetInt("OpenLevelTest", levelNumber);
            PlayerPrefs.SetInt("OpenLevel", levelNumber);
            PlayerPrefs.Save();

            if (playNow)
            {
                if (EditorApplication.isPlaying)
                    EditorApplication.isPlaying = false;
                else
                    EditorApplication.isPlaying = true;
            }
        }


        private void GUILevelSelector()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Level editor", GUILayout.Width(150));
                if (GUILayout.Button("Test level", GUILayout.Width(158)))
                {
                    TestLevel();
                }
                if (GUILayout.Button("Save", GUILayout.Width(50)))
                {
                    dirtyLevel = true;
                    SaveLevel(levelNumber);
                }

            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Level", GUILayout.Width(50));
                GUILayout.Space(100);
                if (GUILayout.Button("<<", GUILayout.Width(50)))
                {
                    PreviousLevel();
                }

                string changeLvl = GUILayout.TextField(" " + levelNumber, GUILayout.Width(50));
                if (int.Parse(changeLvl) != levelNumber)
                {
                    subLevelNumber = 1;
                    if (LoadLevel(int.Parse(changeLvl)))
                        levelNumber = int.Parse(changeLvl);

                }

                if (GUILayout.Button(">>", GUILayout.Width(50)))
                {
                    NextLevel();
                }

                if (GUILayout.Button(new GUIContent("+", "add level"), GUILayout.Width(20)))
                {
                    AddLevel();
                }

                if (GUILayout.Button(new GUIContent("- ", "remove current level"), GUILayout.Width(20)))
                {
                    RemoveLevel();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            //Sub Level

            GUILayout.BeginHorizontal();
            {
//            GUILayout.Space(60);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Sub Level", GUILayout.Width(80));
                GUILayout.Space(70);
                if (GUILayout.Button("<<", GUILayout.Width(50)))
                {
                    PreviousSubLevel();
                }

                GUILayout.Label(" " + SubLevelNumber + " / " + subLevelNumberTotal, GUILayout.Width(50));

                if (GUILayout.Button(">>", GUILayout.Width(50)))
                {
                    NextSubLevel();
                }

                if (GUILayout.Button(new GUIContent("+", "add sublevel"), GUILayout.Width(20)))
                {
                    AddSubLevel();
                }

                if (GUILayout.Button(new GUIContent("- ", "remove current sublevel"), GUILayout.Width(20)))
                {
                    RemoveSubLevel();
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
        }

        private void GUILevelSize()
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            int oldValue = levelData.GetField(subLevelNumber - 1).maxRows + levelData.GetField(subLevelNumber - 1).maxCols;
            GUILayout.Label("Columns", GUILayout.Width(50));
            GUILayout.Space(100);
            levelData.GetField(subLevelNumber - 1).maxCols = EditorGUILayout.IntField("",
                levelData.GetField(subLevelNumber - 1).maxCols, GUILayout.Width(50));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Rows", GUILayout.Width(50));
            GUILayout.Space(100);
            levelData.GetField(subLevelNumber - 1).maxRows = EditorGUILayout.IntField("",
                levelData.GetField(subLevelNumber - 1).maxRows, GUILayout.Width(50));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (levelData.GetField(subLevelNumber - 1).maxRows < 3)
                levelData.GetField(subLevelNumber - 1).maxRows = 3;
            if (levelData.GetField(subLevelNumber - 1).maxCols < 3)
                levelData.GetField(subLevelNumber - 1).maxCols = 3;
            if (levelData.GetField(subLevelNumber - 1).maxRows > 11)
                levelData.GetField(subLevelNumber - 1).maxRows = 11;
            if (levelData.GetField(subLevelNumber - 1).maxCols > 11)
                levelData.GetField(subLevelNumber - 1).maxCols = 11;
            if (oldValue != levelData.GetField(subLevelNumber - 1).maxRows + levelData.GetField(subLevelNumber - 1).maxCols)
            {
                Initialize();
                InitializeSublevel();
                dirtyLevel = true;
                // SaveLevel();
            }

        }

        private void GUILimit()
        {
            GUILayout.BeginHorizontal();
            {

                GUILayout.Label("Limit", EditorStyles.label, GUILayout.Width(50));
                GUILayout.Space(100);
                LIMIT limitTypeSave = levelData.limitType;
                int oldLimit = levelData.limit;
                levelData.limitType = (LIMIT)EditorGUILayout.EnumPopup(levelData.limitType, GUILayout.Width(93));
                if (levelData.limitType == LIMIT.MOVES)
                    levelData.limit = EditorGUILayout.IntField(levelData.limit, GUILayout.Width(50));
                else
                {
                    GUILayout.BeginHorizontal();
                    int limitMin = EditorGUILayout.IntField(levelData.limit / 60, GUILayout.Width(30));
                    GUILayout.Label(":", GUILayout.Width(10));
                    int limitSec =
                        EditorGUILayout.IntField(levelData.limit - (levelData.limit / 60) * 60, GUILayout.Width(30));
                    levelData.limit = limitMin * 60 + limitSec;
                    GUILayout.EndHorizontal();
                }

                if (levelData.limit <= 0)
                    levelData.limit = 1;
                if (limitTypeSave != levelData.limitType || oldLimit != levelData.limit)
                    dirtyLevel = true;
                // 	SaveLevel();
            }
            GUILayout.EndHorizontal();
        }

        private void GUIColorLimit()
        {
            GUILayout.BeginHorizontal();

            int saveInt = levelData.colorLimit;
            GUILayout.Label("Color limit", EditorStyles.label, GUILayout.Width(100));
            GUILayout.Space(50);
            levelData.colorLimit = (int)GUILayout.HorizontalSlider(levelData.colorLimit, 3, 6, GUILayout.Width(100));
            levelData.colorLimit = EditorGUILayout.IntField("", levelData.colorLimit, GUILayout.Width(50));
            if (levelData.colorLimit < 3)
                levelData.colorLimit = 3;
            if (levelData.colorLimit > 6)
                levelData.colorLimit = 6;

            GUILayout.EndHorizontal();

            if (saveInt != levelData.colorLimit)
            {
                dirtyLevel = true;
                // SaveLevel();
            }
        }


        private void GUIStars()
        {
            GUILayout.BeginHorizontal();
//        GUILayout.Space(35);

            //GUILayout.BeginVertical();

            GUILayout.Label("Stars", GUILayout.Width(30));

            GUILayout.Space(120);
            GUILayout.BeginHorizontal();
            int s = 0;
            s = EditorGUILayout.IntField("", levelData.star1, GUILayout.Width(50));
            if (s != levelData.star1)
            {
                levelData.star1 = s;
                dirtyLevel = true;
                // SaveLevel();
            }

            if (levelData.star1 <= 0)
                levelData.star1 = 100;
            s = EditorGUILayout.IntField("", levelData.star2, GUILayout.Width(50));
            if (s != levelData.star2)
            {
                levelData.star2 = s;
                dirtyLevel = true;
                // SaveLevel();
            }

            if (levelData.star2 < levelData.star1)
                levelData.star2 = levelData.star1 + 10;
            s = EditorGUILayout.IntField("", levelData.star3, GUILayout.Width(50));
            if (s != levelData.star3)
            {
                levelData.star3 = s;
                dirtyLevel = true;
                // SaveLevel();
            }

            if (levelData.star3 < levelData.star2)
            {
                levelData.star3 = levelData.star2 + 10;
                dirtyLevel = true;
                // SaveLevel();
            }

            GUILayout.EndHorizontal();
            //GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void GUINoRegen()
        {
            GUILayout.BeginHorizontal();
            {

                GUILayout.Label(new GUIContent("Don't Regen", "Don't regenerate if no matches possible"),
                    GUILayout.Width(80));
                bool s = false;
                GUILayout.Space(70);
                s = EditorGUILayout.Toggle(levelData.GetField().noRegenLevel, GUILayout.Width(50));
                if (s != levelData.GetField().noRegenLevel)
                {
                    levelData.GetField().noRegenLevel = s;
                    dirtyLevel = true;
                    // SaveLevel();
                }
            }
            GUILayout.EndHorizontal();
        }

  

        string[] GetTutorialNames() => new[] {"Disabled","SIMPLE", ItemsTypes.HORIZONTAL_STRIPED.ToString(), ItemsTypes.PACKAGE.ToString(), ItemsTypes.TimeBomb.ToString()};


        private bool IsBlockTarget(string blockName, string[] targets)
        {
            var list = targets.Where(i => blockName.Contains(i));
            return list.Count() > 0;
        }

     

        private void ResetDirection()
        {
            var squares = levelData.GetField(subLevelNumber - 1).levelSquares;
         

            dirtyLevel = true;
        }


        private int GetIndexByDirection(Vector2 direction)
        {
            if (direction == Vector2.right)
                return 3;
            if (direction == Vector2.left)
                return 1;
            if (direction == Vector2.up)
                return 2;
            return 0;
        }



        private Texture GetArrowByAngle(float angle)
        {
            return arrows[(int)(angle % 90)];
        }

        private Texture GetArrowByVector(Vector2 direction, bool enterPoint)
        {
            var arr = arrows;
            if (enterPoint) arr = arrows_enter;
            if (direction == Vector2.up)
                return arr[3];
            if (direction == Vector2.left)
                return arr[1];
            if (direction == Vector2.right)
                return arr[2];
            return arr[0];
        }


  

        private void GUIBlocks()
        {
            GUILayout.BeginHorizontal();
            {
                //GUILayout.Space(30);
                GUILayout.BeginVertical();
                {
                    GUILayout.Label("Tools:", EditorStyles.boldLabel);
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button(new GUIContent("Clear", "clear all field"), GUILayout.Width(50),
                            GUILayout.Height(50)))
                        {
                            ClearLevel();
                            // SaveLevel();
                        }

                        GUILayout.BeginHorizontal();
                        {
                            //GUILayout.Space(30);
                            UnityEngine.GUI.color = new Color(1, 1, 1, 1f);
                            foreach (SquareTypes squareTypeItem in EnumUtil.GetValues<SquareTypes>())
                            {
                                if (squareTypeItem == SquareTypes.NONE) continue;
                                if (GUILayout.Button(
                                    new GUIContent(Square.GetSquareTexture(squareTypeItem), squareTypeItem.ToString()),
                                    GUILayout.Width(50), GUILayout.Height(50)))

                                    squareType = squareTypeItem;
                            }

                            UnityEngine.GUI.color = new Color(1, 1, 1, 1f);

                            if (GUILayout.Button(new GUIContent("X", "Clear block"), GUILayout.Width(50),
                                GUILayout.Height(50)))
                            {
                                squareType = SquareTypes.NONE;
                            }

                            if (GUILayout.Button(new GUIContent("Fill+", "Fill with selected block, second click change or clear filling"), GUILayout.Width(50),
                                GUILayout.Height(50)))
                            {
                                FillLevel();
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private void ClearLevel()
        {
            for (int i = 0; i < levelData.GetField(subLevelNumber - 1).levelSquares.Length; i++)
            {
                levelData.GetField(subLevelNumber - 1).levelSquares[i].obstacle = SquareTypes.NONE;
            }

            dirtyLevel = true;
        }

        private void FillLevel()
        {
            for (int i = 0; i < levelData.GetField(subLevelNumber - 1).levelSquares.Length; i++)
            {
                var squareBlocks = levelData.GetField(subLevelNumber - 1).levelSquares[i];
                var tempType = squareType;
                if (
                    (squareBlocks.obstacle == squareType))
                {
                    var sqPos = squareBlocks.position;
                    squareType = SquareTypes.EmptySquare;
                    SetSquareType(sqPos.x, sqPos.y);
                    squareType = tempType;

                }
                else if ( (squareBlocks.obstacle == squareType))
                {
                    var sqPos = squareBlocks.position;
                    SetSquareType(sqPos.x, sqPos.y);
                }
            }

            dirtyLevel = true;
        }

        private SquareBlocks squareBlockSelected; //for teleport linking

        private void GUIGameField()
        {
            GUILayout.BeginVertical();
            for (int row = 0; row < levelData.GetField(subLevelNumber - 1).maxRows; row++)
            {
                GUILayout.BeginHorizontal();

                for (int col = 0; col < levelData.GetField(subLevelNumber - 1).maxCols; col++)
                {
                    Color squareColor = new Color(0.8f, 0.8f, 0.8f);
                    var imageButton = new object();
                    SquareBlocks squareBlock = levelData.GetBlock(row, col);
                    squareBlock.position = new Vector2Int(col, row);
                    UnityEngine.GUI.color = squareColor;
                    if (GUILayout.Button(imageButton as Texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        Debug.Log(squareType);
                            SetSquareType(col, row);
                    }


                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

        }



 
        private void SaveLevel(int _levelNumber)
        {
            squareBlockSelected = null;
            if (dirtyLevel)
            {
                if (!FileName.Contains(".txt"))
                    FileName += ".txt";
                SaveToScriptable(_levelNumber);
                // SaveCommonValues();
                // SaveField();
                dirtyLevel = false;
            }
        }

        private void AddLevel()
        {
            squareBlockSelected = null;
            SaveLevel(levelNumber);
            levelNumber = GetLastLevel()+1;
            SubLevelNumber = 1;
            ScriptableLevelManager.CreateFileLevel(levelNumber, levelData);
            levelData = ScriptableLevelManager.LoadLevel(levelNumber);
            Initialize();
            ClearLevel();
            SaveLevel(levelNumber);
            subLevelNumberTotal = GetSubLevelsCount();
        }

        private int GetLastLevel()
        {
            int lastLevel = LoadingManager.GetLastLevelNum();
            if(lastLevel == 0) lastLevel = levelScriptable.levels.Count;
            return lastLevel;
        }

        private void AddSubLevel()
        {
            squareBlockSelected = null;
            SaveLevel(levelNumber);
            levelData.AddNewField();
            SubLevelNumber = GetSubLevelsCount();
            Initialize();
            InitializeSublevel();
            SaveLevel(levelNumber);
            subLevelNumberTotal = GetSubLevelsCount();
        }

        private void RemoveSubLevel()
        {
            if (GetSubLevelsCount() > 1)
            {
                SaveLevel(levelNumber);
                levelData.RemoveField();
                SubLevelNumber = GetSubLevelsCount();
                Initialize();
                // InitializeSublevel();
                SaveLevel(levelNumber);
                subLevelNumberTotal = GetSubLevelsCount();
            }
        }

        private int GetSubLevelsCount()
        {
            return levelData.fields.Count; //GetSublevelsScriptable();
        }

        private int GetSublevelsScriptable()
        {
            return levelScriptable.levels[levelNumber - 1].fields.Count;
        }

        private void RemoveLevel()
        {
            File.Delete(levelPath+"Level_"+levelNumber+".asset");
            levelNumber--; 
            LoadLevel(levelNumber);
        }

        private void NextLevel()
        {
            SaveLevel(levelNumber);
            if (levelNumber + 1 <= GetLastLevel())
            {
                levelNumber++;
                SubLevelNumber = 1;
                LoadLevel(levelNumber);
            }
        }

        private void PreviousLevel()
        {
            SaveLevel(levelNumber);
            levelNumber--;
            SubLevelNumber = 1;
            if (levelNumber < 1)
                levelNumber = 1;
            if (!LoadLevel(levelNumber))
            {
                levelNumber++;
                LoadLevel(levelNumber);
            }
        }

        private void NextSubLevel()
        {
            if (SubLevelNumber + 1 <= GetSubLevelsCount())
            {
                SaveLevel(levelNumber);
                SubLevelNumber++;
            }

            // if (!LoadLevel(levelNumber, SubLevelNumber))
            // 	SubLevelNumber--;
        }

        private void PreviousSubLevel()
        {
            SaveLevel(levelNumber);
            SubLevelNumber--;
            if (SubLevelNumber < 1)
                SubLevelNumber = 1;
            // if (!LoadLevel(levelNumber, SubLevelNumber))
            // SubLevelNumber++;
        }

        private bool dirtyLevel;

        private void SetSquareType(int col, int row)
        {
            dirtyLevel = true;
            SquareBlocks squareBlock = levelData.GetBlock(row, col);

            squareBlock.obstacle = squareType;
            SaveLevel(levelNumber);

            update = true;
            // SaveLevel();
            // GetSquare(col, row).type = (int) squareType;
        }

        private void SaveToScriptable(int _levelNumber)
        {
            SquareBlocks[] levelSquares = levelData.GetField(subLevelNumber - 1).levelSquares;
     

            if (levelScriptable.levels.Count() < _levelNumber)
                levelScriptable.levels.Add(levelData);
            else
                levelScriptable.levels[_levelNumber - 1] = levelData.DeepCopy(_levelNumber);
            ScriptableLevelManager.SaveLevel(levelPath, _levelNumber, levelData);

//        EditorUtility.SetDirty(levelScriptable);
//        AssetDatabase.SaveAssets();
        }

        private bool LoadLevel(int currentLevel)
        {
            levelData = LoadingManager.LoadlLevel(currentLevel, levelData);
            squareBlockSelected = null;
            // PlayerPrefs.SetInt("OpenLevelTest", currentLevel);
            // PlayerPrefs.Save();

            if (levelData != null)
            {
                Initialize();
                return true;
            }

            return false;
        }

        #endregion
    }
}