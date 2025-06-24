using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using System.Threading.Tasks;
using TMPro;
using System.Linq;

public class PermanentDetail : MonoBehaviour
{
    [Header("パネル")]
    public GameObject pokemonInfoPanel;

    [Header("カード情報プレハブ")]
    public CardInfo cardInfoPrefab;

    public TextMeshProUGUI cardNameText;

    public ScrollRect pokemonScroll;
    public TextMeshProUGUI effectText;
    public Image CardImage;

    Permanent _permanent;

    [SerializeField] TMP_FontAsset CardNameFont_ENG;
    [SerializeField] TMP_FontAsset CardNameFont_JPN;

    CardSource topCard = null;

    public async void OpenUnitDetail(Permanent permanent)
    {
        GManager.instance.PlayDecisionSE();

        _permanent = permanent;

        if (permanent.TopCard != null)
        {
            topCard = permanent.TopCard;
        }

        this.gameObject.SetActive(true);

        CardImage.sprite = await permanent.TopCard.GetCardSprite();

        if (cardNameText != null)
        {
            if (ContinuousController.instance.language == Language.ENG)
            {
                cardNameText.font = CardNameFont_ENG;
                cardNameText.text = _permanent.TopCard.BaseENGCardNameFromEntity;
            }

            else
            {
                cardNameText.font = CardNameFont_JPN;
                cardNameText.text = _permanent.TopCard.BaseJPNCardNameFromEntity;
            }
        }

        List<ICardEffect> cardEffects = new List<ICardEffect>();

        foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
        {
            bool add = false;

            if (cardEffect is IDisableCardEffect)
            {
                add = true;
            }

            else
            {
                if (cardEffect.CanUse(null))
                {
                    if (!string.IsNullOrEmpty(cardEffect.EffectName))
                    {
                        add = true;
                    }
                }
            }

            if (add)
            {
                cardEffects.Add(cardEffect);
            }
        }

        string effectString = "";

        #region security attack
        if (permanent.IsDigimon)
        {
            if (permanent.Strike_AllowMinus >= 0)
            {
                effectString += $"Security Attack : {permanent.Strike}\n";
            }

            else
            {
                effectString += $"Security Attack : {permanent.Strike} ({permanent.Strike_AllowMinus})\n";
            }

            effectString += $"-------------------------\n";
        }
        #endregion

        #region unblockable
        if (permanent.IsUnblockable)
        {
            effectString += $"- Unblockable\n";
        }
        #endregion

        #region blocker
        if (permanent.HasBlocker)
        {
            effectString += $"- Blocker\n";
        }
        #endregion

        #region Pierce
        if (permanent.HasPierce)
        {
            effectString += $"- Pierce\n";
        }
        #endregion

        #region Reboot
        if (permanent.HasReboot)
        {
            effectString += $"- Reboot\n";
        }
        #endregion

        #region Evade
        if (permanent.HasEvade)
        {
            effectString += $"- Evade\n";
        }
        #endregion

        #region Rush
        if (permanent.HasRush)
        {
            effectString += $"- Rush\n";
        }
        #endregion

        #region Alliance
        if (permanent.HasAlliance)
        {
            effectString += $"- Alliance\n";
        }
        #endregion

        #region Barrier
        if (permanent.HasBarrier)
        {
            effectString += $"- Barrier\n";
        }
        #endregion

        #region Retaliation
        if (permanent.HasRetaliation)
        {
            for (int i = 0; i < permanent.RetaliationCount; i++)
            {
                effectString += $"- Retaliation\n";
            }
        }
        #endregion

        #region Jamming
        if (permanent.HasJamming)
        {
            effectString += $"- Jamming\n";
        }
        #endregion

        #region Raid
        if (permanent.HasRaid)
        {
            effectString += $"- Raid\n";
        }
        #endregion

        #region Mind Link
        if (permanent.HasMindLink)
        {
            effectString += $"- Mind Link\n";
        }
        #endregion

        #region Fortitude
        if (permanent.HasFortitude)
        {
            effectString += $"- Fortitude\n";
        }
        #endregion

        #region Blitz
        if (permanent.HasBlitz)
        {
            effectString += $"- Blitz\n";
        }
        #endregion

        #region Scapegoat
        if (permanent.HasScapegoat)
        {
            effectString += $"- Scapegoat\n";
        }
        #endregion

        #region Collision
        /*if (permanent.hascCollision)
        {
            effectString += $"- Collision\n";
        }*/
        #endregion

        #region Partition
        if (permanent.HasPartition)
        {
            effectString += $"- Partition\n";
        }
        #endregion

        #region Security Attack Changes
        if (permanent.HasSecurityAttackChanges)
        {
            for (int i = 0; i < permanent.SecurityAttackChanges.Count; i++)
            {
                string text = permanent.SecurityAttackChanges[i] > 0 ? $"- Security Attack +{permanent.SecurityAttackChanges[i]}\n" : $"- Security Attack {permanent.SecurityAttackChanges[i]}\n";

                effectString += text;
            }
        }
        #endregion

        #region effect
        foreach (ICardEffect cardEffect in cardEffects)
        {
            if (cardEffect is IChangeSAttackEffect)
            {
                continue;
            }

            if (cardEffect is ICanSuspendByDigisorptionEffect)
            {
                continue;
            }

            if (cardEffect is IAddSkillEffect)
            {
                continue;
            }

            if (cardEffect is IBlockerEffect)
            {
                continue;
            }

            if (cardEffect is IRushEffect)
            {
                continue;
            }

            if (cardEffect is IRebootEffect)
            {
                continue;
            }

            if (cardEffect is IAddDigivolutionRequirementEffect)
            {
                continue;
            }

            if (cardEffect is CanNotBeDestroyedByBattleClass && cardEffect.EffectName == "Jamming")
            {
                continue;
            }

            if (cardEffect.IsNotShowUI)
            {
                continue;
            }

            effectString += $"- {cardEffect.EffectName}\n";
        }
        #endregion

        effectText.text = effectString.Replace("、", ",").Replace("，", ",");
        effectText.raycastTarget = false;

        for (int i = 0; i < pokemonScroll.content.childCount; i++)
        {
            Destroy(pokemonScroll.content.GetChild(i).gameObject);
        }

        //Adds Top card to stack
        CardInfo topCardInfo = Instantiate(cardInfoPrefab, pokemonScroll.content);
        topCardInfo.SetUpCardInfo(permanent.TopCard, permanent);

        //Adds Digivolution Cards
        foreach (CardSource cardSource in permanent.DigivolutionCards.Clone())
        {
            CardInfo cardInfo = Instantiate(cardInfoPrefab, pokemonScroll.content);
            cardInfo.SetUpCardInfo(cardSource);
        }

        //Adds Linked Cards
        foreach (CardSource cardSource in permanent.LinkedCards.Clone())
        {
            CardInfo cardInfo = Instantiate(cardInfoPrefab, pokemonScroll.content);
            cardInfo.SetUpCardInfo(cardSource);
        }

        Vector3 targetPositon = Vector3.zero;
        Vector3 startPosition = Vector3.zero;

        if (_permanent.ShowingPermanentCard.transform.position.x > 27)
        {
            targetPositon = new Vector3(-390, 0, 0);
            startPosition = new Vector3(-130, 0, 0);
        }

        else
        {
            targetPositon = new Vector3(390, 0, 0);
            startPosition = new Vector3(130, 0, 0);
        }

        pokemonInfoPanel.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);

        float animationTime = 0.12f;

        var sequence = DOTween.Sequence();

        sequence
            .Append(pokemonInfoPanel.transform.DOScale(new Vector3(1.3f, 1.3f, 1.3f), animationTime));

        sequence.Play();

        await Task.Delay(TimeSpan.FromSeconds(Time.deltaTime));

        pokemonScroll.verticalNormalizedPosition = 1;
    }

    bool _first = false;
    public void CloseUnitDetail()
    {
        if (_first)
        {
            if (Opening.instance != null)
            {
                Opening.instance.PlayCancelSE();
            }
        }

        _first = true;

        gameObject.SetActive(false);
    }

    public void OnClickCardImage()
    {
        if (_permanent != null)
        {
            if (_permanent.TopCard != null)
            {
                GManager.instance.cardDetail.OpenCardDetail(_permanent.TopCard, true);

                if (GManager.instance != null)
                {
                    GManager.instance.PlayDecisionSE();
                }
            }
        }

        if (topCard != null)
        {
            GManager.instance.cardDetail.OpenCardDetail(topCard, true);

            if (GManager.instance != null)
            {
                GManager.instance.PlayDecisionSE();
            }
        }
    }
}