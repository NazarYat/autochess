using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class FigureBase : MonoBehaviour
{
    public int PlayerIndex { get; set; }
    public Inventory Inventory;
    public Shop Shop;
    public Board Board;
    public bool IsBusy { get; private set; } = false;

    public virtual int Health { get; set; }
    public virtual int Damage => throw new NotImplementedException();
    public virtual int Speed => throw new NotImplementedException();
    public virtual int AttackSpeed => throw new NotImplementedException();
    public virtual int AttackRange => throw new NotImplementedException();

    private Queue<Func<IEnumerator>> ActionQueue = new Queue<Func<IEnumerator>>();
    private Action InterruptActionCallBack = null;

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
    public virtual IEnumerator Move(Vector3 targetPosition)
    {
        throw new NotImplementedException();
    }
    public virtual IEnumerator Hit(Vector3 targetPosition)
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
        throw new NotImplementedException();
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
