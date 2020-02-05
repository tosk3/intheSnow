using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    [SerializeField] bool useRandomSeed;
    [SerializeField] string seed;
    [SerializeField] Vector2 chunkSize;
    [SerializeField] Vector2 chunkPos;
    System.Random PseudoRandom;


   // Start is called before the first frame update
   void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateChunk()
    {
       
    }

    public void GenerateSeed()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        seed += Mathf.PerlinNoise(chunkPos.x, chunkPos.y).ToString();

        PseudoRandom = new System.Random(seed.GetHashCode());
    }

    public string GetRandomSeed()
    {
        return seed;
    }
    public Vector2 PassChuckSize()
    {
        return chunkSize;
    }

}
