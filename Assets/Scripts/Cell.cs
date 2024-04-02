using UnityEngine;

public class Cell : MonoBehaviour
{
    public enum CellState { Claimed, Fillable, Path, Temporary }
    public CellState state = CellState.Fillable;

    public Color fillableColor;
    public Color claimedColor;
    public Color pathColor;

    public SpriteRenderer spriteRenderer;
    public Vector2Int gridPosition;

    public void SetGridPosition(int x, int y)
    {
        gridPosition = new Vector2Int(x, y);
    }

    public void SetState(CellState newState)
    {
        state = newState;
        UpdateVisualState();
    }

    public void ResetState()
    {
        SetState(CellState.Fillable);
    }

    void UpdateVisualState()
    {
        switch (state)
        {
            case CellState.Claimed:
                spriteRenderer.color = claimedColor; // Example color
                break;
            case CellState.Fillable:
                spriteRenderer.color = fillableColor;
                break;
            case CellState.Path:
                spriteRenderer.color = pathColor;
                break;
        }
    }
}