using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class DragDropArrowScript : MonoBehaviour
{
    Vector2 pos1;
    Vector2 pos2;
    Vector2 pos3;
    Camera mainCam;
    public float pos2ChangeInY = 5;
    public int amountOfChildren = 8;
    List<RectTransform> children = new List<RectTransform>();
    RectTransform canvas;
    Vector2 screenRef = new Vector2(0,0);
    float timer = 0;
    float tAmount = 0;
    PalPlacementSystem pSystem;
    void Start()
    {
        pSystem = GameObject.Find("PlacementSystem").GetComponent<PalPlacementSystem>();
        canvas = GameObject.Find("Canvas").GetComponent<RectTransform>();
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        foreach (RectTransform child in transform)
        {
            children.Add(child);
        }
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        pos1 = rectTransform.position;
        pos2 = new Vector2(pos1.x, pos1.y + pos2ChangeInY);
        tAmount = 1 / children.Count;
    }

    // Update is called once per frame
    void Update()
    {
        if (!PalPlacementSystem.isBuying)
        {
            Destroy(gameObject);
        }
        //Debug.Log("Pos1: " + pos1);
        //Debug.Log("Pos2: " + pos2);
        //Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, Input.mousePosition, mainCam, out screenRef);
        Vector3 mouseAngle = Vector3.zero;
        if (PalPlacementSystem.canPlace)
        {
            Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPosition = pSystem.grid.WorldToCell(mousePos);
            Vector3 worldPosition = pSystem.grid.CellToWorld(gridPosition);
            Vector3 worldPosChanged = new Vector3(worldPosition.x + 0.75f, worldPosition.y + 0.75f, 0);
            Vector3 finalPosition = mainCam.WorldToScreenPoint(worldPosChanged);
            pos3 = finalPosition; //was Input.mousePosition
            mouseAngle = new Vector3(pos3.x, pos3.y, 0) - children[children.Count - 1].position;
        }
        else
        {
            mouseAngle = Input.mousePosition;
            pos3 = Input.mousePosition;
        }
        float angle = Mathf.Atan2(mouseAngle.y, mouseAngle.x) * Mathf.Rad2Deg;

        timer += Time.deltaTime;
        if (timer > 1)
        {
            timer = 0;
        }
        for (int i = 0; i < children.Count; i++)
        {
            //float trueTAmount = 1 / amountOfChildren;
            RectTransform rTransform = children[i].GetComponent<RectTransform>();
            rTransform.position = bezierCurve(0.0625f * i);
            //children[i].position = pos3;
        }
        children[children.Count - 1].rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    public void destroyArrow()
    {
        Destroy(gameObject);
    }

    Vector2 bezierCurve(float t)
    {
        return (1 - t) * ((1 - t) * pos1 + t * pos2) + t * ((1 - t) * pos2 + t * pos3);
    }
}
