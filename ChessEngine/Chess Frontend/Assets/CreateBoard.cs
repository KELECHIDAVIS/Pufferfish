using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateBoard : MonoBehaviour
{
    int[] boardTiles = { 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1 };

    public GameObject darkTile, lightTile; 

    void Start()
    {
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                if (boardTiles[r * 8 + c] == 0)
                {
                    Vector3 pos = new Vector3(r, 0, c);
                    GameObject tile = Instantiate(lightTile, pos, Quaternion.identity);
                }
                else
                {
                    Vector3 pos = new Vector3(r, 0, c);
                    GameObject tile = Instantiate(darkTile, pos, Quaternion.identity);
                }
            }

        }

    }
}
