using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private Color[] blockColors;
    [SerializeField] private GameObject gridBoxPrefab;
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private HashSet<GameObject> blocks = new HashSet<GameObject>();

    [SerializeField] private GridRow[] gridRows;

    [SerializeField] private int xGridSize;
    [SerializeField] private int yGridSize;

    [System.Serializable]
    private class GridBox
    {
        public Vector3 position;
        public GameObject block;

        public GridBox(Vector3 _position)
        {
            position = _position;
        }

        public bool IsBlockInPosition()
        {
            return block.transform.position == position;
        }
    }

    [System.Serializable]
    private class GridRow
    {
        public GridBox[] gridBoxes;
        public GridRow(int rowSize, float yRowPosition)
        {
            gridBoxes = new GridBox[rowSize];
            for (int i = 0; i < rowSize; i++)
            {
                Vector3 boxPosition = new Vector3(i, yRowPosition, 0);
                gridBoxes[i] = new GridBox(boxPosition);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GameEventManager.current.onBlockClick += BlockDestruction;

        InitiateGrid();
        StartFillGameGrid();

        InvokeRepeating("CheckAndFillGrid", 0, 0.01f);
    }

    private void InitiateGrid()
    {
        gridRows = new GridRow[yGridSize];

        //Create gridRow and gridBoxes
        for (int i = 0; i < yGridSize; i++)
        {
            gridRows[i] = new GridRow(xGridSize, i);
        }

        //Instantiate box prefabs
        for (int i = 0; i < gridRows.Length; i++)
        {
            for (int j = 0; j < gridRows[i].gridBoxes.Length; j++)
            {
                GameObject newGridBoxGO = Instantiate(gridBoxPrefab, gridRows[i].gridBoxes[j].position, Quaternion.identity);
                newGridBoxGO.transform.parent = transform;
            }
        }
    }

    public void StartFillGameGrid()
    {
        StartCoroutine(FillGameGrid());
    }

    IEnumerator FillGameGrid()
    {
        //Disable player input with event
        GameEventManager.current.GridFillStart();

        int securityCounter = 10000;
        while(!IsGridFull() && securityCounter > 0)
        {
            //Make all blocks with free space down go down
            MakeBlocksGoDown();

            //Fill empty spaces at upper row
            FillUpperRowWithBlocks();

            yield return new WaitForSeconds(0.1f);
            securityCounter--;
        }

        //Enable player input with event
        GameEventManager.current.GridFillFinish();
    }

    private bool IsGridFull()
    {
        for (int i = 0; i < gridRows.Length; i++)
            for (int j = 0; j < gridRows[i].gridBoxes.Length; j++)
                if (gridRows[i].gridBoxes[j].block == null)
                    return false;
        return true;
    }

    private void FillUpperRowWithBlocks()
    {
        GridRow upperGridRow = gridRows[gridRows.Length - 1];
        int gridRowLength = upperGridRow.gridBoxes.Length;

        for (int i = 0; i < gridRowLength; i++)
            if (upperGridRow.gridBoxes[i].block == null)
                InstantiateBlock(i, gridRows.Length - 1);

        UpdateGrid();
    }

    private void InstantiateBlock(int xSpawnPos, int ySpawnPos)
    {
        GameObject newBlock = Instantiate(blockPrefab);
        blocks.Add(newBlock);
        newBlock.transform.position = new Vector2(xSpawnPos, ySpawnPos);

        //Give a random color
        int randomIndex = Random.Range(0, blockColors.Length);
        newBlock.GetComponent<SpriteRenderer>().color = blockColors[randomIndex];
    }

    private void UpdateGrid()
    {
        foreach (GameObject block in blocks)
        {
            int xBlockPosition = Mathf.RoundToInt(block.transform.position.x);
            int yBlockPosition = Mathf.RoundToInt(block.transform.position.y);
            gridRows[yBlockPosition].gridBoxes[xBlockPosition].block = block;
        }
    }

    //Make all rows above a row index go down
    private void MakeBlocksGoDown()
    {
        for (int i = 1; i < gridRows.Length ; i++)
        {
            for (int j = 0; j < gridRows[i].gridBoxes.Length; j++)
            {
                if (gridRows[i].gridBoxes[j].block != null && gridRows[i - 1].gridBoxes[j].block == null)
                {
                    gridRows[i - 1].gridBoxes[j].block = gridRows[i].gridBoxes[j].block;
                    gridRows[i].gridBoxes[j].block.transform.position += new Vector3(0, -1, 0);
                    gridRows[i].gridBoxes[j].block = null;
                }
            }
        }
    }

    private void BlockDestruction(Vector2 blockPos, Color colorToDestroy)
    {
        //Check if blockPos is out of bounds
        if (blockPos.x < 0 || blockPos.x >= xGridSize || blockPos.y < 0 || blockPos.y >= yGridSize)
            return;

        //Get block object from pos
        int xBlockPosition = Mathf.RoundToInt(blockPos.x);
        int yBlockPosition = Mathf.RoundToInt(blockPos.y);
        GameObject block = gridRows[yBlockPosition].gridBoxes[xBlockPosition].block;

        //Check if is the same color to destroy
        if (block == null)
            return;

        //Destroy the block and remove it from data if is the right color
        if (block.GetComponent<SpriteRenderer>().color == colorToDestroy)
        {
            blocks.Remove(block);
            Destroy(block);
            gridRows[yBlockPosition].gridBoxes[xBlockPosition].block = null;
        }
        else
            return;

        //Make the behavior recursive for adjacent blocks 
        BlockDestruction(new Vector2(xBlockPosition - 1, yBlockPosition), colorToDestroy);
        BlockDestruction(new Vector2(xBlockPosition + 1, yBlockPosition), colorToDestroy);
        BlockDestruction(new Vector2(xBlockPosition, yBlockPosition - 1), colorToDestroy);
        BlockDestruction(new Vector2(xBlockPosition, yBlockPosition + 1), colorToDestroy);

        return;
    }

    private void CheckAndFillGrid()
    {
        if (IsGridFull())
            return;

        StartFillGameGrid();
    }
}
