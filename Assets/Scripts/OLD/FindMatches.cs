using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FindMatches : MonoBehaviour
{
    private Board _board;
    public readonly List<GameObject> CurrentMatches = new List<GameObject>();

    // Use this for initialization
    private void Start()
    {
        _board = FindObjectOfType<Board>();
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCo());
    }

    private IEnumerable<GameObject> IsRowBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        if (dot1.isRowBomb)
        {
            return CurrentMatches.Union(GetRowPieces(dot1.row));
        }

        if (dot2.isRowBomb)
        {
            return CurrentMatches.Union(GetRowPieces(dot2.row));
        }

        return dot3.isRowBomb ? CurrentMatches.Union(GetRowPieces(dot3.row)) : new List<GameObject>();
    }

    private IEnumerable<GameObject> IsColumnBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        if (dot1.isColumnBomb)
        {
            return CurrentMatches.Union(GetColumnPieces(dot1.column));
        }

        if (dot2.isColumnBomb)
        {
            return CurrentMatches.Union(GetColumnPieces(dot2.column));
        }

        return dot3.isColumnBomb ? CurrentMatches.Union(GetColumnPieces(dot3.column)) : new List<GameObject>();
    }

    private void AddToListAndMatch(GameObject dot)
    {
        if (!CurrentMatches.Contains(dot))
        {
            CurrentMatches.Add(dot);
        }

        dot.GetComponent<Dot>().isMatched = true;
    }

    private void GetNearbyPieces(GameObject dot1, GameObject dot2, GameObject dot3)
    {
        AddToListAndMatch(dot1);
        AddToListAndMatch(dot2);
        AddToListAndMatch(dot3);
    }

    private IEnumerator FindAllMatchesCo()
    {
        yield return new WaitForSeconds(.2f);
        for (var i = 0; i < _board.width; i++)
        {
            for (var j = 0; j < _board.height; j++)
            {
                var currentDot = _board.allDots[i, j];
                if (currentDot == null)
                    continue;
                var currentDotDot = currentDot.GetComponent<Dot>();
                if (i > 0 && i < _board.width - 1)
                {
                    var leftDot = _board.allDots[i - 1, j];
                    if (leftDot == null)
                        continue;
                    var leftDotDot = leftDot.GetComponent<Dot>();
                    var rightDot = _board.allDots[i + 1, j];
                    if (rightDot == null)
                        continue;
                    var rightDotDot = rightDot.GetComponent<Dot>();
                    if (leftDot != null && rightDot != null && leftDot.CompareTag(currentDot.tag) &&
                        rightDot.CompareTag(currentDot.tag))
                    {
                        yield return CurrentMatches.Union(IsRowBomb(leftDotDot, currentDotDot, rightDotDot));
                        yield return CurrentMatches.Union(IsColumnBomb(leftDotDot, currentDotDot, rightDotDot));

                        GetNearbyPieces(leftDot, currentDot, rightDot);
                    }
                }

                if (j <= 0 || j >= _board.height - 1)
                    continue;
                var upDot = _board.allDots[i, j + 1];
                if (upDot == null)
                    continue;
                var upDotDot = upDot.GetComponent<Dot>();
                var downDot = _board.allDots[i, j - 1];
                if (downDot == null)
                    continue;
                var downDotDot = downDot.GetComponent<Dot>();
                if (upDot == null || downDot == null || !upDot.CompareTag(currentDot.tag) ||
                    !downDot.CompareTag(currentDot.tag))
                    continue;
                yield return CurrentMatches.Union(IsColumnBomb(upDotDot, currentDotDot, downDotDot));
                yield return CurrentMatches.Union(IsRowBomb(upDotDot, currentDotDot, downDotDot));

                GetNearbyPieces(upDot, currentDot, downDot);
            }
        }
    }

    public void MatchPiecesOfColor(string color)
    {
        for (var i = 0; i < _board.width; i++)
        {
            for (var j = 0; j < _board.height; j++)
            {
                //Check if that piece exists
                if (_board.allDots[i, j] == null)
                    continue;
                //Check the tag on that dot
                if (_board.allDots[i, j].CompareTag(color))
                {
                    //Set that dot to be matched
                    _board.allDots[i, j].GetComponent<Dot>().isMatched = true;
                }
            }
        }
    }

    private IEnumerable<GameObject> GetColumnPieces(int column)
    {
        var dots = new List<GameObject>();
        for (var i = 0; i < _board.height; i++)
        {
            if (_board.allDots[column, i] == null)
                continue;
            dots.Add(_board.allDots[column, i]);
            _board.allDots[column, i].GetComponent<Dot>().isMatched = true;
        }

        return dots;
    }

    private IEnumerable<GameObject> GetRowPieces(int row)
    {
        var dots = new List<GameObject>();
        for (var i = 0; i < _board.width; i++)
        {
            if (_board.allDots[i, row] == null)
                continue;
            dots.Add(_board.allDots[i, row]);
            _board.allDots[i, row].GetComponent<Dot>().isMatched = true;
        }

        return dots;
    }

    public void CheckBombs()
    {
        //Did the player move something?
        if (_board.currentDot == null)
            return;
        //Is the piece they moved matched?
        if (_board.currentDot.isMatched)
        {
            //make it unmatched
            _board.currentDot.isMatched = false;
            //Decide what kind of bomb to make
            /*
                int typeOfBomb = Random.Range(0, 100);
                if(typeOfBomb < 50){
                    //Make a row bomb
                    board.currentDot.MakeRowBomb();
                }else if(typeOfBomb >= 50){
                    //Make a column bomb
                    board.currentDot.MakeColumnBomb();
                }
                */
            if ((_board.currentDot.swipeAngle > -45 && _board.currentDot.swipeAngle <= 45)
                || (_board.currentDot.swipeAngle < -135 || _board.currentDot.swipeAngle >= 135))
            {
                _board.currentDot.MakeRowBomb();
            }
            else
            {
                _board.currentDot.MakeColumnBomb();
            }
        }
        //Is the other piece matched?
        else if (_board.currentDot.otherDot != null)
        {
            var otherDot = _board.currentDot.otherDot.GetComponent<Dot>();
            //Is the other Dot matched?
            if (!otherDot.isMatched)
                return;
            //Make it unmatched
            otherDot.isMatched = false;
            /*
                    //Decide what kind of bomb to make
                    int typeOfBomb = Random.Range(0, 100);
                    if (typeOfBomb < 50)
                    {
                        //Make a row bomb
                        otherDot.MakeRowBomb();
                    }
                    else if (typeOfBomb >= 50)
                    {
                        //Make a column bomb
                        otherDot.MakeColumnBomb();
                    }
                    */
            if (_board.currentDot.swipeAngle > -45 && _board.currentDot.swipeAngle <= 45 ||
                _board.currentDot.swipeAngle < -135 || _board.currentDot.swipeAngle >= 135)
            {
                otherDot.MakeRowBomb();
            }
            else
            {
                otherDot.MakeColumnBomb();
            }
        }
    }
}