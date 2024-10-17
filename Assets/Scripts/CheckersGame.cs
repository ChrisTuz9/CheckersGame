using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckersGame : MonoBehaviour
{
    public GameObject blackTilePreFabs, whiteTilePreFabs, blackPiecePreFabs, whitePiecePreFabs, blackKingPreFabs, whiteKingPreFabs;

    public GameObject gameOverPanel;
    public TMPro.TextMeshProUGUI gameOverText;
    public Button newGameButton;

    public GameObject moveLogContent;
    public GameObject moveLogEntryPrefab;


    private Board board;
    private MoveLogger moveLogger;
    private List<Move> moves = new List<Move>();
    private Piece selectedPiece = null;
    private bool isWhiteTurn;
    private bool captureChain;

    void Start()
    {
        newGameButton.onClick.AddListener(StartNewGame);
        StartNewGame();
    }

    public void StartNewGame()
    {
        Time.timeScale = 1;
        isWhiteTurn = true;
        captureChain = false;
        selectedPiece = null;

        gameOverPanel.SetActive(false);

        if (board != null)
        {
            moves.Clear();
            moveLogger.ClearMoveLog();
            board.DestroyBoard();
            Destroy(board);
        }

        PieceColor selectedColor = (PieceColor)PlayerPrefs.GetInt("SelectedColor", (int)PieceColor.WHITE);

        board = gameObject.AddComponent<Board>();
        board.SetTilesPreFabs(whiteTilePreFabs, blackTilePreFabs);
        board.SetPiecesPreFabs(whitePiecePreFabs, blackPiecePreFabs, whiteKingPreFabs, blackKingPreFabs);
        board.CreateBoard(selectedColor);

        moveLogger = gameObject.AddComponent<MoveLogger>();
        moveLogger.SetMoveLogger(moveLogContent, moveLogEntryPrefab);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                GameObject clickedObject = hit.collider.gameObject;

                if (clickedObject.tag == "Piece")
                {
                    Piece piece = board.GetPieceByGameObject(clickedObject);
                    if (piece != null && IsCorrectTurn(piece))
                    {
                        if (!captureChain)
                        {
                            selectedPiece = piece;
                        }
                    }
                }
                else if (clickedObject.tag == "Tile" && selectedPiece != null)
                {
                    MovePiece(clickedObject);
                }
            }
        }
    }

    private bool IsCorrectTurn(Piece piece)
    {
        return (isWhiteTurn && piece.Color == PieceColor.WHITE) || (!isWhiteTurn && piece.Color == PieceColor.BLACK);
    }

    private void MovePiece(GameObject tile)
    {
        Vector2 oldPosition = selectedPiece.GetPosition();
        Vector2 newPosition = new Vector2(Mathf.RoundToInt(tile.transform.position.x), Mathf.RoundToInt(tile.transform.position.y));

        if (!board.IsDiagonalMove(oldPosition, newPosition))
        {
            return;
        }

        bool isPieceOnPath = !board.IsPathClear(oldPosition, newPosition);
        bool isCaptureMove = Mathf.Abs(oldPosition.x - newPosition.x) >= 2 && Mathf.Abs(oldPosition.y - newPosition.y) >= 2 && isPieceOnPath;

        if (isCaptureMove)
        {
            int directionX = (int)Mathf.Sign(newPosition.x - oldPosition.x);
            int directionY = (int)Mathf.Sign(newPosition.y - oldPosition.y);

            bool validCapture = false;
            Piece pieceToCapture = null;

            if (Mathf.Abs(newPosition.x - oldPosition.x) > 2 && !selectedPiece.IsKing)
                return;

            for (int i = 1; i < Mathf.Abs(newPosition.x - oldPosition.x); i++)
            {
                Piece potentialPiece = board.GetPieceAtPosition((int)oldPosition.x + directionX * i, (int)oldPosition.y + directionY * i);

                if (potentialPiece != null && potentialPiece.Color != selectedPiece.Color)
                {
                    if (pieceToCapture == null)
                    {
                        pieceToCapture = potentialPiece;
                        validCapture = true;
                    }
                    else
                    {
                        validCapture = false;
                        break;
                    }
                }
            }

            if (validCapture)
            {
                board = board - pieceToCapture;
                board.UpdatePiecePosition((int)oldPosition.x, (int)oldPosition.y, (int)newPosition.x, (int)newPosition.y);

                if (board.CanPieceCapture((int)newPosition.x, (int)newPosition.y))
                {
                    captureChain = true;
                    selectedPiece = board.GetPieceAtPosition((int)newPosition.x, (int)newPosition.y);
                    SaveMoveToHistory(selectedPiece, oldPosition, newPosition);
                }
                else
                {
                    EndTurn(selectedPiece, oldPosition, newPosition);
                }
            }
        }
        else if (!board.IsCapturePossibleForColor(isWhiteTurn ? PieceColor.WHITE : PieceColor.BLACK))
        {
            if (!selectedPiece.IsKing)
            {
                int directionY = selectedPiece.Color == board.playerColor ? 1 : -1;
                if (Mathf.Abs(oldPosition.x - newPosition.x) == 1 && (newPosition.y - oldPosition.y) == directionY)
                {
                    board.UpdatePiecePosition((int)oldPosition.x, (int)oldPosition.y, (int)newPosition.x, (int)newPosition.y);
                    EndTurn(selectedPiece, oldPosition, newPosition);
                }
            }
            else
            {
                if (!isPieceOnPath)
                {
                    board.UpdatePiecePosition((int)oldPosition.x, (int)oldPosition.y, (int)newPosition.x, (int)newPosition.y);
                    EndTurn(selectedPiece, oldPosition, newPosition);
                }
            }
        }
    }

    private void SaveMoveToHistory(Piece piece, Vector2 oldPosition, Vector2 newPosition)
    {
        Move move = new Move();
        move.SaveMove(piece, oldPosition, newPosition);
        moves.Add(move);
        AddMoveToDisplay(moves.Count, moves[^1]);
    }
    private void AddMoveToDisplay(int nr, Move move)
    {
        move.Deconstruct(out Piece piece, out Vector2 oldPos, out Vector2 newPos);
        string moveText = $"{nr}. {piece.Color}: {ConvertPositionToString(oldPos)} -> {ConvertPositionToString(newPos)}\n";
        moveLogger.AddMoveToLog(moveText);
    }

    private string ConvertPositionToString(Vector2 position)
    {
        char column = (char)('A' + position.x);
        int row = (int)position.y + 1;
        return $"{column}{row}";
    }

    private void EndTurn(Piece piece, Vector2 oldPosition, Vector2 newPosition)
    {
        captureChain = false;
        selectedPiece = null;
        isWhiteTurn = !isWhiteTurn;

        SaveMoveToHistory(piece, oldPosition, newPosition);

        CheckGameOver();
    }

    private void CheckGameOver()
    {
        if (board.IsGameOver(isWhiteTurn ? PieceColor.WHITE : PieceColor.BLACK))
        {
            if ((board.playerPiecesCount == 0) || (isWhiteTurn && board.playerColor == PieceColor.WHITE) || (!isWhiteTurn && board.playerColor == PieceColor.BLACK))
            {
                gameOverText.text = "YOU LOSE!";
            }
            else if ((board.enemyPiecesCount == 0) || (isWhiteTurn && board.playerColor == PieceColor.BLACK) || (!isWhiteTurn && board.playerColor == PieceColor.WHITE))
            {
                gameOverText.text = "YOU WIN!";
            }

            gameOverPanel.SetActive(true);
            Time.timeScale = 0;
        }
    }
}