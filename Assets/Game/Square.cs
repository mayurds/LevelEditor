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

   
        public bool IsDirectionRestricted(Vector2 dir)
        {
            foreach (var restriction in directionRestriction)
            {
                if (restriction == dir) return true;
            }
            return false;
        }
       
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
        }

        public void SetDontDestroyOnMove()
        {
            dontDestroyOnThisMove = true;
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

     

        public List<Vector2> directionRestriction = new List<Vector2>();
        public int orderInSequence; //latest square has 0, enter square has last number
        public List<Square> sequence = new List<Square>();
        private int destroyIteration;
        private bool dontDestroyOnThisMove;
        public Square[] sequenceBeforeThisSquare;
        public GameObject border;
        protected Square mainSquqre;
        private GameObject marmaladeTarget;

  
        public FieldBoard GetField()
        {
            return field;
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