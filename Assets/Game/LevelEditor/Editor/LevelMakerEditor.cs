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
    public Texture hexTex;
    private void OnFocus()
        {
            levelScriptable = Resources.Load("Levels/LevelScriptable") as LevelScriptable;

        hexTex = Resources.Load("Graphics/Hex") as Texture;
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
        }
    private void InitializeSubHexlevel()
    {
        int mapSize = Mathf.Max(levelData.GetField(subLevelNumber - 1).maxCols, levelData.GetField(subLevelNumber - 1).maxRows);
        int totalCount = 0;
        Vector2 ogpos = new Vector2(0, 0);
        for (int q = -mapSize; q <= mapSize; q++)
        {
            int r1 = Mathf.Max(-mapSize, -q - mapSize);
            int r2 = Mathf.Min(mapSize, -q + mapSize);
            for (int r = r1; r <= r2; r++)
            {
                totalCount++;
            }
        }

        levelData.GetField(subLevelNumber - 1).levelSquares = new SquareBlocks[totalCount];
        for (int i = 0; i < levelData.GetField(subLevelNumber - 1).levelSquares.Length; i++)
        {
            SquareBlocks sqBlocks = new SquareBlocks();
            sqBlocks.obstacle = SquareTypes.NONE;
            levelData.GetField(subLevelNumber - 1).levelSquares[i] = sqBlocks;
        }
        totalCount = 0;
        for (int q = -mapSize; q <= mapSize; q++)
        {
            int r1 = Mathf.Max(-mapSize, -q - mapSize);
            int r2 = Mathf.Min(mapSize, -q + mapSize);
            for (int r = r1; r <= r2; r++)
            {
                ogpos.x = 1f * Mathf.Sqrt(3.0f) * (q + r / 2.0f);
                ogpos.y = 1f * 3.0f / 2.0f * r;
           //     tile.index = new CubeIndex(q, r, -q - r);
                SquareBlocks squareBlock = levelData.GetField().levelSquares[totalCount];
                squareBlock.tileIndex = new Vector3Int(q, r, -q - r);
                squareBlock.position = ogpos;
                totalCount++;
            }
        }
    }
 
    private void OnGUI()
        {
            if (!levelData.fields.Any())
            {
                OnFocus();
                return;
            }
            UnityEngine.GUI.changed = false;

     
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
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
                    GUILevelSelector();
                    GUILayout.Space(10);
                    GUILimit();
                    GUILayout.Space(10);
                    GUILayout.Space(10);
                    GUILayout.Space(10);
                    GUILayout.Space(10);
                    GUILevelSize();
                    GUILayout.Space(10);
                    GUILayout.Space(10);
                GUIBlocks();
                if (levelData.gridType == GridType.Square)
                {
                    GUIGameField();
                }
                else
                {
                 
                    GUIHexBlocks();
                }
         
                    GUILayout.Space(10);
               
                }
            }
            else if (selected == 1)
            {
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
                    LevelUtils.CreateFileLevel( i+1, ls.levels[i]);
                }
            }
        }

      
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

            GUILayout.BeginHorizontal();
            {
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
            switch (levelData.gridType)
            {
                case GridType.Square:
                    InitializeSublevel();
                    break;
                case GridType.Hex:
                    InitializeSubHexlevel();
                    break;
            }

            SaveLevel(levelNumber);
            dirtyLevel = true;
                // SaveLevel();
            }

        }

        private void GUILimit()
        {

        GUILayout.BeginHorizontal();
        {

            GUILayout.Label("GridDirection", EditorStyles.label, GUILayout.Width(50));
            GUILayout.Space(100);
            levelData.gridDirection = (GridDirection)EditorGUILayout.EnumPopup(levelData.gridDirection, GUILayout.Width(93));
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Grid Type", EditorStyles.label, GUILayout.Width(50));
            GUILayout.Space(100);
            levelData.gridType = (GridType)EditorGUILayout.EnumPopup(levelData.gridType, GUILayout.Width(93));
            if(gridType != levelData.gridType)
            {
                gridType = levelData.gridType;
                dirtyLevel = true;
                Initialize();
                switch (gridType)
                {
                    case GridType.Square:
                        InitializeSublevel();
                        break;
                    case GridType.Hex:
                        InitializeSubHexlevel();
                        break;
                }
                SaveLevel(levelNumber);
            }
        }
        GUILayout.EndHorizontal();

    }
    GridType gridType;
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
            LevelUtils.CreateFileLevel(levelNumber, levelData);
            levelData = LevelUtils.LoadLevel(levelNumber);
            Initialize();
            ClearLevel();
            SaveLevel(levelNumber);
            subLevelNumberTotal = GetSubLevelsCount();
        }

        private int GetLastLevel()
        {
            int lastLevel = LevelUtils.GetLastLevelNum();
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

        private void SetSquareType(float col, float row)
        {
            dirtyLevel = true;
            SquareBlocks squareBlock = levelData.GetBlock((int) row, (int)col);

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
            LevelUtils.SaveLevel(levelPath, _levelNumber, levelData);

//        EditorUtility.SetDirty(levelScriptable);
//        AssetDatabase.SaveAssets();
        }

        private bool LoadLevel(int currentLevel)
        {
            levelData = LevelUtils.LoadlLevel(currentLevel, levelData);
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
    private void GUIHexBlocks()
    {
 
        Vector2 pos = new Vector2(200, 300);
        Vector2 ogpos = new Vector2(200, 300);

        int mapSize = Mathf.Max(levelData.GetField(subLevelNumber - 1).maxCols, levelData.GetField(subLevelNumber - 1).maxRows);
        int hexRadius = 1 * 30;
        for (int q = -mapSize; q <= mapSize; q++)
        {
            int r1 = Mathf.Max(-mapSize, -q - mapSize);
            int r2 = Mathf.Min(mapSize, -q + mapSize);
            for (int r = r1; r <= r2; r++)
            {
                pos.x = hexRadius * Mathf.Sqrt(3.0f) * (q + r / 2.0f);
                pos.y = hexRadius * 3.0f / 2.0f * r;
                ogpos.x = 1 * Mathf.Sqrt(3.0f) * (q + r / 2.0f);
                ogpos.y = 1 * 3.0f / 2.0f * r;
                if(levelData.GetField().GetHex(ogpos) != null)
                {
                    SquareBlocks squareBlock = levelData.GetField().GetHex(new Vector3Int(q, r, -q - r));
                    squareBlock.position = ogpos;
                }    
                Rect buttonrect = new Rect(pos + new Vector2(200 + (4 * hexRadius) , 450 + (5 * hexRadius)), new Vector2(50, 50));
                GUI.color = new Color(44f / 255f, 44f / 255f, 44f / 255f, 1);
                GUI.DrawTexture(buttonrect, hexTex);
                GUILayout.BeginArea(buttonrect);
                GUI.color = new Color(0,0,0,0);

                if (GUILayout.Button(hexTex, new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(50) }))
                {
                    
                    SetHexType(new Vector3Int(q, r, -q - r));
                }
                GUILayout.EndArea();
            }
        }
       
    }
    private void SetHexType(Vector3Int vector3Int)
    {
        dirtyLevel = true;
        SquareBlocks squareBlock = levelData.GetBlock(subLevelNumber - 1,vector3Int);
        squareBlock.obstacle = squareType;
        SaveLevel(levelNumber);
        update = true;
    }
}
