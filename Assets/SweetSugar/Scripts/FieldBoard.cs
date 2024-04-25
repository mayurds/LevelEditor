using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SweetSugar.Scripts.Blocks;
using SweetSugar.Scripts.Core;
using SweetSugar.Scripts.System;
using SweetSugar.Scripts.System.Pool;
using UnityEngine;
using Random = UnityEngine.Random;

 namespace SweetSugar.Scripts.Level
 {
     /// <summary>
     /// Field object
     /// </summary>
     public class FieldBoard : MonoBehaviour
     {
         // public SquareBlocks[] fieldData.levelSquares = new SquareBlocks[81];
         public GameObject squarePrefab;
         public Sprite squareSprite1;
         public GameObject outline1;
         public GameObject outline2;
         public GameObject outline3;

         // public int fieldData.maxRows = 9;
         // public int fieldData.maxCols = 9;
         public float squareWidth = 1.2f;
         public float squareHeight = 1.2f;
         public Vector2 firstSquarePosition;
         public Square[] squaresArray;
         public Transform GameField;
         public Hashtable countedSquares = new Hashtable();
         public FieldData fieldData;
         private GameObject pivot;

         public int enterPoints;

         // Use this for initialization
         private void OnEnable()
         {
             GameField = transform;
         }

         /// <summary>
         /// Initialize field
         /// </summary>
         public void CreateField()
         {
             var chessColor = false;
             //squaresArray = new Square[maxCols * maxRows];

             for (var row = 0; row < fieldData.maxRows; row++)
             {
                 if (fieldData.maxCols % 2 == 0)
                     chessColor = !chessColor;
                 for (var col = 0; col < fieldData.maxCols; col++)
                 {
                     CreateSquare(col, row, chessColor);
                     chessColor = !chessColor;
                 }
             }
             foreach (var i in squaresArray.ToList())
             { 
                 i.SetBorderDirection();
                 if(!i.IsNone())
                     i.SetOutline();
             }
             enterPoints = squaresArray.Count(i => i.isEnterPoint);
             foreach (var i in squaresArray.ToList())
             {
                 // i.SetMask(); 
             }
             SetOrderInSequence();
             foreach (var i in squaresArray.ToList())
             {
                 i.sequenceBeforeThisSquare = i.GetSeqBeforeFromThis();
                 if (i.sequence.All(x => !x.IsNone() && !x.undestroyable) && i.sequence.Any() && i.sequence.Any(x=>x.isEnterPoint))
                     i.linkedEnterSquare = true;
             }
             SetPivot();
             SetPosY(0);
         }

         /// <summary>
         /// Set order for the squares sequience 
         /// </summary>
         private void SetOrderInSequence()
         {
             var list = GetSquareSequence();
             foreach (var seq in list)
             {
                 var order = 0;
                 foreach (var sq in seq)
                 {
                     sq.orderInSequence = order;
                     sq.sequence = seq;
                     order++;
                 }
             }

         }


         private void SetPosY(int y)
         {
             transform.position = new Vector2(transform.position.x, transform.position.y + (y - GetPosition().y));
         }

         public void SetPosition(Vector2 pos)
         {
             transform.position = new Vector2(transform.position.x + (pos.x - GetPosition().x), transform.position.y + (pos.y - GetPosition().y));
         }

         private void SetPivot()
         {
             // transform.position = GetCenter();
             pivot = new GameObject();
             pivot.name = "Pivot";
             pivot.transform.SetParent(transform);
             pivot.transform.position = GetCenter();
             // foreach (var square in squaresArray)
             // {
             //     square.transform.SetParent(transform);
             //     if (square.item != null)
             //         square.item.transform.SetParent(transform);
             // }
         }

         private Vector2 GetCenter()
         {
             var minX = squaresArray.Min(x => x.transform.position.x);
             var minY = squaresArray.Min(x => x.transform.position.y);
             var maxX = squaresArray.Max(x => x.transform.position.x);
             var maxY = squaresArray.Max(x => x.transform.position.y);
             var pivotPosMin = new Vector2(minX, minY);
             var pivotPosMax = new Vector2(maxX, maxY);
             var pivotPos = pivotPosMin + (pivotPosMax - pivotPosMin) * 0.5f;
             return pivotPos;
         }

         public Vector2 GetPosition()
         {
             return pivot?.transform.position ?? Vector2.zero;
         }

   

    




         /// <summary>
         /// Get squares sequence from first to end
         /// </summary>
         /// <returns></returns>
         public List<List<Square>> GetSquareSequence()
         {
             var enterSquares = squaresArray.Where(i => i.isEnterPoint);
             var listofSequences = ListofSequences(enterSquares);
             var l = listofSequences.SelectMany(i => i);
             var squaresNotJoinEnter = squaresArray.Where(i => !l.Contains(i));
             var topSquares = new List<Square>();
        
        
             listofSequences.AddRange( ListofSequences(topSquares));
             return listofSequences;
         }

         /// <summary>
         /// Get list of all squares sequences
         /// </summary>
         /// <param name="enterSquares"></param>
         /// <returns></returns>
         private List<List<Square>> ListofSequences(IEnumerable<Square> enterSquares)
         {
             var listofSequences = new List<List<Square>>();
             foreach (var enterSquare in enterSquares)
             {
                 var sequence = new List<Square>();
                 sequence.Add(enterSquare);
                 sequence = GetSquareSequenceStep(sequence);
                 sequence.Reverse();
                 listofSequences.Add(sequence);
             }

             return listofSequences;
         }

         /// <summary>
         /// Get square sequece
         /// </summary>
         /// <param name="sequence"></param>
         /// <returns></returns>
         private List<Square> GetSquareSequenceStep(List<Square> sequence)
         {
             var nextSquare = sequence.LastOrDefault().GetNextSquare();
             if (nextSquare != null && !nextSquare.IsNone())
             {
                 sequence.Add(nextSquare);
                 sequence = GetSquareSequenceStep(sequence);
             }
             return sequence;
         }

         /// <summary>
         /// Get sequence of the square
         /// </summary>
         /// <param name="square"></param>
         /// <returns></returns>
         public List<Square> GetCurrentSequence(Square square)
         {
             return GetSquareSequence().Where(i => i.Any(x => x == square)).SelectMany(i => i).ToList();
         }

         /// <summary>
         /// Create a square
         /// </summary>
         /// <param name="col">column</param>
         /// <param name="row">row</param>
         /// <param name="chessColor">color switch</param>
         private void CreateSquare(int col, int row, bool chessColor = false)
         {
             GameObject squareObject = null;
             var squareBlock = fieldData.levelSquares[row * fieldData.maxCols + col];
             squareObject = Instantiate(squarePrefab, firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight), Quaternion.identity);
             if (chessColor)
             {
                 squareObject.GetComponent<SpriteRenderer>().sprite = squareSprite1;
             }
             squareObject.transform.SetParent(GameField);//set parent later
             squareObject.transform.localPosition = firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight);
             var square = squareObject.GetComponent<Square>();
             squaresArray[row * fieldData.maxCols + col] = square;
             square.field = this;
             square.row = row;
             square.col = col;
             square.type = SquareTypes.EmptySquare;

         }

         /// <summary>
         /// Get bottom row
         /// </summary>
         /// <returns>returns list of squares</returns>
         public List<Square> GetBottomRow()
         {
             var itemsList = GetSquareSequence().Select(i => i.FirstOrDefault()).Where(i => i.type != SquareTypes.NONE).ToList();
             return itemsList;
         }

         /// <summary>
         /// Get field rect
         /// </summary>
         /// <returns>rect</returns>
         public Rect GetFieldRect()
         {
             var square = GetSquare(0, 0);
             var squareRightBottom = GetSquare(fieldData.maxCols - 1, fieldData.maxRows - 1);
             return new Rect(square.transform.position.x, square.transform.position.y, squareRightBottom.transform.position.x - square.transform.position.x, square.transform.position.y - squareRightBottom.transform.position.y);
         }

         /// <summary>
         /// Get squares from a rect
         /// </summary>
         /// <param name="rect">rect</param>
         public List<Square> GetFieldSeqment(RectInt rect)
         {
             List<Square> squares = new List<Square>();
             for (int row = rect.yMin; row <= rect.yMax; row++)
             {
                 for (int col = rect.xMin; col <= rect.xMax; col++)
                 {
                     squares.Add(GetSquare(col, row));
                 }
             }
             return squares;
         }

         /// <summary>
         /// Get top row
         /// </summary>
         /// <returns>list of squares</returns>
         public List<Square> GetTopRow()
         {
             return squaresArray.Where(i => !i.IsNone()).GroupBy(i => i.col).Select(i => new { Sq = i.OrderBy(x => x.row).First() }).Select(i => i.Sq).ToList();
         }

         public List<Square> GetSimpleItemsInRow(int count)
         {

             var list = squaresArray
                 .Where(i => i.GetSubSquare().IsFree())
                 .Where(y => y.GetVerticalNeghbors().Count(z => z.IsFree()) == 2)
                 .Select(x => new { Index = x.row, Value = x })
                 .GroupBy(i => i.Index)
                 .Select(i => i.Select(x => x.Value).Take(count).ToList())
                 .ToArray();

             var v1 = list.GetValue(Random.Range(0, list.Length)) as List<Square>;
             return v1;
         }

         public List<Square> GetSquares(bool withUndestroyble = false)
         {
             var list = new List<Square>();
             foreach (var item in squaresArray)
             {
                 if (withUndestroyble && item.GetSubSquare().IsObstacle() && item.GetSubSquare().undestroyable)
                     list.Add(item);
                 else
                     list.Add(item);

             }
             return list;
         }

         public Square GetSquare(Vector2 vector)
         {
             return GetSquare(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
         }

         public Square GetSquare(int col, int row, bool safe = false)
         {
             if (!safe)
             {
                 if (row >= fieldData.maxRows || col >= fieldData.maxCols || row < 0 || col < 0)
                     return null;
                 return squaresArray[row * fieldData.maxCols + col];
             }

             row = Mathf.Clamp(row, 0, fieldData.maxRows - 1);
             col = Mathf.Clamp(col, 0, fieldData.maxCols - 1);
             return squaresArray[row * fieldData.maxCols + col];
         }




         /// <summary>
         /// Get squares of particular type
         /// </summary>
         /// <param name="type"></param>
         /// <returns></returns>
         public int CountSquaresByType(string type)
         {
             var squareType = (SquareTypes)Enum.Parse(typeof(SquareTypes), type.Replace("SweetSugar.Scripts.TargetScripts.",""));

             return /*squaresArray.Count(item => item.type == squareType) + */squaresArray.WhereNotNull().SelectMany(i => i.subSquares).Count(item => item.type == squareType);
         }


         /// <summary>
         /// Get top square in a column
         /// </summary>
         /// <param name="col">column</param>
         /// <returns></returns>
         public Square GetTopSquareInCol(int col)
         {
             return squaresArray.Where(i => i.IsFree() && i.col == col).OrderBy(i => i.row).FirstOrDefault();
         }

         /// <summary>
         /// Get items by color
         /// </summary>
         /// <param name="color"></param>
         /// <returns></returns>
   

        

         public FieldBoard DeepCopy()
         {
             var other = (FieldBoard)MemberwiseClone();
             other.squaresArray = new Square[fieldData.maxCols * fieldData.maxRows];
             for (var i = 0; i < squaresArray.Count(); i++)
             {
                 var square = squaresArray[i];
                 other.squaresArray[i] = square.DeepCopy();
             }
             return other;
         }

     }
 }

