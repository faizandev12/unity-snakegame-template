using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(BoxCollider2D))]
public class Snake : MonoBehaviour
{
    private bool hasCrownedHead = false;

    public Transform segmentPrefab;
    public Vector2Int direction = Vector2Int.right;
    public float speed = 20f;
    public float speedMultiplier = 1f;
    public int initialSize = 4;
    public bool moveThroughWalls = false;

    private readonly List<Transform> segments = new List<Transform>();
    private Vector2Int input;
    private float nextUpdate;
    private Transform currentBodyPrefab;

    public AudioClip eatSound;
    public AudioClip obstacleSound;
    //public AudioClip moveSound;
    private AudioSource audioSource;


    [Header("Snake Head Sprites")]
    public Sprite pinkHeadSprite;
    public Sprite blueHeadSprite;

    [Header("Crowned Snake Head Sprites")]
    public Sprite crownedPinkHeadSprite;
    public Sprite crownedBlueHeadSprite;

    [Header("Snake Body Prefabs")]
    public Transform pinkBodyPrefab;
    public Transform blueBodyPrefab;

    private void Start()
    {
        int selected = PlayerPrefs.GetInt("SelectedSnake", 0);

        if (selected == 0)
        {
            // Pink snake
            GetComponent<SpriteRenderer>().sprite = pinkHeadSprite;
            currentBodyPrefab = pinkBodyPrefab;
        }
        else
        {
            // Blue snake
            GetComponent<SpriteRenderer>().sprite = blueHeadSprite;
            currentBodyPrefab = blueBodyPrefab;
        }
        audioSource = GetComponent<AudioSource>();

        ResetState();
    }

    private void Update()
    {

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteKey("HighScore");
            PlayerPrefs.Save();
        }
        // Only crown the snake if the current score exceeds the high score (and isn't the first time playing)
        int previousHighScore = PlayerPrefs.GetInt("HighScore", 0);
        if (!hasCrownedHead && ScoreManager.Instance.CurrentScore >= previousHighScore)
        {
            hasCrownedHead = true;
            UpdateToCrownedHead();
        }

        // Only allow turning up or down while moving in the x-axis
        if (direction.x != 0f)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
                input = Vector2Int.up;
                //audioSource.PlayOneShot(moveSound);
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
                input = Vector2Int.down;
                //audioSource.PlayOneShot(moveSound);
            }
        }
        // Only allow turning left or right while moving in the y-axis
        else if (direction.y != 0f)
        {
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
                input = Vector2Int.right;
                //audioSource.PlayOneShot(moveSound);
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
                input = Vector2Int.left;
                //audioSource.PlayOneShot(moveSound);
            }
        }
    }

    private void FixedUpdate()
    {
        // Wait until the next update before proceeding
        if (Time.time < nextUpdate) {
            return;
        }

        // Set the new direction based on the input
        if (input != Vector2Int.zero) {
            direction = input;
        }

        // Set each segment's position to be the same as the one it follows. We
        // must do this in reverse order so the position is set to the previous
        // position, otherwise they will all be stacked on top of each other.
        for (int i = segments.Count - 1; i > 0; i--) {
            segments[i].position = segments[i - 1].position;
        }

        // Move the snake in the direction it is facing
        // Round the values to ensure it aligns to the grid
        int x = Mathf.RoundToInt(transform.position.x) + direction.x;
        int y = Mathf.RoundToInt(transform.position.y) + direction.y;
        transform.position = new Vector2(x, y);

        // Set the next update time based on the speed
        nextUpdate = Time.time + (1f / (speed * speedMultiplier));
    }

    public void Grow()
    {
        Transform segment = Instantiate(currentBodyPrefab);
        segment.position = segments[segments.Count - 1].position;
        segments.Add(segment);

    }

    public void ResetState()
    {
        direction = Vector2Int.right;
        transform.position = Vector3.zero;
        ScoreManager.Instance.ResetScore();
        hasCrownedHead = false;

        // Start at 1 to skip destroying the head
        for (int i = 1; i < segments.Count; i++) {
            Destroy(segments[i].gameObject);
        }

        // Clear the list but add back this as the head
        segments.Clear();
        segments.Add(transform);

        // -1 since the head is already in the list
        for (int i = 0; i < initialSize - 1; i++) {
            Grow();
        }
        // Reset to default head sprite
        int selected = PlayerPrefs.GetInt("SelectedSnake", 0);
        if (selected == 0 && pinkHeadSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = pinkHeadSprite;
        }
        else if (selected == 1 && blueHeadSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = blueHeadSprite;
        }
    }

    public bool Occupies(int x, int y)
    {
        foreach (Transform segment in segments)
        {
            if (Mathf.RoundToInt(segment.position.x) == x &&
                Mathf.RoundToInt(segment.position.y) == y) {
                return true;
            }
        }

        return false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Food"))
        {
            if (eatSound != null && audioSource != null)
            {
                StartCoroutine(PlaySoundInstantly(eatSound));
            }
            Grow();
            ScoreManager.Instance.AddScore(1); // Add score
        }
        else if (other.gameObject.CompareTag("Obstacle"))
        {
            StartCoroutine(PlaySoundInstantly(obstacleSound));
            ResetState();
        }
        else if (other.gameObject.CompareTag("Wall"))
        {
            if (moveThroughWalls) {
                Traverse(other.transform);
            } else {
                ResetState();
            }
        }
    }

    private void Traverse(Transform wall)
    {
        Vector3 position = transform.position;

        if (direction.x != 0f) {
            position.x = Mathf.RoundToInt(-wall.position.x + direction.x);
        } else if (direction.y != 0f) {
            position.y = Mathf.RoundToInt(-wall.position.y + direction.y);
        }

        transform.position = position;
    }
    public void MainMenuGame()
    {
        SceneManager.LoadScene("MainMenu");
    }
    private System.Collections.IEnumerator PlaySoundInstantly(AudioClip clip)
    {
        audioSource.Stop(); // Stop any currently playing sound
        audioSource.PlayOneShot(clip); // Play the new sound
        yield return null; // Wait one frame to let the sound trigger
    }
    private void UpdateToCrownedHead()
    {
        int selected = PlayerPrefs.GetInt("SelectedSnake", 0);
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (selected == 0 && crownedPinkHeadSprite != null)
        {
            sr.sprite = crownedPinkHeadSprite;
        }
        else if (selected == 1 && crownedBlueHeadSprite != null)
        {
            sr.sprite = crownedBlueHeadSprite;
        }
    }

    /*
    private void GameOver()
    {
        int currentScore = ScoreManager.Instance.CurrentScore;
        int storedHighScore = PlayerPrefs.GetInt("HighScore", 0);

        if (storedHighScore > 0 && currentScore > storedHighScore)
        {
            ShowKingCrown(); // Your logic to show crown/label
        }
        else
        {
            HideKingCrown();
        }

        // Update high score if needed
        if (currentScore > storedHighScore)
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
            PlayerPrefs.Save();
        }

        // Now reset game or show UI
        ResetState();
    }

    */

}
