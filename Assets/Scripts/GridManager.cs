using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

[Serializable]
public class GridManager
{
    public GameObject cellPrefab;
    public Transform parent;
    public int gridRow = 4;
    public int gridColumn = 4;
    public int maxRetries = 10;
    public Cell[,] cells;
    public List<int> characterPositionsCellIds = new List<int>();
    public List<int> disableCellIds = new List<int>();
    public List<int> showCellIdList = new List<int>();
    public List<Vector2Int> availablePositions = null;
    public bool showQuestionWordPosition = false;
    public bool isMCType = false;
    public int gridCount = 0;

    public Cell[,] CreateGrid(string[] multipleWords = null, string spellWord = null, Sprite cellSprite = null)
    {
        char[] letters = null;
        if (multipleWords != null && multipleWords.Length > 0)
        {
            this.isMCType = true;
        }

        if (!string.IsNullOrEmpty(spellWord))
        {
            letters = this.ShuffleStringToCharArray(spellWord);
            this.isMCType = false;
        }

        this.cells = new Cell[this.gridRow, this.gridColumn];
        this.availablePositions = new List<Vector2Int>();
        for (int i = 0; i < this.gridRow; i++)
        {
            for (int j = 0; j < this.gridColumn; j++)
            {
                GameObject cellObject = GameObject.Instantiate(cellPrefab, this.parent != null ? this.parent : null);
                cellObject.name = "Cell_" + i + "_" + j;
                Cell cell = cellObject.GetComponent<Cell>();
                cell.SetTextContent("");
                cell.row = i;
                cell.col = j;
                this.cells[i, j] = cell;
                this.cells[i, j].cellId = this.gridCount;
                this.availablePositions.Add(new Vector2Int(i, j));
                this.gridCount += 1;
            }
        }
        //System.Random random = new System.Random();
        //this.availablePositions = this.availablePositions.OrderBy(x => random.Next()).ToList();

        this.showCellIdList = this.GenerateUniqueRandomIntegers(this.isMCType ? multipleWords.Length : letters.Length, 
                                                                0, 
                                                                cells.Length);

        this.characterPositionsCellIds = this.GenerateUniqueRandomIntegers(4, 0, cells.Length, this.showCellIdList);

        for (int i=0; i < this.showCellIdList.Count; i++)
        {
            Vector2Int position =  this.availablePositions[this.showCellIdList[i]];
            this.cells[position.x, position.y].SetTextContent(this.isMCType ? multipleWords[i]: letters[i].ToString(),                         default, cellSprite);
        }
        
        return cells;
    }

    public Vector3 newCharacterPosition
    {
        get
        {
            var id = this.GenerateUniqueRandomIntegers(1, 0, this.cells.Length, this.showCellIdList, this.characterPositionsCellIds)[0];
            Debug.Log("new Position id" + id);
            var newCellVector = this.availablePositions[id];
            return this.cells[newCellVector.x, newCellVector.y].transform.localPosition;
        }
    }

    public List<int> GenerateUniqueRandomIntegers(int count, int minValue, int maxValue, params List<int>[] excludedLists)
    {
        HashSet<int> uniqueIntegers = new HashSet<int>();
        HashSet<int> combinedExcludedSet = new HashSet<int>(this.disableCellIds);

        // Combine all excluded lists into a single set
        foreach (var list in excludedLists)
        {
            foreach (var item in list)
            {
                combinedExcludedSet.Add(item);
            }
        }

        System.Random random = new System.Random();
        while (uniqueIntegers.Count < count)
        {
            int randomNumber = random.Next(minValue, maxValue);
            if (!combinedExcludedSet.Contains(randomNumber))
            {
                uniqueIntegers.Add(randomNumber);
            }
        }

        return new List<int>(uniqueIntegers);
    }


    char[] ShuffleStringToCharArray(string input)
    {
        char[] letters = input.ToCharArray();
        System.Random random = new System.Random();
        letters = letters.OrderBy(x => random.Next()).ToArray();

        return letters;
    }
    public void UpdateGridWithWord(string[] newMultipleWords=null, string newWord=null)
    {
       this.PlaceWordInGrid(newMultipleWords, newWord);
    }

    public void setAllCellsStatus(bool status = false)
    {
        foreach (var cell in cells)
        {
            if (!cell.isSelected)
            {
                cell.setCellStatus(status);
            }
        }
    }


    void PlaceWordInGrid(string[] multipleWords = null, string spellWord = null)
    {
        char[] letters = null;
        if (multipleWords != null && multipleWords.Length > 0)
        {
            this.isMCType = true;
        }

        if (!string.IsNullOrEmpty(spellWord))
        {
            letters = this.ShuffleStringToCharArray(spellWord);
            this.isMCType = false;
        }

        //System.Random random = new System.Random();
        //this.availablePositions = this.availablePositions.OrderBy(x => random.Next()).ToList();

        for (int i = 0; i < this.gridRow; i++)
        {
            for (int j = 0; j < this.gridColumn; j++)
            {
                this.cells[i, j].SetTextContent("");
            }
        }

        this.showCellIdList = this.GenerateUniqueRandomIntegers(this.isMCType ? multipleWords.Length : letters.Length,
                                                                0,
                                                                cells.Length);
        this.characterPositionsCellIds = this.GenerateUniqueRandomIntegers(4, 0, cells.Length, this.showCellIdList);

        for (int i = 0; i < this.showCellIdList.Count; i++)
        {
            Vector2Int position = this.availablePositions[this.showCellIdList[i]];
            this.cells[position.x, position.y].SetTextContent(this.isMCType ? multipleWords[i] : letters[i].ToString());
        }
    }



}
