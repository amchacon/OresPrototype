using Unity.Collections;
using UnityEngine;

public class TestMatrix : MonoBehaviour
{
    public PieceData[,] matrixTest;
    public NativeArray<PieceData> nativeTest;

    // Start is called before the first frame update
    void Start()
    {
        matrixTest = new PieceData[16, 10];
        nativeTest = new NativeArray<PieceData>(160, Allocator.Temp);
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                matrixTest[x, y] = new PieceData()
                {
                    x = x,
                    y = y
                };
            }
        }

        int index = 0;
        foreach (var item in matrixTest)
        {
            nativeTest[index] = item;
            index++;
        }

        Debug.Log($"X: {nativeTest[96].x} | Y: {nativeTest[96].y}");

        nativeTest.Dispose();
    }

}
