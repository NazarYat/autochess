using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public Board Board;
    public Inventory Inventory;
    private GameObject _previewFigure = null;
    private FigureBase _figure = null;
    public FigureBase Figure
    {
        get => _figure;
        set
        {
            if (_figure == value && Figure == null) return;
            
            _figure = value;
            UpdateView();
        }
    }
    private bool _isSpawnPoint = false;
    public bool IsSpawnPoint
    {
        get => _isSpawnPoint;
        set
        {
            if (_isSpawnPoint == value) return;

            _isSpawnPoint = value;
            UpdateView();
        }
    }
    private bool _isSpawnPointActive = false;
    public bool IsSpawnPointActive
    {
        get => _isSpawnPointActive;
        set
        {
            if (_isSpawnPointActive == value) return;

            _isSpawnPointActive = value;
            UpdateView();
        }
    }
    public void UpdateView() => GetComponent<MeshRenderer>().enabled = CanPreview;
    public bool CanPreview => IsSpawnPoint && _figure == null && IsSpawnPointActive && Inventory.SelectedFigure != null;
    public void LeftMousButtonClick()
    {
        Inventory.UseFigure(Inventory.SelectedFigure);
    }
    void OnMouseEnter()
    {
        if (!CanPreview) return;
        CreatePreviewFigure(Inventory.SelectedFigure.Prefab);
    }
    void OnMouseExit()
    {
        if (!CanPreview) return;
        DestroyPreviewFigure();
    }
    public void CreatePreviewFigure(GameObject figurePrefab)
    {
        if (_previewFigure != null) return;
        var f = Instantiate(figurePrefab, transform.position, Quaternion.identity);
        f.transform.SetParent(transform);
        _previewFigure = f;
    }
    public void DestroyPreviewFigure()
    {
        if (_previewFigure == null) return;
        Destroy(_previewFigure);
    }
    public bool PlaceFigure(GameObject figurePrefab)
    {
        if (Figure != null) return false;
        var f = Instantiate(figurePrefab, transform.position, Quaternion.identity);
        f.transform.SetParent(transform);

        Figure = f.GetComponent<FigureBase>();

        return true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
