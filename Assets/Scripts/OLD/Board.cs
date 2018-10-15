using System.Collections;
using UnityEngine;

public enum GameState
{
    wait,
    move
}


public class Board : MonoBehaviour
{
    public GameState currentState = GameState.move;
    public int width;
    public int height;
    public int offSet;
    public GameObject tilePrefab;
    public GameObject[] dots;
    public GameObject destroyParticle;
    private BackgroundTile[,] allTiles;
    public GameObject[,] allDots;
    public Dot currentDot;
    private FindMatches findMatches;


    // Use this for initialization
    private void Start()
    {
        findMatches = FindObjectOfType<FindMatches>();
        allTiles = new BackgroundTile[width, height];
        allDots = new GameObject[width, height];
        SetUp();
    }

    private void SetUp()
    {
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var tempPosition = new Vector2(i, j + offSet);
                var backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity);
                backgroundTile.transform.parent = transform;
                backgroundTile.name = "( " + i + ", " + j + " )";

                var dotToUse = Random.Range(0, dots.Length);

                var maxIterations = 0;

                while (MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                {
                    dotToUse = Random.Range(0, dots.Length);
                    maxIterations++;
                    Debug.Log(maxIterations);
                }

                var dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                dot.GetComponent<Dot>().row = j;
                dot.GetComponent<Dot>().column = i;
                dot.transform.parent = transform;
                dot.name = "( " + i + ", " + j + " )";
                allDots[i, j] = dot;
            }
        }
    }

    private bool MatchesAt(int column, int row, GameObject piece)
    {
        if (column > 1 && row > 1)
        {
            if (allDots[column - 1, row].CompareTag(piece.tag) && allDots[column - 2, row].CompareTag(piece.tag))
            {
                return true;
            }

            if (allDots[column, row - 1].CompareTag(piece.tag) && allDots[column, row - 2].CompareTag(piece.tag))
            {
                return true;
            }
        }
        else if (column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allDots[column, row - 1].CompareTag(piece.tag) && allDots[column, row - 2].CompareTag(piece.tag))
                {
                    return true;
                }
            }

            if (column <= 1)
                return false;
            if (allDots[column - 1, row].CompareTag(piece.tag) && allDots[column - 2, row].CompareTag(piece.tag))
            {
                return true;
            }
        }

        return false;
    }

    private void DestroyMatchesAt(int column, int row)
    {
        if (!allDots[column, row].GetComponent<Dot>().isMatched)
            return;
        //How many elements are in the matched pieces list from find matches?
        if (findMatches.CurrentMatches.Count == 4 || findMatches.CurrentMatches.Count == 7)
        {
            findMatches.CheckBombs();
        }

        var particle = Instantiate(destroyParticle,
            allDots[column, row].transform.position,
            Quaternion.identity);
        Destroy(particle, .5f);
        Destroy(allDots[column, row]);
        allDots[column, row] = null;
    }

    public void DestroyMatches()
    {
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }

        findMatches.CurrentMatches.Clear();
        StartCoroutine(DecreaseRowCo());
    }

    private IEnumerator DecreaseRowCo()
    {
        var nullCount = 0;
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
                {
                    allDots[i, j].GetComponent<Dot>().row -= nullCount;
                    allDots[i, j] = null;
                }
            }

            nullCount = 0;
        }

        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoardCo());
    }

    private void RefillBoard()
    {
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                    continue;
                var tempPosition = new Vector2(i, j + offSet);
                var dotToUse = Random.Range(0, dots.Length);
                var piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                allDots[i, j] = piece;
                piece.GetComponent<Dot>().row = j;
                piece.GetComponent<Dot>().column = i;
            }
        }
    }

    private bool MatchesOnBoard()
    {
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                    continue;
                if (allDots[i, j].GetComponent<Dot>().isMatched)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private IEnumerator FillBoardCo()
    {
        RefillBoard();
        yield return new WaitForSeconds(.5f);

        while (MatchesOnBoard())
        {
            yield return new WaitForSeconds(.5f);
            DestroyMatches();
        }

        findMatches.CurrentMatches.Clear();
        currentDot = null;
        yield return new WaitForSeconds(.5f);
        currentState = GameState.move;
    }
}