using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
 
public struct ShortestPathJob : IJob
{
    public Vector3Int startCoordinates;
    public Vector3Int destinationCoordinates;
    NativeHashMap<Vector3Int, JobNode> openSet;
    NativeHashMap<Vector3Int, JobNode> closedSet;
    NativeMultiHashMap<Vector3Int, Vector3Int> neighbourDict;
    public NativeList<Vector3Int> finalPath;

    public ShortestPathJob(Vector3Int startCoordinates, int setCapacity, Vector3Int destinationCoordinates,  NativeMultiHashMap<Vector3Int, Vector3Int> neighbourDict, NativeList<Vector3Int> finalPath)
    {
        openSet = new NativeHashMap<Vector3Int, JobNode>(setCapacity,Allocator.TempJob);
        closedSet = new NativeHashMap<Vector3Int, JobNode>(setCapacity, Allocator.TempJob);
        this.finalPath = finalPath;
        this.neighbourDict = neighbourDict;
        this.startCoordinates = startCoordinates;
        this.destinationCoordinates = destinationCoordinates;
    }

    public NativeList<Vector3Int> ReversePath(NativeList<Vector3Int> originalList)
    { 
        NativeList<Vector3Int> reversedPath = new NativeList<Vector3Int>();
        for (int i = originalList.Length - 1; i >= 0; i--)
        {
            reversedPath.Add(originalList[i]);
        }
        return reversedPath;
    }

    public void Execute()
    {
        JobNode startNode = new JobNode(startCoordinates);
        JobNode targetNode = new JobNode(destinationCoordinates);
        openSet.TryAdd(startCoordinates, startNode);
        while (openSet.Length > 0)
        {
            JobNode node = openSet.GetValueArray(Allocator.Temp)[0];

            foreach (var possibleNode in openSet.GetValueArray(Allocator.Temp))
            {
                if (possibleNode.GetFCost() < node.GetFCost() || possibleNode.GetFCost() == node.GetFCost())
                {
                    if (possibleNode.hCost < node.hCost)
                        node = possibleNode;
                }
            }

            openSet.Remove(node.coordinates);
            closedSet.TryAdd(node.coordinates, node);

            if (node.coordinates == destinationCoordinates)
            {
                JobNode currentPathNode = new JobNode();
                closedSet.TryGetValue(destinationCoordinates, out currentPathNode);
                while (currentPathNode.coordinates != startNode.coordinates)
                {
                    finalPath.Add(currentPathNode.coordinates);
                    closedSet.TryGetValue(currentPathNode.parent, out currentPathNode);
                }
                finalPath = ReversePath(finalPath);
                break;
            }

            NativeMultiHashMap<Vector3Int, Vector3Int>.Enumerator possibleNeighbourCoordinates = neighbourDict.GetValuesForKey(node.coordinates);

            NativeList<JobNode> neighbourNodes = new NativeList<JobNode>(Allocator.TempJob);
            foreach (Vector3Int neighbour in possibleNeighbourCoordinates)
            {
                JobNode openSetNeighbour = new JobNode();
                openSet.TryGetValue(neighbour, out openSetNeighbour);

                JobNode closedSetNeighbour = new JobNode();
                closedSet.TryGetValue(neighbour, out closedSetNeighbour);

                JobNode neighbourNode = (openSet.ContainsKey(neighbour)) ? openSetNeighbour : ((closedSet.ContainsKey(neighbour)) ? closedSetNeighbour : new JobNode(neighbour));
                neighbourNodes.Add(neighbourNode);
            }
            possibleNeighbourCoordinates.Dispose();

            for (int i = 0; i < neighbourNodes.Length; i++)
            {
                JobNode neighbour = neighbourNodes[i];
                if (closedSet.ContainsKey(neighbour.coordinates))
                {
                    continue;
                }

                int newCostToNeighbour = node.gCost + JobNode.GetDistance(node, neighbour);
                if (newCostToNeighbour < neighbour.gCost || !openSet.ContainsKey(neighbour.coordinates))
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = JobNode.GetDistance(neighbour, targetNode);
                    neighbour.parent = node.coordinates;

                    if (!openSet.ContainsKey(neighbour.coordinates))
                        openSet.TryAdd(neighbour.coordinates, neighbour);
                }
            }

            neighbourNodes.Dispose();
        }

        openSet.Dispose();
        closedSet.Dispose();
    }

    public NativeList<JobNode> GetNeighbourNodes(JobNode node)
    {
        NativeMultiHashMap<Vector3Int,Vector3Int>.Enumerator possibleNeighbourCoordinates = neighbourDict.GetValuesForKey(node.coordinates);

        NativeList<JobNode> neighbourNodes = new NativeList<JobNode>(Allocator.TempJob);
        foreach (Vector3Int neighbour in possibleNeighbourCoordinates)
        {
            JobNode openSetNeighbour = new JobNode();
            openSet.TryGetValue(neighbour, out openSetNeighbour);

            JobNode closedSetNeighbour = new JobNode();
            closedSet.TryGetValue(neighbour, out closedSetNeighbour);

            JobNode neighbourNode = (openSet.ContainsKey(neighbour)) ? openSetNeighbour : ((closedSet.ContainsKey(neighbour)) ? closedSetNeighbour : new JobNode(neighbour));
            neighbourNodes.Add(neighbourNode);
        }
        possibleNeighbourCoordinates.Dispose();

        return neighbourNodes;
    }
}
