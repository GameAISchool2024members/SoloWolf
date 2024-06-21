using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using MBaske.Sensors.Grid;
using System;


public class Pacman : Agent
{
    public Movement movement { get; private set; }
    // public AnimatedSprite deathSequence;
    public SpriteRenderer spriteRenderer;
    private new Collider2D collider;
    public bool pacmanAttack = false;
    
    public GridBuffer Buffer;

    public GridBuffer m_SensorBuffer;

    public int WallChannel = 2;
    public int PelletChannel = 1;
    public int GhostChannel = 0;

    public float x_transform = 14f;
    public float y_transform = 16f;


    // public Dictionary<Vector2, bool> pelletsPositions = new Dictionary<Vector2, bool>();
    

    
    [SerializeField]
    [Tooltip("Width and height of the maze.")]
    private Vector2Int m_MazeSize = new Vector2Int(64, 64);

    [SerializeField]
    [Tooltip("The number of grid cells the agent can observe in any cardinal direction. " +
    "The resulting grid observation will always have odd dimensions, as the agent " +
    "is located at its center position, e.g. radius = 10 results in grid size 21 x 21.")]
    private int m_LookDistance = 10;
    private Vector2Int m_GridPosition;


    private void Awake(){
        this.movement = GetComponent<Movement>();
        this.spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
        Buffer = new GridBuffer(4, m_MazeSize);
        m_GridPosition = new Vector2Int(Mathf.RoundToInt(this.transform.position.x + x_transform), Mathf.RoundToInt(this.transform.position.y + y_transform));

        int length = m_LookDistance * 2 + 1;
        // The ColorGridBuffer supports PNG compression.
        m_SensorBuffer = new ColorGridBuffer(3, length, length);
        var sensorComp = GetComponentInChildren<MBaske.Sensors.Grid.GridSensorComponent>();
        sensorComp.GridBuffer = m_SensorBuffer;
        // Labels for sensor debugging.
        sensorComp.ChannelLabels = new List<ChannelLabel>()
        {
            new ChannelLabel("Wall", new Color32(0, 128, 255, 255)),
            new ChannelLabel("Pellet", new Color32(64, 255, 64, 255)),
            new ChannelLabel("Ghost", new Color32(255, 64, 64, 255))
        };
    }
    
    
    public override void OnEpisodeBegin()
    {
        // deathSequence.enabled = false;
        movement.ResetState();
        gameObject.SetActive(true);
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(movement.direction);
    }


    private void Update(){
        m_GridPosition = new Vector2Int(Mathf.RoundToInt(this.transform.position.x + x_transform), Mathf.RoundToInt(this.transform.position.y + y_transform));
        UpdateSensorBuffer();
        if (Input.GetKey(KeyCode.UpArrow))
        {
            this.movement.SetDirection(Vector2.up);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            this.movement.SetDirection(Vector2.down);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            this.movement.SetDirection(Vector2.left);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            this.movement.SetDirection(Vector2.right);
        }
        
        float angle = Mathf.Atan2(this.movement.direction.y, this.movement.direction.x); // Gets the angle of dir
        this.transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward); // Sets rot to the angle
        
    }

    private void UpdateSensorBuffer()
        {
            m_SensorBuffer.Clear();

            // Current FOV.
            int xMin = m_GridPosition.x - m_LookDistance;
            int xMax = m_GridPosition.x + m_LookDistance;
            int yMin = m_GridPosition.y - m_LookDistance;
            int yMax = m_GridPosition.y + m_LookDistance;

            for (int mx = xMin; mx <= xMax; mx++)
            {
                int sx = mx - xMin;
                for (int my = yMin; my <= yMax; my++)
                {
                    int sy = my - yMin;
                    // TryRead -> FOV might extend beyond maze bounds.
                    if (Buffer.TryRead(WallChannel, mx, my, out float wall))
                    {
                        // Copy maze -> sensor.
                        m_SensorBuffer.Write(WallChannel, sx, sy, wall);
                        m_SensorBuffer.Write(PelletChannel, sx, sy, Buffer.Read(PelletChannel, mx, my));
                        m_SensorBuffer.Write(GhostChannel, sx, sy, Buffer.Read(GhostChannel, mx, my));
                    }
                }
            }
        }

    // public void DeathSequence()
    // {
    //     enabled = false;
    //     spriteRenderer.enabled = false;
    //     collider.enabled = false;
    //     movement.enabled = false;
    //     deathSequence.enabled = true;
    //     deathSequence.Restart();
    // }
    public void ResetState()
    {
        enabled = true;
        spriteRenderer.enabled = true;
        collider.enabled = true;
        // deathSequence.enabled = false;
        movement.ResetState();
        gameObject.SetActive(true);
    }
}
