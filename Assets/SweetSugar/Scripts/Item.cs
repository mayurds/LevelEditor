using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SweetSugar.Scripts.Blocks;
using SweetSugar.Scripts.Core;
using SweetSugar.Scripts.Level;
using SweetSugar.Scripts.System;
using SweetSugar.Scripts.System.Combiner;
using SweetSugar.Scripts.System.Pool;
using UnityEngine;
using UnityEngine.Playables;
using Random = UnityEngine.Random;

namespace SweetSugar.Scripts.Items
{
//types of items
    public enum ItemsTypes
    {
        NONE = 0,
        VERTICAL_STRIPED = 4,
        HORIZONTAL_STRIPED = 5,
        PACKAGE = 3,
        MULTICOLOR = 1,
        INGREDIENT = 6,
        SPIRAL = 7,
        MARMALADE = 2,
        TimeBomb = 8
    }
    /// <summary>
    /// Item class is control item behaviour like switchings, animations, turning to another bonus item, destroying
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Animator))]

    public class Item : ItemMonoBehaviour, IField
    {
        public int instanceID;
        [Header("Item can MATCH with other items")]
        public bool Combinable;
        [Header("Item can MATCH with a bonus items")]
        public bool CombinableWithBonus;
        [Header("Item score")]
        public int scoreForItem = 10;
     
        //sprite rendered reference
        public SpriteRenderer[] sprRenderer
        {
            get
            {
                if(SpriteRenderers == null || SpriteRenderers.Length==0)
                    SpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
                return SpriteRenderers;
            }
        }

        private SpriteRenderer[] SpriteRenderers;
        //square object reference
        public Square square;
        /// Square from the item falling out
        [HideInInspector]
        public Square previousSquare;
        //should item fall after check
        public bool needFall;
        //current field reference
        [HideInInspector]
        public FieldBoard field;
        //is that item dragging
        [HideInInspector]
        public bool dragThis;
        //Not null if this items targeted by marmalade
        [HideInInspector]
        public GameObject marmaladeTarget;
        [HideInInspector]
        public Vector3 mousePos;
        [HideInInspector]
        public Vector3 deltaPos;
        //direction of switching this item
        [HideInInspector]
        public Vector3 switchDirection;
        //neighborSquare for switching
        private Square neighborSquare;
        //Item switching with
        private Item switchItem;
        //true if the item falling
        public bool falling;
        //Next type which item is going to became
        private ItemsTypes nextType = ItemsTypes.NONE;
        //current item type
        public ItemsTypes currentType = ItemsTypes.NONE;
        [HideInInspector] public ItemsTypes debugType = ItemsTypes.NONE;
        public ItemsTypes NextType
        {
            get { return nextType; }

            set
            {
                nextType = value;
            }
        }
        //child transform with sprite
        [HideInInspector] public Transform itemAnimTransform;
        //order in the squares sequence
        [HideInInspector] public int orderInSequence;
        //restriction from getting from pool
        [HideInInspector] 
        public bool canBePooled;
        [HideInInspector]
        public int COLORView;
        private int COLOR;
      
        //if true - item just created
        private bool justCreatedItem;
        public bool JustCreatedItem
        {
            get { return justCreatedItem; }

            set
            {
                if (value)
                    sprRenderer.WhereNotNull().ToList().ForEach(i => i.sortingLayerID = SortingLayer.NameToID("Item mask"));
                else
                    sprRenderer.WhereNotNull().ToList().ForEach(i => i.sortingLayerID = 0);

                justCreatedItem = value;
            }
        }
        //animator component
        [HideInInspector]
        public Animator anim;
        //gonna be destroy
        [HideInInspector] public bool destroying;
        //animation interface purpose
        [HideInInspector] public bool animationFinished;
        //animation value
        private float xScale;
        //animation value
        private float yScale;
        //item was set by editor
        [HideInInspector] public bool tutorialItem;
        //true - item should be switched in interactive tutorial
        [HideInInspector] public bool tutorialUsableItem;
        //true - item destroyed by multicolor
        [HideInInspector] public bool globalExplEffect;
        /// The destroy in the next destroying iteration
        internal bool destroyNext;
        //dont destroy this item, i.e. just appeared bonus item
        [HideInInspector] public bool dontDestroyOnThisMove;
        //editor item link
        /// set this item Undestroyable for current combine
        public bool dontDestroyForThisCombine;
        /// playable director for package animation
        [HideInInspector] public PlayableDirector director;
        public GameObject plusTime;
        public GameObject plusTimeObj;
        private void OnEnable()
        {
            switch (currentType)
            {
                case ItemsTypes.INGREDIENT:
                    anim.SetBool("ingredient_idle", true);
                    anim.SetFloat("ingredient_offset", Random.Range(0.0f, 1.0f));
                    break;
                case ItemsTypes.PACKAGE:
                    anim.SetBool("package_idle", true);
                    break;
            }
            InitItem();
        }

        //init variable after getting from pool
        public virtual void InitItem()
        {
            anim?.Rebind();
            anim?.ResetTrigger("disappear");
            destroycoroutineStarted = false;
            var animController = anim.runtimeAnimatorController;
            anim.runtimeAnimatorController = null;
            anim.enabled = false;
            if(transform.childCount>0)
            {
                transform.GetChild(0).transform.localScale = Vector3.one;
                transform.GetChild(0).transform.localPosition = Vector3.zero;
            }
//        if(sprRenderer.FirstOrDefault())
//            sprRenderer.FirstOrDefault().sortingOrder = 2;
            globalExplEffect = false;
            StartCoroutine(RareUpdate());
            GetComponentsInChildren<SpriteRenderer>().ForEachY(i => i.enabled = true);
            destroying = false;
            falling = false;
            destroyNext = false;
            tutorialItem = false;
            previousSquare = null;
            switchDirection = Vector3.zero;
            dragThis = false;
            fallingID = 0;
            debugType = currentType;
            ResetAnimTransform();
            anim.enabled = true;
            anim.runtimeAnimatorController = animController;
            marmaladeTarget = null;
        
            if(currentType == ItemsTypes.NONE && LevelManager.THIS.levelData.limitType == LIMIT.TIME ){
                if(Random.Range(0, 20) == 1 ){
                    plusTimeObj = Instantiate(plusTime, transform.position, Quaternion.identity, transform);
                    BindSortingOrder bindSortingOrder = plusTimeObj.transform.GetChild(0).GetComponent<BindSortingOrder>();
                    bindSortingOrder.sourceObject = GetSpriteRenderer();
                    bindSortingOrder.destObjectSR = plusTimeObj.GetComponent<SpriteRenderer>();
                }
            }
        }

        private void Awake()
        {
            director = GetComponent<PlayableDirector>();
            anim = GetComponent<Animator>();
            instanceID = GetInstanceID();
            name = "item " + currentType + instanceID;
            itemAnimTransform = transform.childCount>0 ? transform.GetChild(0): transform;
            defaultTransformPosition = itemAnimTransform.localPosition;
            defaultTransformScale = itemAnimTransform.localScale;
            defaultTransformRotation = itemAnimTransform.localRotation;
            gameObject.AddComponent<ItemDebugInspector>();
        }

        // Use this for initialization
        protected override void Start()
        {
            if (NextType != ItemsTypes.NONE)
            {
                transform.position = square.transform.position;
                falling = false;
            }
            xScale = transform.localScale.x;
            yScale = transform.localScale.y;
            base.Start();
        }

        /// Generate color for this Item
        

      
        //animation event "Appear"
        public void SetAppeared()
        {
            if (currentType == ItemsTypes.PACKAGE || currentType == ItemsTypes.MULTICOLOR || currentType == ItemsTypes.VERTICAL_STRIPED || currentType == ItemsTypes.HORIZONTAL_STRIPED || currentType == ItemsTypes.MARMALADE)
                anim.SetBool("package_idle", true);
        }
        //animation event "Disappear"
        public void SetDissapeared()
        {
            SmoothDestroy();
        }
        //start idle animation
        private IEnumerator AnimIdleStart()
        {
            var xScaleDest1 = xScale - 0.05f;
            var xScaleDest2 = xScale;
            var speed = Random.Range(0.02f, 0.07f);

            var trigger = false;
            while (true)
            {
                if (!trigger)
                {
                    if (xScale > xScaleDest1)
                    {
                        xScale -= Time.deltaTime * speed;
                        yScale += Time.deltaTime * speed;
                    }
                    else
                        trigger = true;
                }
                else
                {
                    if (xScale < xScaleDest2)
                    {
                        xScale += Time.deltaTime * speed;
                        yScale -= Time.deltaTime * speed;
                    }
                    else
                        trigger = false;
                }

                transform.localScale = new Vector3(xScale, yScale, 1);
                yield return new WaitForFixedUpdate();
            }
        }

        //reset drag variables
        private void ResetDrag()
        {
            dragThis = false;
            //   transform.position = square.transform.position + Vector3.back * 0.2f;
            switchDirection = Vector3.zero;
            //    switchItem.transform.position = neighborSquare.transform.position + Vector3.back * 0.2f;
            neighborSquare = null;
            switchItem = null;
        }

   
        IEnumerator RareUpdate()
        {
            while (true)
            {
 
                if (dontDestroyForThisCombine /*&& (lastCombine?.items.Where(i=>i != this).AllNull() ?? true)*/)
                {
                    yield return new WaitForSeconds(0.5f);

                    dontDestroyForThisCombine = false;
                }

                yield return new WaitForSeconds(0.3f);
            }
        }

        //Switching start method

        private bool NotContainsBoth(ItemsTypes type1, ItemsTypes type2) => (currentType != type1 && switchItem.currentType != type2);

        private bool ContainsBoth(ItemsTypes type1, ItemsTypes type2) =>
            currentType == type1 && switchItem.currentType == type2 ||
            currentType == type2 && switchItem.currentType == type1;

        /// Cloud effect animation for different direction levels
        public IEnumerator DirectionAnimation(Action callback)
        {
            GameObject itemPrefabObject = gameObject;

            var duration = 0.5f;
            Vector2 destPos = itemPrefabObject.transform.localPosition + (Vector3)square.direction * 0.1f;
            var startPos = itemPrefabObject.transform.localPosition;
            var curveX = new AnimationCurve(new Keyframe(0, startPos.x), new Keyframe(duration / 2, destPos.x));
            var curveY = new AnimationCurve(new Keyframe(0, startPos.y), new Keyframe(duration / 2, destPos.y));
            curveX.postWrapMode = WrapMode.PingPong;
            curveY.postWrapMode = WrapMode.PingPong;

            var startTime = Time.time;
            float distCovered = 0;
            while (distCovered < duration)
            {
          
                if (distCovered > duration / 10)
                    callback();
                distCovered = (Time.time - startTime);
                if (itemPrefabObject)
                    itemPrefabObject.transform.localPosition = new Vector3(curveX.Evaluate(distCovered), curveY.Evaluate(distCovered), 0);
                else
                    yield break;
                yield return new WaitForFixedUpdate();
                //            if (switchDirection != Vector3.zero)
                //            {
                //                itemPrefabObject.transform.localPosition = Vector3.zero;
                //                yield break;
                //            }
            }
        }
    
        //Change type if necessary
        public void CheckAndChangeTypes()
        {
            var itemsTypeChange = field.GetItems();
            var listChangingType = itemsTypeChange.Where(i => i.NextType != ItemsTypes.NONE);
            if (LevelManager.THIS.gameStatus == GameState.Playing)
            {
                foreach (var item in listChangingType)
                {
                    item.ChangeType();
                }
            }
        }

        //virtual method for bonus items
        public virtual void Check(Item item1, Item item2)
        {

        
        }
    
        //bonus animation after switching
        public void BonusesAnimation(Item item1, Item item2)
        {
            var list = new[] {item1, item2};
            if(list.Any(i=>!i.Combinable || !i.CombinableWithBonus)) return;
            bool isMulticolor = list.Any(i=>i.currentType == ItemsTypes.MULTICOLOR);
            if (item1.currentType != ItemsTypes.NONE && item2.currentType != ItemsTypes.NONE || isMulticolor)
            {
                if(!isMulticolor)
                    gameObject.AddComponent<GameBlocker>();
                Vector2 middlePos = item2.transform.position + (item1.transform.position - item2.transform.position).normalized * 0.5f;
                list = list.OrderBy(i => i.GetComponent<ItemCombineBehaviour>()?.priority?? 100).ToArray();
                item1 = list.First();
                item2 = list.Last();
                item1.sprRenderer.FirstOrDefault().sortingOrder = 3;
            }
        }

   
        ///is switching item is bonus
        private bool IsSwitchBonus()
        {
            if (((currentType == ItemsTypes.MULTICOLOR || switchItem.currentType == ItemsTypes.MULTICOLOR) &&
                 (Combinable || switchItem.Combinable)) || (currentType == ItemsTypes.MULTICOLOR && switchItem.currentType == ItemsTypes.MULTICOLOR))
                return true;
            if (currentType > 0 && switchItem.currentType > 0 && (currentType != ItemsTypes.INGREDIENT && switchItem.currentType != ItemsTypes.INGREDIENT) && Combinable && switchItem.Combinable)
                return true;
            if (!Combinable && !switchItem.Combinable && currentType != ItemsTypes.MULTICOLOR && switchItem.currentType != ItemsTypes.MULTICOLOR)
                return false;
            return false;
        }

      
     
        /// <summary>
        /// Get mouse position
        /// </summary>
        /// <returns></returns>
        public Vector3 GetMousePosition()
        {
            return Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        /// <summary>
        /// change square link and start fall
        /// </summary>
        /// <param name="_square"></param>
        public void ReplaceCurrentSquareToFalling(Square _square)
        {
            //        Debug.Log(instanceID + " replace square from " + square.GetPosition() +" to " + _square.GetPosition() );
            _square.Item = this;
            square.Item = null;
            previousSquare = square;
            square = _square;
            needFall = true;
            if (!justCreatedItem)
                StartFalling();
        }

        /// <summary>
        /// checking square below and start fall if square is empty
        /// </summary>
        public void CheckSquareBelow()
        {
            if (!falling)
                square.CheckFallOut();
            if (!needFall && !falling) CheckNearEmptySquares();
        }

        /// <summary>
        /// start falling animation
        /// </summary>
        public void StartFalling()
        {
            StartFallingTo(GenerateWaypoints(square));
        }

        public void StartFallingTo(List<Waypoint> generateWaypoints)
        {
//        if (LevelManager.THIS.StopFall) return;
            if (!falling && needFall && fallingID == 0)
            {
                falling = true;

                StartCoroutine(FallingCor(generateWaypoints, true));
            }
        }

        /// <summary>
        /// start falling diagonally
        /// </summary>
        private void StartFallingSides()
        {
            if (falling || !needFall || destroying || fallingID > 0) return;
            var waypoints = new List<Waypoint>
            {
                new Waypoint(transform.position, null),
                new Waypoint(square.transform.position, null)
            };
            falling = true;

            StartCoroutine(FallingCor(waypoints, true));
        }

        public List<Waypoint> GenerateWaypoints(Square targetSquare)
        {
            var waypoints = LevelManager.THIS.field.GetWaypoints(previousSquare, targetSquare, new List<Square>());
            if (waypoints.Any()) return waypoints;
            if (!targetSquare.isEnterPoint)
                waypoints = LevelManager.THIS.field.GetWaypoints(targetSquare.enterSquare, targetSquare, new List<Square>());
            if (waypoints.Any()) return waypoints;
            waypoints.Add(new Waypoint(targetSquare.transform.position + Vector3.back * 0.2f, null));
            waypoints.Add(new Waypoint(targetSquare.transform.position + Vector3.back * 0.2f, null));
            return waypoints;
        }

   

        int fallingID;
        private Vector3 defaultTransformPosition;
        private Vector3 defaultTransformScale;
        private Quaternion defaultTransformRotation;

        ///falling item animation
        private IEnumerator FallingCor(List<Waypoint> waypoints, bool animate, Action callback = null)
        {
            if (fallingID > 0) yield break;
            fallingID++;
            falling = true;
            needFall = false;
            var destPos = waypoints.LastOrDefault().destPosition + Vector3.back * 0.2f;
            var startTime = Time.time;
            var startPos = transform.position;
            var distance = Vector2.Distance(startPos, waypoints.FirstOrDefault().destPosition);
            float speed = LevelManager.THIS.fallingCurve.Evaluate(0);
            int sideFall = 2;
//        if (waypoints.Count() > 1 && waypoints[0].destPosition.x != waypoints[1].destPosition.x && waypoints[0].destPosition.y != waypoints[1].destPosition.y)
//            sideFall = 2;
            if (LevelManager.THIS.gameStatus == GameState.PreWinAnimations)
                speed = 10;
            var pauseTime = Time.time;
            float totalPauseTime = 0.0f;
            var fallStopped = false;

            yield return new WaitWhile(() => LevelManager.THIS.StopFall);

            var startTimeGlobal = Time.time;

            for (var i = 0; i < waypoints.Count; i++)
            {
                var waypoint = waypoints[i];
                destPos = waypoint.destPosition + Vector3.back * 0.2f;
                startPos = transform.position;
                distance = Vector2.Distance(startPos, destPos);
                if (distance < 0.2f) continue;
                startTime = Time.time;
                float fracJourney = 0;
                while (fracJourney < .9f)
                {
                    if(LevelManager.THIS.StopFall)
                    {
                        fallStopped = true;
                        pauseTime = Time.time;
                    }
                    yield return new WaitWhile(() => LevelManager.THIS.StopFall);
                    if(fallStopped && !LevelManager.THIS.StopFall)
                    {
                        fallStopped = false;
                        totalPauseTime += Time.time - pauseTime;
                        startTime += totalPauseTime;
                        startTimeGlobal += totalPauseTime;
                    }

                    speed = LevelManager.THIS.fallingCurve.Evaluate(Time.time - startTimeGlobal ) * sideFall;
                    var direction = (destPos - startPos).normalized;
                    RaycastHit2D[] raycastHits = new RaycastHit2D[2];
                    Physics2D.RaycastNonAlloc(transform.position + direction*-.5f, direction, raycastHits,.8f, 1 << LayerMask.NameToLayer("Item"));
                    RaycastHit2D hit2D = raycastHits.FirstOrDefault(x=>x.transform != transform);
                    if(!hit2D || !hit2D.transform.GetComponent<Item>().falling 
                              || ((Vector2)destPos-(Vector2)startPos).normalized != ((Vector2)square.transform.position-(Vector2)startPos).normalized
                              || ((Vector2)destPos-(Vector2)startPos).normalized != square.direction
                              || !ShouldSkipItem(hit2D))
                    {
                        var distCovered = (Time.time - startTime) * speed;
                        fracJourney = distCovered / distance;
                        transform.position = Vector2.Lerp(startPos, destPos, fracJourney);
                    }
                    else if(ShouldSkipItem(hit2D))
                    {
                        startPos = transform.position;    
                        startTimeGlobal = Time.time;
                        startTime = Time.time;
                    }

                    if (fracJourney < 1)
                        yield return new WaitForEndOfFrame();
                    if (waypoint.instant && Vector2.Distance(square.transform.position, transform.position)>2)
                    {
                        Vector3 pos = square.GetReverseDirection();
                        transform.position = destPos + pos * field.squareHeight + Vector3.back * 0.2f;
                        JustCreatedItem = true;
                        falling = false;
                        needFall = true;
                        fallingID = 0;
                        List<Waypoint> list = new List<Waypoint> {new Waypoint(square.transform.position, square)};
                        StartFallingTo(list);
                        yield break;
//                    break;
                    }
                    if (fracJourney >= 0.5f)
                    {
                        var squareNew = square.GetNextSquare();
                        if (squareNew != null && squareNew.Item == null && squareNew.IsFree())
                        {
                            JustCreatedItem = false;
                            square.Item = null;
                            squareNew.Item = this;
                            square = squareNew;
                            waypoints.Add(new Waypoint(squareNew.transform.position + Vector3.back * 0.2f, squareNew));
                            destPos = waypoint.destPosition + Vector3.back * 0.2f;
                            distance = Vector2.Distance(startPos, destPos);
                        }
                    }

                }
            }

            JustCreatedItem = false;
            if (previousSquare?.Item == this) previousSquare.Item = null;
            destPos = waypoints.LastOrDefault().destPosition + Vector3.back * 0.2f;
            if (!waypoints.Any()) destPos = square.transform.position + Vector3.back * 0.2f;

            anim.SetTrigger("stop");
//        transform.position = destPos;// square.transform.position;
            if (distance > 0.5f && animate)
            {
            }

            //        transform.position = Square1.transform.position + Vector3.back * 0.2f;

            fallingID = 0;
            // Invoke("StopFallFinished", 0.2f);
            StopFallFinished();
            yield return new WaitWhile(() => falling);
            yield return new WaitForSeconds(LevelManager.THIS.waitAfterFall);
            CheckSquareBelow();
            if (callback != null) callback();
            transform.position = destPos;// square.transform.position;
            if (!needFall)
            {
                ResetAnimTransform();

                yield return new WaitForSeconds(0.2f);
                OnStopFall();
            }
        }
        /// <summary>
        ///  Compare collided items, item with bigger magnitude should fall first
        /// </summary>
        /// <param name="hit2D"></param>
        /// <returns></returns>
        private bool ShouldSkipItem(RaycastHit2D hit2D)
        {
            return (hit2D.transform.GetComponent<Item>().square.transform.position - transform.position).magnitude > (square.transform.position-transform.position).magnitude 
                   && square.direction == hit2D.transform.GetComponent<Item>().square.direction;
        }

        /// <summary>
        /// fall finished events
        /// </summary>
        public void StopFallFinished()
        {
            falling = false;
            if (square.Item != this) DestroyBehaviour();
        }

        /// <summary>
        /// On stop after fall event for Ingredient
        /// </summary>
        public virtual void OnStopFall()
        {
        
        }

        public void ResetPackageAnimation(float t)
        {
            canBePooled = false;
            Invoke("ResetAnimTransform", t);
        }

        public void ResetAnimTransform()
        {
            canBePooled = true;
            itemAnimTransform.transform.localPosition = defaultTransformPosition;
            itemAnimTransform.transform.localScale = defaultTransformScale;
            itemAnimTransform.transform.localRotation = defaultTransformRotation;
        }

        /// <summary>
        /// check near diagonally square
        /// </summary>
        public void CheckNearEmptySquares()
        {
            var nearEmptySquareDetected = false;
            if (!square.CanGoOut() || LevelManager.THIS.StopFall) return;
            // if (square.nextSquare && square.nextSquare.Item && (square.nextSquare.Item.falling || square.nextSquare.Item.needFall || square.nextSquare.Item.destroying)) return;
            Vector2 lookingDirection1 = new Vector2(1, 1);
            float dirAngle = Vector2.Angle(Vector2.down, square.direction);
            dirAngle = Mathf.Sign(Vector3.Cross(lookingDirection1, square.direction).z) < 0 ? (360 - dirAngle) % 360 : dirAngle;
            lookingDirection1 = (Quaternion.Euler(0f, 0f, dirAngle) * lookingDirection1);
            //        if (square.row < LevelManager.This.levelData.maxRows - 1 && square.col < LevelManager.This.levelData.maxCols)
            {
                var checkingSquare = field.GetSquare(square.GetPosition() + lookingDirection1);
                if (checkingSquare && (!checkingSquare.IsItemAbove() && GetBarrierBefore(checkingSquare)))
                    nearEmptySquareDetected = CheckNearSquare(nearEmptySquareDetected, checkingSquare);
            }

            //        if (square.row < LevelManager.This.levelData.maxRows - 1 && square.col > 0)
            {
                var checkingSquare = field.GetSquare((Vector3)square.GetPosition() + Quaternion.Euler(0f, 0f, 90f) * lookingDirection1);
                if (checkingSquare && (!checkingSquare.IsItemAbove() && GetBarrierBefore(checkingSquare)))
                    nearEmptySquareDetected = CheckNearSquare(nearEmptySquareDetected, checkingSquare);
            }
            //        if(LevelManager.This.gameStatus == GameState.Playing)
            //            square.GetPreviousSquare()?.GetAllNeghborsCross().Where(i=>i!=square).Select(i=>i.Item).WhereNotNull().Where(i=>!i.destroying && !i.falling).ToList().ForEach(i=>i?.CheckNearEmptySquares());
        }

        private bool GetBarrierBefore(Square checkingSquare)
        {
            Square[] orderedEnumerable = checkingSquare.sequenceBeforeThisSquare;
            if (orderedEnumerable.Count() == 0) return true;
            foreach (var sq in orderedEnumerable)
            {
                if (!sq.IsFree()) return true;
                if (sq.isEnterPoint) return false;
                if (!sq.linkedEnterSquare) return true;
            }
            return false;
        }


        private bool CheckNearSquare(bool nearEmptySquareDetected, Square checkingSquare)
        {
            if (!checkingSquare.IsNone() && checkingSquare.CanGoInto() && checkingSquare.Item == null && !falling)
            {
                if ((checkingSquare.Item == null || (bool)checkingSquare.Item?.destroying ||
                     (bool)checkingSquare.Item?.falling) && checkingSquare.GetItemAbove() == null)
                {
                    square.Item = null;
                    previousSquare = square;
                    checkingSquare.Item = this;
                    square = checkingSquare;
                    needFall = true;
                    StartFallingSides();
                    nearEmptySquareDetected = true;
                }
            }

            return nearEmptySquareDetected;
        }

        public Item GetLeftItem()
        {
            var sq = square.GetNeighborLeft();
            if (sq != null)
            {
                if (sq.Item != null)
                    return sq.Item;
            }

            return null;
        }

        public Item GetTopItem()
        {
            var sq = square.GetNeighborTop();
            if (sq != null)
            {
                if (sq.Item != null)
                    return sq.Item;
            }

            return null;
        }
        /// <summary>
        /// Change item type methods
        /// </summary>
        /// <param name="newType"></param>
        public void SetType(ItemsTypes newType)
        {
            NextType = newType;
            ChangeType();
        }

        public void ChangeType(Action<Item> callback=null, bool dontDestroyForThisCombine = true)
        {
            if ( NextType != ItemsTypes.NONE )
            {
  
                // UndestroyableForThisCombine = true;
            }
        }


        /// <summary>
        /// animation event trigger after destroy
        /// </summary>
        public void SetAnimationDestroyingFinished()
        {
            animationFinished = true;
        }

        /// <summary>
        /// hide item
        /// </summary>
        /// <param name="hide"></param>
        public void Hide(bool hide)
        {
            gameObject.SetActive(!hide);
        }
        /// <summary>
        /// hide sprites
        /// </summary>
        /// <param name="hide"></param>
        public void HideSprites(bool hide)
        {
            GetComponentsInChildren<SpriteRenderer>().ForEachY(i => i.enabled = !hide);
        }

        #region Destroying

        void OnNextMove()
        {
            dontDestroyOnThisMove = false;
            LevelManager.OnTurnEnd -= OnNextMove;
        }

        [HideInInspector] public Item explodedItem;
        private bool destroycoroutineStarted;

        /// <summary>
        /// destroying item 
        /// </summary>
        /// <param name="showScore"></param>
        /// <param name="particles"></param>
        /// <param name="explodedItem"></param>
        /// <param name="explEffect"></param>

        public void DestroyItem(bool showScore = false, bool particles = true, Item explodedItem = null, bool explEffect = false)
        {
            if (!gameObject.activeSelf) return;
            if (square.type == SquareTypes.WireBlock)
            {
                square.DestroyBlock();
                StopDestroy();
                return;
            }



            if (currentType == ItemsTypes.INGREDIENT && square.nextSquare != null) return;

            if (destroying) return;
            if (this == null) return;
            StopCoroutine(AnimIdleStart());
            destroying = true;
            // square.item = null;

            if(!destroycoroutineStarted)
                StartCoroutine(DestroyCor(showScore, particles, explodedItem, explEffect));
        }

        private IEnumerator DestroyCor(bool showScore = false, bool particles = true, Item explodedItem = null, bool explEffect = false)
        {
            destroycoroutineStarted = true;
            if (explodedItem != null)
                switchItem = explodedItem;
            if (explEffect || globalExplEffect)
            {
                var partcl1 =
                    Instantiate(Resources.Load("Prefabs/Effects/Replace"), transform.position,
                        Quaternion.identity) as GameObject;
                Destroy(partcl1, 1f);
            }

            if ((currentType == ItemsTypes.NONE && NextType == ItemsTypes.NONE || currentType == ItemsTypes.SPIRAL) && particles)
            {
                PlayDestroyAnimation("destroy");
                // HideSprites(true);
                var partcl2 = ObjectPooler.Instance.GetPooledObject("FireworkSplash");
                if (partcl2 != null)
                {
                    partcl2.transform.position = transform.position;
                }
            }
        
            Destroy(itemAnimTransform.GetComponent<Animator>());
            yield return new WaitForFixedUpdate();
        }

        public void StopDestroy()
        {
            destroying = false;
            destroyNext = false;
            destroycoroutineStarted = false;
        }

        private void PlayDestroyAnimation(string anim_name)
        {
            if (anim != null ) anim.SetTrigger(anim_name);
        }

        public void SmoothDestroy()
        {
            if(gameObject.activeSelf)
                StartCoroutine(SmoothDestroyCor());
        }

        private IEnumerator SmoothDestroyCor()
        {
            anim.SetTrigger("destroy");
            if(currentType == ItemsTypes.MULTICOLOR)
            {
                var partcl2 = ObjectPooler.Instance.GetPooledObject("FireworkSplashMulticolor");
                partcl2.transform.position = transform.position;
            }
            yield return new WaitForSeconds(0.5f);
            square.Item = null;
            HideSprites(true);
            DestroyBehaviour();
        }

        #endregion

        public Sprite GetSprite()
        {
            return GetComponent<SpriteRenderer>() != null
                ? GetComponent<SpriteRenderer>().sprite
                : transform.GetComponentInChildren<SpriteRenderer>()?.sprite;
        }

        public SpriteRenderer GetSpriteRenderer()
        {
            return sprRenderer.FirstOrDefault();
        }

        public FieldBoard GetField()
        {
            return field;
        }

        public class Waypoint
        {
            public Vector3 destPosition;
            public Square square;
            public bool instant;

            public Waypoint(Vector3 vector, Square _square)
            {
                destPosition = vector;
                square = _square;
            }
        }

        public Item DeepCopy()
        {
            var other = (Item)MemberwiseClone();
            return other;
        }

        public void OnColorChanged(int color)
        {
            COLOR = color;
        }

        public GameObject GetMarmaladeTarget
        {
            get => marmaladeTarget;
            set => marmaladeTarget = value;
        }

        public GameObject GetGameObject => gameObject;
        public Item GetItem => this;
    }
}