using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Board : MonoBehaviour
{
    public CellsGenerator CellGenerator;

    private int _sizeX = 8;
    public int SizeX
    {
        get => _sizeX;
        set
        {
            if (_sizeX == value) return;
            _sizeX = value;
            CellGenerator.rows = value;
        }
    }
    private int _sizeY = 8;
    public int SizeY
    {
        get => _sizeY;
        set
        {
            if (_sizeY == value) return;
            _sizeY = value;
            CellGenerator.columns = value;
        }
    }
    public GameObject[,] Cells => CellGenerator.cells;

    public List<FigureBase> Figures { get; private set; } = new List<FigureBase>();
    public int ActionDurationMilliseconds = 1000;

    public IEnumerable Tick()
    {
        while (true)
        {
            foreach (var figure in Figures)
            {
                figure.Action(ActionDurationMilliseconds);
            }
            
            yield return new WaitForSeconds(ActionDurationMilliseconds);

            if (Figures.GroupBy(figure => figure.PlayerIndex).Count() <= 0)
            {
                ProcessGameEnd();
            }
        }
    }

    private void ProcessGameEnd()
    {
        foreach (var figure in Figures)
        {
            Destroy(figure.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        CellGenerator.GenerateCells();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
