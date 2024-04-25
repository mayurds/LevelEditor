using System.Collections;
using UnityEngine;

     public class FieldBoard : MonoBehaviour
     {
         public GameObject squarePrefab;
         public Sprite squareSprite1;
         public float squareWidth = 1.2f;
         public float squareHeight = 1.2f;
         public Vector2 firstSquarePosition;
         public Transform GameField;
         public Hashtable countedSquares = new Hashtable();
         public int enterPoints;
         private void OnEnable()
         {
             GameField = transform;
         }
     }

