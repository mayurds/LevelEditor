using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SweetSugar.Scripts.Core;
using SweetSugar.Scripts.Level;
using SweetSugar.Scripts.System;
using SweetSugar.Scripts.System.Pool;
using UnityEngine;

namespace SweetSugar.Scripts.Blocks
{
    public enum FindSeparating
    {
        NONE = 0,
        HORIZONTAL,
        VERTICAL
    }

//TODO: Add new target

    public enum SquareTypes
    {
        NONE = 0,
        EmptySquare,
        SugarSquare,
        WireBlock,
        SolidBlock,
        ThrivingBlock,
        JellyBlock,
        SpiralBlock,
        UndestrBlock
    }
    /// <summary>
    /// Handles square behaviour like destroying, generating item if the square empty, determines items directions, and obstacles behaviour
    /// </summary>
    public class Square : MonoBehaviour
    {
        [Header("Score for destroying")]
        public int score;
        /// Item occupies this square
        /// position on the field
        public int row;
        public int col;
        /// sqaure type
        public SquareTypes type;
        /// sprite of border
        private Sprite borderSprite;

        [Header("can item move inside the square")]
        public bool canMoveIn;
        [Header("can item fall out of the square")]
        public bool canMoveOut;
        [Header("can be the square destroyed")]
        public bool undestroyable;
        /// EDITOR: direction of items
        [HideInInspector] public Vector2 direction;
        /// EDITOR: true - square is begging of the items flow
        [HideInInspector] public bool isEnterPoint;
        ///EDITOR: enter square for current sequence of the squares, Sequence is array of squares along with direction (all squares by column)
        [HideInInspector] public Square enterSquare;
        /// Next square by direction flow
        [HideInInspector] public Square nextSquare;
        /// current field
        [HideInInspector] public FieldBoard field;
        /// mask for the top squares
        public GameObject mask;
        /// teleport effect gameObject
        public GameObject teleportEffect;
        /// teleport square object

        /// subsquares of the current square, obstacle or jelly
        [HideInInspector] public List<Square> subSquares = new List<Square>();
        /// if true - this square has enter square upper by items flow
        [HideInInspector] public bool linkedEnterSquare;
        private void Awake()//init some objects
        {
            GetInstanceID();
            border = Resources.Load<GameObject>("Border");
        }

        // Use this for initialization
        void Start()//init some objects
        {
            name = "Square_" + col + "_" + row;
            if (( direction != Vector2.down) && enterSquare && type != SquareTypes.NONE)
            {
                if (orderInSequence == 0)
                    CreateArrow(isEnterPoint);
                else if (direction != Vector2.down && isEnterPoint)
                    CreateArrow(isEnterPoint);
            }
        }
        /// <summary>
        /// Create animated arrow for bottom row
        /// </summary>
        /// <param name="enterPoint"></param>
        void CreateArrow(bool enterPoint)
        {
            var obj = Instantiate(Resources.Load("Prefabs/Arrow")) as GameObject;
            obj.transform.SetParent(transform);
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = Vector3.zero + Vector3.down * 0.5f;
            if(enterPoint)
                obj.transform.localPosition = Vector3.zero - Vector3.down * 2f;
            var angle = Vector3.Angle(Vector2.down, direction);
            angle = Mathf.Sign(Vector3.Cross(Vector2.down, direction).z) < 0 ? (360 - angle) % 360 : angle;
            Vector2 pos = obj.transform.localPosition;
            pos = Quaternion.Euler(0, 0, angle) * pos;
            obj.transform.localPosition = pos;
            obj.transform.rotation = Quaternion.Euler(0, 0, angle);
            ParticleSystem.MainModule mainModule = obj.GetComponent<ParticleSystem>().main;
            mainModule.startRotation = -angle * Mathf.Deg2Rad;
        }
        /// <summary>
        /// Check is the square is empty
        /// </summary>
        /// <returns></returns>
   
        private bool IsTypeExist(SquareTypes _type)
        {
            return subSquares.Count(i => i.type == _type) > 0;
        }
     
        /// <summary>
        /// generate spiral item
        /// </summary>
        /// <summary>
        /// check is which direction is restricted
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public bool IsDirectionRestricted(Vector2 dir)
        {
            foreach (var restriction in directionRestriction)
            {
                if (restriction == dir) return true;
            }
            return false;
        }
        /// <summary>
        /// Get neighbor methods
        /// </summary>
        /// <param name="considerRestrictions"></param>
        /// <param name="safe"></param>
        /// <returns></returns>
        public Square GetNeighborLeft(bool considerRestrictions = true, bool safe = false)
        {
            if (considerRestrictions && (IsDirectionRestricted(Vector2.left))) return null;
            if (col == 0 && !safe)
                return null;
            var square = field.GetSquare(col - 1, row, safe);
            // if (considerRestrictions && (square?.IsNone() ?? false)) return null;
            return square;
        }

        public Square GetNeighborRight(bool considerRestrictions = true, bool safe = false)
        {
            if (considerRestrictions && (IsDirectionRestricted(Vector2.right))) return null;
            if (col >= field.fieldData.maxCols && !safe)
                return null;
            var square = field.GetSquare(col + 1, row, safe);
            // if (considerRestrictions && (square?.IsNone() ?? false)) return null;
            return square;
        }

        public Square GetNeighborTop(bool considerRestrictions = true, bool safe = false)
        {
            if (considerRestrictions && (IsDirectionRestricted(Vector2.up))) return null;
            if (row == 0 && !safe)
                return null;
            var square = field.GetSquare(col, row - 1, safe);
            // if (considerRestrictions && (square?.IsNone() ?? false)) return null;
            return square;
        }

        public Square GetNeighborBottom(bool considerRestrictions = true, bool safe = false)
        {
            if (considerRestrictions && (IsDirectionRestricted(Vector2.down))) return null;
            if (row >= field.fieldData.maxRows && !safe)
                return null;
            var square = field.GetSquare(col, row + 1, safe);
            // if (considerRestrictions && (square?.IsNone() ?? false)) return null;
            return square;
        }

        /// <summary>
        /// Get next square along with direction
        /// </summary>
        /// <param name="safe"></param>
        /// <returns></returns>
        public Square GetNextSquare(bool safe = false)
        {
            return nextSquare;
        }
 
        /// <summary>
        /// Get squares sequence before this square
        /// </summary>
        /// <returns></returns>
        public Square[] GetSeqBeforeFromThis()
        {
            var sq = sequence;//field.GetCurrentSequence(this);
            return sq.Where(i => i.orderInSequence > orderInSequence).OrderBy(i => i.orderInSequence).ToArray();
        }
     
        /// <summary>
        /// Get item above the square by flow
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// Get reverse direction
        /// </summary>
        /// <returns></returns>
        public Vector2 GetReverseDirection()
        {
            Vector3 pos = Vector2.up;
            if (direction == Vector2.down) pos = Vector2.up;
            if (direction == Vector2.up) pos = Vector2.down;
            if (direction == Vector2.left) pos = Vector2.right;
            if (direction == Vector2.right) pos = Vector2.left;
            return pos;
        }


    
 
        /// <summary>
        /// true if square disabled in editor
        /// </summary>
        /// <returns></returns>
        public bool IsNone()
        {
            return type == SquareTypes.NONE;
        }
        /// <summary>
        /// true if square available and non Undestroyable
        /// </summary>
        /// <returns></returns>
        public bool IsAvailable()
        {
            return !IsNone() && !IsUndestroyable();
        }
        /// <summary>
        /// true if has destroyable obstacle
        /// </summary>
        /// <returns></returns>
        public bool IsHaveDestroybleObstacle()
        {
            return !IsUndestroyable() && (!GetSubSquare()?.canMoveIn ?? false) ;
        }
        /// <summary>
        /// true if the square has obstacle
        /// </summary>
        /// <returns></returns>
        public bool IsObstacle()
        {
            return (!GetSubSquare().CanGoInto() || !GetSubSquare().CanGoOut() || IsUndestroyable());
        }

        public bool IsUndestroyable()
        {
            return GetSubSquare()?.undestroyable?? false;
        }

        /// <summary>
        /// true if the square available for Item
        /// </summary>
        /// <returns></returns>
        public bool IsFree()
        {
            return (!IsNone() && !IsObstacle());
        }
        /// <summary>
        /// Can item fall out of the square
        /// </summary>
        /// <returns></returns>
        public bool CanGoOut()
        {
            return (GetSubSquare()?.canMoveOut ?? false) && (!IsUndestroyable()) ;
        }
        /// <summary>
        /// can item fall to the square
        /// </summary>
        /// <returns></returns>
        public bool CanGoInto()
        {
            return (GetSubSquare()?.canMoveIn ?? false) && (!IsUndestroyable()) ; //TODO: none square falling through
        }
        /// <summary>
        /// Next move event
        /// </summary>
        void OnNextMove()
        {
            dontDestroyOnThisMove = false;
            LevelManager.OnTurnEnd -= OnNextMove;
        }

        public void SetDontDestroyOnMove()
        {
            dontDestroyOnThisMove = true;
            LevelManager.OnTurnEnd += OnNextMove;
        }
        /// <summary>
        /// destroy block (obstacle or jelly)
        /// </summary>
        public void DestroyBlock()
        {
            if (destroyIteration == 0)
            {
                destroyIteration = LevelManager.THIS.destLoopIterations;
            }
            else return;
            if (IsUndestroyable())
                return;
       

     
            if (subSquares.Count > 0)
            {
                var subSquare = GetSubSquare();
                if (type == SquareTypes.JellyBlock) return;
           

                subSquares.Remove(subSquare);
                Destroy(subSquare.gameObject);
                subSquare = GetSubSquare();
                if (subSquares.Count > 0)
                    type = subSquare.type;

                if (subSquares.Count == 0)
                    type = SquareTypes.EmptySquare;

            }
        }
        /// <summary>
        /// Get sub-square (i.e. layered obstacles)
        /// </summary>
        /// <returns></returns>
        public Square GetSubSquare()
        {
            if (subSquares.Count == 0)
                return this;

            return subSquares?.LastOrDefault();
        }
        /// <summary>
        /// Get group of squares for cloud animation on different direction levels
        /// </summary>
        /// <param name="groups"></param>
        /// <param name="group"></param>
        /// <param name="forPair"></param>
        /// <returns></returns>
        public List<List<Square>> GetGroupsSquare(List<List<Square>> groups, List<Square> group = null, bool forPair = true)
        {
            var list = GetAllNeghborsCross();
            if (forPair)
            {
                list = list.Where(i => i.direction == direction).ToList();
                if (direction.y == 0)
                    list = list.Where(i => i.col == col).ToList();
                else
                    list = list.Where(i => i.row == row).ToList();
            }
            else
            {
                list = list.Where(i => i.direction == direction).ToList();
            }

            if (group == null)
            {
                foreach (var sq in list)
                {
                    group = groups.Find(i => i.Contains(sq));
                }
                if (group == null) { group = new List<Square>(); groups.Add(group); }
            }
            if (!group.Contains(this))
                group.Add(this);
            list.RemoveAll(i => group.Any(x => x.Equals(i)));
            foreach (var sq in list)
            {
                groups = sq.GetGroupsSquare(groups, group);
            }
            return groups;
        }


        // [HideInInspector]
        public List<Square> squaresGroup;

        /// <summary>
        /// Get square position on the field
        /// </summary>
        /// <returns></returns>
        public Vector2 GetPosition()
        {
            return new Vector2(col, row);
        }

        /// <summary>
        /// check square is bottom 
        /// </summary>
        /// <returns></returns>
        public bool IsBottom()
        {
            return row == LevelManager.THIS.field.fieldData.maxRows - 1;
        }
    
        public List<Square> GetVerticalNeghbors()
        {
            var sqList = new List<Square>();
            Square nextSquare = null;
            nextSquare = GetNeighborBottom();
            if (nextSquare != null)
                sqList.Add(nextSquare);
            nextSquare = GetNeighborTop();
            if (nextSquare != null)
                sqList.Add(nextSquare);
            return sqList;
        }

        public List<Square> GetAllNeghborsCross()
        {
            var sqList = new List<Square>();
            Square nextSquare = null;
            nextSquare = GetNeighborBottom();
            if (nextSquare != null && !nextSquare.IsNone())
                sqList.Add(nextSquare);
            nextSquare = GetNeighborTop();
            if (nextSquare != null && !nextSquare.IsNone())
                sqList.Add(nextSquare);
            nextSquare = GetNeighborLeft();
            if (nextSquare != null && !nextSquare.IsNone())
                sqList.Add(nextSquare);
            nextSquare = GetNeighborRight();
            if (nextSquare != null && !nextSquare.IsNone())
                sqList.Add(nextSquare);
            return sqList;
        }
    
        /// <summary>
        /// Have square solid obstacle above, used for diagonally falling items animation
        /// </summary>
        /// <returns></returns>
        public bool IsHaveSolidAbove()
        {
            if (isEnterPoint) return false;
            var seq = sequenceBeforeThisSquare;
            return seq.Any(square => square != this && (square.GetSubSquare().CanGoOut() == false || square.GetSubSquare().CanGoInto() == false ||
                                                        IsUndestroyable() || square.IsNone()));
        }
   

        public Sprite GetSprite()
        {
            if (subSquares.Count > 0)
                return subSquares.Last().GetComponent<SpriteRenderer>().sprite;
            return GetComponent<SpriteRenderer>().sprite;
        }

        public SpriteRenderer GetSpriteRenderer()
        {
            if (subSquares.Count > 0)
                return subSquares.Last().GetComponent<SpriteRenderer>();
            return GetComponent<SpriteRenderer>();
        }

        public void SetBorderDirection()
        {
            var square = GetNeighborRight();
            if(IsNone()) return;
            if (direction + (square?.direction ?? direction) == Vector2.zero && (!square?.IsNone() ?? false))
            {
                SetBorderDirection(Vector2.right);
            }
            square = GetNeighborBottom();
            if (direction + (square?.direction ?? direction) == Vector2.zero && (!square?.IsNone() ?? false))
            {
                SetBorderDirection(Vector2.down);
            }

        }

        public List<Vector2> directionRestriction = new List<Vector2>();
        public int orderInSequence; //latest square has 0, enter square has last number
        public List<Square> sequence = new List<Square>();
        private int destroyIteration;
        private bool dontDestroyOnThisMove;
        public Square[] sequenceBeforeThisSquare;
        public GameObject border;
        protected Square mainSquqre;
        private GameObject marmaladeTarget;

        private void SetBorderDirection(Vector2 dir)
        {
            var border = new GameObject();
            var spr = border.AddComponent<SpriteRenderer>();
            spr.sortingOrder = 1;
            spr.sprite = borderSprite;
            border.transform.SetParent(transform);
            border.transform.localScale = Vector3.one;
            if (dir != Vector2.down && dir != Vector2.up) border.transform.rotation = Quaternion.Euler(0, 0, 90);
            border.transform.localPosition = Vector2.zero + dir * (LevelManager.THIS.squareWidth + 0.1f);
            SetSquareRestriction(dir);
        }

        public void SetSquareRestriction(Vector2 dir)
        {
            directionRestriction.Add(dir);
            if (dir == Vector2.right)
                GetNeighborRight(false)?.SetSquareRestriction(Vector2.left);
            if (dir == Vector2.down)
                GetNeighborBottom(false)?.SetSquareRestriction(Vector2.up);
        }

        public FieldBoard GetField()
        {
            return field;
        }

 
  

        public void SetOutline()
        {
            if (GetNeighborBottom()?.IsNone() ?? true) Instantiate(border, transform.position, Quaternion.Euler(0, 0, 90), transform);
            if (GetNeighborTop()?.IsNone() ?? true) Instantiate(border, transform.position, Quaternion.Euler(0, 0, -90), transform);
            if (GetNeighborLeft()?.IsNone() ?? true) Instantiate(border, transform.position, Quaternion.Euler(0, 0, 0), transform);
            if (GetNeighborRight()?.IsNone() ?? true) Instantiate(border, transform.position, Quaternion.Euler(0, 0, 180), transform);
        }

        public Square DeepCopy()
        {
            var other = (Square)MemberwiseClone();
            return other;
        }
        
        public GameObject GetMarmaladeTarget
        {
            get => marmaladeTarget;
            set => marmaladeTarget = value;
        }
        public GameObject GetGameObject => gameObject;
    }
}