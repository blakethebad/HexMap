using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    Camera mainCamera;
    public HexMap map;
    public MapEditor mapEditor;

    private void Start()
    {
        mainCamera = Camera.main;
    }


    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            HexCell clickedHex = map.GetSelectedHex(hit.point);
            mapEditor.EditHexes(clickedHex);
        }
    }
}
