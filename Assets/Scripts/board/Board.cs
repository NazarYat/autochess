using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

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
    public Cell[,] Cells => CellGenerator.cells;

    public List<FigureBase> Figures { get; private set; } = new List<FigureBase>();
    public int ActionDurationMilliseconds = 1000;
    public int InitialBalane = 100;
    public Inventory Inventory;
    public Text TimerText;
    public Text RoundText;
    public GameObject ShopGameObject;

    public bool IsGameActive = false;
    private int _roundNumber = 0;
    public int RoundNumber
    {
        get => _roundNumber;
        set
        {
            if (_roundNumber == value) return;
            _roundNumber = value;
            if (RoundText != null)
                RoundText.text = $"Round: {value}";
        }
    }
    private bool _isPlacingFigure = false;
    public bool IsPlacingFigure
    {
        get => _isPlacingFigure;
        set
        {
            if (_isPlacingFigure == value) return;
            _isPlacingFigure = value;

            foreach (var c in Cells)
            {
                c.IsSpawnPointActive = value;
            }
        }
    }
    private int[] PlayersScore = new int[2] { 0, 0 };
    public Text[] PlayerScoreTexts;

    public Cell[] GetCellsInTouch(int x, int y, int n = 1)
    {
        List<Cell> cells = new List<Cell>();

        for (int dx = -n; dx <= n; dx++)
        {
            for (int dy = -n; dy <= n; dy++)
            {
                // Skip the center cell itself
                if (dx == 0 && dy == 0)
                    continue;

                int newX = x + dx;
                int newY = y + dy;

                // Check bounds
                if (newX >= 0 && newX < SizeX && newY >= 0 && newY < SizeY)
                {
                    if (Cells[newX, newY].Figure != null)
                    {
                        cells.Add(Cells[newX, newY]);
                    }
                }
            }
        }

        return cells.ToArray();
    }
    
    public void StartGame()
    {
        RoundNumber++;
        if (RoundNumber == 1)
        {
            Inventory.AddMoney(InitialBalane);
        }
        IsGameActive = true;
        StartShopPeriod();
    }
    public void StartShopPeriod()
    {
        ShopGameObject.SetActive(true);
        Inventory.Reset();
        Inventory.CanUpgradeFigures = true;

        StartCoroutine(Timer(15, () =>
        {
            ShopGameObject.SetActive(false);
            StartBattlePeriod();
        }));
    }
    public void StartBattlePeriod()
    {
        Inventory.CanUpgradeFigures = false;
        Inventory.CanPalceFigures = true;

        foreach (var figure in Figures)
        {
            Destroy(figure.gameObject);
        }

        Figures.Clear();

        ShopGameObject.SetActive(false);

        if (Inventory.Figures.Count() == 0)
        {
            while (true)
            {
                var item = ShopGameObject.GetComponent<Shop>().GetRandomItem(Inventory.Money);

                if (item == null)
                {
                    break;
                }

                Inventory.AddItem(item);
            }
        }


        StartCoroutine(Tick());
        StartCoroutine(Bot());

        StartCoroutine(Timer(180, () =>
        {
            ProcessGameEnd();
        }));
    }

    public IEnumerator Timer(int seconds, Action timeoutCallBack)
    {
        while (seconds > 0)
        {
            yield return new WaitForSeconds(1);
            seconds--;

            TimerText.text = $"{(seconds / 60).ToString()}:{(seconds % 60).ToString()}";
        }
        timeoutCallBack?.Invoke();

        yield break;

    }
    public IEnumerator Tick()
    {
        while (IsGameActive)
        {
            foreach (var figure in Figures)
            {
                figure.Action(ActionDurationMilliseconds);
            }
            
            yield return new WaitForSeconds(ActionDurationMilliseconds / 1000.0f);

            if (ShouldStopGame())
            {
                yield return new WaitForSeconds(1);
                ProcessGameEnd();
                yield break;
            }
        }
        yield return new WaitForSeconds(1);
        ProcessGameEnd();
        yield break;
    }

    private bool ShouldStopGame()
    {
        return Figures.GroupBy(f => f.PlayerIndex).Count() <= 1 && Inventory.UnusedFigures.Count() == 0;
    }

    private void ProcessGameEnd()
    {
        Debug.Log("GameOver");
        IsGameActive = false;
        Inventory.CanPalceFigures = false;

        // process winner here

        var winnerIndex = Figures.GroupBy(x => x.PlayerIndex)
            .OrderByDescending(x => x.Sum(y => y.Health))
            .FirstOrDefault().Key;

        PlayersScore[winnerIndex] += 1;

        PlayerScoreTexts[winnerIndex].text = PlayersScore[winnerIndex].ToString();

        
        StopAllCoroutines();
        StartGame();
    }
    public void RegisterFigureDeath(FigureBase figure)
    {
        Figures.Remove(figure);
        if (figure.PlayerIndex == 0)
        {
            Debug.Log($"Earned money: {figure.FigureData.UpgradePrice}");
            Inventory.AddMoney(figure.FigureData.UpgradePrice);
        }
        Destroy(figure.gameObject);
    }
    public IEnumerator Bot()
    {
        var playerFigures = new List<ShopItemData>(Inventory.Figures);

        while (IsGameActive)
        {
            var x = UnityEngine.Random.Range(0, SizeX);
            var y = SizeY - UnityEngine.Random.Range(0, 2) - 1;

            var cell = Cells[x, y];

            if (cell.Figure == null && playerFigures.Count() > 0)
            {
                var figureIndex = 0;
                
                figureIndex = UnityEngine.Random.Range(0, playerFigures.Count());

                if (cell.PlaceFigure(playerFigures[figureIndex].CreateInstance(cell.transform, 0)))
                {
                    playerFigures.Remove(playerFigures[figureIndex]);
                }
            }

            yield return new WaitForSeconds(UnityEngine.Random.Range(1, 1));
        }

        yield break;
    }
    // Start is called before the first frame update
    void Start()
    {
        CellGenerator.GenerateCells();
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
