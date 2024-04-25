using System;
using System.Collections;
using System.Linq;
using UnityEngine;

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
                     chessColor = !chessColor;
                 }
             }
     
         
             SetPivot();
             SetPosY(0);
         }

         /// <summary>
         /// Set order for the squares sequience 
         /// </summary>
        

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
             pivot = new GameObject();
             pivot.name = "Pivot";
             pivot.transform.SetParent(transform);
         }

        
         public Vector2 GetPosition()
         {
             return pivot?.transform.position ?? Vector2.zero;
         }
     }
 }

