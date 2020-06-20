using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject ScriptManager;
    //number of pieces height/width
    public int width;
    public int height;
    //offset for where the board as a whole starts
    private float xOffset;
    private float yOffset;
    public float zOffset;

    private List<GameObject> _activePieces;
    private List<bool> alreadyFalling;
    private List<bool> checks;
    private List<GameObject> boardBackground;
    private bool currentCheck = false;
    private GameObject selectedObject = null;
    private GameObject selectionIndicator = null;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void Init()
    {
        
        ScriptManager = GameObject.Find("ScriptManager");
        _activePieces = new List<GameObject>();
        checks = new List<bool>();
        boardBackground = new List<GameObject>();
        GameObject g;
        var pieceManager = ScriptManager.GetComponent<PieceManager>();
        selectionIndicator = pieceManager.Indicator;
        xOffset = -pieceManager.SpriteWidth * width / 2;
        yOffset = -pieceManager.SpriteHeight * height / 2;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                g.transform.localScale = new Vector3(pieceManager.SpriteWidth, pieceManager.SpriteHeight, 1);
                g.transform.localPosition = new Vector3(pieceManager.SpriteWidth * j + xOffset, pieceManager.SpriteHeight * i + yOffset, zOffset);
                boardBackground.Add(g);
                checks.Add(false);
                _activePieces.Add(null);
            }
        }
    }

    bool boardUpdated = false;

    void DropPiece(int widthLocation, int heightLocation)
    {
        var pieceManager = ScriptManager.GetComponent<PieceManager>();
        int piecesCount = pieceManager.PiecesToCopy.Count;
        int selectedPieceIndex = Random.Range(0, piecesCount);
        _activePieces[heightLocation * width + widthLocation] = Instantiate(pieceManager.PiecesToCopy[selectedPieceIndex], new Vector3(widthLocation * pieceManager.SpriteWidth + xOffset, pieceManager.SpriteHeight * (height + heightLocation) + yOffset, zOffset - 1.1f), Quaternion.identity);
        _activePieces[heightLocation * width + widthLocation].AddComponent<PieceHandler>();
        _activePieces[heightLocation * width + widthLocation].SetActive(true);
        _activePieces[heightLocation * width + widthLocation].GetComponent<PieceHandler>().Initialize(new Vector3(widthLocation * pieceManager.SpriteWidth + xOffset, heightLocation * pieceManager.SpriteHeight + yOffset, zOffset - 1.1f), selectedPieceIndex);
    }

    int matchResult(int currentHeight, int currentWidth, int previousAmount, int previousPieceType, bool isHorizontal, bool checkingValid = false, bool backwards = false)
    {
        int modifyAmount = backwards ? -1 : 1;
        if(currentHeight >= height || currentWidth >= width || currentHeight < 0 || currentWidth < 0)
        {
            return previousAmount;
        }
        if (checks[currentHeight * width + currentWidth] != currentCheck)
        {
            return previousAmount;
        }
        if(_activePieces[currentHeight * width + currentWidth].GetComponent<PieceHandler>().isMoving)
        {
            return previousAmount;
        }
        if (previousPieceType != _activePieces[currentHeight * width + currentWidth].GetComponent<PieceHandler>().pieceType)
        {
            return previousAmount;
        }
        if (!checkingValid)
        {
            checks[currentHeight * width + currentWidth] = !checks[currentHeight * width + currentWidth];
        }
        return matchResult(isHorizontal ? currentHeight : currentHeight + modifyAmount, isHorizontal ? currentWidth + modifyAmount : currentWidth, previousAmount + 1, previousPieceType, isHorizontal, checkingValid, backwards);
    }
    

    void swapColors(int index1, int index2)
    {
        int tmp = _activePieces[index1].GetComponent<PieceHandler>().pieceType;
        _activePieces[index1].GetComponent<PieceHandler>().pieceType = _activePieces[index2].GetComponent<PieceHandler>().pieceType;
        _activePieces[index2].GetComponent<PieceHandler>().pieceType = tmp;
    }

    bool validSwap(int index1, int index2)
    {
        swapColors(index1, index2);
        int currentMatchResult = 1;
        int currentHeight = index1 / width;
        int currentWidth = index1 % width;
        currentMatchResult += matchResult(currentHeight, currentWidth + 1, 0, _activePieces[index1].GetComponent<PieceHandler>().pieceType, true, true, false);
        currentMatchResult += matchResult(currentHeight, currentWidth - 1, 0, _activePieces[index1].GetComponent<PieceHandler>().pieceType, true, true, true);
        if(currentMatchResult >= 5)
        {
            swapColors(index1, index2);
            return true;
        }

        currentMatchResult = 1;
        currentMatchResult += matchResult(currentHeight + 1, currentWidth, 0, _activePieces[index1].GetComponent<PieceHandler>().pieceType, false, true, false);
        currentMatchResult += matchResult(currentHeight - 1, currentWidth, 0, _activePieces[index1].GetComponent<PieceHandler>().pieceType, false, true, true);
        if(currentMatchResult >= 5)
        {
            swapColors(index1, index2);
            return true;
        }

        currentMatchResult = 1;
        currentHeight = index2 / width;
        currentWidth = index2 % width;
        currentMatchResult += matchResult(currentHeight, currentWidth + 1, 0, _activePieces[index2].GetComponent<PieceHandler>().pieceType, true, true, false);
        currentMatchResult += matchResult(currentHeight, currentWidth - 1, 0, _activePieces[index2].GetComponent<PieceHandler>().pieceType, true, true, true);
        if (currentMatchResult >= 5)
        {
            swapColors(index1, index2);
            return true;
        }

        currentMatchResult = 1;
        currentMatchResult += matchResult(currentHeight + 1, currentWidth, 0, _activePieces[index2].GetComponent<PieceHandler>().pieceType, false, true, false);
        currentMatchResult += matchResult(currentHeight - 1, currentWidth, 0, _activePieces[index2].GetComponent<PieceHandler>().pieceType, false, true, true);
        if (currentMatchResult >= 5)
        {
            swapColors(index1, index2);
            return true;
        }

        swapColors(index1, index2);
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray;
        RaycastHit rayHit;
        GameObject clickedObject;
        bool boardUpdatedNow = false;
        if (Input.GetMouseButtonDown(0))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out rayHit))
            {
                clickedObject = rayHit.collider.gameObject;
                if (clickedObject != null)
                {
                    if (boardBackground.FindIndex(o => o == clickedObject) != null)
                    {
                        clickedObject = _activePieces[boardBackground.FindIndex(o => o == clickedObject)];
                    }
                    else if (_activePieces.Find(o => o == clickedObject) == null)
                    {
                        return;
                    }
                    if (selectedObject == null)
                    {
                        selectedObject = clickedObject;

                        selectionIndicator.SetActive(true);
                        selectionIndicator.transform.localPosition = new Vector3(selectedObject.transform.position.x, selectedObject.transform.position.y, selectedObject.transform.position.z - 0.1f);
                    }
                    else
                    {
                        int objOneIndex = _activePieces.FindIndex(o => o == selectedObject);
                        int objTwoIndex = _activePieces.FindIndex(o => o == clickedObject);
                        if(objOneIndex == objTwoIndex + 1 || objOneIndex == objTwoIndex - 1 || objOneIndex == objTwoIndex + width || objOneIndex == objTwoIndex - width)
                        {
                            if(validSwap(objOneIndex, objTwoIndex))
                            {
                                Vector3 tmp = _activePieces[objOneIndex].GetComponent<PieceHandler>().designatedSpot;
                                _activePieces[objOneIndex].GetComponent<PieceHandler>().setDesignatedSpot(_activePieces[objTwoIndex].GetComponent<PieceHandler>().designatedSpot);
                                _activePieces[objTwoIndex].GetComponent<PieceHandler>().setDesignatedSpot(tmp);
                                GameObject tmpPiece = _activePieces[objOneIndex];
                                _activePieces[objOneIndex] = _activePieces[objTwoIndex];
                                _activePieces[objTwoIndex] = tmpPiece;
                                selectedObject = null;
                                selectionIndicator.SetActive(false);
                            } else
                            {
                                _activePieces[objOneIndex].GetComponent<PieceHandler>().setFakeMoveSpot(_activePieces[objTwoIndex].GetComponent<PieceHandler>().designatedSpot);
                                _activePieces[objTwoIndex].GetComponent<PieceHandler>().setFakeMoveSpot(_activePieces[objOneIndex].GetComponent<PieceHandler>().designatedSpot);
                                selectedObject = null;
                                selectionIndicator.SetActive(false);
                            }
                        } else
                        {
                            selectedObject = clickedObject;
                            selectionIndicator.transform.localPosition = new Vector3(selectedObject.transform.position.x, selectedObject.transform.position.y, selectedObject.transform.position.z - 0.1f);
                        }
                    }
                }
            }
        }

        for (int i = 0; i < height; i++)
        {
            for(int j = 0; j < width; j++)
            {
                if(_activePieces[i * width + j] == null)
                {
                    bool drop = true;
                    int curIndex = i * width + j;
                    while(curIndex < width * height)
                    {
                        if(_activePieces[curIndex] != null)
                        {
                            _activePieces[curIndex].GetComponent<PieceHandler>().setDesignatedSpot(_activePieces[curIndex].GetComponent<PieceHandler>().designatedSpot - new Vector3(0, ScriptManager.GetComponent<PieceManager>().SpriteHeight, 0));
                            _activePieces[curIndex - width] = _activePieces[curIndex];
                            _activePieces[curIndex] = null;
                        }
                        curIndex += width;
                    }
                    if (drop)
                    {
                        DropPiece(j, height - 1);
                    }
                    boardUpdated = true;
                    boardUpdatedNow = true;
                }
            }
        }
        if (boardUpdatedNow)
        {
            return;
        }
        int max = -1;
        int cur = 0;
        bool isHoriz = false;
        int startHeight = 0;
        int startWidth = 0;
        if (boardUpdated)
        {
            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    cur = matchResult(i, j, 0, _activePieces[i * width + j].GetComponent<PieceHandler>().pieceType, true);
                    if(cur > max)
                    {
                        max = cur;
                        startHeight = i;
                        startWidth = j;
                        isHoriz = true;
                    }
                }
            }
            currentCheck = !currentCheck;
            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    cur = matchResult(i, j, 0, _activePieces[i * width + j].GetComponent<PieceHandler>().pieceType, false);
                    if(cur > max)
                    {
                        max = cur;
                        startHeight = i;
                        startWidth = j;
                        isHoriz = false;

                    }
                }
            }
            currentCheck = !currentCheck;
            if(max >= 5)
            {
                for(int i = 0; i < max; i++)
                {
                    Destroy(_activePieces[(startHeight + (isHoriz ? 0 : i)) * width + (startWidth + (isHoriz ? i : 0))]);
                    _activePieces[(startHeight + (isHoriz ? 0 : i)) * width + (startWidth + (isHoriz ? i : 0))] = null;
                }
            }
        }
    }
}
