using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityHelpers;

public class FunkyMesh
{
    private Transform mainParent;
    private List<MeshPart> meshParts = new List<MeshPart>();

    public void AddVertices(IEnumerable<Vector3> vertices, IEnumerable<int> triangles)
    {
        IEnumerable<Vector3> remainingVertices = vertices;
        IEnumerable<int> remainingTriangles = triangles;

        while (remainingVertices.Count() > 0)
        {
            MeshPart lastMeshPart;
            //In case if some mesh parts already exist
            //squeeze in the beginning of our new vertices
            //to fill up any existing space
            if (meshParts.Count > 0)
                lastMeshPart = meshParts.Last();
            else
                lastMeshPart = AddMeshPart();

            int currentVertexCount = MeshHelpers.MAX_VERTICES - lastMeshPart.GetVertexCount();
            lastMeshPart.AddVertices(vertices.Take(currentVertexCount), triangles.FindTriangles(0, currentVertexCount - 1), null);

            remainingVertices = vertices.Skip(currentVertexCount);
            remainingTriangles = triangles.FindTriangles(0, currentVertexCount - 1, MeshHelpers.TriangleSearchType.none);
            remainingTriangles = remainingTriangles.ShiftTriangleIndices(-currentVertexCount);

            if (remainingVertices.Count() > 0)
                AddMeshPart();
        }
    }
    public void SetVertices(IEnumerable<Vector3> vertices, IEnumerable<int> triangles)
    {
        ExpandBy((ulong)vertices.Count());
        var lastMeshPart = meshParts.Last();
        lastMeshPart.SetVertices(vertices, triangles);
    }

    public ulong VertexCount()
    {
        ulong totalCount = 0;
        foreach (var meshPart in meshParts)
            totalCount += (ulong)meshPart.GetVertexCount();
        return totalCount;
    }

    private MeshPart AddMeshPart()
    {
        var meshPart = GenerateMeshPart();
        meshParts.Add(meshPart);
        return meshPart;
    }
    public void ExpandBy(ulong addedVertices)
    {
        ulong verticesLeft = addedVertices;
        while (NeedsNewPart(verticesLeft))
        {
            AddMeshPart();
            verticesLeft -= MeshHelpers.MAX_VERTICES;
        }
    }
    private bool NeedsNewPart(ulong addedVertices)
    {
        ulong vertexCount = VertexCount();
        ulong requiredMeshes = (vertexCount + addedVertices) / MeshHelpers.MAX_VERTICES;
        return (ulong)meshParts.Count < requiredMeshes;
    }
    private MeshPart GenerateMeshPart()
    {
        if (mainParent == null)
        {
            GameObject parentObject = new GameObject("Funky Object");
            mainParent = parentObject.transform;
        }

        GameObject funkyMesh = new GameObject("Mesh_Part_" + meshParts.Count);
        funkyMesh.transform.SetParent(mainParent, false);
        return funkyMesh.AddComponent<MeshPart>();
    }
}
