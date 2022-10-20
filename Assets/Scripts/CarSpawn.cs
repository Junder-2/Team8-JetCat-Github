using System.Collections;
using System.Collections.Generic;
using Car;
using Level;
using UnityEngine;
using Random = UnityEngine.Random;

public class CarSpawn : MonoBehaviour
{
    [Range(-1, 1)] [SerializeField] private int driveDirection;
    [SerializeField] private Vector2 spawnTimeRange = new Vector2(1, 3);
    [SerializeField] private float roadWidth;
    [SerializeField] private float speedMultiplier = 1;

    [SerializeField] private float noiseStrength = .05f;
    [Range(2, 100)][SerializeField] private int pathPoints = 3;

    [SerializeField] private bool useOverride;

    private LevelManager _levelManager;

    private WayPoint[] _generatedPoints;

    private void Awake()
    {
        _levelManager = transform.parent.GetComponent<LevelManager>();
    }

    private void OnEnable()
    {
        GenerateWayPoints();
        
        StopAllCoroutines();
        StartCoroutine(SpawnLoop(GetNextSpawnTime()));
    }

    private void GenerateWayPoints()
    {
        float directionSign = driveDirection == 0 ? MathHelper.RandomSign() : driveDirection;

        var position = transform.position;
        Vector3 startPos = position + Vector3.right * (roadWidth * directionSign);
        Vector3 endPos = position - Vector3.right * (roadWidth * directionSign);

        _generatedPoints = useOverride
            ? CarHelpers.GenerateFromPositions(localPoints.ToArray())
            : CarHelpers.GenerateStraightPath(startPos, endPos, noiseStrength, pathPoints);
    }
    
    IEnumerator SpawnLoop(float spawnWait)
    {
        yield return new WaitForSeconds(spawnWait);
        SpawnCar(_levelManager.GetRules().GetRandomCar());
        StartCoroutine(SpawnLoop(GetNextSpawnTime()));
    }

    void SpawnCar(GameObject carPrefab)
    {
        float directionSign = driveDirection == 0 ? MathHelper.RandomSign() : driveDirection;
        
        CarBehaviour car = Instantiate(carPrefab, _generatedPoints[0].Position, Quaternion.Euler(0, 90*directionSign, 0)).GetComponent<CarBehaviour>();
        
        car.SetWayPointData(_generatedPoints);
        
        car.MultiplySpeed(speedMultiplier != 0 ? speedMultiplier : 1);
    }

    public bool UseEditorOverride() => useOverride;
    public int GetDriveDirection() => driveDirection;
    float GetNextSpawnTime() => Random.Range(spawnTimeRange.x, spawnTimeRange.y);

    public List<Vector3> localPoints;

    public void SetDefaultPositions()
    {
        localPoints = new List<Vector3>(GetDefaultPositions());
    }

    public Vector3[] GetDefaultPositions()
    {
        var position = transform.position;
        Vector3[] points =
        {
            position + Vector3.right * roadWidth,
            position - Vector3.right * roadWidth
        };

        return points;
    }
}
