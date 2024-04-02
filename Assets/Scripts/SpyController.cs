using System.Collections.Generic;
using UnityEngine;

public class SpyController : MonoBehaviour
{
    public float speed = 2.0f;
    private Vector2 direction;
    private GridManager gridManager;
    private GameManager gameManager;

    void Start()
    {
        float[] possibleAngles = new float[] { 45f, 135f, -45f, -135f };
        float angle = possibleAngles[Random.Range(0, possibleAngles.Length)];
        direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        gridManager = FindObjectOfType<GridManager>();
        gameManager = FindObjectOfType<GameManager>();
        speed = FindObjectOfType<GameManager>().enemySpeed;
    }
    void Update()
    {
        if (!GameStateManager.Instance.isPaused())
        Move();
    }

    void Move()
    {
        Vector2 startPosition = transform.position;
        Vector2 targetPosition = startPosition + (direction * speed * Time.deltaTime);
        Vector2 currentPosition = startPosition;
        float step = 0.2f;//speed * Time.deltaTime / Vector2.Distance(startPosition, targetPosition);
        bool collisionDetected = false;
        for (float t = 0; t <= 1; t += step)
        {
            Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, t);
            if (CheckCollision(currentPosition, newPosition))
            {
                collisionDetected = true;
                break;
            }
            currentPosition = newPosition;
        }
        if (!collisionDetected)
        {
            transform.position = targetPosition;
        }
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    bool CheckCollision(Vector2 fromPosition, Vector2 toPosition)
    {
        Vector2 movementDirection = (toPosition - fromPosition).normalized;
        Vector2 checkPosition = toPosition + (movementDirection * 0.5f);
        
        Cell checkCell = gridManager.GetCellAtPosition(checkPosition);// + new Vector2(0.5f,0.5f));
        if (checkCell != null)
        {
            switch (checkCell.state)
            {
                case Cell.CellState.Fillable:
                    ReflectDirection(gridManager.GetCellAtPosition(fromPosition), checkCell); // + new Vector2(0.5f,0.5f)
                    return true;
            }
        }
        else
        {
        Vector2 edgeNormal = Vector2.zero;
        if (checkPosition.x < 0.0f) edgeNormal = Vector2.right;
        else if (checkPosition.x > (gridManager.width-1)) edgeNormal = Vector2.left;
        else if (checkPosition.y < 0.0f) edgeNormal = Vector2.up;
        else if (checkPosition.y > (gridManager.height-1)) edgeNormal = Vector2.down;
        direction = Vector2.Reflect(direction, edgeNormal);
        return true;
        }
        return false;
    }

    void ReflectDirection(Cell fromCell, Cell toCell)
    {
        Vector2 fromCellPosition = gridManager.GetCellWorldPosition(fromCell.gridPosition);
        Vector2 toCellPosition = gridManager.GetCellWorldPosition(toCell.gridPosition);
        Vector2Int diff = Vector2Int.RoundToInt(toCellPosition - fromCellPosition);
        Vector2 hitNormal;
        
        if (diff.x == -1 && diff.y == -1)
        {
            Cell rightCell = gridManager.GetCellAtPosition(toCell.gridPosition + new Vector2Int(1, 0));
            Cell upCell = gridManager.GetCellAtPosition(toCell.gridPosition + new Vector2Int(0, 1));

            if (rightCell.state == Cell.CellState.Fillable && upCell.state == Cell.CellState.Fillable)
                hitNormal = new Vector2(1,1).normalized;
            else
            if (rightCell.state == Cell.CellState.Fillable)
                hitNormal = new Vector2(0,1);
            else
                hitNormal = new Vector2(1,0);
            //hitNormal = rightCell.state == Cell.CellState.Claimed || upCell.state == Cell.CellState.Claimed ? new Vector2(-1, -1) : new Vector2(1, 1);
        }
        else if (diff.x == -1 && diff.y == 1)
        {
            Cell rightCell = gridManager.GetCellAtPosition(toCell.gridPosition + new Vector2Int(1, 0));
            Cell downCell = gridManager.GetCellAtPosition(toCell.gridPosition + new Vector2Int(0, -1));

            if (rightCell.state == Cell.CellState.Fillable && downCell.state == Cell.CellState.Fillable)
                hitNormal = new Vector2(1,-1).normalized;
            else
            if (rightCell.state == Cell.CellState.Fillable)
                hitNormal = new Vector2(0,-1);
            else
                hitNormal = new Vector2(1,0);
            //hitNormal = rightCell.state == Cell.CellState.Claimed || downCell.state == Cell.CellState.Claimed ? new Vector2(-1, 1) : new Vector2(1, -1);
        }
        else if (diff.x == 1 && diff.y == -1)
        {
            Cell leftCell = gridManager.GetCellAtPosition(toCell.gridPosition + new Vector2Int(-1, 0));
            Cell upCell = gridManager.GetCellAtPosition(toCell.gridPosition + new Vector2Int(0, 1));
            
            if (leftCell.state == Cell.CellState.Fillable && upCell.state == Cell.CellState.Fillable)
                hitNormal = new Vector2(-1,1).normalized;
            else
            if (leftCell.state == Cell.CellState.Fillable)
                hitNormal = new Vector2(0,1);
            else
                hitNormal = new Vector2(-1,0);
            //hitNormal = leftCell.state == Cell.CellState.Claimed || upCell.state == Cell.CellState.Claimed ? new Vector2(1, -1) : new Vector2(-1, 1);
        }
        else if (diff.x == 1 && diff.y == 1)
        {
            Cell leftCell = gridManager.GetCellAtPosition(toCell.gridPosition + new Vector2Int(-1, 0));
            Cell downCell = gridManager.GetCellAtPosition(toCell.gridPosition + new Vector2Int(0, -1));
            
            if (leftCell.state == Cell.CellState.Fillable && downCell.state == Cell.CellState.Fillable)
                hitNormal = new Vector2(-1,-1).normalized;
            else
            if (leftCell.state == Cell.CellState.Fillable)
                hitNormal = new Vector2(0,-1);
            else
                hitNormal = new Vector2(-1,0);
            //hitNormal = leftCell.state == Cell.CellState.Claimed || downCell.state == Cell.CellState.Claimed ? new Vector2(1, 1) : new Vector2(-1, -1);
        }
        else if (diff.y == 0 && diff.x == 1)
        {
            hitNormal = new Vector2(-1, 0);
        }
        else if (diff.y == 0 && diff.x == -1)
        {
            hitNormal = new Vector2(1, 0);
        }
        else if (diff.y == 1 && diff.x == 0)
        {
            hitNormal = new Vector2(0, 1);
        }
        else //if (diff.y == -1 && diff.x == 0)
        {
            hitNormal = new Vector2(0, -1);
        }


        direction = Vector2.Reflect(direction, hitNormal);
    }
}
