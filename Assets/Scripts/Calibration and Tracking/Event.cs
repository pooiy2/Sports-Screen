using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

//Event manager for baseball pitching simulator

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

    //Added for baseball
    bool strike = false;
    List<GameObject> collisions = new List<GameObject>();
    float height;
    float width;
    float t;
    float wait = 1;

    //Runs before update
    private void Awake() {
        height =  GetComponent<Camera>().orthographicSize;
        width  = height * GetComponent<Camera>().aspect;
        t = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            //Calculate score by seeing if it falls within the square for strike zone
            //x is width, y is height
            x = Input.mousePosition.x;
            y = Input.mousePosition.y;
            print(x);
            print(Screen.width);
            print("Width, Height");
            print(width);
            print(height);
            //Normalizing the values of x and y based on the size and aspect ratio of the camera
            x/=Screen.width;
            y/=Screen.height;
            x*=width * 2; 
            y*=height * 2;
            x-=width;
            y-=height;

            hit.x = x;
            hit.y = y;
            
            print(x);
            print(y);
            if (x>=-1.7 & x<=1.76 & y>=-1.51 & y<=1.59){
                player.text = "Strike";
                P2score+=1;
                score2.text = P2score.ToString();
                collisions.Add(Instantiate(prefab1,
                    hit,
                    Quaternion.identity));
            } else {
                player.text = "Ball";
                P1score+=1;
                score1.text = P1score.ToString();
                collisions.Add(Instantiate(prefab,
                    hit,
                    Quaternion.identity));
            }
            if (P1score>=4){
                player.text = "Walk";
                foreach (GameObject mark in collisions){
                    Destroy(mark);
                }
                P2score = 0;
                P1score = 0;
                score2.text = P2score.ToString();
                score1.text = P1score.ToString();
            } else if (P2score>=3){
                player.text = "Strikeout";
                foreach (GameObject mark in collisions){
                    Destroy(mark);
                }
                P2score = 0;
                P1score = 0;
                score2.text = P2score.ToString();
                score1.text = P1score.ToString();
            }
        }
        /*For dartboard
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
        }*/
    }
    /*Stuff for sensor and dartboard
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
    }*/

    //Gives an x, y coordinate and tells script that a change has occured
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