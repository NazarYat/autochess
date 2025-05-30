using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeFigure : FigureBase
{
    public override IEnumerator Attack()
    {
        var cells = Board.GetCellsInTouch((int)CurrentCell.Coordinates.x, (int)CurrentCell.Coordinates.y);

        foreach (var cell in cells)
        {
            if (cell.Figure != null && cell.Figure.PlayerIndex != PlayerIndex)
            {
                cell.Figure.RegisterAttack(this, DamageLevelized, () => { });
            }
        }

        yield return new WaitForSeconds(AttackTime);

        yield break;
    }
    void Start()
    {
        Health = MaxHealthLevelized;
    }

    void Update()
    {
        
    }
}
