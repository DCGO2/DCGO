using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CircleSlider : MonoBehaviour
{
 
     public bool b=true;
	 public Image image;
	 public float speed=0.5f;

	[SerializeField] bool doRotate = true;

  float time =0f;
  
  public Text progress;
	bool isClockwise = true;
  
  void Start()
  {
	  
	image = GetComponent<Image>();
  }
  
    void Update()
    {
		if(b)
		{
			time += Time.deltaTime*speed;

			if(image.fillClockwise)
            {
				image.fillAmount = time;
			}

			else
            {
				image.fillAmount = 1 - time;
			}
			
			if(progress)
			{
				progress.text = (int)(image.fillAmount*100)+"%";
			}
			
            if(time>1)
		    {
			    if (doRotate)
			    {
				    image.fillClockwise = !image.fillClockwise;
			    }

			    time =0;
		    }
        }
    }
}
