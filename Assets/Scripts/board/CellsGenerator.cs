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

    public void GenerateCells()
    {
        cells = new GameObject[rows, columns];

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                GameObject cell = Instantiate(cellPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                cell.transform.localScale = new Vector3(cellSize - cellOffset, 1, cellSize - cellOffset);
                cell.transform.parent = this.transform;
                cell.transform.localPosition = new Vector3(x * cellSize + (cellSize * 0.5f), 0, y * cellSize + (cellSize * 0.5f));

                if (y > 1)
                {
                    cell.GetComponent<MeshRenderer>().enabled = false;
                }

                cells[x, y] = cell;
            }
        }
    }

    void Update()
    {

    }
}