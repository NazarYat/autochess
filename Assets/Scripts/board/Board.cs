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

    public void StartGame()
    {
        RoundNumber++;
        Inventory.AddMoney(InitialBalane);
        IsGameActive = true;
        StartShopPeriod();
    }
    public void StartShopPeriod()
    {
        ShopGameObject.SetActive(true);
        Inventory.CanUpgradeFigures = true;

        StartCoroutine(Timer(10, () =>
        {
            ShopGameObject.SetActive(false);
            StartBattlePeriod();
        }));
    }
    public void StartBattlePeriod()
    {
        Inventory.CanUpgradeFigures = false;
        Inventory.CanPalceFigures = true;
        ShopGameObject.SetActive(false);
        StartCoroutine(Tick());
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

            TimerText.text = seconds.ToString();
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
            
            yield return new WaitForSeconds(ActionDurationMilliseconds);

            if (Figures.GroupBy(figure => figure.PlayerIndex).Count() <= 0)
            {
                ProcessGameEnd();
            }
        }
        yield break;
    }

    private void ProcessGameEnd()
    {
        IsGameActive = false;
        Inventory.CanPalceFigures = false;
        foreach (var figure in Figures)
        {
            Destroy(figure.gameObject);
        }
        StopAllCoroutines();
        StartGame();
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
