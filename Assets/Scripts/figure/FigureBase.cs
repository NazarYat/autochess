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
    public Image HealthImage;
    public Material[] LevelsMaterials;

    public int PlayerIndex { get; set; }
    private Cell _currentCell;
    public Cell CurrentCell 
    { 
        get => _currentCell;
        set
        {
            if (_currentCell?.Figure == this)
            {
                _currentCell.Figure = null;
            }
            _currentCell = value;
        }
    }
    public bool IsBusy { get; private set; } = false;
    public virtual int Health { get; set; }
    public virtual int Damage => throw new NotImplementedException();
    public virtual int Speed => throw new NotImplementedException();
    public virtual int AttackSpeed => throw new NotImplementedException();
    public virtual int AttackRange => throw new NotImplementedException();

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

    public virtual void RegisterAttack(FigureBase attacker, int damage, Action deathCallback = null)
    {
        Health -= damage;

        if (Health <= 0)
        {
            Health = 0;
            deathCallback?.Invoke();
            StartActionInstant(Die);
        }
    }
    public virtual IEnumerator Attack(FigureBase target)
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
        if (IsBusy) { return; }

        if (ActionQueue.Count == 0)
        {
            ProcessCreateNextStep();
        }

        if (ActionQueue.Count > 0)
        {
            StartAction(ActionQueue.Dequeue());
        }
        
    }
    protected virtual void ProcessCreateNextStep()
    {
        var nearestFigureCell = Board.FindNearestCellWithFigure((int)CurrentCell.Coordinates.x, (int)CurrentCell.Coordinates.y);

        if (nearestFigureCell != null) // There is an enemy figure on the board
        {
            if (Math.Abs(nearestFigureCell.Coordinates.x - CurrentCell.Coordinates.x) <= 1 &&
                Math.Abs(nearestFigureCell.Coordinates.y - CurrentCell.Coordinates.y) <= 1)
            {
                ActionQueue.Enqueue(() => Attack(nearestFigureCell.Figure));
            }
            else
            {
                // Move to nearest figure

                var nextx = CurrentCell.Coordinates.x;
                var nexty = CurrentCell.Coordinates.y;

                if (Math.Abs(nearestFigureCell.Coordinates.x - CurrentCell.Coordinates.x) < Math.Abs(nearestFigureCell.Coordinates.y - CurrentCell.Coordinates.y))
                {
                    if (nearestFigureCell.Coordinates.x > CurrentCell.Coordinates.x)
                    {
                        nextx++;
                    }
                    else if (nearestFigureCell.Coordinates.x < CurrentCell.Coordinates.x)
                    {
                        nextx--;
                    }
                }
                else
                {
                    if (nearestFigureCell.Coordinates.y > CurrentCell.Coordinates.y)
                    {
                        nexty++;
                    }
                    else if (nearestFigureCell.Coordinates.y < CurrentCell.Coordinates.y)
                    {
                        nexty--;
                    }
                
                }
                ActionQueue.Enqueue(() => Move(Board.Cells[(int)nextx, (int)nexty]));
            }
        }
    }
    public void StartAction(Func<IEnumerator> action)
    {
        StartCoroutineInternal(action);

        IsBusy = true;
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
                yield break;
            }
        }
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
