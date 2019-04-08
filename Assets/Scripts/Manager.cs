using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public bool currentPlayer;
    public Material redMaterial;
    public Material blueMaterial;
    public GameObject xObject;
    public GameObject oObject;
    public GameObject gridPointObject;
    public GameObject highlight;
    public GameObject currentGrid;
    public List<GameObject> currentMoves;

    void PopulateMoves()
    {
        currentMoves = new List<GameObject>();
        foreach(Transform child in currentGrid.transform)
        {
            if (PrefabUtility.GetPrefabType(child.gameObject) == PrefabUtility.GetPrefabType(gridPointObject))
            {
                currentMoves.Add(child.gameObject);
            }
        }
    }

    void GetMove(GameObject gridPoint)
    {
        if (currentMoves.Contains(gridPoint))
        {
            Vector3 placement = gridPoint.transform.position;
            GameObject move;
            if (currentPlayer == false)
                move = Instantiate(xObject);
            else
                move = Instantiate(oObject);
            move.transform.position = placement;

            currentGrid = GameObject.Find("Main Grid/Grid Spots/" + gridPoint.name);
            Destroy(gridPoint);
            PopulateMoves();
        }
    }

    void UpdateHighlight(Vector3 position, bool largeMove = false)
    {
        // Assign Highlight Material
        if (currentPlayer == false)
        {
            var components = highlight.GetComponentsInChildren<MeshRenderer>();
            foreach (var component in components)
            {
                component.material = redMaterial;
            }
        }
        else if (currentPlayer == true)
        {
            var components = highlight.GetComponentsInChildren<MeshRenderer>();
            foreach (var component in components)
            {
                component.material = blueMaterial;
            }
        }

        // Move and Scale Highlight Object
        if (largeMove)
        {
            highlight.transform.position = new Vector3(0, -2, 0); // uh sure Unity why not
            highlight.transform.localScale = new Vector3(3, 3, 1);
        }
        else
        {
            highlight.transform.position = position;
            highlight.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    void Start()
    {
        int move = Random.Range(0, 2);
        if (move == 0)
            currentPlayer = false;
        else
            currentPlayer = true;

        UpdateHighlight(highlight.transform.position, true);
    }

    void Update()
    {

    }
}
