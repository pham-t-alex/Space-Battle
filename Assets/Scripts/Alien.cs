using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class Alien : NetworkBehaviour
{
    private NetworkVariable<int> health = new NetworkVariable<int>();
    [SerializeField] private int maxHealth = 5;

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float reloadTimeLeft;
    [SerializeField] private float reloadTime;

    [SerializeField] private float moveSpeed;
    private bool frontLine;
    private int mapIndex;
    private Vector2 targetPosition;
    private bool finishedPath = false;
    private int world = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }
        health.Value = maxHealth;
        reloadTimeLeft = reloadTime;
    }

    public void Initialize(bool front, int world)
    {

        this.world = world;
        frontLine = front;
        mapIndex = 1;
        finishedPath = false;
        GetNextMapPosition();
    }

    void GetNextMapPosition()
    {
        if (finishedPath) return;
        List<Vector2> path;
        if (frontLine) path = GameController.Instance.Map.frontLinePath;
        else path = GameController.Instance.Map.backLinePath;

        if (mapIndex < path.Count)
        {
            targetPosition = path[mapIndex] + GameController.Instance.GetWorldCenter(world);
            return;
        }

        finishedPath = true;
        List<MapEndRegion> ends;
        if (frontLine) ends = GameController.Instance.Map.frontLineEnds;
        else ends = GameController.Instance.Map.backLineEnds;
        MapEndRegion region = ends[Random.Range(0, ends.Count)];
        Vector2 regionExtents = region.bounds / 2f;
        // the region's relative center + some random x and y + world center
        targetPosition = region.center + new Vector2(Random.Range(-regionExtents.x, regionExtents.x), Random.Range(-regionExtents.y, regionExtents.y)) + GameController.Instance.GetWorldCenter(world);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer)
        {
            return;
        }
        UpdateShoot();
        UpdateMove();
    }

    public void UpdateShoot()
    {
        if (reloadTimeLeft == 0) return;
        reloadTimeLeft -= Time.deltaTime;
        if (reloadTimeLeft > 0) return;
        Shoot();
        reloadTimeLeft = reloadTime;
    }

    public void UpdateMove()
    {
        if ((Vector2)transform.position == targetPosition && finishedPath) return;
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        if ((Vector2)transform.position == targetPosition)
        {
            mapIndex++;
            GetNextMapPosition();
        }
    }

    public void Damage(int damage)
    {
        if (IsServer)
        {
            health.Value -= damage;
            if (health.Value < 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Shoot()
    {
        if (!IsServer)
        {
            return;
        }
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            projectile.GetComponent<NetworkObject>().Spawn();
        }
    }
}
