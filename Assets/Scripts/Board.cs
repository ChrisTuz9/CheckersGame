using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Board : MonoBehaviour
{
    private const short boardSize = 8;
    private GameObject[,] tiles = new GameObject[boardSize, boardSize];
    private GameObject blackTilePreFabs, whiteTilePreFabs, blackPiecePreFabs, whitePiecePreFabs, blackKingPreFabs, whiteKingPreFabs;
    private int _playerPiecesCount, _enemyPiecesCount;
    public Piece[,] pieces = new Piece[boardSize, boardSize];
    public PieceColor playerColor { get; set; }
    public PieceColor enemyColor { get; set; }

    public int playerPiecesCount 
    {
        get
        {
            return _playerPiecesCount;
        }
        private set
        {
            _playerPiecesCount = value;
        }
    }
    public int enemyPiecesCount
    {
        get
        {
            return  _enemyPiecesCount;
        }
        private set
        {
            _enemyPiecesCount = value;
        }
    }

    public void SetTilesPreFabs(GameObject whiteTilePreFabs, GameObject blackTilePreFabs)
    {
        this.whiteTilePreFabs = whiteTilePreFabs;
        this.blackTilePreFabs = blackTilePreFabs;
    }

    public void SetPiecesPreFabs(GameObject whitePiecePreFabs, GameObject blackPiecePreFabs, GameObject whiteKingPreFabs, GameObject blackKingPreFabs)
    {
        this.whitePiecePreFabs = whitePiecePreFabs;
        this.blackPiecePreFabs = blackPiecePreFabs;
        this.whiteKingPreFabs = whiteKingPreFabs;
        this.blackKingPreFabs = blackKingPreFabs;
    }

    public void CreateBoard(PieceColor playerColor)
    {
        this.playerColor = playerColor;
        enemyColor = playerColor == PieceColor.WHITE ? PieceColor.BLACK : PieceColor.WHITE;

        bool isTileBlack = false;
        foreach (int row in Enumerable.Range(0, boardSize))
        {
            foreach (int col in Enumerable.Range(0, boardSize))
            {
                GameObject tilePrefab = isTileBlack ? blackTilePreFabs : whiteTilePreFabs;
                Vector2 position = new Vector2(col, row);
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
                tiles[row, col] = tile;

                isTileBlack = !isTileBlack;
            }
            isTileBlack = !isTileBlack;
        }

        PlacePieces(rowFirst: 0, rowLast: 3, playerColor == PieceColor.WHITE ? whitePiecePreFabs : blackPiecePreFabs, playerColor, out _playerPiecesCount);
        PlacePieces(rowFirst: boardSize - 3, rowLast:  boardSize, enemyColor == PieceColor.WHITE ? whitePiecePreFabs : blackPiecePreFabs, enemyColor, out _enemyPiecesCount);
    }

    private void PlacePieces(int rowFirst, int rowLast, GameObject PiecePrefab, PieceColor pieceColor, out int piecesCount)
    {
        piecesCount = 0;

        for (int row = rowFirst; row < rowLast; row++)
        {
            int colFirst = (row + 1) % 2, colLast = colFirst + 7;
            for (int col = colFirst; col < colLast; col += 2)
            {
                Vector3 position = new Vector3(col, row, -1);
                GameObject pieceObject = Instantiate(PiecePrefab, position, Quaternion.identity);
                Piece piece = new Piece(pieceColor, pieceObject);
                pieces[row, col] = piece;
                piecesCount++;
            }
        }
    }

    public bool inBoardRange(int x, int y)
    {
        return x >= 0 && x < boardSize && y >= 0 && y < boardSize;
    }

    public bool IsDiagonalMove(Vector2 oldPosition, Vector2 newPosition)
    {
        return Mathf.Abs(newPosition.x - oldPosition.x) == Mathf.Abs(newPosition.y - oldPosition.y);
    }

    public Piece GetPieceAtPosition(int x, int y)
    {
        if (inBoardRange(x, y))
        {
            return pieces[y, x];
        }
        return null;
    }

    public Piece GetPieceByGameObject(GameObject pieceObject)
    {
        foreach (int row in Enumerable.Range(0, boardSize))
        {
            foreach (int col in Enumerable.Range(0, boardSize))
            {
                Piece piece = GetPieceAtPosition(col, row);
                if (piece != null && piece.PieceObject == pieceObject)
                {
                    return piece;
                }
            }
        }
        return null;
    }

    public void UpdatePiecePosition(int oldX, int oldY, int newX, int newY)
    {
        if (inBoardRange(oldX, oldY) && inBoardRange(newX, newY))
        {
            if (pieces[newY, newX] == null)
            {
                Piece piece = pieces[oldY, oldX];
                RemovePieceAtPosition(oldX, oldY);
                Vector3 position = new Vector3(newX, newY, -1);
                if (!piece.IsKing && ((newY == 0 && piece.Color == enemyColor) || (newY == boardSize - 1 && piece.Color == playerColor)))
                {
                    Destroy(piece.PieceObject);
                    piece.PromoteToKing();
                    piece.PieceObject = Instantiate(piece.Color == PieceColor.WHITE ? whiteKingPreFabs : blackKingPreFabs, position, Quaternion.identity);
                }
                else
                {
                    piece.SetPosition(position);
                }
                pieces[newY, newX] = piece;
            }
        }
    }

    public void RemovePieceAtPosition(int x, int y)
    {
        if (inBoardRange(x, y))
        {
            Piece piece = pieces[y, x];
            if (piece != null)
            {
                pieces[y, x] = null;
            }
        }
    }


    public bool IsPathClear(Vector2 oldPosition, Vector2 newPosition)
    {
        int directionX = (int)Mathf.Sign(newPosition.x - oldPosition.x);
        int directionY = (int)Mathf.Sign(newPosition.y - oldPosition.y);
        int distance = Mathf.Abs((int)newPosition.x - (int)oldPosition.x);

        for (int i = 1; i < distance; i++)
        {
            int checkX = (int)oldPosition.x + directionX * i;
            int checkY = (int)oldPosition.y + directionY * i;

            if (GetPieceAtPosition(checkX, checkY) != null)
            {
                return false;
            }
        }
        return true;
    }


    public bool CanPieceCapture(int x, int y)
    {
        Piece piece = GetPieceAtPosition(x, y);
        if (piece == null)
            return false;

        int[] deltaX = { -1, 1, -1, 1 };
        int[] deltaY = { 1, 1, -1, -1 };

        if (piece.IsKing)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int distance = 1; inBoardRange(x + deltaX[i] * distance, y + deltaY[i] * distance); distance++)
                {
                    if (CheckCaptureInDirection(x, y, deltaX[i], deltaY[i], distance))
                        return true;
                }
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                if (CheckCaptureInDirection(x, y, deltaX[i], deltaY[i], 1))
                    return true;
            }
        }

        return false;
    }

    private bool CheckCaptureInDirection(int x, int y, int dirX, int dirY, int distance)
    {
        bool foundOpponent = false;
        int middleX = 0, middleY = 0;

        for (int i = 1; i <= distance; i++)
        {
            int checkX = x + dirX * i;
            int checkY = y + dirY * i;

            if (!inBoardRange(checkX, checkY))
                return false;

            Piece piece = GetPieceAtPosition(checkX, checkY);

            if (piece != null)
            {
                if (piece.Color == GetPieceAtPosition(x, y).Color)
                {
                    return false;
                }
                else if (foundOpponent)
                {
                    return false;
                }
                else
                {
                    foundOpponent = true;
                    middleX = checkX;
                    middleY = checkY;
                }
            }
        }

        int captureX = x + dirX * (distance + 1);
        int captureY = y + dirY * (distance + 1);

        if (inBoardRange(captureX, captureY) && GetPieceAtPosition(captureX, captureY) == null && foundOpponent)
        {
            return true;
        }

        return false;
    }


    public bool IsCapturePossibleForColor(PieceColor color)
    {
        foreach (int row in Enumerable.Range(0, boardSize))
        {
            foreach (int col in Enumerable.Range(0, boardSize))
            {
                Piece piece = GetPieceAtPosition(col, row);
                if (piece != null && piece.Color == color && CanPieceCapture(col, row))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool HasAnyValidMove(PieceColor color)
    {
        foreach (int row in Enumerable.Range(0, boardSize))
        {
            foreach (int col in Enumerable.Range(0, boardSize))
            {
                Piece piece = GetPieceAtPosition(col, row);
                if (piece != null && piece.Color == color)
                {
                    if (CanPieceMove(col, row) || CanPieceCapture(col, row))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool CanPieceMove(int x, int y)
    {
        Piece piece = GetPieceAtPosition(x, y);
        if (piece == null) return false;

        int[] deltaX = { -1, 1 };
        int[] deltaY = { playerColor == PieceColor.WHITE ? (piece.Color == PieceColor.WHITE ? 1 : -1) : (piece.Color == PieceColor.WHITE ? -1 : 1) };

        if (piece.IsKing)
        {
            deltaY = new int[] { 1, -1 };
        }

        for (int i = 0; i < deltaX.Length; i++)
        {
            for (int j = 0; j < deltaY.Length; j++)
            {
                int newX = x + deltaX[i];
                int newY = y + deltaY[j];
                if (inBoardRange(newX, newY) && GetPieceAtPosition(newX, newY) == null)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsGameOver(PieceColor color)
    {
        return _playerPiecesCount == 0 || _enemyPiecesCount == 0 || !HasAnyValidMove(color);
    }

    public void DestroyBoard()
    {
        foreach (int row in Enumerable.Range(0, boardSize))
        {
            foreach (int col in Enumerable.Range(0, boardSize))
            {
                Piece piece = GetPieceAtPosition(col, row);
                if (piece != null)
                {
                    Destroy(piece.PieceObject);
                    pieces[row, col] = null;
                }
            }
        }

        foreach (GameObject tile in tiles)
        {
            if (tile != null)
            {
                Destroy(tile);
            }
        }
    }

    public static Board operator - (Board board, Piece piece)
    {
        int x = (int)piece.GetPosition().x;
        int y = (int)piece.GetPosition().y;

        if (board.inBoardRange(x, y) && board.pieces[y, x] != null)
        {
            board.RemovePieceAtPosition(x, y);
            if (piece.Color == board.playerColor)
            {
                board.playerPiecesCount--;
            }
            else
            {
                board.enemyPiecesCount--;
            }
            Destroy(piece.PieceObject);
        }
        return board;
    }
}