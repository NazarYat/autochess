using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeFigure : FigureBase
{
    public override IEnumerator Move(Cell targetPosition)
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
    public override IEnumerator Attack()
    {
        var cells = Board.GetCellsInTouch((int)CurrentCell.Coordinates.x, (int)CurrentCell.Coordinates.y);

        foreach (var cell in cells)
        {
            if (cell.Figure != null && cell.Figure != this)
            {
                cell.Figure.RegisterAttack(this, Damage, () => { });
            }
        }

        yield return new WaitForSeconds(AttackTime);

        yield break;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override IEnumerator Die()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);

        Board.Figures.Remove(this);
        CurrentCell.Figure = null;
    }
}
