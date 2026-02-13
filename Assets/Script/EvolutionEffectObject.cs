using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EvolutionEffectObject : MonoBehaviour
{
    CardSource CardSource;
    [SerializeField] Animator anim;
    [SerializeField] Image CardImage;
    [SerializeField] List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();
    internal float animTime = 1.45f;
    public void Init()
    {
        this.gameObject.SetActive(false);
    }

    public virtual IEnumerator EvolutionEffectAnimation(CardSource cardSource, CardSource[] jogressEvoRoots = null, string message = "")
    {
        if (cardSource == null)
        {
            yield break;
        }

        if (ContinuousController.instance != null)
        {
            if (!ContinuousController.instance.showCutInAnimation)
            {
                yield break;
            }
        }

        if (!string.IsNullOrEmpty(message))
        {
            if (texts != null)
            {
                foreach (TextMeshProUGUI text in texts)
                {
                    if (text != null)
                    {
                        text.text = message;
                    }
                }
            }
        }

        anim.enabled = true;

        this.gameObject.SetActive(true);

        CardImage.sprite = cardSource.CardSprite;

        anim.SetInteger("Evolution", 1);

        yield return new WaitForSeconds(animTime);

        anim.SetInteger("Evolution", -1);

        anim.enabled = false;

        this.gameObject.SetActive(false);
    }

    public void PlaySE()
    {
        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().EvolutionSE_Ultimate);
    }
}
