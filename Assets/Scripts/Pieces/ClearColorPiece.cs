/// <summary>
/// Special Item: Removes all pieces of a given color
/// </summary>
public class ClearColorPiece : ClearablePiece
{
    public ColorPiece.ColorType Color { get; set; }

    public override void Clear()
    {
        base.Clear();
        piece.GridRef.ClearColor(Color);
    }
}
