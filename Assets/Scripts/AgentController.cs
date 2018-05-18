using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public GameObject agentPrefab;
    public bool useHPAStar = true;
    public int speed;
    public float zValue = 1;

    private GameObject agent = null;
    bool isAgentCreated = false;

    private GridTile currentPos;
    private Vector3 destination;
    private LinkedListNode<Edge> currentEdge = null;

    #region Sibling Components

    private MapLoader mapLoader;

    #endregion


    public void SetNewPosition(GridTile pos)
    {
        if (!isAgentCreated)
        {
            currentPos = pos;
            agent = Instantiate(agentPrefab, transform, false);
            agent.transform.localPosition = new Vector3(pos.x + 0.5f, pos.y + 0.5f, zValue);
            agent.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            isAgentCreated = true;
        } else
        {
            LinkedList<Edge> path = mapLoader.GetPath(currentPos, pos, useHPAStar);
            currentEdge = path.First;
            GridTile curr = currentEdge.Value.end.pos;
            destination = new Vector3(curr.x + 0.5f, curr.y + 0.5f, zValue);
        }
    }

    private void Awake()
    {
        mapLoader = GetComponent<MapLoader>();
    }

    private void Update()
    {
        if (currentEdge != null)
        {
            MoveAgent();
        }
    }

    private void MoveAgent()
    {
        Vector3 direction;
        float distance;

        float distToComplete = Time.deltaTime * speed;
        while (distToComplete > 0)
        {
            direction = destination - agent.transform.localPosition;
            distance = direction.magnitude;
            direction.Normalize();

            if (distance > distToComplete)
                distance = distToComplete;

            // Move
            agent.transform.localPosition += distance * direction;

            // If arrived at destination
            if (agent.transform.localPosition == destination)
            {
                currentPos = currentEdge.Value.end.pos;
                currentEdge = currentEdge.Next;
                if (currentEdge == null)
                    // We've arrived
                    return;
                else
                {
                    GridTile pos = currentEdge.Value.end.pos;
                    destination = new Vector3(pos.x + 0.5f, pos.y + 0.5f, zValue);
                }
            }

            // Update distance to complete
            distToComplete -= distance;
        }
    }
}

