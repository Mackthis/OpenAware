using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGridBehavior<Data> : MonoBehaviour
{
    public (int, int, int) gridSize, chunkSize;
    public GameObject chunkObject;

    public ChunkGrid<Data> chunkGrid;
    public GameObject[,,] chunkObjects;


    protected void Start()
    {
        OnStart();
    }

    protected virtual void OnStart()
    {
        // Create the ChunkGrid
        ChunkGridFactory<Data> factory = new ChunkGridFactory<Data>(gridSize, chunkSize);
        this.chunkGrid = factory.Create();

        // Create Chunk objects with chunk behaviour for each chunk
        IChunk<Data>[,,] chunks = this.chunkGrid.Chunks;
        chunkObjects = new GameObject[chunkSize.Item1, chunkSize.Item2, chunkSize.Item3];
        for (int x = 0; x < chunks.GetLength(0); x++)
            for (int y = 0; y < chunks.GetLength(0); y++)
                for (int z = 0; z < chunks.GetLength(0); z++)
                {
                    GameObject chunkObject = Instantiate(this.chunkObject);
                    //chunkObject.GetComponent("ChunkBehaviour").Chunk = chunks[x,y,z];
                }
    }

    protected void Update() { OnUpdate(); }

    protected virtual void OnUpdate() { }
}
