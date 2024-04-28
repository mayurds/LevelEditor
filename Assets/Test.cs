using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public List<Vector3> positions = new List<Vector3>();
    IEnumerator Start()
    {
        positions = new List<Vector3>();
        Vector2 pos = new Vector2(0, 0);
        Vector2 ogpos = new Vector2(0, 0);
        int mapSize = Mathf.Max(5, 5);
        int hexRadius = 1;

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
                positions.Add(ogpos);
                GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                gameObject.transform.position = ogpos;
                yield return new WaitForSeconds(1);
            }
        }
    }
   
}
