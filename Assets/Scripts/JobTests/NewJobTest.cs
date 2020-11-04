using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class NewJobTest : MonoBehaviour
{
    public float myNumber = 5;
    public NativeArray<float> myData;
    public JobHandle myHandle;

    private void OnEnable()
    {
        myData = new NativeArray<float>(1, Allocator.Persistent);
        myData[0] = 2;
    }

    private void OnDisable()
    {
        myData.Dispose();
    }

    private void Start()
    {
        SimpleJob simpleJob = new SimpleJob
        {
            number = myNumber,
            data = myData
        };

        //agenda o job, mas num executa!
        myHandle = simpleJob.Schedule();
        //roda o job agendado
        JobHandle.ScheduleBatchedJobs();
        //aguarda o job completar
        myHandle.Complete();

        if (myHandle.IsCompleted)
        {
            Debug.Log(simpleJob.data[0]);
        }
    }
}

public struct SimpleJob : IJob
{
    public float number;
    public NativeArray<float> data;

    public void Execute()
    {
        data[0] += number;
    }
}