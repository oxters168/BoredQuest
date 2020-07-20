using UnityEngine;
using UnityHelpers;

public class JigsawGame : MonoBehaviour
{
    public GameObject[] pieces;

    void Start()
    {
        //doathing();
    }

    private void doathing()
    {
        pieces = new GameObject[25];
        int seed = 1337;
        StartCoroutine(JigsawPuzzle.Generate(5, 5, 0.5f, 0.5f, 0.01f, 5, seed, null, pieces));
    }
}
