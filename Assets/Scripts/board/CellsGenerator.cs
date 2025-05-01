using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellsGenerator : MonoBehaviour
{
    public GameObject cellPrefab;
    public int rows = 5;
    public int columns = 5;
    public float cellSize = 1.0f;
    public GameObject[,] cells;
    public float cellOffset = 0.1f;
    void Start()
    {
        cells = new GameObject[rows, columns];
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                Vector3 position = new Vector3(x * cellSize, 0, y * cellSize);
                GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity);
                cell.transform.localScale = new Vector3(cellSize - cellOffset, 1, cellSize - cellOffset);
                cells[x, y] = cell;
            }
        }
    }

    void Update()
    {
        
    }
}
