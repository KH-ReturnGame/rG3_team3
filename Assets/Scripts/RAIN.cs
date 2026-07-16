using UnityEngine;
using System.Collections;

public class PoopSpawner : MonoBehaviour
{
    [SerializeField] private GameObject poopPrefab; // 여기에 Rigidbody2D와 Poop이 붙은 프리팹을 넣으세요.
    [SerializeField] private float spawnInterval = 0.5f;
    [SerializeField] private float spawnRangeX = 8f;
    [SerializeField] private float fallingSpeed = 5f;

    [Header("각도 설정 (0도는 수직 아래)")]
    [SerializeField] private float minAngle = -30f;
    [SerializeField] private float maxAngle = 30f;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnPoop();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnPoop()
    {
        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        Vector3 spawnPosition = new Vector3(transform.position.x + randomX, transform.position.y, 0f);

        float randomAngle = Random.Range(minAngle, maxAngle);
        Quaternion spawnRotation = Quaternion.Euler(0f, 0f, randomAngle);

        // 생성과 동시에 물리 속도 부여
        GameObject newPoop = Instantiate(poopPrefab, spawnPosition, spawnRotation);
        Rigidbody2D rb = newPoop.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 launchDirection = spawnRotation * Vector2.down;
            rb.linearVelocity = launchDirection * fallingSpeed;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 leftPos = transform.position + Vector3.left * spawnRangeX;
        Vector3 rightPos = transform.position + Vector3.right * spawnRangeX;
        Gizmos.DrawLine(leftPos, rightPos);
    }
}