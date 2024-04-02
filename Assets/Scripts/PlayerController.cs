using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    private Vector2 targetPosition;
    private Vector2 movementDirection;
    private bool isMoving = false;
    public GridManager gridManager;
    public GameManager gameManager;

    // Add these lines within the PlayerController class
    void OnEnable()
    {
        SwipeDetector.OnSwipeLeft += HandleSwipeLeft;
        SwipeDetector.OnSwipeRight += HandleSwipeRight;
        SwipeDetector.OnSwipeUp += HandleSwipeUp;
        SwipeDetector.OnSwipeDown += HandleSwipeDown;
    }

    void OnDisable()
    {
        SwipeDetector.OnSwipeLeft -= HandleSwipeLeft;
        SwipeDetector.OnSwipeRight -= HandleSwipeRight;
        SwipeDetector.OnSwipeUp -= HandleSwipeUp;
        SwipeDetector.OnSwipeDown -= HandleSwipeDown;
    }
    void HandleSwipeLeft()
    {
        if (!GameStateManager.Instance.isPaused())
        {
            movementDirection = Vector2.left;
        }
    }

    void HandleSwipeRight()
    {
        if (!GameStateManager.Instance.isPaused())
        {
            movementDirection = Vector2.right;
        }
    }

    void HandleSwipeUp()
    {
        if (!GameStateManager.Instance.isPaused())
        {
            movementDirection = Vector2.up;
        }
    }

    void HandleSwipeDown()
    {
        if (!GameStateManager.Instance.isPaused())
        {
            movementDirection = Vector2.down;
        }
    }
    void Start()
    {
        SetStartingPosition();
        targetPosition = transform.position;
    }

    void SetStartingPosition()
    {
        // Placing the player at the bottom center of the grid
        float startX = gridManager.width / 2f;
        float startY = 0; // Bottom of the grid
        transform.position = new Vector3(startX, startY, 0);
        movementDirection = Vector2.zero;
    }

    public void ResetPosition()
    {
        SetStartingPosition(); // Resets the player to the starting position
        targetPosition = transform.position; // Updates the target position to the new starting position
        StopAllCoroutines(); // Stops the player movement coroutine if it's running
        isMoving = false; // Resets the movement flag
    }

    void Update()
    {
        UpdateDirection();
        if (isMoving)
        {
            var realPosition = ManualInterpolation(transform.position, targetPosition, speed * Time.deltaTime);
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            if ((Vector2)transform.position == targetPosition)
            {
                isMoving = false;
                Cell currentCell = gridManager.GetCellAtPosition(transform.position);
                if (currentCell != null)
                {
                    HandleCellInteraction(currentCell);
                }
                if (movementDirection!=Vector2.zero)
                {
                    transform.position = realPosition;
                    UpdateDirection();
                }
            }
        }
    }

    Vector2 ManualInterpolation(Vector2 start, Vector2 end, float t)
    {
        Vector2 direction = (end - start).normalized;
        float distance = Vector2.Distance(start, end);
        if (distance < t)
        {
            // Move beyond the target by t
            return start + direction * t;
        }
        else
        {
            // If the distance is not smaller than t, just return the target position
            return end;
        }
        //return new Vector2(start.x + (end.x - start.x) * t, start.y + (end.y - start.y) * t);
    }

    void UpdateDirection()
    {
        if (!GameStateManager.Instance.isPaused() && !isMoving)
        {
            // Update movement direction based on input
            UpdateMovementDirection();

            // Calculate the next target position based on the current movement direction
            Vector2 attemptedPosition = targetPosition + movementDirection;

            // Check if the attempted position is within the grid bounds and is fillable
            if (IsPositionValid(attemptedPosition))
            {
                targetPosition = attemptedPosition;
                isMoving = true;
            }
        }
    }
    void UpdateMovementDirection()
    {
        int horizontal = (int)Input.GetAxisRaw("Horizontal");
        int vertical = (int)Input.GetAxisRaw("Vertical");

        // Prevent diagonal movement and update direction only if there's input
        if (horizontal != 0)
        {
            movementDirection = new Vector2(horizontal, 0);
        }
        else if (vertical != 0)
        {
            movementDirection = new Vector2(0, vertical);
        }
    }

    bool IsPositionValid(Vector2 position)
    {
        // Check grid bounds
        if (position.x < 0 || position.x >= gridManager.width || position.y < 0 || position.y >= gridManager.height)
        {
            return false;
        }

        return true;
        // Check if the cell is fillable
        //Cell cell = gridManager.GetCellAtPosition(position);
        //return cell != null && cell.state == Cell.CellState.Fillable;
    }



    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Enemy") || collider.gameObject.CompareTag("Spy"))
        {
            KillPlayer();
        }
    }

    void KillPlayer()
    {
        // Logic to handle player death
        gameManager.PlayerDies();
        Debug.Log("Player has been killed!");
    }

    void HandleCellInteraction(Cell cell)
    {
        if (cell.state == Cell.CellState.Fillable)
        {
            // Mark the cell as part of the path
            cell.SetState(Cell.CellState.Path);
            // Add the cell to a list in GridManager to keep track of the path
            gridManager.AddCellToPath(cell);
        }
        else if (cell.state == Cell.CellState.Path)
        {
            // Handle the case where the player intersects their own path
            if (gridManager.currentPath.Count == 0 || gridManager.currentPath[gridManager.currentPath.Count - 1] != cell)
            {
                gameManager.PlayerDies();
            }
        }
        else if (cell.state == Cell.CellState.Claimed && gridManager.IsPathStarted())
        {
            gridManager.CompleteLoop(transform.position);
            movementDirection = Vector2.zero;
        }
    }
}