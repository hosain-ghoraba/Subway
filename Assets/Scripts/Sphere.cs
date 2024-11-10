

using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using System;
using System.Linq;
using Random = UnityEngine.Random;
using Unity.VisualScripting;
using TMPro;
using UnityEditor;



public class Sphere : MonoBehaviour
{

    [SerializeField] TMP_Text upperText;

    [SerializeField] GameObject pauseScreen;
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] TMP_Text scoreText;

    int centerLineX;
    int centerLineZ;
    int groundSurface;
    int linesCount;
    int rowsCount;
    int tileWidth;
    int tileLength;
    float fallingStartTime;
    int maxSpeed;
    int normalSpeed;
    float curSpeed;
    int maxFuel;
    float curFuel;

    int normalFuelDecreaseRate;
    int curfuelDecreaseRate;
    float score;
    bool gameEnded;
    bool isInvincible;
    int fallingMaxDuration;
    int obstacleHeight;
    int curLine;
    LinkedList<GameObject[]> grid;
    [SerializeField] GameObject NormalTilePrefab;
    [SerializeField] GameObject EmptyTilePrefab;
    [SerializeField] GameObject burningTilePrefab;
    [SerializeField] GameObject suppliesTilePrefab;
    [SerializeField] GameObject boostTilePrefab;
    [SerializeField] GameObject stickyTilePrefab;
    [SerializeField] GameObject obstaclePrefab;


    // audio sources

    [SerializeField] AudioSource boostAudioSource;
    [SerializeField] AudioSource stickyAudioSource;
    [SerializeField] AudioSource suppliesAudioSource;
    [SerializeField] AudioSource burningAudioSource;
    [SerializeField] AudioSource obstacleAudioSource;
    [SerializeField] AudioSource fallAudioSource;
    [SerializeField] AudioSource errorAudioSource;
    [SerializeField] AudioSource magicAudioSource;


    void Start()
    {
        AudioManager.Instance.PlayGameSound(true);

        // the Time.timeScale is very very impportant to enable game restart
        Time.timeScale = 1;
        centerLineX = 0;
        curLine = centerLineX;
        centerLineZ = 0;
        groundSurface = 0;
        linesCount = 3;
        rowsCount = 100;
        fallingStartTime = 0;
        normalSpeed = 15;
        curSpeed = normalSpeed;
        maxSpeed = 2 * normalSpeed;
        maxFuel = 50;
        curFuel = maxFuel;
        normalFuelDecreaseRate = 1;
        curfuelDecreaseRate = normalFuelDecreaseRate;
        score = 0;
        gameEnded = false;
        fallingMaxDuration = 2;
        isInvincible = false;

        gameOverScreen.SetActive(false);

        MeshRenderer meshRenderer = NormalTilePrefab.GetComponent<MeshRenderer>();
        tileWidth = (int)meshRenderer.bounds.size.x;
        tileLength = (int)meshRenderer.bounds.size.z;
        obstacleHeight = (int)obstaclePrefab.GetComponent<MeshRenderer>().bounds.size.y;

        GenerateGrid();
        transform.position = new Vector3(centerLineX, groundSurface + 10, centerLineZ);
    }

    void Update()
    {

        if (gameEnded)
        {
            return;
        }
        if (fallingStartTime != 0)
        {
            if (Time.time - fallingStartTime > fallingMaxDuration)
            {
                EndGame();
            }
            return;

        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 0)
                Resume();
            else
            {
                Pause();
            }
            return;
        }
        if (fallingStartTime == 0)
        {

            MoveGridBack();
            HandleHorizontalMovement();
            HandleJump();
            HandleCheats();
            upperText.text = "Fuel: " + Math.Round(curFuel, 1) + "\n";
            upperText.text += "Speed: " + GetSpeedAnnotation() + "\n";
            upperText.text += "Score: " + Math.Round(score, 1) + "\n";
            upperText.text += "Invincible: " + (isInvincible ? "Yes\n" : "No\n");
        }
        // if the backrow of the grid is outsize the camera view, cut it, then put a shuffled copy of it 
        // at the end of the grid
        DecreaseFuel();
        IncreaseScore();

        if (!Back3TilesAreVisible())
        {
            ExtendGrid();
        }

    }

    private void HandleCheats()
    {

        if (Input.GetKeyDown(KeyCode.U))
        {
            isInvincible = !isInvincible;
            magicAudioSource.PlayOneShot(magicAudioSource.clip);

        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            curSpeed /= 2;
            magicAudioSource.PlayOneShot(magicAudioSource.clip);

        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            curSpeed *= 2;
            magicAudioSource.PlayOneShot(magicAudioSource.clip);

        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            curFuel = maxFuel;
            magicAudioSource.PlayOneShot(magicAudioSource.clip);

        }
    }

    private void EndGame()
    {
        Time.timeScale = 0;
        gameEnded = true;
        gameOverScreen.SetActive(true);
        scoreText.text = "Score : " + Math.Round(score, 1);
        StopAllEffects(obstacleAudioSource);
        AudioManager.Instance.PlayPauseScreenSound();
    }
    private void MoveGridBack()
    {
        // this moves the sphere, but we dont want that, we want to move the whole grid
        // transform.position += curSpeed * Time.deltaTime * Vector3.forward;

        // here is the grid movement
        foreach (GameObject[] row in grid)
        {
            foreach (GameObject tile in row)
            {
                tile.transform.position += curSpeed * Time.deltaTime * Vector3.back;
            }
        }

    }
    private void OnCollisionEnter(Collision other)
    {
        switch (other.gameObject.tag)
        {
            case "obstacle":
                {
                    PlayFeedback(obstacleAudioSource);
                    if (!isInvincible)
                    {
                        EndGame();
                    }
                    else
                    {
                        transform.position = new Vector3(transform.position.x, groundSurface + obstacleHeight, transform.position.z);
                    }
                    break;
                }
            case "empty-tile":
                AudioManager.Instance.GetAudioSource().Stop();
                PlayFeedback(fallAudioSource);
                fallingStartTime = Time.time;
                Destroy(other.gameObject);
                Rigidbody rb = GetComponent<Rigidbody>();
                Vector3 pullDirection = (other.transform.position - transform.position).normalized;
                float pullingMagnitude = 10 * curSpeed;
                rb.AddForce(pullDirection * pullingMagnitude, ForceMode.Acceleration);
                break;
            case "supplies-tile":
                PlayFeedback(suppliesAudioSource);
                curFuel = maxFuel;
                break;
            case "boost-tile":
                PlayFeedback(boostAudioSource);
                curSpeed = maxSpeed;
                break;
            case "sticky-tile":
                PlayFeedback(stickyAudioSource);
                curSpeed = normalSpeed;
                break;
            case "burning-tile":
                PlayFeedback(burningAudioSource);
                curfuelDecreaseRate = 10 * normalFuelDecreaseRate;
                break;
        }
    }
    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("burning-tile"))
        {
            curfuelDecreaseRate = normalFuelDecreaseRate;
        }
    }

    private void HandleHorizontalMovement()
    {
        int direction = (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) ? 1 : (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) ? -1 : 0;
        int newX = (curLine + direction) * tileWidth;
        if (newX >= GetLeftLineX() && newX <= GetRightLineX())
        {
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            curLine += direction;
        }
        else
        {
            Debug.Log("new x is " + newX + " left is " + GetLeftLineX() + " right is " + GetRightLineX());
            PlayFeedback(errorAudioSource);
        }
        // // hadle horizontal movement
        // int direction2 = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) ? 1 : Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) ? -1 : 0;
        // float newZ = transform.position.z + direction2 * tileLength;
        // transform.position = new Vector3(transform.position.x, transform.position.y, newZ);
    }
    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsTouchingGround())
            {
                float force = 15;
                GetComponent<Rigidbody>().AddForce(Vector3.up * force, ForceMode.Impulse);
            }
            else
            {
                PlayFeedback(errorAudioSource);
            }
        }
    }
    private void GenerateGrid()
    {
        grid = new LinkedList<GameObject[]>();
        for (int i = 0; i < rowsCount; i++)
        {
            string mode = i < 5 ? "NormalOnly" : i % 3 != 0 ? "GoodOnly" : "All";
            grid.AddLast(GenerateRow(i, mode));
        }
    }
    private GameObject[] GenerateRow(int rowNum, string mode)
    {
        HashSet<GameObject> candidatePrefabs = GetSuitablePrefabs(mode);
        GameObject[] row = new GameObject[linesCount];
        for (int i = 0; i < linesCount; i++)
        {
            row[i] = GenerateObject(i, rowNum, candidatePrefabs);
        }
        return row;
    }
    private GameObject GenerateObject(int lineNum, int rowNum, HashSet<GameObject> candidatePrefabs)
    {
        GameObject randomPrefab = candidatePrefabs.ElementAt(Random.Range(0, candidatePrefabs.Count));
        // down code is for positioning the tile in the right place & rotation
        int newX = GetLeftLineX() + lineNum * tileWidth;
        int newY = groundSurface;
        if (randomPrefab == obstaclePrefab)
        {
            newY += obstacleHeight / 2;
        }
        int newZ = centerLineZ + rowNum * tileLength;
        Vector3 position = new Vector3(newX, newY, newZ);
        Quaternion rotation = Quaternion.Euler(90, 0, 0);
        GameObject tile = Instantiate(randomPrefab, position, rotation);
        return tile;

    }
    private int GetLeftLineX()
    {
        if (linesCount % 2 != 1)
        {
            throw new Exception("linesCount must be odd");
        }
        int tilesToMove = linesCount / 2;
        return centerLineX - tilesToMove * tileWidth;
    }
    private int GetRightLineX()
    {
        if (linesCount % 2 != 1)
        {
            throw new Exception("linesCount must be odd");
        }
        int tilesToMove = linesCount / 2;
        return centerLineX + tilesToMove * tileWidth;
    }
    private HashSet<GameObject> GetSuitablePrefabs(string mode)
    {
        HashSet<GameObject> candidatePrefabs = new HashSet<GameObject>();
        if (mode.Equals("NormalOnly"))
        {
            candidatePrefabs.Add(NormalTilePrefab);

        }
        else if (mode.Equals("GoodOnly"))
        {
            // add 2 normal tiles to make the probability of getting a normal tile higher
            candidatePrefabs.Add(NormalTilePrefab);
            candidatePrefabs.Add(NormalTilePrefab);

            candidatePrefabs.Add(suppliesTilePrefab);
            candidatePrefabs.Add(boostTilePrefab);
            // candidatePrefabs.Add(stickyTilePrefab);

        }
        else if (mode.Equals("BadOnly"))
        {
            candidatePrefabs.Add(obstaclePrefab);
            candidatePrefabs.Add(EmptyTilePrefab);
            candidatePrefabs.Add(burningTilePrefab);
        }
        else if (mode.Equals("All"))
        {
            candidatePrefabs.Add(NormalTilePrefab);
            candidatePrefabs.Add(suppliesTilePrefab);
            candidatePrefabs.Add(boostTilePrefab);
            candidatePrefabs.Add(stickyTilePrefab);

            candidatePrefabs.Add(obstaclePrefab);
            candidatePrefabs.Add(EmptyTilePrefab);
            candidatePrefabs.Add(burningTilePrefab);

        }
        return candidatePrefabs;
    }
    private void DecreaseFuel()
    {
        curFuel -= curfuelDecreaseRate * Time.deltaTime;
        if (curFuel <= 0)
        {
            EndGame();
        }
    }
    private void IncreaseScore()
    {
        score += Time.deltaTime;
    }
    private void PlayFeedback(AudioSource audioSource)
    {
        StopAllEffects();
        audioSource.PlayOneShot(audioSource.clip);
    }
    private string GetSpeedAnnotation()
    {
        float factor = curSpeed / normalSpeed;
        if (factor <= 0.1)
            return "Snail";
        if (factor <= 0.25)
            return "Turtle";
        if (factor <= 0.5)
            return "Slow";
        if (factor <= 0.75)
            return "Normal-";
        if (factor <= 1)
            return "Normal";
        if (factor <= 1.5)
            return "Normal+";
        if (factor <= 2)
            return "High";
        return "High++";
    }
    private void ExtendGrid()
    {
        GameObject[] firstRow = grid.First.Value;
        grid.RemoveFirst();
        GameObject[] lastRow = grid.Last.Value;
        Shuffle(firstRow);

        // Calculate the new Z position based on the last row's Z position
        // int newZ = rowsCount * tileLength;
        int newZ = (int)lastRow[0].transform.position.z + tileLength;

        for (int i = 0; i < linesCount; i++)
        {
            // Ensure the Y position is consistent
            int newY = groundSurface;
            // this if is crucial
            if (firstRow[i].CompareTag("obstacle"))
            {
                newY += obstacleHeight / 2;
            }

            firstRow[i].transform.position = new Vector3(firstRow[i].transform.position.x, newY, newZ);
        }
        grid.AddLast(firstRow);
    }
    private bool Back3TilesAreVisible()
    {
        GameObject[] row1 = grid.First.Value;
        grid.RemoveFirst();
        GameObject[] row2 = grid.First.Value;
        grid.RemoveFirst();
        GameObject[] row3 = grid.First.Value;
        grid.RemoveFirst();

        bool answer = true;
        foreach (GameObject tile in row3)
            if (!tile.GetComponent<Renderer>().isVisible)
            {
                answer = false;
                break;
            }
        // put rows back
        grid.AddFirst(row3);
        grid.AddFirst(row2);
        grid.AddFirst(row1);

        return answer;

    }
    private void Shuffle(GameObject[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int randomIndex = Random.Range(i, array.Length);
            GameObject temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }
    private bool IsTouchingGround()
    {
        SphereCollider sphereCollider = gameObject.GetComponent<SphereCollider>();
        float radius = sphereCollider.radius * transform.localScale.y; // Account for object scaling
        float bottomY = transform.position.y - radius;
        float tolerance = 0.3f;
        return Mathf.Abs(bottomY - groundSurface) <= tolerance;
    }
    private void StopAllEffects(AudioSource except = null)
    {
        List<AudioSource> audioSources = new List<AudioSource> { boostAudioSource, stickyAudioSource, suppliesAudioSource, burningAudioSource, obstacleAudioSource, fallAudioSource, errorAudioSource };
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource != except)
            {
                audioSource.Stop();
            }
        }
    }
    public void Resume()
    {
        Time.timeScale = 1;
        pauseScreen.SetActive(false);
        AudioManager.Instance.PlayGameSound();
    }

    public void Pause()
    {
        StopAllEffects();
        Time.timeScale = 0;
        pauseScreen.SetActive(true);
        AudioManager.Instance.PlayPauseScreenSound();
    }
}
