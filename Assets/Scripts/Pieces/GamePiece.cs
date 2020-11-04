using UnityEngine;

[System.Serializable]
public struct PieceData
{
    public int x;
    public int y;
    public bool normal;
};

public class GamePiece : MonoBehaviour
{
    public int score;

    public PieceData data;

    public int X
    {
        get { return data.x; }
        set
        {
            if (IsMovable())
            {
                data.x = value;
            }
        }
    }

    public int Y
    {
        get { return data.y; }
        set
        {
            if (IsMovable())
            {
                data.y = value;
            }
        }
    }

    public GridManager.PieceType Type { get; private set; }
    public GridManager GridRef { get; private set; }
    public MovablePiece MovableComponent { get; private set; }
    public ColorPiece ColorComponent { get; private set; }
    public ClearablePiece ClearableComponent { get; private set; }

    public bool IsMovable() => MovableComponent != null;

    public bool IsColored() => ColorComponent != null;

    public bool IsClearable() => ClearableComponent != null;

    void Awake()
    {
        MovableComponent = GetComponent<MovablePiece>();
        ColorComponent = GetComponent<ColorPiece>();
        ClearableComponent = GetComponent<ClearablePiece>();
    }

    public void Init(int _x, int _y, GridManager _grid, GridManager.PieceType _type)
    {
        data.x = _x;
        data.y = _y;
        GridRef = _grid;
        Type = _type;
        if (Type != GridManager.PieceType.EMPTY)
        {
            ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, ColorComponent.NumColors));
        }
    }

    void OnMouseEnter()
    {
        GridRef.MouseEnterHandler(this);
    }

    void OnMouseDown()
    {
        GridRef.MouseDownHandler(this);
    }

}
