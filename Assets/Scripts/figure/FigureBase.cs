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
    public Material DamageMaterial;
    public Material HealMaterial;
    public PercentBar HealthBar;
    public Image[] Images;

    public float Damage;
    public float Speed;
    public float AttackTime;
    public float SelfHealValue;
    public int AttackRange;
    public float MaxHealth;

    protected float DamageLevelized => Damage + Damage * (Level / 5.0f);
    protected float MaxHealthLevelized => MaxHealth + MaxHealth * (Level / 5.0f);
    protected float SelfHealValueLevelized => SelfHealValue + SelfHealValue * (Level / 5.0f);

    private int _playerIndex = -1;
    public int PlayerIndex 
    {
        get => _playerIndex;
        set
        {
            if (_playerIndex == value) return;

            _playerIndex = value;

            if (PlayerIndexColors != null && _playerIndex < PlayerIndexColors.Length)
            {
                foreach (var image in Images)
                {
                    image.color = PlayerIndexColors[PlayerIndex];
                }
            }
        }
    }
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

            HealthBar.Value = Health / MaxHealthLevelized;
        }
    }
    private Queue<Func<IEnumerator>> ActionQueue = new Queue<Func<IEnumerator>>();
    private Action InterruptActionCallBack = null;
    public Color[] PlayerIndexColors;

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
    public ShopItemData FigureData { get; set; }
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

        if (Health < 0)
        {
            Health = 0;
            deathCallback?.Invoke();
            StartActionInstant(Die);
        }
        else
        {
            StartCoroutine(TakeDamage());
        }
    }
    public virtual void RegisterHeal(float damage)
    {
        Health += damage;

        if (Health > MaxHealthLevelized)
        {
            Health = MaxHealthLevelized;
        }
        else
        {
            StartCoroutine(Heal());
        }

    }
    public virtual IEnumerator Attack()
    {
        throw new NotImplementedException();
    }
    public virtual IEnumerator Move(Cell targetPosition)
    {
        targetPosition.Figure = this;

        Vector3 targetCoordinates = targetPosition.transform.position;

        while (Vector3.Distance(transform.position, targetCoordinates) > 0.01f)
        {
            Vector3 direction = (targetCoordinates - transform.position).normalized;
            transform.position += direction * Speed * Time.deltaTime;

            yield return null;
        }

        transform.position = targetCoordinates;
    }
    public virtual IEnumerator Die()
    {
        yield return new WaitForSeconds(0.5f);
        CurrentCell.Figure = null;
        Board.RegisterFigureDeath(this);

        Debug.Log($"Dead figure index: {PlayerIndex}");

        StopAllCoroutines();

        yield break;

    }
    public virtual IEnumerator Heal()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0, 2) / 10);
        transform.Find("Mesh").GetComponent<MeshRenderer>().material = HealMaterial;

        yield return new WaitForSeconds(0.5f);
        ProcessLevelChanged();

        yield break;

    }
    public virtual IEnumerator TakeDamage()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0, 2) / 10);
        transform.Find("Mesh").GetComponent<MeshRenderer>().material = DamageMaterial;

        yield return new WaitForSeconds(0.1f);
        ProcessLevelChanged();

        yield break;
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
        if (this == null || gameObject == null) return;

        var nearestFigurePath = GetPathToEnemy((int)CurrentCell.Coordinates.x, (int)CurrentCell.Coordinates.y);

        if (nearestFigurePath == null) return;

        if (nearestFigurePath?.Count > AttackRange)
        {

            ActionQueue.Enqueue(() => Move(nearestFigurePath.Pop()));
            RegisterHeal(SelfHealValue);
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
