using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WinnerScene : MonoBehaviour
{
    //public int winnerPlayer; //set winning player
    [SerializeField] private GameObject player1WinnerFrame;
    [SerializeField] private GameObject player2WinnerFrame;
    [SerializeField] private GameObject player1Frame;
    [SerializeField] private GameObject player2Frame;
    [SerializeField] GameObject player1Prefab;
    [SerializeField] GameObject player2Prefab;
    [SerializeField] GameObject player1Text;
    [SerializeField] GameObject player2Text;
    Transform spawnPoint;
    [SerializeField] WinnerSO winner;
    

    private void Awake()
    {
        spawnPoint = this.transform; //set whatever spawnpoint
    }

    private void Start()
    {
        if (winner.winner == 1) //whatever logic for player 
        {
            LoadWinner(player1Prefab, player1Text, player1Frame, player1WinnerFrame);
        }
        else if (winner.winner == 2) //whatever logic for player 
        {
            LoadWinner(player2Prefab, player2Text, player2Frame, player2WinnerFrame);
        }

        //loadscene menu?
    }

    private void LoadWinner(GameObject prefab, GameObject text, GameObject frame, GameObject winFrame)
    {
        Instantiate(prefab, spawnPoint);
        frame.SetActive(true);
        winFrame.SetActive(true);
        text.SetActive(true);
    }
}
