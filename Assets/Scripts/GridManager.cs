using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject cellPrefab;
    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;
    public int totalCells = 0;
    public int totalClaimedCells = 0;



    private Cell[,] grid;
    public List<Cell> claimedCells = new List<Cell>();
    public List<Cell> fillableCells = new List<Cell>();
    public List<Cell> currentPath = new List<Cell>();

    public void CreateGrid()
    {
        grid = new Cell[width, height];
        claimedCells.Clear();
        fillableCells.Clear();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cellObj = Instantiate(cellPrefab, new Vector3(x * cellSize, y * cellSize, 0), Quaternion.identity);
                cellObj.transform.parent = this.transform;
                Cell cell = cellObj.GetComponent<Cell>();
                cell.SetGridPosition(x, y); // Set the grid position of the cell
                // Set the edges of the grid to a claimed state
                if (x <= 1 || y <= 1 || x >= width - 2 || y >= height - 2)
                {
                    cell.SetState(Cell.CellState.Claimed);
                    claimedCells.Add(cell);
                }
                else
                {
                    cell.SetState(Cell.CellState.Fillable);
                    fillableCells.Add(cell);
                    totalCells++;
                }
                grid[x, y] = cell;
            }
        }
    }

    public void ClearGrid()
    {
        claimedCells.Clear();
        fillableCells.Clear();
        if (grid != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y] != null)
                    {
                        Destroy(grid[x, y].gameObject);
                    }
                }
            }
        }
        grid = null;
        totalCells = 0;
        totalClaimedCells = 0;
        currentPath.Clear();
    }
    public Cell GetCellAtPosition(Vector2 position)
    {
        int x = Mathf.RoundToInt(position.x / cellSize);
        int y = Mathf.RoundToInt(position.y / cellSize);

        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return grid[x, y];
        }
        return null;
    }

    public Cell GetCellAtPosition(Vector2Int position)
    {
        return grid[position.x, position.y];
    }

    public void AdjustCamera()
    {
        // Assuming the camera is orthographic
        Camera.main.orthographic = true;

        // Calculate the size needed to fit the grid
        float cameraSizeX = (width * cellSize) / Camera.main.aspect / 2;
        float cameraSizeY = (height * cellSize) / 2;
        Camera.main.orthographicSize = Mathf.Max(cameraSizeX, cameraSizeY);

        // Center the camera on the grid
        Camera.main.transform.position = new Vector3((width * cellSize) / 2 - cellSize / 2, (height * cellSize) / 2 - cellSize / 2, -10);
    }

    public void AddCellToPath(Cell cell)
    {
        if (!currentPath.Contains(cell))
        {
            currentPath.Add(cell);
        }
    }

    public bool IsPathStarted()
    {
        return currentPath.Count > 0;
    }

    public void CompleteLoop(Vector2 playerPosition)
    {
        FillEnclosedArea();
        ClearPath();
    }

    public Vector2 GetCellWorldPosition(Vector2Int gridPosition)
    {
        float worldX = gridPosition.x * cellSize + cellSize / 2;
        float worldY = gridPosition.y * cellSize + cellSize / 2;
        return new Vector2(worldX, worldY);
    }

    public void ClearPath()
    {
        currentPath.ForEach(cell => cell.ResetState());
        currentPath.Clear();
    }

    Cell GetCellAt(Vector2Int position)
    {
        return grid[position.x, position.y];
    }

    void FillEnclosedArea()
    {
        int newlyClaimedCount = 0;

        // Mark all cells in the current path as claimed
        foreach (Cell cell in currentPath)
        {
            if(cell.state != Cell.CellState.Claimed) // Check if the cell is not already claimed
            {
                cell.SetState(Cell.CellState.Claimed);
                newlyClaimedCount++; // Increment the total claimed cells count
            }
        }
        currentPath.Clear();

        // Mark all fillable cells as temporary to prepare for flood fill
        foreach (Cell cell in grid)
        {
            if (cell.state == Cell.CellState.Fillable)
            {
                cell.SetState(Cell.CellState.Temporary);
            }
        }

        // Use enemy positions as starting points for the flood fill
        GameManager gameManager = FindObjectOfType<GameManager>();
        foreach (EnemyController enemy in gameManager.enemies)
        {
            Cell enemyCell = GetCellAt(new Vector2Int(Mathf.RoundToInt(enemy.transform.position.x), Mathf.RoundToInt(enemy.transform.position.y)));
            if (enemyCell != null)
            {
                ScanlineFloodFill(enemyCell);
            }
        }

        // The remaining temporary cells are enclosed and should be claimed

        foreach (Cell cell in grid)
        {
            if (cell.state == Cell.CellState.Temporary)
            {
                cell.SetState(Cell.CellState.Claimed);
                newlyClaimedCount++;
            }
        }

        totalClaimedCells += newlyClaimedCount; // Update the total claimed cells count
        gameManager.IncreaseScore(newlyClaimedCount);
        gameManager.Percent((float)totalClaimedCells/(float)totalCells);
    }

    // Removed CountClaimedCells function as it's no longer needed
    void ScanlineFloodFill(Cell startCell)
    {
        if (startCell == null || startCell.state != Cell.CellState.Temporary)
        {
            return;
        }

        Queue<Cell> queue = new Queue<Cell>();
        queue.Enqueue(startCell);

        while (queue.Count > 0)
        {
            Cell cell = queue.Dequeue();
            if (cell.state == Cell.CellState.Temporary)
            {
                cell.SetState(Cell.CellState.Fillable);
                Vector2Int[] directions = { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0) };

                foreach (Vector2Int dir in directions)
                {
                    Cell neighbor = GetCellAt(cell.gridPosition + dir);
                    if (neighbor.state == Cell.CellState.Temporary)
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
    }
}