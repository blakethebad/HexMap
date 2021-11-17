using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapEditor : MonoBehaviour
{
    enum OptionalToggle { Ignore, Yes, No }
    OptionalToggle riverMode, roadMode;

    public Color[] colors;
    public HexMap map;
    private Color activeColor;
    private int activeElevation;


    bool applyElevation;
    bool applycolor;
    int brushSize;

    bool isDrag;
    HexDirection dragDirection;
    HexCell previousCell;

    private void Awake()
    {
        SelectColor(3);
    }

    void Update()
    {
        if (
            Input.GetMouseButton(0) &&
            !EventSystem.current.IsPointerOverGameObject()
        )
        {
            HandleInput();
        }
        else
        {
            previousCell = null;
        }
    }

    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            HexCell currentCell = map.GetSelectedHex(hit.point);
            if (previousCell && previousCell != currentCell)
            {
                ValidateDrag(currentCell);
            }
            else
            {
                isDrag = false;
            }
            EditHexes(currentCell);
            previousCell = currentCell;
        }
        else
        {
            previousCell = null;
        }
    }

    void ValidateDrag(HexCell currentCell)
    {
        for (
            dragDirection = HexDirection.NorthEast;
            dragDirection <= HexDirection.NorthWest;
            dragDirection++
        )
        {
            if (previousCell.GetNeighbor(dragDirection) == currentCell)
            {
                isDrag = true;
                return;
            }
        }
        isDrag = false;
    }

    public void EditHexes(HexCell centerHex)
    {
        int centerX = centerHex.thisHex.q;
        
        int centerZ = centerHex.thisHex.r;

        for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
        {
            for (int x = centerX - r; x <= centerX + brushSize; x++)
            {
                EditHex(map.GetCell(new Hex(x, z, -x-z)));
            }
        }
        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--)
        {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
                EditHex(map.GetCell(new Hex(x, z, -x - z)));
            }
        }
    }

    public void EditHex(HexCell cell)
    {
        if (cell)
        {
            if (applycolor)
            {
                cell.Color = activeColor;
            }

            if (applyElevation)
            {
                cell.Elevation = activeElevation;
            }
            if (riverMode == OptionalToggle.No)
            {
                cell.RemoveRiver();
            }
            if (isDrag)
            {
                HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                if (otherCell)
                {
                    if (riverMode == OptionalToggle.Yes)
                    {
                        otherCell.SetOutgoingRiver(dragDirection);
                    }
                    if (roadMode == OptionalToggle.Yes)
                    {
                        otherCell.AddRoad(dragDirection);
                    }
                }
            }
        }
    }

    public void SelectColor(int index)
    {
        applycolor = index >= 0;
        
        if (applycolor)
        {
            activeColor = colors[index];
        }
        
    }

    public void SelectElevation(float elevation)
    {
        activeElevation = (int)elevation;

    }

    public void SetElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    public void SetBrushSize(float size)
    {
        brushSize = (int)size;

    }

    public void SetRiverMOde(int mode)
    {
        riverMode = (OptionalToggle)mode;
    }

    public void SetRoadMode(int mode)
    {
        roadMode = (OptionalToggle)mode;
    }
}
