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

    public Cell[,] CreateGrid(Sprite cellSprite = null)
    {       
        this.cells = new Cell[this.gridRow, this.gridColumn];
        this.availablePositions = new List<Vector2Int>();
        for (int i = 0; i < this.gridRow; i++)
        {
            for (int j = 0; j < this.gridColumn; j++)
            {
                this.createCell(i, j);
            }
        }    
        return this.cells;
    }

    private void createCell(int rowId, int columnId)
    {
        GameObject cellObject = GameObject.Instantiate(cellPrefab, this.parent != null ? this.parent : null);
        cellObject.name = "Cell_" + this.gridCount;
        Cell cell = cellObject.GetComponent<Cell>(); 
        cell.SetTextContent("");
        cell.row = rowId;
        cell.col = columnId;
        this.cells[rowId, columnId] = cell;
        this.cells[rowId, columnId].cellId = this.gridCount;
        this.availablePositions.Add(new Vector2Int(rowId, columnId));
        this.gridCount += 1;
    }

    public Vector3 newCharacterPosition
    {
        get
        {
            this.characterPositionsCellIds.Clear();
            foreach(var cell in this.cells)
            {
                if(cell != null && cell.isPlayerStayed)
                {
                    this.characterPositionsCellIds.Add(cell.cellId);
                }
            }
            var id = this.GenerateUniqueRandomIntegers(1, 0, this.cells.Length, this.showCellIdList, this.characterPositionsCellIds)[0];
            LogController.Instance.debug("new Position id" + id);
            var newCellVector = this.availablePositions[id];
            return this.cells[newCellVector.x, newCellVector.y].transform.localPosition;
        }
    }

    public void updateNewWordPosition(Cell currentCell)
    {
        this.characterPositionsCellIds.Clear();
        foreach (var cell in this.cells)
        {
            if (cell != null && cell.isPlayerStayed && !this.characterPositionsCellIds.Contains(cell.cellId))
            {
                this.characterPositionsCellIds.Add(cell.cellId);
            }
        }
        var newWordId = this.GenerateUniqueRandomIntegers(1, 0, this.cells.Length, this.showCellIdList, this.characterPositionsCellIds)[0];
        LogController.Instance.debug("new Word Position id" + newWordId);
        this.showCellIdList.Add(newWordId);
        var newCellVector = this.availablePositions[newWordId];
        this.cells[newCellVector.x, newCellVector.y].SetTextContent(currentCell.content.text);
        if (this.showCellIdList.Contains(currentCell.cellId))
        {
            this.showCellIdList.Remove(currentCell.cellId);
            currentCell.SetTextContent("");
        }
    }

    public List<int> CharacterPositionsCellIds {
        get
        {
            this.characterPositionsCellIds = this.GenerateUniqueRandomIntegers(LoaderConfig.Instance.gameSetup.playerNumber, 0, this.cells.Length, this.showCellIdList);
            return this.characterPositionsCellIds;
        }
    }



    public List<int> CharacterStartupPositionsCellIds
    {
        get
        {
            this.characterPositionsCellIds.Clear();
            this.characterPositionsCellIds.Add(36); // p1
            this.characterPositionsCellIds.Add(40); // p2
            this.characterPositionsCellIds.Add(8); // p3
            this.characterPositionsCellIds.Add(12); // p4

            return this.characterPositionsCellIds;
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

            // Skip if the number is excluded
            if (combinedExcludedSet.Contains(randomNumber))
            {
                continue;
            }

            // Check if the number satisfies the difference > 1 condition
            bool isValid = true;

            // To avoid foreach, use HashSet.Contains for direct neighbor checks
            if (uniqueIntegers.Contains(randomNumber - 1) || uniqueIntegers.Contains(randomNumber + 1) || 
                uniqueIntegers.Contains(randomNumber - this.gridColumn) || uniqueIntegers.Contains(randomNumber + this.gridColumn) || 
                uniqueIntegers.Contains(randomNumber - this.gridColumn - 1) || uniqueIntegers.Contains(randomNumber - this.gridColumn + 1) || 
                uniqueIntegers.Contains(randomNumber + this.gridColumn - 1) || uniqueIntegers.Contains(randomNumber + this.gridColumn + 1))
            {
                isValid = false;
            }
            // If valid, add to the set
            if (isValid)
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
                cell.setCellDebugStatus(status);
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
                                                                this.cells.Length,
                                                                this.characterPositionsCellIds);

        for (int i = 0; i < this.showCellIdList.Count; i++)
        {
            Vector2Int position = this.availablePositions[this.showCellIdList[i]];
            char letter = (char)('A' + i);
            string displayText = $"{letter}";
            //string displayText = $"{letter}: {multipleWords[i]}";

            this.cells[position.x, position.y].SetTextContent(this.isMCType ? displayText : letters[i].ToString());
        }
    }



}
