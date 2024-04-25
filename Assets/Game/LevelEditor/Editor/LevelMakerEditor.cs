using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

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

        private static LevelMakerEditor window;
        private bool life_settings_show;
        private bool boost_show;
        private bool failed_settings_show;
        private bool gems_shop_show;
        string levelPath = "Assets/Game/Resources/Levels/";

        private LevelData levelData;
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
        public Texture Texture1;
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

            Texture1 = (Texture)AssetDatabase.LoadAssetAtPath("Assets/SweetSugar/Textures_png/EditorSprites/arrow.png", typeof(Texture));

        }

        private void OnLostFocus()
        {
            dirtyLevel = true;
            SaveLevel(levelNumber);
        }

        private void Initialize()
        {
            subLevelNumberTotal = GetSubLevelsCount();
            if (levelNumber < 1)
                levelNumber = 1;
            life_settings_show = true;
            boost_show = true;
            failed_settings_show = true;
            gems_shop_show = true;
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
                        GUILayout.Space(70);
                        GUILayout.EndHorizontal();
                        GUILayout.Space(10);
                        GUIStars();
                        GUILayout.Space(10);
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
            GUILayout.Label("Game settings:", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.BeginHorizontal();
          

            if (GUILayout.Button("Clear player prefs", GUILayout.Width(150)))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("Player prefs cleared");
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

            GUILayout.Space(20);

            GUILayout.Space(20);

            GUILayout.Space(20);


            failed_settings_show = EditorGUILayout.Foldout(failed_settings_show, "Failed settings:");

            GUILayout.Space(20);
        }

      

        #endregion
        #region leveleditor

        private void TestLevel(bool playNow = true, bool testByPlay = true)
        {
            dirtyLevel = true;
            SaveLevel(levelNumber);
            if (EditorSceneManager.GetActiveScene().name != "game") EditorSceneManager.OpenScene("Assets/SweetSugar/Scenes/game.unity");
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
                                    new GUIContent(Texture1, squareTypeItem.ToString()),
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
