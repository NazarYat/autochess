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

    public Text BalanceText;
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
    public void UseFigure((string, int) id)
    {
        var figure = Figures.FirstOrDefault(f => f.Name == id.Item1 && f.UpgradePrice == id.Item2);

        if (figure == null) return;

        if (!UsedFigures.ContainsKey(id))
        {
            UsedFigures.Add(id, 1);
        }
        else
        {
            UsedFigures[id]++;
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
                item.transform.Find("PriceText").GetComponent<Text>().text = "$" + g.Key.Item2.ToString();
                item.transform.Find("UpgradeButton").GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (Money - g.FirstOrDefault().UpgradePrice < 0) return;
                    RemoveMoney(g.FirstOrDefault().UpgradePrice);
                    g.FirstOrDefault()?.Upgrade();
                    UpdateView();
                });

                item.transform.Find("PlaceButton").GetComponent<Button>().onClick.AddListener(() =>
                {
                    UseFigure(g.Key);
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
