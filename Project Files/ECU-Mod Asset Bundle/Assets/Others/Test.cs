using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {
    public float gradientValue = 0;
	Gradient gradient;
	GradientColorKey[] colorKey;
	GradientAlphaKey[] alphaKey;

	// Use this for initialization
	void Start () {
        gradient = new Gradient();

        /*
        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        colorKey = new GradientColorKey[2];
        colorKey[0].color = new Color(1.0f, 0.64f, 0.0f); //Orange
        colorKey[0].time = 0f;

        colorKey[1].color = Color.green;
        colorKey[1].time = 0.65f;

        colorKey[1].color = Color.red;
        colorKey[2].time = 1f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0f;

        alphaKey[1].alpha = 0.5f;
        alphaKey[1].time = 0.65f;

        alphaKey[2].alpha = 0f;
        alphaKey[2].time = 1.0f;
        */

        colorKey = new GradientColorKey[3];
        colorKey[0].color = new Color(1.0f, 0.64f, 0.0f);
        colorKey[0].time = 0.0f;
        
        colorKey[1].color = Color.green;
        colorKey[1].time = 0.50f;

        colorKey[2].color = Color.red;
        colorKey[2].time = 0.8f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        alphaKey = new GradientAlphaKey[3];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;

        alphaKey[1].alpha = 0.5f;
        alphaKey[1].time = 0.50f;

        alphaKey[2].alpha = 0.2f;
        alphaKey[2].time = 0.8f;



        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)


        gradient.SetKeys(colorKey, alphaKey);

        CameraFocus(this.gameObject.transform);
    }

    void CameraFocus(Transform target)
    {
        Vector3 pointOnside = target.position + new Vector3(target.localScale.x * 0.5f, 0.0f, target.localScale.z * 0.5f);
        float aspect = (float)Screen.width / (float)Screen.height;
        float maxDistance = (target.localScale.y * 0.5f) / Mathf.Tan(Mathf.Deg2Rad * (Camera.main.fieldOfView / aspect));
        Camera.main.transform.position = Vector3.MoveTowards(pointOnside, target.position, -maxDistance);
        Camera.main.transform.LookAt(target.position);

    }

    float timer = 0;
    float counter = 0;
    float counter2 = 1;

    // Update is called once per frame
    void Update () {
        timer += Time.deltaTime;
        if(timer >= 0.05f)
        {
            if(counter < 1)
            {
                timer = 0;
                counter += 0.1f;
                this.gameObject.GetComponent<Renderer>().material.color = gradient.Evaluate(counter);
            }
            else if(counter2 > 0)
            {
                timer = 0;
                counter2 -= 0.1f;
                this.gameObject.GetComponent<Renderer>().material.color = gradient.Evaluate(counter2);
            }
            if(counter >= 1 && counter2 <= 0)
            {
                counter = 0;
                counter2 = 1;
                this.gameObject.GetComponent<Renderer>().material.color = gradient.Evaluate(0);
            }
        }
        
    }
}
