using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ShopItemData
{
    public string Name;
    public Sprite Icon;
    public int Price;
    public GameObject Prefab;
    public int UpgradePrice;
    public int Level;

    public ShopItemData(string name, Sprite icon, int price, GameObject prefab)
    {
        Name = name;
        Icon = icon;
        Price = price;
        Prefab = prefab;
    }
    public ShopItemData Copy()
    {
        var copy = new ShopItemData(Name, Icon, Price, Prefab);
    }
    public void Upgrade()
    {
        Level++;
        Price += UpgradePrice;
    }
}

public class Shop : MonoBehaviour
{
    public Inventory Inventory;
    public GameObject ShopItemPrefab;
    public Transform ShopContentParent;

    [Header("Layout Settings")]
    public Vector2 ItemSpacing = new Vector2(0f, 0f); // Distance between items (X: columns, Y: rows)
    public Vector2 ItemOffset = new Vector2(0f, 0f);       // Starting position offset

    [Header("Grid Settings")]
    public int ItemsPerRow = 1; // Set >1 for grid-like layout

    public ShopItemData[] ItemsForSale;

    void Start()
    {
        CreateShopItems();
    }

    void CreateShopItems()
    {
        for (int i = 0; i < ItemsForSale.Length; i++)
        {
            var itemData = ItemsForSale[i];
            GameObject newItem = Instantiate(ShopItemPrefab, ShopContentParent);

            // Grid position calculation
            int row = i / ItemsPerRow;
            int col = i % ItemsPerRow;

            var trans = newItem.GetComponent<RectTransform>();

            float posX = trans.localPosition.x + ItemOffset.x + (col * ItemSpacing.x);
            float posY = trans.localPosition.y + ItemOffset.y + (row * ItemSpacing.y);

            trans.localPosition = new Vector3(posX, posY, 0);

            ConfigureShopItem(newItem, itemData);
        }
    }

    void ConfigureShopItem(GameObject itemObject, ShopItemData itemData)
    {
        itemObject.transform.Find("NameText").GetComponent<Text>().text = itemData.Name;
        // itemObject.transform.Find("IconImage").GetComponent<Image>().sprite = itemData.Icon;
        itemObject.transform.Find("PriceText").GetComponent<Text>().text = "$" + itemData.Price;

        Button buyButton = itemObject.transform.Find("BuyButton").GetComponent<Button>();
        buyButton.onClick.AddListener(() => OnBuyButtonClicked(itemData));
    }

    void OnBuyButtonClicked(ShopItemData item)
    {
        Debug.Log($"Buying item: {item.Name} for ${item.Price}");
        Inventory.AddItem(item);
    }
}