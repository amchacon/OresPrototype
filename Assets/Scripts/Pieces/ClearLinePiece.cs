/// <summary>
/// Special Item: Removes all pieces of a given row or column
/// </summary>
public class ClearLinePiece : ClearablePiece
{
    public bool isRow;

    public override void Clear()
    {
        base.Clear();

        if (isRow)
        {
            piece.GridRef.ClearRow(piece.Y);
        }
        else
        {
            piece.GridRef.ClearColumn(piece.X);
        }
    }
}
