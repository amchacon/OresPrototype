using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class TransformJobSystemDemo : MonoBehaviour
{
    
    private NativeArray<int> myData;

    private JobHandle transformJobHandle;
    private TransformAccessArray transformAccessArray;

    private void OnEnable()
    {
        myData = new NativeArray<int>(1, Allocator.Persistent);
        for (int i = 0; i < myData.Length; i++)
        {
            myData[i] = i;
        }
        Transform[] myTransforms = { this.transform };
        transformAccessArray = new TransformAccessArray(myTransforms);
    }

    private void OnDisable()
    {
        myData.Dispose();
        transformAccessArray.Dispose();
    }

    private void Update()
    {
        MoveToPositionJob transformJob = new MoveToPositionJob()
        {
            newX = Random.Range(1, 4),
            newY = Random.Range(1, 4),
            dt = Time.deltaTime
        };
        transformJobHandle = transformJob.Schedule(transformAccessArray);
        JobHandle.ScheduleBatchedJobs();
        transformJobHandle.Complete();
        if (transformJobHandle.IsCompleted)
        {
            Debug.Log("Transform Job Completed!");
        }
    }
}

[BurstCompile(CompileSynchronously = true)]
public struct MoveToPositionJob : IJobParallelForTransform
{
    public int newX;
    public int newY;
    public float dt;

    public void Execute(int index, TransformAccess transform)
    {
        transform.localPosition += new Vector3(newX * dt, newY * dt, 0);
    }
}