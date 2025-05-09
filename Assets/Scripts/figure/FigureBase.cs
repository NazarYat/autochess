using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public abstract class FigureBase : MonoBehaviour
{
    public Inventory Inventory;
    public Shop Shop;
    public Board Board;
    public Text LevelText;
    public Material[] LevelsMaterials;
    public PercentBar HealthBar;

    public float Damage;
    public float Speed;
    public float AttackTime;
    public int AttackRange;
    public float MaxHealth;

    public int PlayerIndex { get; set; }
    private Cell _currentCell;
    public Cell CurrentCell 
    { 
        get => _currentCell;
        set
        {
            if (value == _currentCell) return;

            if (_currentCell?.Figure == this)
            {
                _currentCell.Figure = null;
            }
            _currentCell = value;
        }
    }
    public bool IsBusy { get; private set; } = false;
    private float _health;
    public virtual float Health 
    { 
        get => _health;
        set
        {
            if (_health == value) return;

            _health = value;

            HealthBar.SetValue(Health / MaxHealth);

            Debug.Log($"Health: {Health / MaxHealth}");
        }
    }
    private Queue<Func<IEnumerator>> ActionQueue = new Queue<Func<IEnumerator>>();
    private Action InterruptActionCallBack = null;

    private int _level = 1;
    public int Level
    {
        get => _level;
        set
        {
            if (value < 1)
            {
                _level = 1;
            }
            else
            {
                _level = value;
            }

            ProcessLevelChanged();
        }
    }
    private Material GetMaterialForLevel(int level)
    {
        if (level < 1 || level > LevelsMaterials.Length)
        {
            return LevelsMaterials[0];
        }
        else
        {
            return LevelsMaterials[level - 1];
        }
    }
    private void ProcessLevelChanged()
    {
        transform.Find("Mesh").GetComponent<MeshRenderer>().material = GetMaterialForLevel(Level);
        LevelText.text = Level.ToString();
    }

    public virtual void RegisterAttack(FigureBase attacker, float damage, Action deathCallback = null)
    {
        Health -= damage;

        if (Health <= 0)
        {
            Health = 0;
            deathCallback?.Invoke();
            StartActionInstant(Die);
        }
    }
    public virtual IEnumerator Attack()
    {
        throw new NotImplementedException();
    }
    public virtual IEnumerator Move(Cell targetPosition)
    {
        throw new NotImplementedException();
    }
    public virtual IEnumerator Die()
    {
        throw new NotImplementedException();
    }
    public void Action(int duration)
    {
        if (IsBusy) return;

        if (ActionQueue.Count == 0)
        {
            ProcessCreateNextStep();
        }

        if (ActionQueue.Count > 0)
        {
            StartAction(ActionQueue.Dequeue());
        }
        
    }
    public Stack<Cell> GetPathToEnemy(int startX, int startY)
    {
        bool[,] visited = new bool[Board.SizeX, Board.SizeY];
        Queue<(int x, int y)> queue = new Queue<(int x, int y)>();
        Dictionary<(int x, int y), (int x, int y)> parents = new Dictionary<(int x, int y), (int x, int y)>();

        visited[startX, startY] = true;
        queue.Enqueue((startX, startY));

        // Enforce stable direction order: Up, Left, Right, Down
        (int dx, int dy)[] directions = new (int, int)[]
        {
        (0, -1), // Up
        (-1, 0), // Left
        (1, 0),  // Right
        (0, 1)   // Down
        };

        while (queue.Count > 0)
        {
            var (cx, cy) = queue.Dequeue();
            var current = Board.Cells[cx, cy];

            // Found an enemy
            if (current.Figure != null && current.Figure.PlayerIndex != PlayerIndex)
            {
                var path = new Stack<Cell>();
                var node = (cx, cy);

                while (node != (startX, startY))
                {
                    path.Push(Board.Cells[node.cx, node.cy]);
                    node = parents[node];
                }

                return path;
            }

            // Visit neighbors in consistent order
            foreach (var (dx, dy) in directions)
            {
                int nx = cx + dx;
                int ny = cy + dy;

                if (nx < 0 || ny < 0 || nx >= Board.SizeX || ny >= Board.SizeY)
                    continue;

                if (visited[nx, ny])
                    continue;

                var neighbor = Board.Cells[nx, ny];

                if (neighbor.Figure == null || neighbor.Figure.PlayerIndex != PlayerIndex)
                {
                    visited[nx, ny] = true;
                    parents[(nx, ny)] = (cx, cy);
                    queue.Enqueue((nx, ny));
                }
            }
        }

        return null; // No enemy found
    }
    protected virtual void ProcessCreateNextStep()
    {
        var nearestFigurePath = GetPathToEnemy((int)CurrentCell.Coordinates.x, (int)CurrentCell.Coordinates.y);

        if (nearestFigurePath == null) return;

        if (nearestFigurePath?.Count > AttackRange)
        {
            ActionQueue.Enqueue(() => Move(nearestFigurePath.Pop()));
        }
        else
        {
            ActionQueue.Enqueue(() => Attack());
        }
    }
    public void StartAction(Func<IEnumerator> action)
    {
        IsBusy = true;
        StartCoroutineInternal(action);
    }
    public void StartActionInstant(Func<IEnumerator> action)
    {
        InterruptActionCallBack?.Invoke();
        StartCoroutineInternal(action);

        IsBusy = true;
    }
    public IEnumerator CoroutineBase(Func<IEnumerator> func)
    {
        var stop = false;
        InterruptActionCallBack += () =>
        {
            stop = true;
            IsBusy = false;
        };

        var ienum = func();

        while (ienum.MoveNext())
        {
            yield return ienum.Current;

            if (stop)
            {
                IsBusy = false;
                yield break;
            }
        }
        IsBusy = false;

    }
    public void StartCoroutineInternal(Func<IEnumerator> action)
    {
        StartCoroutine(CoroutineBase(action));
    }
    // Start is called before the first frame update
    void Start()
    {
        Health = MaxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
