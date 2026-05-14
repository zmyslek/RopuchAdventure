using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GryficaRandomSpawner : MonoBehaviour
{
    [SerializeField]
    GameObject gryficaPrefab;

    [SerializeField]
    Vector2 spawnAreaMin = new Vector2(-8.0f, -2.0f);

    [SerializeField]
    Vector2 spawnAreaMax = new Vector2(8.0f, 2.0f);

    [SerializeField]
    float minSpawnDelay = 2.0f;

    [SerializeField]
    float maxSpawnDelay = 5.0f;

    [SerializeField]
    float minHorizontalSeparation = 1.0f;

    [SerializeField]
    float minDistanceFromDziunia = 3.0f;

    GameObject currentGryfica;
    float lastSpawnX;
    bool hasSpawnedBefore;

    void Start()
    {
        GameObject resourcePrefab = Resources.Load<GameObject>("Prefabs/GryficaMain");
        if (resourcePrefab != null)
        {
            gryficaPrefab = resourcePrefab;
        }

        if (gryficaPrefab == null)
        {
            Debug.LogWarning("GryficaRandomSpawner: No prefab assigned and no Resources prefab at 'Prefabs/GryficaMain' found.");
            enabled = false;
            return;
        }

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (enabled)
        {
            if (currentGryfica == null)
            {
                SpawnOne();

                float waitTime = Random.Range(Mathf.Max(0.1f, minSpawnDelay), Mathf.Max(minSpawnDelay, maxSpawnDelay));
                yield return new WaitForSeconds(waitTime);
                continue;
            }

            yield return null;
        }
    }

    void SpawnOne()
    {
        // Load prefab fresh each time to avoid caching issues
        GameObject prefab = Resources.Load<GameObject>("Prefabs/GryficaMain");
        if (prefab == null)
        {
            Debug.LogError("GryficaRandomSpawner: Failed to load 'Prefabs/GryficaMain' from Resources!");
            return;
        }

        Camera cam = Camera.main;
        float desiredX = float.NaN;
        if (cam != null)
        {
            float camHalfWidth = cam.orthographicSize * cam.aspect;
            float camLeft = cam.transform.position.x - camHalfWidth;
            float camRight = cam.transform.position.x + camHalfWidth;
            float offset = 0.5f;
            desiredX = Random.value < 0.5f ? camLeft - offset : camRight + offset;
        }

        Collider2D floorCollider = float.IsNaN(desiredX) ? PickRandomFloorCollider() : FindFloorColliderNearX(desiredX);
        if (floorCollider == null)
        {
            Debug.LogWarning("GryficaRandomSpawner: No object with tag 'Floor' found. Using raw random area spawn.");

            Vector3 fallbackPos = new Vector3(
                Random.Range(Mathf.Min(spawnAreaMin.x, spawnAreaMax.x), Mathf.Max(spawnAreaMin.x, spawnAreaMax.x)),
                Random.Range(Mathf.Min(spawnAreaMin.y, spawnAreaMax.y), Mathf.Max(spawnAreaMin.y, spawnAreaMax.y)),
                0.0f
            );

            currentGryfica = Instantiate(prefab, fallbackPos, prefab.transform.rotation);
            lastSpawnX = fallbackPos.x;
            hasSpawnedBefore = true;
            return;
        }

        float spawnX = PickSpawnXOnFloor(floorCollider.bounds.min.x, floorCollider.bounds.max.x);
        float floorTopY = floorCollider.bounds.max.y;

        Vector3 tempPos = new Vector3(spawnX, floorTopY + 10.0f, 0.0f);
        // Instantiate with prefab's original rotation to preserve child positioning
        GameObject newGryfica = Instantiate(prefab, tempPos, prefab.transform.rotation);

        // Use BoxCollider2D specifically for floor alignment
        BoxCollider2D boxCollider = newGryfica.GetComponentInChildren<BoxCollider2D>();
        if (boxCollider != null)
        {
            float currentBottomY = boxCollider.bounds.min.y;
            float delta = floorTopY - currentBottomY;
            newGryfica.transform.position += new Vector3(0.0f, delta, 0.0f);
        }

        // Ensure spawned Gryfica faces left (walking direction towards player/camera)
        SpriteRenderer s = newGryfica.GetComponentInChildren<SpriteRenderer>();
        if (s != null)
        {
            s.flipX = true; // face left
        }

        currentGryfica = newGryfica;
        lastSpawnX = currentGryfica.transform.position.x;
        hasSpawnedBefore = true;
    }

    Collider2D PickRandomFloorCollider()
    {
        GameObject[] floorObjects = GameObject.FindGameObjectsWithTag("Floor");
        if (floorObjects == null || floorObjects.Length == 0)
        {
            return null;
        }

        List<Collider2D> floorColliders = new List<Collider2D>();
        for (int i = 0; i < floorObjects.Length; i++)
        {
            Collider2D floorCollider = floorObjects[i].GetComponent<Collider2D>();
            if (floorCollider != null)
            {
                if (!IsFloorNearDziunia(floorCollider))
                {
                    floorColliders.Add(floorCollider);
                }
            }
        }

        if (floorColliders.Count == 0)
        {
            return null;
        }

        int floorIndex = Random.Range(0, floorColliders.Count);
        return floorColliders[floorIndex];
    }

    Collider2D FindFloorColliderNearX(float desiredX)
    {
        GameObject[] floorObjects = GameObject.FindGameObjectsWithTag("Floor");
        if (floorObjects == null || floorObjects.Length == 0)
        {
            return null;
        }

        List<Collider2D> floorColliders = new List<Collider2D>();
        for (int i = 0; i < floorObjects.Length; i++)
        {
            Collider2D floorCollider = floorObjects[i].GetComponent<Collider2D>();
            if (floorCollider != null)
            {
                if (!IsFloorNearDziunia(floorCollider))
                {
                    floorColliders.Add(floorCollider);
                }
            }
        }

        if (floorColliders.Count == 0)
        {
            return null;
        }

        // Prefer a floor whose horizontal range already contains desiredX
        for (int i = 0; i < floorColliders.Count; i++)
        {
            Bounds b = floorColliders[i].bounds;
            if (desiredX >= b.min.x && desiredX <= b.max.x)
            {
                return floorColliders[i];
            }
        }

        // Otherwise pick the closest floor horizontally
        float bestDist = float.MaxValue;
        Collider2D best = null;
        for (int i = 0; i < floorColliders.Count; i++)
        {
            Bounds b = floorColliders[i].bounds;
            float dist = 0.0f;
            if (desiredX < b.min.x) dist = b.min.x - desiredX;
            else if (desiredX > b.max.x) dist = desiredX - b.max.x;
            else dist = 0.0f;

            if (dist < bestDist)
            {
                bestDist = dist;
                best = floorColliders[i];
            }
        }

        return best;
    }

    bool IsFloorNearDziunia(Collider2D floor)
    {
        if (floor == null) return false;

        DziuniaScript[] dziunias = GameObject.FindObjectsOfType<DziuniaScript>();
        if (dziunias == null || dziunias.Length == 0) return false;

        float floorMinX = floor.bounds.min.x;
        float floorMaxX = floor.bounds.max.x;

        foreach (var d in dziunias)
        {
            if (d == null || d.transform == null) continue;
            float dx = 0.0f;
            float dX = d.transform.position.x;
            if (dX < floorMinX) dx = floorMinX - dX;
            else if (dX > floorMaxX) dx = dX - floorMaxX;
            else dx = 0.0f;

            if (dx < minDistanceFromDziunia)
            {
                return true;
            }
        }

        return false;
    }

    float PickSpawnXOnFloor(float minX, float maxX)
    {
        float clampedMin = Mathf.Min(minX, maxX);
        float clampedMax = Mathf.Max(minX, maxX);

        if (!hasSpawnedBefore || Mathf.Abs(clampedMax - clampedMin) < minHorizontalSeparation)
        {
            return Random.Range(clampedMin, clampedMax);
        }

        for (int i = 0; i < 8; i++)
        {
            float candidate = Random.Range(clampedMin, clampedMax);
            if (Mathf.Abs(candidate - lastSpawnX) >= Mathf.Max(0.0f, minHorizontalSeparation))
            {
                return candidate;
            }
        }

        return Random.Range(clampedMin, clampedMax);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = new Vector3((spawnAreaMin.x + spawnAreaMax.x) * 0.5f, (spawnAreaMin.y + spawnAreaMax.y) * 0.5f, 0.0f);
        Vector3 size = new Vector3(Mathf.Abs(spawnAreaMax.x - spawnAreaMin.x), Mathf.Abs(spawnAreaMax.y - spawnAreaMin.y), 0.0f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, size);
    }
}
