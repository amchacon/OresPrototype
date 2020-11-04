using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;
using Random = UnityEngine.Random;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Burst;

[BurstCompile]
public struct PushColumnJob : IJobParallelForTransform
{
    public float newX;
    public float newY;
    public void Execute(int index, TransformAccess transform)
    {
        transform.localPosition = new Vector3(newX,newY);
    }
}

public class GridManager : MonoBehaviour
{
    public enum PieceType
    {
        EMPTY,
        NORMAL,
    };

    [System.Serializable]
    public struct PiecePrefab
    {
        public PieceType type;
        public GameObject prefab;
    };

    public Action<int> OnPieceCleared;
    public Action OnLimitReached;
    public Action OnPotentialMatchesEnded;

    public int maxWidth;
    public int curWidth;
    public int height;
    public int piecesToMatch = 2;
    public PiecePrefab[] piecePrefabs;

    private Dictionary<PieceType, GameObject> piecePrefabDict;
    private List<GamePiece> matches = new List<GamePiece>();
    private List<GamePiece> potentialMatches = new List<GamePiece>();
    private GamePiece[,] pieces;
    private GamePiece pressedPiece;
    private float snapTime = 0.1f;
    private bool gameOver = false;

    private void Awake()
    {
        //piecesData = new NativeArray<PieceData>(maxWidth * height, Allocator.Persistent);
        //allPiecesTransform = new Transform[maxWidth * height];

        gameOver = false;
        CreateGrid();
    }

    /// <summary>
    /// FOR TEST/STUDY ONLY. PLEASE IGNORE IT!!
    /// </summary>
    //void GetData()
    //{
    //    int index = 0;
    //    foreach (var p in pieces)
    //    {
    //        piecesData[index] = new PieceData
    //        {
    //            x = p.X,
    //            y = p.Y,
    //            normal = p.Type == PieceType.NORMAL ? true : false
    //        };
    //        allPiecesTransform[index] = p.transform;
    //        index++;
    //    }
    //    transformAccessArray = new TransformAccessArray(allPiecesTransform);
    //}

#if UNITY_EDITOR
    //Only for tests and debug
    private void Update()
    {
        // Q# Restart scene
        if (Input.GetKeyDown(KeyCode.Q))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }

        // P# Creates new column on Right side and push all current columns to left (until limit)
        if (Input.GetKeyDown(KeyCode.P))
        {
            PushColumns();
        }

        // R# Removes entire random row
        if (Input.GetKeyDown(KeyCode.R))
        {
            ClearRow(0);
            StartCoroutine(SnapPieces());
        }

        // C# Removes entire column on left side
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearColumn(Random.Range(0, curWidth-1));
            StartCoroutine(SnapPieces());
        }

        // K# Removes all pieces of a random color type
        if (Input.GetKeyDown(KeyCode.K))
        {
            int cInt = Random.Range(0, 6);
            ColorPiece.ColorType cType = (ColorPiece.ColorType)cInt;
            ClearColor(cType);
            Debug.Log($"Removing elements with color: {cType}");
            StartCoroutine(SnapPieces());
        }
    }
#endif

    private void CreateGrid()
    {
        piecePrefabDict = new Dictionary<PieceType, GameObject>();

        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            if (!piecePrefabDict.ContainsKey(piecePrefabs[i].type))
            {
                piecePrefabDict.Add(piecePrefabs[i].type, piecePrefabs[i].prefab);
            }
        }

        pieces = new GamePiece[maxWidth, height];

        int startCount = 0;
        PieceType pieceType = PieceType.NORMAL;
        for (int x = 0; x < maxWidth; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CreateNewPiece(x, y, pieceType);
            }
            startCount++;
            if (startCount >= curWidth)
            {
                pieceType = PieceType.EMPTY;
            }
        }
    }

    public void MouseEnterHandler(GamePiece piece)
    {
        //TODO: hover effect
        if (gameOver)
        {
            return;
        }
    }

    public void MouseDownHandler(GamePiece piece)
    {
        if (gameOver)
        {
            return;
        }

        if (pressedPiece == null && piece.Type == PieceType.NORMAL)
        {
            pressedPiece = piece;
            if (ClearAllValidMatches())
            {
                StartCoroutine(SnapPieces());
            }
        }
        pressedPiece = null;
    }

    private void CreateNewPiece(int x, int y, PieceType type)
    {
        GamePiece newPiece = Instantiate(piecePrefabDict[type], GetWorldPosition(x, y), Quaternion.identity).GetComponent<GamePiece>();
        newPiece.transform.parent = transform;
        pieces[x, y] = newPiece;
        pieces[x, y].Init(x, y, this, type);
    }

    private void CreateNewColumn()
    {
        for (int i = 0; i < height; i++)
        {
            Destroy(pieces[0, i].gameObject);
            CreateNewPiece(0, i, PieceType.NORMAL);
        }

    }

    public void PushColumns()
    {
        if (curWidth < maxWidth)
        {
            for (int x = curWidth - 1; x >= 0; x--)
            {
                for (int y = 0; y < height; y++)
                {
                    GamePiece piece = pieces[x, y];
                    GamePiece pieceLeft = pieces[x + 1, y];
                    if (piece.IsMovable())
                    {
                        Destroy(pieceLeft.gameObject);

                        //piece.MovableComponent.Move(x + 1, y, snapTime);
                        piece.X = pieceLeft.X;
                        piece.Y = pieceLeft.Y;

                        ExecuteJob(piece);

                        pieces[x + 1, y] = piece;
                        CreateNewPiece(x, y, PieceType.EMPTY);
                    }
                }
            }
            curWidth++;
            CreateNewColumn();
            SFXManager.instance.PlaySFX(Clip.Push);
        }
        else
        {
            gameOver = true;
            OnLimitReached?.Invoke();
        }
    }

    //TODO: Consider changing it to work with a NativeArray of all pieces that have to move!
    private void ExecuteJob(GamePiece piece)
    {
        JobHandle jobHandle;
        TransformAccessArray transformAccessArray;

        Vector3 pos = GetWorldPosition(piece.X, piece.Y);
        Transform[] pieceTransform = { piece.transform };
        transformAccessArray = new TransformAccessArray(pieceTransform);

        PushColumnJob transformJob = new PushColumnJob()
        {
            newX = pos.x,
            newY = pos.y
        };

        jobHandle = transformJob.Schedule(transformAccessArray);
        JobHandle.ScheduleBatchedJobs();
        jobHandle.Complete();
        transformAccessArray.Dispose();
    }

    public Vector2 GetWorldPosition(int x, int y)
    {
        var p = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height / 5, 0));
        return new Vector2((p.x - 1) - x, (p.y + 1) + y);
    }

    private bool ClearAllValidMatches()
    {
        matches.Clear();
        matches.Add(pressedPiece);
        potentialMatches.Clear();
        potentialMatches = GetPiecesOfColor(pressedPiece.ColorComponent.Color);
        FindMatchesRecursively(potentialMatches, 0);

        if(matches.Count >= piecesToMatch)
        {
            foreach (var piece in matches)
            {
                ClearPiece(piece.X, piece.Y);
                SFXManager.instance.PlaySFX(Clip.Clear);
            }
            return true;
        }
        return false;
    }

    private List<GamePiece> GetPiecesOfColor(ColorPiece.ColorType pieceColor)
    {
        var sameColor = from GamePiece p in pieces
                        where p.Type != PieceType.EMPTY
                        && p.ColorComponent.Color == pieceColor
                        && p != pressedPiece
                        select p;
        return sameColor.ToList();
    }

    private void FindMatchesRecursively(List<GamePiece> potentialMatches, int iStart)
    {
        int count = matches.Count;
        for (int i = iStart; i < matches.Count; i++)
        {
            var t = potentialMatches.Where(p => IsAdjacent(matches[i], p)).Select(p => p).ToList();
            matches.AddRange(t);
            potentialMatches.RemoveAll(x => t.Contains(x));
        }
        if (matches.Count > count)
        {
            iStart++;
            FindMatchesRecursively(potentialMatches, iStart);
        }
    }

    private bool IsAdjacent(GamePiece piece1, GamePiece piece2)
    {
        return (piece1.X == piece2.X && (int)Mathf.Abs(piece1.Y - piece2.Y) == 1)
        || (piece1.Y == piece2.Y && (int)Mathf.Abs(piece1.X - piece2.X) == 1);
    }

    private bool ClearPiece(int x, int y)
    {
        if (pieces[x, y].IsClearable())
        {
            pieces[x, y].ClearableComponent.Clear();
            CreateNewPiece(x, y, PieceType.EMPTY);
            return true;
        }
        return false;
    }

    internal void PieceCleared(GamePiece piece)
    {
        OnPieceCleared?.Invoke(piece.score);
    }

    private IEnumerator SnapPieces()
    {
        yield return new WaitWhile(SnapPieceStep);
        yield return new WaitWhile(SnapColumnStep);
    }

    private bool SnapPieceStep()
    {
        bool movedPiece = false;
        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < curWidth; x++)
            {
                GamePiece piece = pieces[x, y];
                if (piece.Type == PieceType.EMPTY)
                {
                    GamePiece pieceAbove = pieces[x, y + 1];
                    if (pieceAbove.IsMovable())
                    {
                        Destroy(piece.gameObject);
                        pieceAbove.MovableComponent.Move(x, y, snapTime);
                        pieces[x, y] = pieceAbove;
                        CreateNewPiece(x, y + 1, PieceType.EMPTY);
                        movedPiece = true;
                    }
                }
            }
        }
        return movedPiece;
    }


    private bool SnapColumnStep()
    {
        bool movedColumn = false;
        var gap = GetEmptyColumnIndex();
        if (gap != null)
        {
            for (int x = gap.Value + 1; x < curWidth; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    GamePiece piece = pieces[x, y];
                    if (piece.IsMovable())
                    {
                        GamePiece pieceRight = pieces[gap.Value, y];
                        Destroy(pieceRight.gameObject);
                        piece.MovableComponent.Move(gap.Value, y, snapTime);
                        pieces[gap.Value, y] = piece;
                        CreateNewPiece(x, y, PieceType.EMPTY);
                    }
                }
                gap++;
            }
            movedColumn = true;
            curWidth--;
        }
        return movedColumn;
    }

    private int? GetEmptyColumnIndex()
    {
        for (int x = 0; x < curWidth; x++)
        {
            if (IsColumnEmpty(x))
            {
                return x;
            }
        }
        return null;
    }

    bool IsColumnEmpty(int col)
    {
        if (pieces[col, 0].Type == PieceType.NORMAL)
        {
            return false;
        }
        return true;
    }

    #region Special Items
    internal void ClearRow(int row)
    {
        for (int x = 0; x < maxWidth; x++)
        {
            ClearPiece(x, row);
        }
    }

    internal void ClearColumn(int column)
    {
        for (int y = 0; y < height; y++)
        {
            ClearPiece(column, y);
        }
    }

    internal void ClearColor(ColorPiece.ColorType color)
    {
        for (int x = 0; x < maxWidth; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (pieces[x, y].IsColored() && (pieces[x, y].ColorComponent.Color == color))
                {
                    ClearPiece(x, y);
                }
            }
        }
    }
    #endregion

}
