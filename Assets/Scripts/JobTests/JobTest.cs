using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class JobTest : MonoBehaviour
{

    public struct MyJob : IJob
    {
        public float a;
        public float b;
        public NativeArray<float> result;

        public void Execute()
        {
            result[0] = a + b;
        }
    }


    // Job adding one to a value
    public struct AddOneJob : IJob
    {
        public NativeArray<float> result;

        public void Execute()
        {
            result[0] = result[0] + 1;
        }
    }

    private void Start()
    {
        // Create a native array of a single float to store the result in. This example waits for the job to complete
        NativeArray<float> result = new NativeArray<float>(1, Allocator.TempJob);

        // Setup the data for job #1
        MyJob jobData = new MyJob();
        jobData.a = 10;
        jobData.b = 10;
        jobData.result = result;

        // Schedule job #1
        JobHandle firstHandle = jobData.Schedule();

        // Setup the data for job #2
        AddOneJob incJobData = new AddOneJob();
        incJobData.result = result;

        // Schedule job #2
        JobHandle secondHandle = incJobData.Schedule(firstHandle);

        // Wait for job #2 to complete
        secondHandle.Complete();

        // All copies of the NativeArray point to the same memory, you can access the result in "your" copy of the NativeArray
        float aPlusB = result[0];

        // Free the memory allocated by the result array
        result.Dispose();

        Debug.Log($"O valor de sei la o que é {aPlusB}");
    }

}
