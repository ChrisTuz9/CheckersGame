using UnityEngine;

public class Piece
{
    public PieceColor Color { get; private set; }
    public GameObject PieceObject { get; set; }
    public bool IsKing { get; private set; }

    public Piece(PieceColor color, GameObject pieceObject)
    {
        this.Color = color;
        this.PieceObject = pieceObject;
    }

    public void SetPosition(Vector3 position)
    {
        PieceObject.transform.position = position;
    }

    public Vector3 GetPosition()
    {
        return PieceObject.transform.position;
    }

    public void PromoteToKing()
    {
        IsKing = true;
    }
}