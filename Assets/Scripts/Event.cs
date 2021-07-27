using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Event : MonoBehaviour
{
    public float x = 0;
    public float y = 0;
    public Text score1;
    public Text score2;
    public Text player;
    int P1score = 0;
    int P2score = 0;
    int score = 0;
    int turn = 0;
    Vector3 hit = Vector3.zero;
    public GameObject prefab;
    public GameObject prefab1;
    public bool change = false;

    // Update is called once per frame
    void Update()
    {
        
        if (change) {
            //Calculate score using distance from center
            double s = Math.Pow(Math.Pow(x, 2) + Math.Pow(y, 2), 0.5);
            score = Score(s);
            hit.x = x;
            hit.y = y;
            //Add score to Player 1
            if (turn==0){
                P1score+=score;
                Instantiate(prefab,
                    hit,
                    Quaternion.identity);
                score1.text = P1score.ToString();
                turn = 1;
                player.text = "Player 2";
            } 
            //Add score to player 2
            else if (turn==1){
                P2score+=score;
                Instantiate(prefab1,
                    hit,
                    Quaternion.identity);
                score2.text = P2score.ToString();
                turn = 0;
                player.text = "Player 1";
            }
            change = false;
        }
    }

    int Score(double score){
        if (score<.25){
            return 10;
        } else if (score<.75){
            return 9;
        } else if (score<1.25){
            return 8;
        } else if (score<1.75){
            return 7;
        } else if (score<2.25){
            return 6;
        } else if (score<2.75){
            return 5;
        } else if (score<3.25){
            return 4;
        } else if (score<3.75){
            return 3;
        } else if (score<4.25){
            return 2;
        } else if (score<4.75){
            return 1;
        } else {
            return 0;
        } 
    }

    public void set(Vector2 points){
        //print(GetComponent<Camera>().orthographicSize);
        float size = GetComponent<Camera>().orthographicSize;
        points.x = 1 - points.x;
        //points.y = 1-points.y;
        y = (points.y * size * 2) - size;
        size = size/9 * 16;
        x = (points.x * size * 2) - size;
        change = true;
    }
}
