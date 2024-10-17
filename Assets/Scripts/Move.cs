using UnityEngine;

public sealed class Move : MonoBehaviour
{
    private Piece piece;
    private Vector2 oldPosition;
    private Vector2 newPosition;

    public Move()
    {

    }

    public void SaveMove(Piece piece, Vector2 oldPosition, Vector2 newPosition)
    {
        this.piece = piece;
        this.oldPosition = oldPosition;
        this.newPosition = newPosition;
    }

    public void Deconstruct(out Piece piece, out Vector2 oldPosition, out Vector2 newPosition)
    {
        piece = this.piece;
        oldPosition = this.oldPosition;
        newPosition = this.newPosition;
    }
}
