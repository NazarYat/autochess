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
    public Vector2 Coordinates { get; set; } = new Vector2(0, 0);
    public void UpdateView() => GetComponent<MeshRenderer>().enabled = CanPreview;
    public bool CanPreview => IsSpawnPoint && _figure == null && IsSpawnPointActive && Inventory.SelectedFigure != null;
    void OnMouseEnter()
    {
        if (!CanPreview) return;
        CreatePreviewFigure();
    }
    void OnMouseExit()
    {
        if (!CanPreview) return;
        DestroyPreviewFigure();
    }
    void OnMouseDown()
    {
        if (!CanPreview) return;
        if (Inventory.SelectedFigure == null) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            PlaceFigure(Inventory.SelectedFigure.CreateInstance(transform));
            Inventory.UseFigure(Inventory.SelectedFigure);
        }
    }
    
    public void CreatePreviewFigure()
    {
        if (_previewFigure != null) return;
        var f = Inventory.SelectedFigure.CreateInstance(transform);
        f.transform.SetParent(transform);
        _previewFigure = f;
    }
    public void DestroyPreviewFigure()
    {
        if (_previewFigure == null) return;
        Destroy(_previewFigure);
    }
    public bool PlaceFigure(GameObject f)
    {
        if (Figure != null) return false;
        f.transform.SetParent(transform);


        Figure = f.GetComponent<FigureBase>();
        Figure.CurrentCell = this;
        if (!Board.Figures.Contains(Figure))
        {
            Board.Figures.Add(Figure);
        }
        Figure.Board = Board;

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
