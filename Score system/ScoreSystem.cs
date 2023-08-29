using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;
using System;

public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem instance;

    [BoxGroup("UI Elements"), SerializeField]
    TextMeshProUGUI scoreText;
    [BoxGroup("UI Elements"), SerializeField]
    TextMeshProUGUI letterGradeText;
    [BoxGroup("UI Elements"), SerializeField]
    Slider letterGradeProgressMeter;
    [BoxGroup("UI Elements"), SerializeField]
    Image letterGradeProgressMeterBackground;
    [BoxGroup("UI Elements"), SerializeField]
    Slider multiplierProgressMeter;
    [BoxGroup("UI Elements"), SerializeField]
    TextMeshProUGUI multiplierText;
    [BoxGroup("UI Elements"), SerializeField]
    GameObject floatingTextPrefab;
    [BoxGroup("UI Elements"), SerializeField]
    Transform HUD;

    [BoxGroup("Score"), ReadOnly]
    public int score;
    [BoxGroup("Score"), ReadOnly, SerializeField]
    float temporaryScore;

    [BoxGroup("Letter Grade"), SerializeField]
    int[] targetScore;
    [BoxGroup("Letter Grade")]
    public char[] letterGrades;
    [BoxGroup("Letter Grade")]
    public Color[] gradeColor;
    [BoxGroup("Letter Grade")]
    public int currentGrade;

    [BoxGroup("Grade vfx")]
    public GameObject[] backgroundVFX;
    public Image explosion;
    public Animator explosionAnimator;

    [BoxGroup("Multiplier"), SerializeField]
    int currentMultiplier = 1;
    [BoxGroup("Multiplier"), SerializeField]
    int[] multiplierThreshold;
    [BoxGroup("Multiplier"), ReadOnly, SerializeField]
    int multiplierProgressValue;
    [BoxGroup("Multiplier"), ReadOnly, SerializeField]
    float multiplierProgressValueTmp;

    [BoxGroup("SFX"), SerializeField]
    AK.Wwise.Event[] letterGradeIncreaseSFX, multiplierLevelIncreaseSFX;
    /*[BoxGroup("SFX"), SerializeField]
    AK.Wwise.Event multiplierBreakSFX;*/

    [FoldoutGroup("Letter Grade Text")]
    public GradeTextMaterials[] cTextMaterials;
    [FoldoutGroup("Letter Grade Text")]
    public GradeTextMaterials[] bTextMaterials;
    [FoldoutGroup("Letter Grade Text")]
    public GradeTextMaterials[] aTextMaterials;
    [FoldoutGroup("Letter Grade Text")]
    public GradeTextMaterials[] sTextMaterials;
    [FoldoutGroup("Letter Grade Text")]
    public GradeTextMaterials[] xTextMaterials;


    bool animateLetterGradeText = false;
    float letterGradeAnimationTimer = 0;
    GradeTextMaterials currentMaterials;
    bool reverseAnimation = false;

    bool startFadeOut = false;
    float fadeTimer = 1;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        //Reset the letter grade text
        letterGradeText.text = letterGrades[currentGrade].ToString();
        letterGradeText.color = gradeColor[currentGrade];
        letterGradeProgressMeterBackground.color = gradeColor[currentGrade];
        letterGradeProgressMeter.minValue = 0;
        letterGradeProgressMeter.maxValue = targetScore[currentGrade];

        //Enable the background VFX
        backgroundVFX[currentGrade].SetActive(true);

        //Reset the multiplier
        multiplierProgressMeter.minValue = 0;
        multiplierProgressMeter.maxValue = multiplierThreshold[currentMultiplier - 1];
        multiplierText.text = "X" + currentMultiplier;
        
        //Reset the grade text materials
        foreach (GradeTextMaterials gradeTextMaterials in cTextMaterials)
        {
            foreach (Material m in gradeTextMaterials.materials)
            {
                m.SetFloat("_Animation_Factor", 0);
            }
        }
        foreach (GradeTextMaterials gradeTextMaterials in bTextMaterials)
        {
            foreach (Material m in gradeTextMaterials.materials)
            {
                m.SetFloat("_Animation_Factor", 0);
            }
        }
        foreach (GradeTextMaterials gradeTextMaterials in aTextMaterials)
        {
            foreach (Material m in gradeTextMaterials.materials)
            {
                m.SetFloat("_Animation_Factor", 0);
            }
        }
        foreach (GradeTextMaterials gradeTextMaterials in sTextMaterials)
        {
            foreach (Material m in gradeTextMaterials.materials)
            {
                m.SetFloat("_Animation_Factor", 0);
            }
        }
        foreach (GradeTextMaterials gradeTextMaterials in xTextMaterials)
        {
            foreach (Material m in gradeTextMaterials.materials)
            {
                m.SetFloat("_Animation_Factor", 0);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Increase the score value and Grad Progress meter over time
        if (temporaryScore < score)
        {
            temporaryScore = Mathf.Lerp(temporaryScore, score, 0.1f);
            scoreText.text = temporaryScore.ToString("0000000");
            letterGradeProgressMeter.value = temporaryScore;
        }
        
        //Increase the grade
        if (currentGrade <= targetScore.Length - 1 && temporaryScore >= targetScore[currentGrade])
        {
            //Set the current grade, text and colors
            currentGrade++;
            letterGradeText.text = letterGrades[currentGrade].ToString();
            letterGradeText.color = gradeColor[currentGrade];
            letterGradeProgressMeterBackground.color = gradeColor[currentGrade];
            letterGradeProgressMeter.fillRect.GetComponent<Image>().color = gradeColor[currentGrade];

            //Disable all background VFX
            foreach (GameObject g in backgroundVFX)
            {
                g.SetActive(false);
            }
            //Enable current background VFX
            backgroundVFX[currentGrade].SetActive(true);

            //Play UI explosion VFX
            explosion.color = gradeColor[currentGrade];
            explosionAnimator.SetTrigger("Explode");
            
            //Set the progress meter to reflect the next possible grade
            if (currentGrade <= targetScore.Length - 1)
            {
                letterGradeProgressMeter.minValue = targetScore[currentGrade - 1];
                letterGradeProgressMeter.maxValue = targetScore[currentGrade];
            }

            //SFX
            letterGradeIncreaseSFX[currentGrade - 1].Post(gameObject);

            //Enable the grade text VFX
            switch (currentGrade)
            {
                case 1:
                    currentMaterials = cTextMaterials[0];
                    break;
                case 2:
                    currentMaterials = bTextMaterials[0];
                    break;
                case 3:
                    currentMaterials = aTextMaterials[0];
                    break;
                case 4:
                    currentMaterials = sTextMaterials[0];
                    break;
                case 5:
                    currentMaterials = xTextMaterials[0];
                    break;

            }
            letterGradeAnimationTimer = 0;
            animateLetterGradeText = true;
            
        }

        //Increase the displayed multiplier progress meter
        if (multiplierProgressValueTmp < multiplierProgressValue)
        {
            multiplierProgressValueTmp = Mathf.Lerp(multiplierProgressValueTmp, multiplierProgressValue, 0.1f);
            multiplierProgressMeter.value = multiplierProgressValueTmp;
        }

        //Increase the multiplier
        if (currentMultiplier - 1 <= multiplierThreshold.Length - 1 && multiplierProgressValueTmp >= multiplierThreshold[currentMultiplier - 1])
        {
            currentMultiplier++;
            multiplierText.text = "X" + currentMultiplier;

            if (currentMultiplier - 1 <= multiplierThreshold.Length - 1)
            {
                multiplierProgressMeter.minValue = multiplierThreshold[currentMultiplier - 2];
                multiplierProgressMeter.maxValue = multiplierThreshold[currentMultiplier - 1];
            }

            //Play SFX
            if (multiplierLevelIncreaseSFX[currentMultiplier - 2] != null)
            {
                multiplierLevelIncreaseSFX[currentMultiplier - 2].Post(gameObject);
            }
            
        }

        //Animate the Letter grade text prompt
        if (animateLetterGradeText)
        {
            if (!reverseAnimation)
            {
                letterGradeAnimationTimer += Time.unscaledDeltaTime;
                if (letterGradeAnimationTimer > 2)
                {
                    reverseAnimation = true;
                }
            }
            else if (reverseAnimation)
            {
                letterGradeAnimationTimer -= Time.unscaledDeltaTime;
                if (letterGradeAnimationTimer < 0)
                {
                    reverseAnimation = false;
                    animateLetterGradeText = false;
                }
            }
            foreach (Material m in currentMaterials.materials)
            {
                m.SetFloat("_Animation_Factor", letterGradeAnimationTimer);
            }


        }

        if (startFadeOut)
        {
            fadeTimer -= Time.unscaledDeltaTime;
            
        }

    }

    //Increase the score
    public void IncreaseScore(int scoreAmount)
    {
        multiplierProgressValue += scoreAmount;
        score += scoreAmount * currentMultiplier;
    }

    //Reset the multiplier
    public void ResetMultiplier()
    {
        multiplierProgressValue = 0;
        multiplierProgressValueTmp = 0;
        currentMultiplier = 1;
        multiplierProgressMeter.minValue = 0;
        multiplierProgressMeter.maxValue = multiplierThreshold[currentMultiplier - 1];
        multiplierProgressMeter.value = 0;
        multiplierText.text = "X" + currentMultiplier;

        //SFX
        //multiplierBreakSFX.Post(gameObject);
    }

    //Spawn floating score when score is awarded
    public void SpawnFloatingScore(Vector3 position, int scoreAmount)
    {
        GameObject latestText = Instantiate(floatingTextPrefab, HUD);
        latestText.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(position);
        FloatingScoreText animationScript = latestText.GetComponent<FloatingScoreText>();
        animationScript.targetScore = scoreAmount * currentMultiplier;
        animationScript.StartScoreAnimation();
    }

    public void FadeOutEffects()
    {
        foreach(GameObject g in backgroundVFX)
        {
            g.SetActive(false);
        }
    }
}



[Serializable]
public class GradeTextMaterials
{
    public Material[] materials;
}
