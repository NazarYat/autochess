using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Inventory : MonoBehaviour
{
    private int _money = 0;
    public int Money
    {
        get => _money;
        private set
        {
            if (_money == value) return;

            _money = value;

            BalanceText.text = "$" + value.ToString();
        }
    }
    private bool _canPlaceFigures = false;
    public bool CanPalceFigures
    {
        get => _canPlaceFigures;
        set
        {
            if (_canPlaceFigures == value) return;
            _canPlaceFigures = value;
            foreach (var item in InventoryRepresentativeElements)
            {
                item.Value.transform.Find("PlaceButton").gameObject.SetActive(value);
            }
        }
    }
    private bool _canUpgradeFigures = false;
    public bool CanUpgradeFigures
    {
        get => _canUpgradeFigures;
        set
        {
            if (_canUpgradeFigures == value) return;
            _canUpgradeFigures = value;
            foreach (var item in InventoryRepresentativeElements)
            {
                item.Value.transform.Find("UpgradeButton").gameObject.SetActive(value);
                item.Value.transform.Find("PriceText").gameObject.SetActive(value);
            }
        }
    }
    public Text BalanceText;
    public Board Board;
    public GameObject itemPrefab;
    public Vector2 ItemOffset = new Vector2(0, 0);
    public Vector2 ItemSpacing = new Vector2(0, 0);
    public List<ShopItemData> Figures { get; private set; } = new List<ShopItemData>();
    public Dictionary<(string, int), GameObject> InventoryRepresentativeElements { get; private set; } = new Dictionary<(string, int), GameObject>();
    public Dictionary<(string, int), int> UsedFigures { get; private set; } = new Dictionary<(string, int), int>();

    public void AddMoney(int amount)
    {
        Money += amount;
    }
    public void RemoveMoney(int amount)
    {
        Money -= amount;
    }
    public void AddItem(ShopItemData figure)
    {
        if (Money - figure.Price < 0) return;

        Figures.Add(figure);

        Money -= figure.Price;

        UpdateView();
    }
    private ShopItemData _selectedFigure;
    public ShopItemData SelectedFigure 
    { 
        get => _selectedFigure;
        private set
        {
            if (_selectedFigure == value) return;
            _selectedFigure = value;

            Board.IsPlacingFigure = value != null;
        }
    }
    public void UseSelectedFigure() => UseFigure(SelectedFigure);
    public void UseFigure(ShopItemData figure)
    {
        if (figure == null) return;
        var figures = Figures.Where(f => f.Name == figure.Name && f.Level == figure.Level);

        if (figures.Count() == 0) return;

        var id = (figure.Name, figure.Level);

        if (!UsedFigures.ContainsKey(id))
        {
            UsedFigures.Add(id, 1);
        }
        else
        {
            if (UsedFigures[id] < figures.Count())
            {
                UsedFigures[id]++;
            }
        }

        if (UsedFigures[id] >= figures.Count() && SelectedFigure == figure)
        {
            SelectedFigure = null;
        }

        UpdateView();
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateView();
    }
    private void UpdateView()
    {
        var col = 0;
        foreach (var g in Figures.GroupBy(f => (f.Name, f.UpgradePrice)))
        {
            var count = g.Count();

            if (UsedFigures.TryGetValue(g.Key, out var usedCount))
            {
                count -= usedCount;
            }


            if (InventoryRepresentativeElements.TryGetValue(g.Key, out var go))
            {
                if (count <= 0)
                {
                    Destroy(go);
                    InventoryRepresentativeElements.Remove(g.Key);
                    continue;
                }
                
                go.transform.Find("CountText").GetComponent<Text>().text = count.ToString();
            }
            else
            {
                if (count <= 0) continue;
                
                var item = Instantiate(itemPrefab, transform);
                InventoryRepresentativeElements.Add(g.Key, item);

                item.transform.Find("NameText").GetComponent<Text>().text = g.Key.Item1;
                item.transform.Find("PriceText").GetComponent<Text>().text = "$" + g.First().UpgradePrice.ToString();
                item.transform.Find("UpgradeButton").GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (Money - g.FirstOrDefault().UpgradePrice < 0) return;
                    RemoveMoney(g.FirstOrDefault().UpgradePrice);
                    g.FirstOrDefault()?.Upgrade();
                    UpdateView();
                });

                item.transform.Find("PlaceButton").gameObject.SetActive(CanPalceFigures);
                item.transform.Find("UpgradeButton").gameObject.SetActive(CanUpgradeFigures);
                item.transform.Find("PriceText").gameObject.SetActive(CanUpgradeFigures);
                item.transform.Find("PlaceButton").GetComponent<Button>().onClick.AddListener(() =>
                {
                    SelectedFigure = g.FirstOrDefault();
                });


                var trans = item.GetComponent<RectTransform>();

                float posX = trans.localPosition.x + ItemOffset.x + (col * ItemSpacing.x);
                float posY = trans.localPosition.y + ItemOffset.y;

                trans.localPosition = new Vector3(posX, posY, 0);

            }
            col++;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
