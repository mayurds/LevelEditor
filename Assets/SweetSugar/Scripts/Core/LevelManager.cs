using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SweetSugar.Scripts.Blocks;
using SweetSugar.Scripts.Items;
using SweetSugar.Scripts.Level;
using SweetSugar.Scripts.System;
using SweetSugar.Scripts.System.Orientation;
using UnityEngine;
using UnityEngine.UI;

namespace SweetSugar.Scripts.Core
{
//game state enum
    public enum GameState
    {
        Map,
        PrepareGame,
        RegenLevel,
        Tutorial,
        Pause,
        Playing,
        PreFailed,
        GameOver,
        ChangeSubLevel,
        PreWinAnimations,
        Win,
        WaitForPopup,
        WaitAfterClose,
        BlockedGame,
        BombFailed
    }

    /// <summary>
    /// core-game class, using for handle game states, blocking, sync animations and search mathing and map
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager THIS;
        //life shop reference
        //true if Unity in-apps is enable and imported
        public bool enableInApps;
        //square width for border placement
        public float squareWidth = 1.2f;
        //item which was dragged recently 
        public Item lastDraggedItem;
        //item which was switched succesfully recently 
        public Item lastSwitchedItem;
        //makes scores visible in the game
        public GameObject popupScore;
        //current game level
        public int currentLevel = 1;
        //current sub-level
        private int currentSubLevel = 1;
        public int CurrentSubLevel
        {
            get { return currentSubLevel; }
            set
            {
                currentSubLevel = value;
                levelData.currentSublevelIndex = currentSubLevel - 1;
            }
        }
        //current field reference
        public FieldBoard field => fieldBoards[CurrentSubLevel - 1];
        //EDITOR: cost of continue after failing 
        public int FailedCost;
        //EDITOR: moves gived to continue
        public int ExtraFailedMoves = 5;
        //EDITOR: time gived to continue
        public int ExtraFailedSecs = 30;
        //in-app products for purchasing
        public bool thrivingBlockDestroyed;
        //empty boost reference for system
        //debug settings reference
        public DebugSettings DebugSettings;
        //activate boost in game
        //score gain on this game
        public static int Score;
        //stars gain on this game
        public int stars;
        //striped effect reference
        public GameObject stripesEffect;
        //show popup score on field
        public bool showPopupScores;
        //popup score color
        //Level gameobject reference
        public GameObject Level;
        //Gameobject reference
        //Gameobject reference
        public GameObject FieldsParent;
        //Gameobject reference
        public GameObject NoMoreMatches;
        //Gameobject reference
        public GameObject CompleteWord;
        //Gameobject reference
        public GameObject FailedWord;
        //in game boost reference
        //reference to orientation handler
        public OrientationGameCameraHandle orientationGameCameraHandle;

        //blocking to drag items for a time
        public int moveID;
        //value for regeneration, items with falling or not
        public bool onlyFalling;
        //level loaded, wait until true for some courotines
        public bool levelLoaded;
        //true if Facebook plugin installed
        //combine manager listener
        //true if search of matches has started
        //true if need to check matches again
        //if true - start the level avoind the map for debug
        public bool testByPlay;

        //game events
        #region EVENTS

        public delegate void GameStateEvents();
        public static event GameStateEvents OnMapState;
        public static event GameStateEvents OnEnterGame;
        public static event GameStateEvents OnLevelLoaded;
        public static event GameStateEvents OnWaitForTutorial;
        public static event GameStateEvents OnMenuPlay;
        public static event GameStateEvents OnSublevelChanged;
        public static event GameStateEvents OnMenuComplete;
        public static event GameStateEvents OnStartPlay;
        public static event GameStateEvents OnWin;
        public static event GameStateEvents OnLose;
        public static event GameStateEvents OnTurnEnd;
        public static event GameStateEvents OnCombo;

        //current game state
        private GameState GameStatus;
        public GameState gameStatus
        {
            get { return GameStatus; }
            set
            {
                GameStatus = value;
                switch (value)
                {
                    case GameState.PrepareGame://preparing and initializing  the game
                        LoadLevel(1);
                        CrosssceneData.passLevelCounter++;
                        stars = 0;
                        moveID = 0;
                        fieldBoards = new List<FieldBoard>();
                        CurrentSubLevel = 1;
                        OnEnterGame?.Invoke();
                        GenerateLevel();
                        levelLoaded = true;
                        OnLevelLoaded?.Invoke();
                        break;
                }
            }
        }

      

        //if true - pausing all falling animations
        public bool StopFall => _stopFall.Any();
        //returns last sub-level of this level
        private int GetLastSubLevel()
        {
            return fieldBoards.Count;
        }
        //returns current game state
        public static GameState GetGameStatus()
        {
            return THIS.gameStatus;
        }
        //menu play enabled invokes event
    
        //Switch sub level to next 
        private void ChangeSubLevel()
        {
            CurrentSubLevel++;
            OrientationGameCameraHandle.CameraParameters cameraParameters = orientationGameCameraHandle.GetCameraParameters();
            Vector2 cameraCenter = orientationGameCameraHandle.GetCenterOffset();
            StartCoroutine(AnimateField(field.GetPosition() + cameraCenter, cameraParameters.size));
        }

        #endregion
        //Lock boosts
       
        //Load the level from "OpenLevel" player pref
 
        private void OnEnable()
        {
       
            gameStatus = GameState.PrepareGame;
        }
        //enable map

        private void Awake()
        {
            THIS = this;
            testByPlay = false;
//        testByPlay = true;//enable to instant level run
        }

        // Use this for initialization
        private void Start()
        {
            DebugSettings = Resources.Load<DebugSettings>("Scriptable/DebugSettings");

        }

        private void PrepareGame()
        {
         
        }

        public List<FieldBoard> fieldBoards = new List<FieldBoard>();
        public GameObject FieldBoardPrefab;
        public LevelData levelData;
        internal bool tutorialTime;
    
        //Generate loaded level
        private void GenerateLevel()
        {
            var fieldPos = new Vector3(-0.9f, 0, -10);
            var latestFieldPos = Vector3.right * ((GetLastSubLevel() - 1) * 10) + Vector3.back * 10;

            var i = 0;
            foreach (var item in fieldBoards)
            {
                var _field = item.gameObject;
                _field.transform.SetParent(FieldsParent.transform);
                _field.transform.position = fieldPos + Vector3.right * (i * 15);
                var fboard = _field.GetComponent<FieldBoard>();

                fboard.CreateField();
                latestFieldPos = fboard.GetPosition();

                i++;
            }

            transform.position = latestFieldPos + Vector3.right * 10 + Vector3.back * 10;
        }

        private bool animStarted;
        /// <summary>
        /// move camera to the field
        /// </summary>
        /// <param name="destPos">position of the field</param>
        /// <param name="cameraParametersSize">camera size</param>
        /// <returns></returns>
        private IEnumerator AnimateField(Vector3 destPos, float cameraParametersSize)
        {
            var _camera = GetComponent<Camera>();
            if(animStarted) yield break;
            animStarted = true;
            var duration = 2f;
            var speed = 10f;
            var startPos = transform.position;
            var distance = Vector2.Distance(startPos, destPos);
            var time = distance / speed;
            var curveX = new AnimationCurve(new Keyframe(0, startPos.x), new Keyframe(time, destPos.x));
            var startTime = Time.time;
            float distCovered = 0;
            while (distCovered < distance)
            {
                distCovered = (Time.time - startTime) * speed;
                transform.localPosition = new Vector3(curveX.Evaluate(Time.time - startTime), transform.position.y, 0);
                _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, cameraParametersSize, Time.deltaTime*5);
                yield return new WaitForFixedUpdate();
            }

            _camera.orthographicSize = cameraParametersSize;
            transform.position = destPos;
            yield return new WaitForSeconds(0.5f);
            animStarted = false;
            GameStart();
        }

        //game start
        private void GameStart()
        {
            {
                OnSublevelChanged?.Invoke();
                gameStatus = GameState.Playing;
            }
        }
   
        public Transform movesTransform;
        public int destLoopIterations;
        //Animations after win
        #region RegenerateLevel
  

     
    
        public void DestroyItems(bool withoutEffects = false)
        {
            var items = field.GetItems();
            foreach (var item in items)
            {
                if (item != null)
                {
                    if (item.GetComponent<Item>().currentType != ItemsTypes.INGREDIENT &&
                        item.GetComponent<Item>().currentType == ItemsTypes.NONE)
                    {
                        if (!withoutEffects)
                            item.GetComponent<Item>().DestroyItem();
                        else
                            item.GetComponent<Item>().anim.SetTrigger("disappear");
                    }
                }
            }
        }
    
        #endregion

        public IEnumerator FindMatchDelay()
        {
            yield return new WaitForSeconds(0.2f);
        }


        internal List<GameBlocker> _stopFall = new List<GameBlocker>();
        public int combo;
        public AnimationCurve fallingCurve = AnimationCurve.Linear(0, 0, 1, 0);
        public float waitAfterFall = 0.02f;
        [HideInInspector]
        public bool collectIngredients;

        private Item lastTouchedItem;
    
        private IEnumerator FallingDown()
        {
//        Debug.Log("@@@ Next Move search matches @@@");
            THIS.thrivingBlockDestroyed = false;
            combo = 0;
            var it = field.GetItems();
            for (var i = 0; i < it.Count; i++)
            {
                var item = it[i];
                if (item != null)
                {
                    item.anim.StopPlayback();
                }
            }

            destLoopIterations = 0;
            while (true)
            {
                destLoopIterations++;

                var destroyItemsListed = field.GetItems().Where(i => i.destroyNext).ToList();
                if (destroyItemsListed.Count > 0)
                    yield return new WaitWhileDestroyPipeline(destroyItemsListed, new Delays());
                yield return new WaitWhileDestroying();
                yield return new WaitWhile(()=>StopFall);
                yield return new WaitWhileFall();
                yield return new WaitWhileCollect();
            
            }


//        Debug.Log("<-next turn->");
            if (gameStatus == GameState.Playing && !FindObjectsOfType<AnimateItems>().Any())
            {
                OnTurnEnd?.Invoke();
            }
        }
        /// <summary>
        /// Get square by position
        /// </summary>
        public Square GetSquare(int col, int row, bool safe = false)
        {
            return field.GetSquare(col, row, safe);
        }
        /// <summary>
        /// Get bunch of squares by row number
        /// </summary>
        public List<Square> GetRowSquare(int row)
        {
            var itemsList = new List<Square>();
            for (var col = 0; col < levelData.maxCols; col++)
            {
                Square square = GetSquare(col, row, true);
                if (!square.IsNone())
                    itemsList.Add(square);
            }

            return itemsList;
        }
        /// Get bunch of squares by column number
        public List<Square> GetColumnSquare(int col)
        {
            var itemsList = new List<Square>();
            for (var row = 0; row < levelData.maxRows; row++)
            {
                Square square = GetSquare(col, row, true);
                if (!square.IsNone())
                    itemsList.Add(square);
            }

            return itemsList;
        }
        /// Get bunch of items by row number
        public List<Item> GetRow(int row)
        {
            var itemsList = new List<Item>();
            for (var col = 0; col < levelData.maxCols; col++)
            {
                itemsList.Add(GetSquare(col, row, true).Item);
            }

            return itemsList.WhereNotNull().ToList();
        }
        /// Get bunch of items by column number
        public List<Item> GetColumn(int col)
        {
            var itemsList = new List<Item>();
            for (var row = 0; row < levelData.maxRows; row++)
            {
                itemsList.Add(GetSquare(col, row, true).Item);
            }

            return itemsList.WhereNotNull().ToList();
        }
        /// <summary>
        /// Get squares around the square
        /// </summary>
        /// <param name="square"></param>
        /// <returns></returns>
        public List<Square> GetSquaresAroundSquare(Square square)
        {
            var col = square.col;
            var row = square.row;
            var itemsList = new List<Square>();
            for (var r = row - 1; r <= row + 1; r++)
            {
                for (var c = col - 1; c <= col + 1; c++)
                {
                    itemsList.Add(GetSquare(c, r, true));
                }
            }

            return itemsList;
        }

        /// <summary>
        /// get 9 items around the square
        /// </summary>
        /// <param name="square"></param>
        /// <returns></returns>
        public List<Item> GetItemsAround9(Square square)
        {
            var itemsList = GetItemsAround8(square);
            itemsList.Add(square.Item);
            return itemsList;
        }

        /// get 8 items around the square
        public List<Item> GetItemsAround8(Square square)
        {
            var col = square.col;
            var row = square.row;
            var itemsList = new List<Item>();
            for (var r = row - 1; r <= row + 1; r++)
            {
                for (var c = col - 1; c <= col + 1; c++)
                {
                    var pos = GetSquare(c, r, true).transform.position;
                    RaycastHit2D[] result = new RaycastHit2D[3];
                    Physics2D.LinecastNonAlloc(square.transform.position, pos, result,1<< LayerMask.NameToLayer("Item"));
                    var hit = result.Where(i => i.transform).OrderBy(i => (i.transform.position - pos).magnitude).FirstOrDefault();
                    if(hit && hit.transform)
                        itemsList.Add(hit.transform.GetComponent<Item>());
                }
            }

            return itemsList;
        }

        //striped effect
        public void StripedShow(GameObject obj, bool horrizontal)
        {
            var effect = Instantiate(stripesEffect, obj.transform.position, Quaternion.identity);
            if (!horrizontal)
                effect.transform.Rotate(Vector3.back, 90);
            Destroy(effect, 1);
        }

        //popup score, to use - enable "Popup score" in editor
        public void ShowPopupScore(int value, Vector3 pos, int color)
        {
            // UpdateBar();
            if (showPopupScores)
            {
                var parent = GameObject.Find("CanvasScore").transform;
                var poptxt = Instantiate(popupScore, pos, Quaternion.identity);
                poptxt.transform.GetComponentInChildren<Text>().text = "" + value;

                poptxt.transform.SetParent(parent);
                //   poptxt.transform.position += Vector3.right * 1;
                poptxt.transform.localScale = Vector3.one / 1.5f;
                Destroy(poptxt, 0.3f);
            }
        }

        /// <summary>
        /// check gained stars
        /// </summary>
        public void CheckStars()
        {
            if (Score >= levelData.star1 && stars <= 0)
            {
                stars = 1;
            }

            if (Score >= levelData.star2 && stars <= 1)
            {
                stars = 2;
            }

            if (Score >= levelData.star3 && stars <= 2)
            {
                stars = 3;
            }
        }

        /// <summary>
        /// load level from 
        /// </summary>
        /// <param name="currentLevel"></param>
        public void LoadLevel(int currentLevel)
        {
            levelLoaded = false;
            levelData = LoadingManager.LoadForPlay(currentLevel, levelData);

            if (gameStatus != GameState.Map)
            {
                foreach (var fieldData in levelData.fields)
                {
                    var _field = Instantiate(FieldBoardPrefab);
                    var fboard = _field.GetComponent<FieldBoard>();
                    fboard.fieldData = fieldData;
                    fboard.squaresArray = new Square[fieldData.maxCols * fieldData.maxRows];
                    fieldBoards.Add(fboard);
                }
            }
        }
    }
}