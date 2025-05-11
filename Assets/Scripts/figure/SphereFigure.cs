using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereFigure : FigureBase
{
    public override IEnumerator Attack()
    {
        var cells = Board.GetCellsInTouch((int)CurrentCell.Coordinates.x, (int)CurrentCell.Coordinates.y, AttackRange);

        foreach (var cell in cells)
        {
            if (cell.Figure != null && cell.Figure.PlayerIndex != PlayerIndex)
            {
                cell.Figure.RegisterAttack(this, DamageLevelized, () => { });
                break;
            }
        }

        yield return new WaitForSeconds(AttackTime);

        yield break;
    }
    void Start()
    {
        Health = MaxHealthLevelized;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
