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
    GameObject player;
    public GameObject food;
    public GameObject gameHUD;
    public GameObject deathScreen;

    public bool deathScreenActive = false;
    public static float marginOfError = 0.0005f;
    public static float speed = 0.03f;
    public static int score = 0;

    /// <summary>
    /// Unity start function - ran on first frame
    /// </summary>
    void Start()
    {
        //create the player at 0,0,0
        moveLock = true;
        player = Object.Instantiate(prefabPlayer, new Vector3(0f, 0f, 0f), Quaternion.identity);
        moveLock = false;

        //create the player at 4,0,3
        food = Object.Instantiate(prefabFood, new Vector3(4f, 0f, 3f), Quaternion.identity);
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
                else if (bodyPieces.Count == 1)
                    AddBodyPiece();
            }
        }
        
    }

    /// <summary>
    /// Adds a body piece to the worm
    /// </summary>
    public void AddBodyPiece()
    {
        //checks if the worm has any body pieces
        if (bodyPieceObjects.Count == 0)
        {
            moveLock = true;
            Vector3 position = player.transform.position;
            Vector3 velocity = PlayerController.velocity;
            AddBodyPieceHelper(position, velocity);
            moveLock = false;
        }
        else
        {
            moveLock = true;
            Vector3 position = bodyPieces[bodyPieces.Count - 1].transform.position;
            Vector3 velocity = bodyPieces[bodyPieces.Count - 1].velocity;
            AddBodyPieceHelper(position, velocity);
            moveLock = false;
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
    }

    /// <summary>
    /// function to move the food to a random new location on the board
    /// </summary>
    public void RespawnFood()
    {
        food.transform.SetPositionAndRotation(SpawnLocation(), Quaternion.identity);
    }

    /// <summary>
    /// helper function to determine where the food should respawn
    /// </summary>
    /// <returns>Vector3 with random X and Y within bounds</returns> 
    private Vector3 SpawnLocation()
    {
        return new Vector3(Random.Range(-12, 11), 0, Random.Range(-6, 5));
    }

    /// <summary>
    /// Function to kill the player and end the game
    /// </summary>
    public void Die()
    {
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
        // Hide the game HUD and show the death screen
        deathScreenActive = true;
        gameHUD.SetActive(false);
        deathScreen.SetActive(deathScreenActive);
        
    }
}
