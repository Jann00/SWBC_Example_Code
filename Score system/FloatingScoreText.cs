using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;
using static UnityEngine.GraphicsBuffer;

public class FloatingScoreText : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float randomDirection;
    public float distance = 50f;
    Vector3 direction;
    public AnimationCurve speedCurve;
    public int targetScore;
    public Animator animator;
    public RectTransform rectTransform;
    float score;
    Vector3 oldPosition;

    float animationTimer;

    // Start is called before the first frame update
    public void StartScoreAnimation()
    {
        //Set a random direction
        randomDirection = Random.Range(0f, 360f);
        oldPosition = rectTransform.position;
        direction = oldPosition + ((Quaternion.Euler(0, randomDirection, 0) * new Vector3(distance,distance,distance)));
        //Reset the score value
        score = 0;
        text.text = "+" + score.ToString("0");
        //Start the animation
        animator.SetTrigger("Fade");
        animationTimer = 0;
        
    }

    // Update is called once per frame
    void Update()
    {
        //Increase the text number
        if (score < targetScore)
        {
            score = Mathf.Lerp(score, targetScore, 0.2f);
            text.text = "+" + score.ToString("0");
        }
        animationTimer += Time.deltaTime;
        //Move the text in the assigned direction
        rectTransform.position = Vector3.Lerp(oldPosition, direction, speedCurve.Evaluate(animationTimer));
        //Destroy the text
        if (animationTimer > 2)
        {
            Destroy(gameObject);
        }
    }
}
