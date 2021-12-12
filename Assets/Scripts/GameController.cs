using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Instance variables
    public GameObject prefabPlayer;
    public GameObject prefabBodyPiece;
    public GameObject prefabFood;
    
    public static List<GameObject> bodyPieceObjects = new List<GameObject>();
    public static List<BodyController> bodyPieces = new List<BodyController>();
    public static bool moveLock = false;
    public GameObject player;
    public GameObject food;
    public GameObject gameHUD;
    public GameObject deathScreen;
    public GameObject foodParticle;
    public GameObject playerParticle;

    public bool deathScreenActive = false;
    public static float marginOfError = 0.0005f;
    public static float speed = 0.03f;
    public static int score = 0;

    public static bool addBodyPiece = false;
    public static bool updateSpeed = false;

    public static bool respawnFood = false;

    public static bool snapToGrid = false;
    /// <summary>
    /// Unity start function - ran on first frame
    /// </summary>
    void Start()
    {
        //create the player at 0,0,0
        moveLock = true;
        player = Object.Instantiate(prefabPlayer, new Vector3(0f, 0f, 0f), Quaternion.identity);

        //create the food at a random location
        food = Object.Instantiate(prefabFood, SpawnLocation(), Quaternion.identity);
    }

    /// <summary>
    /// Unity update function - ran every frame
    /// </summary>
    void Update()
    {
        if(!deathScreenActive){
            if(player == null){
            player = Object.Instantiate(prefabPlayer, new Vector3(0f, 0f, 0f), Quaternion.identity);
            food = Object.Instantiate(prefabFood, new Vector3(4f, 0f, 3f), Quaternion.identity);

            }else{
            //when the worm has no body add 2 body pieces 1 frame apart
                if(bodyPieces.Count == 0)
                    AddBodyPiece();
                else if (bodyPieces.Count == 1){
                    AddBodyPiece();
                    moveLock = false;
                }
            }
        }
    }

    void FixedUpdate() {
        if(!deathScreenActive){
            MoveWorm();
            if(addBodyPiece == true){
                AddBodyPiece();
                addBodyPiece = false;          
            }
            if(snapToGrid == true){
                AlignAll();
                snapToGrid = false;
            }
            if(respawnFood == true){
                RespawnFood();
                respawnFood = false;
            }
            if(updateSpeed == true){
                speed += 0.0005f;
                updateSpeed = false;
            }
        }
    }

    private void MoveWorm()
    {
        FindObjectOfType<PlayerController>().MovePlayer();
        for(int i = 0; i < bodyPieces.Count; i++){
            bodyPieces[i].MoveBody();
        }
    }

    public void AlignAll(){
        moveLock = true;
        Vector3 temp = new Vector3(0, 0, 0);
        temp.x = Mathf.Round(player.transform.position.x);
        temp.z = Mathf.Round(player.transform.position.z);
        player.transform.position = temp;

        for(int i = 0; i < bodyPieceObjects.Count; i++){
            Vector3 temp2 = new Vector3(0, 0, 0);
            temp2.x = Mathf.Round(bodyPieceObjects[i].transform.position.x);
            temp2.z = Mathf.Round(bodyPieceObjects[i].transform.position.z);
            bodyPieceObjects[i].transform.position = temp2;            
        }
        moveLock = false;
    }

    /// <summary>
    /// Adds a body piece to the worm
    /// </summary>
    public void AddBodyPiece()
    {
        //checks if the worm has any body pieces
        if (bodyPieceObjects.Count == 0)
        {
            //moveLock = true;
            Vector3 position = player.transform.position;
            Vector3 velocity = PlayerController.velocity;
            AddBodyPieceHelper(position, velocity);
            //moveLock = false;
        }
        else
        {
            //moveLock = true;
            Vector3 position = bodyPieces[bodyPieces.Count - 1].transform.position;
            Vector3 velocity = bodyPieces[bodyPieces.Count - 1].velocity;
            AddBodyPieceHelper(position, velocity);
            //moveLock = false;
        }
    }

    /// <summary>
    /// helper function to create a new bodypiece and add it to the lists
    /// </summary>
    ///<param name="position">The position of the current tail</param>
    ///<param name="velocity">The velocity of the current tail</param>
    private void AddBodyPieceHelper(Vector3 position, Vector3 velocity)
    {
        
        //set the spawn location 1 unit behind the current tail
        if (velocity.x > 0)
            position.x = position.x - 1f;
        else if (velocity.x < 0)
            position.x = position.x + 1f;
        else if (velocity.z > 0)
            position.z = position.z - 1f;
        else if (velocity.z < 0)
            position.z = position.z + 1f;
        
        //create the object, store it in the bodypiece lists, then give it a number
        bodyPieceObjects.Add(Object.Instantiate(prefabBodyPiece, position, Quaternion.identity));
        int bodyPieceNum = bodyPieceObjects.Count - 1;
        bodyPieces.Add(bodyPieceObjects[bodyPieceNum].GetComponent<BodyController>());
        bodyPieces[bodyPieceNum].bodyPieceNum = bodyPieceNum;
        bodyPieces[bodyPieceNum].spawnVelocity = velocity;
    }

    /// <summary>
    /// function to move the food to a random new location on the board
    /// </summary>
    public void RespawnFood()
    {
        Vector3 spawnLocation = SpawnLocation();
        food.transform.SetPositionAndRotation(spawnLocation, Quaternion.identity);
        Instantiate(foodParticle, spawnLocation, Quaternion.identity);
    }

    /// <summary>
    /// helper function to determine where the food should respawn
    /// </summary>
    /// <returns>Vector3 with random X and Y within bounds</returns> 
    private Vector3 SpawnLocation()
    {
        Vector3 location = new Vector3(Random.Range(-12, 12), 0, Random.Range(-6, 6));
        Vector3 headPosition = player.transform.position;

        //check if the head of the worm is where the randomly generated location is
        if((location.x == Mathf.Floor(headPosition.x) || location.x == Mathf.Ceil(headPosition.x)) &&
        (location.z == Mathf.Floor(headPosition.z) || location.z == Mathf.Ceil(headPosition.z))){
            Debug.Log("retrying after attempting to place inside head at position X: " 
             + location.x + " Z: " + location.z);
            
            //retry if worm is at location
            return SpawnLocation();
        }

        //check if he body of the worm is where the randomly generated location is
        for(int i = 0; i < bodyPieces.Count; i++){
            Vector3 temp = bodyPieces[i].transform.position;
            if((location.x == Mathf.Floor(temp.x) || location.x == Mathf.Ceil(temp.x)) &&
            (location.z == Mathf.Floor(temp.z) || location.z == Mathf.Ceil(temp.z))){
                Debug.Log("retrying after attempting to place inside bodypiece " +
                 i + "at position X: " + location.x + " Z: " + location.z);
                
                //retry if worm is at location
                return SpawnLocation();
            }
        }
        return location;
    }

    /// <summary>
    /// Function to kill the player and end the game
    /// </summary>
    public void Die()
    {
        // Instantiate particle on player
        Instantiate(playerParticle, player.transform.position, Quaternion.identity);

        // Check for a new high score
        if (score > PlayerPrefs.GetInt("HighScore"))
            PlayerPrefs.SetInt("HighScore", score);
        
        PlayerPrefs.SetInt("PrevScore", score);
        
        // Hide the game HUD and show the death screen
        deathScreenActive = true;
        gameHUD.SetActive(false);
        deathScreen.SetActive(deathScreenActive);
        
        // Destroy / reset game objects
        foreach(GameObject b in bodyPieceObjects) 
            Destroy(b);
        bodyPieceObjects.Clear();
        bodyPieces.Clear();
        PlayerController.lastRotatePositionQueue.Clear();
        PlayerController.lastVelocityQueue.Clear();
        PlayerController.velocity = new Vector3(speed, 0, 0);
        Destroy(player);
        Destroy(food);
        player = null;
        food = null;
        score = 0;
        moveLock = false;
        addBodyPiece = false;
        updateSpeed = false;
        respawnFood = false;
        snapToGrid = false;
        speed = 0.03f;   
    }
}
