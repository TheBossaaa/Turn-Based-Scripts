using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveGrid : MonoBehaviour
{
    public static MoveGrid instance;

    [SerializeField] private Mesh moveGridMesh;
    [SerializeField] private Material moveGridMaterial;
    [SerializeField] private Vector3 moveGridSize;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private LayerMask whatIsObstacle;
    [SerializeField] private float obstacleCheckRange = 0.5f;

    private Plane plane = new Plane(Vector3.up, 0);
    private Matrix4x4[] matrix4X4s = new Matrix4x4[0];

    private void Awake()
    {
        instance = this;

        HideMovePoints();
    }

    private void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out float distance))
            {
                var worldPosition = ray.GetPoint(distance);
                Debug.Log(worldPosition);
                foreach(var matrix in matrix4X4s)
                {
                    if(Vector3.Distance(matrix.GetPosition(), worldPosition) < 0.5f)
                    {
                        GameManager.instance.ActivePlayer.MoveToPoint(matrix.GetPosition());

                        MoveGrid.instance.HideMovePoints();

                        PlayerInputMenu.instance.HideClassPanel();
                    }
                }
            }
        }
        if (matrix4X4s.Length > 0)
        {
            Graphics.DrawMeshInstanced(moveGridMesh, 0, moveGridMaterial, matrix4X4s);
        }
    }

    public void HideMovePoints()
    {
        matrix4X4s = new Matrix4x4[0];
    }

    public void ShowPointsInRange(float moveRange, Vector3 centerpoint)
    {
        HideMovePoints();

        int range = (int)moveRange;
        var movePoints = GetMovePointsInRange(moveRange, centerpoint);
        matrix4X4s = movePoints.Select(m =>
        {
            Matrix4x4 matrix = Matrix4x4.identity;
            matrix.SetTRS(m, Quaternion.identity, moveGridSize);
            return matrix;
        }).ToArray();
    }

    public List<Vector3> GetMovePointsInRange(float moveRange, Vector3 centerPoint)
    {
        List<Vector3> foundPoints = new List<Vector3>();

        int range = (int)moveRange;
        var center = new Vector2Int(Mathf.RoundToInt(centerPoint.x), Mathf.RoundToInt(centerPoint.z));
        HashSet<Vector2Int> closed = new HashSet<Vector2Int>();
        List<Vector2Int> open = new List<Vector2Int>();

        open.Add(center);

        while(open.Count > 0)
        {
            var position = open[0];
            open.RemoveAt(0);
            closed.Add(position);

            foreach(var neighbor in GetNeighbors(position))
            {
                var worldPos = new Vector3(neighbor.x, 0f, neighbor.y);
                var isGround = Physics.Raycast(worldPos + new Vector3(0, 10f, 0), Vector3.down, out var hit, 20f, whatIsGround);

                if (isGround && Vector3.Distance(centerPoint, worldPos) <= moveRange &&
                    !GameManager.instance.AllCharacters.Any(c => Vector3.Distance(c.transform.position, worldPos) < 0.5f) &&
                    Physics.OverlapSphere(hit.point, obstacleCheckRange, whatIsObstacle).Length == 0)
                {
                    foundPoints.Add(worldPos);

                    if(!open.Contains(neighbor) && !closed.Contains(neighbor))
                    {
                        open.Add(neighbor);
                    }
                }
            }
        }

        return foundPoints;
    }

    private Vector2Int[] GetNeighbors(Vector2Int pos)
    {
        Vector2Int[] neighbors = new Vector2Int[8];
        int index = 0;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if(x == 0 && y == 0) continue;

                neighbors[index++] = pos + new Vector2Int(x, y);
            }
        }
        return neighbors;
    }
}
