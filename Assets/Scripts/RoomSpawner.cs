using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    [Header("Positioning")]
    public Vector2 Start = new Vector2(-125f, -125f);
    public Vector2 Size = new Vector2(250f, 250f);
    public float MaxHeight = 0f;
    public LayerMask ValidLayers;

    [Header("Appearence")]
    public Room[] RoomPrefabs;
    public GameObject[] TreePrefabs;
    [Range(0f, 1f)] public float SizeVariance = 0.1f;
    public int TreesPerRoom = 3;
    public float TreeDistanceOffsetMin = 0f;
    public float TreeDistanceOffsetMax = 1f;


    [Header("Placement Restriction")]
    public float NoiseScale;
    [Range(0f, 1f)] public float NoiseThreshold;

    [Header("Debug")]
    public int DebugSpawnCount = 50;

    [ContextMenu("Test Spawn")]
    public void TestSpawn()
    {
        Spawn(DebugSpawnCount);
    }

    [ContextMenu("Check Object Counts")]
    public void CheckObjectsCount()
    {
        var totalCount = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            totalCount += transform.GetChild(i).childCount;
        }

        Debug.Log($"{transform.childCount} top-level objects, {totalCount} objects in total");
    }

    private void Awake()
    {
        Spawn(DebugSpawnCount);
    }

    public void Spawn(int count)
    {
        var noiseOffset = Random.value * 1000f;

        for (int i = 0; i < count; i++)
        {
            var room = RoomPrefabs[Random.Range(0, RoomPrefabs.Length)];
            var scale = Random.Range(1f - SizeVariance, 1f + SizeVariance);

            var rayPos = new Vector3(Start.x + Size.x * Random.value, MaxHeight, Start.y + Size.y * Random.value);

            if (!Physics.SphereCast(new Ray(rayPos, Vector3.down), room.CanopyRadius * scale, out var hit, MaxHeight)) continue;

            if (!MaskContainsLayer(ValidLayers, hit.collider.gameObject.layer)) continue;

            if (hit.point.y > MaxHeight) continue;

            var noiseVal = Mathf.PerlinNoise(hit.point.x * NoiseScale + noiseOffset, hit.point.z * NoiseScale);
            if (noiseVal < NoiseThreshold) continue;

            var newRoom = PlaceRoomAt(room.Prefab, hit.point, scale);

            PlaceTreeAt(newRoom.transform, hit.point, room.radius * scale);
        }
    }

    public GameObject PlaceRoomAt(GameObject prefab, Vector3 position, float scale)
    {
        var newObj = Instantiate(prefab);
        newObj.transform.parent = transform;
        newObj.transform.position = position;
        newObj.transform.eulerAngles = new Vector3(0f, Random.value * 360f, 0f);
        newObj.transform.localScale = Vector3.one * scale;

        Physics.SyncTransforms();

        return newObj;
    }

    public void PlaceTreeAt(Transform parent, Vector3 position, float radius)
    {
        var count = Random.Range(0, TreesPerRoom + 1);
        if (count == 0)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            var newObj = Instantiate(TreePrefabs[Random.Range(0, TreePrefabs.Length)]);
            newObj.transform.parent = parent;

            var angle = Random.value * Mathf.PI * 2f;
            var r = radius + Random.Range(TreeDistanceOffsetMin, TreeDistanceOffsetMax);
            var rayPos = position + new Vector3(r * Mathf.Cos(angle), 0f, r * Mathf.Sin(angle));
            rayPos.y = MaxHeight;
            if (!Physics.Raycast(rayPos, Vector3.down, out var hit, MaxHeight, ValidLayers)) continue;
            newObj.transform.position = hit.point;

            newObj.transform.eulerAngles = new Vector3(0f, Random.value * 360, 0f);
            newObj.transform.localScale = Vector3.one * Random.Range(1f - SizeVariance, 1f + SizeVariance);
        }
    }

    [ContextMenu("Clear Objects")]
    public void ClearObjects()
    {
        for (var i = transform.childCount -1; i >= 0; i--)
        {
            if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
            else DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    public static bool MaskContainsLayer(LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }
}
