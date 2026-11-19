/**************************************************************
* This script is attached to each obstacle in the scene
* Its purpose is to give each obstacle a random size and speed
*
* Alex Ch
* Fri Nov 13 v1.0
**************************************************************/ 


using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    [Header("Initial Value Options")]
    [Tooltip("The minimum size of an obstacle")]
    [Range(0.4f, 0.6f)]
    [SerializeField] private float minSize = 0.5f;
    [Tooltip("The maximum size of an obstacle")]
    [Range(1.5f, 2.5f)]
    [SerializeField] private float maxSize = 2.0f;
    [Tooltip("The minimum speed of an obstacle")]
    [Range(40f, 60f)]
    [SerializeField] private float minSpeed = 50f;
    [Tooltip("The maximum speed of an obstacle")]
    [Range(100f, 200f)]
    [SerializeField] private float maxSpeed = 150f;

    private Rigidbody2D ObstacleRB;


    void Start()
    {
        ObstacleRB = GetComponent<Rigidbody2D>();
        RandomizeFields();

    }
        private void RandomizeFields()
    {
        // Set randomized size
        float randomSize = Random.Range(minSize, maxSize);
        transform.localScale = new Vector3(randomSize, randomSize, 1);
        
        // Set randomized movement vector
        float randomSpeed = Random.Range(minSpeed, maxSpeed);
        Vector2 randomDirection = Random.insideUnitCircle;

        // Add Force in selected direction with selected speed
        ObstacleRB.AddForce(randomDirection * randomSpeed);

    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
