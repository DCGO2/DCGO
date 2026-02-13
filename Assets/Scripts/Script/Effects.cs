using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using AutoLayout3D;
using System;
using System.Linq;
using TMPro;
using Cinemachine;
public class Effects : MonoBehaviour
{
    [SerializeField] Transform effectParent;
    public void Init()
    {
        ShowUseHandCardParent.gameObject.SetActive(false);
        ShowUseHandCard.transform.localRotation = Quaternion.Euler(0, 0, 0);

        OffShowCard();
        OffShowCard2();

        renderingFiedlPermanentCard.gameObject.SetActive(false);
        renderingHandCard.gameObject.SetActive(false);

        for (int i = 0; i < ShowEffectDiscriptionObjectParent.childCount; i++)
        {
            Destroy(ShowEffectDiscriptionObjectParent.GetChild(i).gameObject);
        }

        ShowEffectDiscriptionObjectParent.GetComponent<VerticalLayoutGroup>().enabled = false;
    }

    #region 実行効果テキスト表示
    [SerializeField] ShowEffectDiscriptionObject ShowEffectDiscriptionObjectPrefab;
    [SerializeField] Transform ShowEffectDiscriptionObjectParent;
    public IEnumerator ShowActivateCardEffectDiscription(ICardEffect cardEffect)
    {
        ShowEffectDiscriptionObjectParent.GetComponent<VerticalLayoutGroup>().enabled = true;

        ShowEffectDiscriptionObject showEffectDiscriptionObject = Instantiate(ShowEffectDiscriptionObjectPrefab, ShowEffectDiscriptionObjectParent);

        showEffectDiscriptionObject.ShowEffectDiscription(cardEffect);

        yield return null;

        ShowEffectDiscriptionObjectParent.GetComponent<VerticalLayoutGroup>().enabled = false;
    }
    #endregion

    #region 手札のカードを使用する
    [Header("手札のカード消滅時エフェクト")]
    public GameObject DeleteHandCardEffect;

    //手札のカードが消えるまでの時間
    public float waitTime_DeleteHandEffect { get; } = 0.29f;

    #region 手札のカードを削除
    public IEnumerator DeleteHandCardEffectCoroutine(CardSource card)
    {
        bool end = false;

        HandCard handCard = card.ShowingHandCard;

        if (handCard == null)
        {
            yield break;
        }

        ContinuousController.instance.PlaySE(GManager.instance.DeleteHandSE);

        if (handCard.cardSource.Owner.HandTransform.GetComponent<HandContoller>() != null)
        {
            handCard.cardSource.Owner.HandTransform.GetComponent<HandContoller>().isDragging = true;
        }

        if (handCard.CostIcons != null)
        {
            if (handCard.CostIcons.Count >= 1)
            {
                handCard.CostIcons[0].transform.parent.gameObject.SetActive(false);
            }
        }

        if (handCard.CostText != null)
        {
            handCard.CostText.transform.parent.gameObject.SetActive(false);
        }

        if (handCard.LevelIcons != null)
        {
            if (handCard.LevelIcons.Count >= 1)
            {
                handCard.LevelIcons[0].transform.parent.gameObject.SetActive(false);
            }
        }

        if (handCard.LevelText != null)
        {
            handCard.LevelText.transform.parent.gameObject.SetActive(false);
        }

        if (handCard.EvoCostIcons != null)
        {
            if (handCard.EvoCostIcons.Count >= 1)
            {
                for (int i = 0; i < handCard.EvoCostIcons.Count; i++)
                {
                    handCard.EvoCostIcons[i].transform.parent.gameObject.SetActive(false);
                }
            }
        }

        handCard.CardImage.sprite = null;

        float shrinkTime = 0.22f;

        var sequence = DOTween.Sequence();

        sequence
            .Append(DOTween.To(() => handCard.transform.localScale, (x) => handCard.transform.localScale = x, new Vector3(0, 0, 0), shrinkTime).SetEase(Ease.OutCubic))
            .AppendCallback(() => { end = true; });

        sequence.Play();

        while (handCard != null)
        {
            if (handCard.transform.localScale.x > 0.2f)
            {
                yield return null;
            }

            else
            {
                break;
            }
        }

        if (handCard != null)
        {
            GameObject deleteHandCerdEffect = Instantiate(DeleteHandCardEffect, GManager.instance.canvas.transform);

            deleteHandCerdEffect.transform.position = handCard.transform.position;

            StartCoroutine(DeleteCoroutine(deleteHandCerdEffect, null));
        }

        while (!end)
        {
            yield return null;
        }

        end = false;

        yield return new WaitForSeconds(0.07f);

        //card.Owner.HandCards.Remove(card);

        //yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(card));

        if (handCard != null)
        {
            Destroy(handCard.gameObject);
        }

        if (card.Owner.HandTransform.GetComponent<HandContoller>() != null)
        {
            card.Owner.HandTransform.GetComponent<HandContoller>().isDragging = false;
        }

        CardObjectController.AlignHand(card.Owner);
    }
    #endregion

    #endregion

    #region エフェクトを削除する
    public static IEnumerator DeleteCoroutine(GameObject effect, FieldPermanentCard fieldPermanentCard)
    {
        yield return new WaitForSeconds(5f);

        Destroy(effect);

        if (fieldPermanentCard != null)
        {
            fieldPermanentCard.IsEffectPlaying = false;
        }
    }
    #endregion

    #region 場のポケモンのスキルが発動した時のエフェクト
    [Header("場のポケモンのスキルが発動した時のエフェクト")]
    public GameObject FieldUnitEffectPrefab;
    public IEnumerator ActivateFieldPokemonSkillEffect(Permanent permanent, ICardEffect cardEffect)
    {
        if (permanent.ShowingPermanentCard != null)
        {
            ContinuousController.instance.PlaySE(GManager.instance.UseSkillSE);

            //pokemon.ShowingPermanentCard.OnSkillName(cardEffect);

            GameObject Effect = Instantiate(FieldUnitEffectPrefab, effectParent);
            Effect.transform.position = permanent.ShowingPermanentCard.transform.position;

            StartCoroutine(DeleteCoroutine(Effect, permanent.ShowingPermanentCard));

            permanent.ShowingPermanentCard.OnUsingSkillEffect();

            yield return new WaitForSeconds(0.48f);
        }
    }
    #endregion

    #region トラッシュのカード効果が発動した時のエフェクト
    public IEnumerator ActivateTrashCardSkillEffect(ICardEffect cardEffect)
    {
        bool end = false;
        var sequence = DOTween.Sequence();

        CardSource cardSource = cardEffect.EffectSourceCard;

        if (cardSource == null)
        {
            yield break;
        }

        if (CardEffectCommons.IsExistOnTrash(cardSource))
        {
            ContinuousController.instance.PlaySE(GManager.instance.UseSkillSE);

            cardSource.Owner.TrashHandCard.gameObject.SetActive(true);
            cardSource.Owner.TrashHandCard.SetUpHandCard(cardSource);
            cardSource.Owner.TrashHandCard.SetUpHandCardImage();
            cardSource.Owner.TrashHandCard.OnOutline();
            cardSource.Owner.TrashHandCard.SetOrangeOutline();
            cardSource.Owner.TrashHandCard.transform.localScale = new Vector3(1, 1, 1);
            cardSource.Owner.TrashHandCard.IsExecuting = true;

            Vector3 startPosition = Vector3.zero;
            Vector3 targetPosition = Vector3.zero;

            if (cardSource.Owner.isYou)
            {
                targetPosition = new Vector3(-190, 95, 0);
            }
            else
            {
                targetPosition = new Vector3(190, -30, 0);
            }

            cardSource.Owner.TrashHandCard.transform.localPosition = startPosition;

            sequence = DOTween.Sequence();

            sequence
                .Append(cardSource.Owner.TrashHandCard.transform.DOScale(new Vector3(1.4f, 1.4f, 1), 0.25f))
                .Join(cardSource.Owner.TrashHandCard.transform.DOLocalMove(targetPosition, 0.25f))
                .AppendCallback(() => end = true);

            sequence.Play();

            yield return new WaitWhile(() => !end);
            end = false;

            sequence = DOTween.Sequence();

            sequence
                .Append(cardSource.Owner.TrashHandCard.transform.DOScale(new Vector3(2f, 2f, 1), 0.25f))
                .AppendCallback(() => end = true);
            sequence.Play();

            GameObject Effect = Instantiate(FieldUnitEffectPrefab, effectParent);
            Effect.transform.position = cardSource.Owner.TrashHandCard.transform.position + new Vector3(0, 1, 0);

            yield return new WaitWhile(() => !end);
            end = false;

            yield return new WaitForSeconds(0.25f);

            sequence = DOTween.Sequence();

            sequence
                .Append(cardSource.Owner.TrashHandCard.transform.DOScale(new Vector3(1.4f, 1.4f, 1), 0.08f))
                .AppendCallback(() => end = true);

            sequence.Play();

            StartCoroutine(DeleteCoroutine(Effect, null));
        }
    }
    #endregion

    #region 処理領域カード効果が発動した時のエフェクト
    public IEnumerator ActivateExecutingCardSkillEffect(CardSource cardSource, ICardEffect cardEffect)
    {
        bool end = false;
        var sequence = DOTween.Sequence();

        if (cardSource.Owner.ExecutingCards.Contains(cardSource))
        {
            HandCard executingHandCard = null;

            if (cardSource.Owner.brainStormObject.BrainStormHandCards.Count((handCard) => handCard.gameObject.activeSelf && handCard.cardSource == cardSource) >= 1)
            {
                foreach (HandCard handCard in cardSource.Owner.brainStormObject.BrainStormHandCards)
                {
                    if (handCard.gameObject.activeSelf && handCard.cardSource == cardSource)
                    {
                        executingHandCard = handCard;
                    }
                }
            }

            else
            {
                foreach (HandCard handCard in cardSource.Owner.brainStormObject.BrainStormHandCards)
                {
                    if (handCard.gameObject.activeSelf)
                    {
                        executingHandCard = handCard;
                    }
                }
            }

            if (executingHandCard != null)
            {
                ContinuousController.instance.PlaySE(GManager.instance.UseSkillSE);

                executingHandCard.gameObject.SetActive(true);
                executingHandCard.SetUpHandCard(cardSource);
                executingHandCard.SetUpHandCardImage();
                executingHandCard.OnOutline();
                executingHandCard.SetOrangeOutline();
                executingHandCard.transform.localScale = new Vector3(1, 1, 1);
                //executingHandCard.SetSkillName(cardEffect);
                executingHandCard.IsExecuting = true;

                sequence = DOTween.Sequence();

                sequence
                    .Append(executingHandCard.transform.DOScale(new Vector3(1.2f, 1.2f, 1), 0.083f))
                    .AppendCallback(() => end = true);

                sequence.Play();

                yield return new WaitWhile(() => !end);
                end = false;

                sequence = DOTween.Sequence();

                sequence
                    .Append(executingHandCard.transform.DOScale(new Vector3(1.4f, 1.4f, 1), 0.16f))
                    .AppendCallback(() => end = true);
                sequence.Play();

                GameObject Effect = Instantiate(FieldUnitEffectPrefab, effectParent);
                Effect.transform.position = executingHandCard.transform.position + new Vector3(0, 1, 0);

                yield return new WaitWhile(() => !end);
                end = false;

                yield return new WaitForSeconds(0.16f);

                sequence = DOTween.Sequence();

                sequence
                    .Append(executingHandCard.transform.DOScale(new Vector3(1f, 1f, 1), 0.083f))
                    .AppendCallback(() => end = true);

                sequence.Play();

                StartCoroutine(DeleteCoroutine(Effect, null));
            }
        }
    }
    #endregion

    #region 手札のカードの効果が発動した時のエフェクト
    public IEnumerator ActivateHandCardSkillEffect(CardSource cardSource, ICardEffect cardEffect)
    {
        bool end = false;
        var sequence = DOTween.Sequence();

        float targetPivotY = 0;

        if (cardSource.Owner.isYou)
        {
            targetPivotY = 0.08f;
        }

        else
        {
            targetPivotY = 1.2f;
        }

        if (cardSource.Owner.HandCards.Contains(cardSource))
        {
            if (cardSource.ShowingHandCard != null)
            {
                ContinuousController.instance.PlaySE(GManager.instance.UseSkillSE);

                foreach (HandCard handCard in GManager.instance.You.HandCardObjects)
                {
                    handCard.GetComponent<Draggable_HandCard>().CanPointerEnterExitAction = false;
                }

                if (cardSource.Owner.HandTransform.GetComponent<HandContoller>() != null)
                {
                    cardSource.Owner.HandTransform.GetComponent<HandContoller>().isDragging = true;
                }

                cardSource.ShowingHandCard.IsExecuting = true;
                cardSource.ShowingHandCard.ShowOpponent = true;
                cardSource.ShowingHandCard.Outline_Select.SetActive(true);
                cardSource.ShowingHandCard.SetOrangeOutline();

                sequence = DOTween.Sequence();

                sequence
                    .Append(cardSource.ShowingHandCard.transform.DOScale(new Vector3(1.3f, 1.3f, 1), 0.25f))
                    .Join(DOTween.To(() => cardSource.ShowingHandCard.GetComponent<RectTransform>().pivot, (x) => cardSource.ShowingHandCard.GetComponent<RectTransform>().pivot = x, new Vector2(0.5f, targetPivotY), 0.12f))
                    .AppendCallback(() => end = true);

                sequence.Play();

                GameObject Effect = Instantiate(FieldUnitEffectPrefab, effectParent);
                Effect.transform.position = cardSource.ShowingHandCard.transform.position + new Vector3(0, 1, 0);

                yield return new WaitWhile(() => !end);
                end = false;

                cardSource.ShowingHandCard.GetComponent<RectTransform>().pivot = new Vector2(0.5f, targetPivotY);
                yield return new WaitForSeconds(0.25f);

                if (cardSource.Owner.HandTransform.GetComponent<HandContoller>() != null)
                {
                    cardSource.Owner.HandTransform.GetComponent<HandContoller>().isDragging = false;
                }

                foreach (HandCard handCard in GManager.instance.You.HandCardObjects)
                {
                    handCard.GetComponent<Draggable_HandCard>().CanPointerEnterExitAction = true;
                }
            }
        }
    }
    #endregion

    #region フィールドキャラカード生成時のエフェクト
    [Header("新ユニット生成時着地エフェクト")]
    public GameObject NewUnitEffect_OnLand;

    [Header("ユニット重ねる時エフェクト")]
    public GameObject EvolutionUnitEffect;

    [Header("炎ポケモンエフェクト")]
    public GameObject RedEvolutionEffect;

    [Header("水ポケモンエフェクト")]
    public GameObject BlueEvolutionEffect;

    [Header("草ポケモンエフェクト")]
    public GameObject GreenEvolutionEffect;

    [Header("雷ポケモンエフェクト")]
    public GameObject YellowEvolutionEffect;

    [Header("超ポケモンエフェクト")]
    public GameObject PurpleEvolutionEffect;

    [Header("闘ポケモンエフェクト")]
    public GameObject OrangeEvolutionEffect;

    [Header("悪ポケモンエフェクト")]
    public GameObject BlackEvolutionEffect;

    [Header("鋼ポケモンエフェクト")]
    public GameObject SilverEvolutionEffect;

    [Header("竜ポケモンエフェクト")]
    public GameObject DragonEvolutionEffect;

    [Header("妖ポケモンエフェクト")]
    public GameObject PinkEvolutionEffect;

    [Header("無ポケモンエフェクト")]
    public GameObject WhiteEvolutionEffect;

    public IEnumerator CreateFieldPermanentCardEffect(FieldPermanentCard fieldPermanentCard, bool isDigiXros, CardSource[] jogressEvoRoots = null, bool HasETB = false)
    {
        if (jogressEvoRoots != null)
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.jogressEffectObject.EvolutionEffectAnimation(fieldPermanentCard.ThisPermanent.TopCard, jogressEvoRoots));
        }

        else if (isDigiXros)
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.digiXrosEffectObject.EvolutionEffectAnimation(fieldPermanentCard.ThisPermanent.TopCard));
        }

        else
        {
            if (fieldPermanentCard.ThisPermanent.TopCard.Level >= 6)
            {
                if (HasETB)
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.EvolutionEffectObject.EvolutionEffectAnimation(fieldPermanentCard.ThisPermanent.TopCard, message: "Mega Digimon"));
                }
            }
        }

        ContinuousController.instance.PlaySE(GManager.instance.PlayPokemonSE);

        fieldPermanentCard.gameObject.SetActive(true);

        fieldPermanentCard.ShowPermanentData(true);

        yield return null;

        bool end = false;

        float fallTime = 0.1f;

        fieldPermanentCard.transform.localPosition = new Vector3(fieldPermanentCard.transform.localPosition.x, fieldPermanentCard.transform.localPosition.y, -30);

        var sequence = DOTween.Sequence();

        sequence
            .Append(DOTween.To(() => fieldPermanentCard.transform.localPosition, (x) => fieldPermanentCard.transform.localPosition = x, new Vector3(fieldPermanentCard.transform.localPosition.x, fieldPermanentCard.transform.localPosition.y, 0), fallTime).SetEase(Ease.OutBounce))
            .AppendCallback(() => { end = true; });

        sequence.Play();

        while (Mathf.Abs(fieldPermanentCard.transform.localPosition.z - (-0.2f)) < 1)
        {
            yield return null;
        }

        //エフェクト生成
        GameObject effect = Instantiate(NewUnitEffect_OnLand, effectParent);
        effect.transform.position = new Vector3(fieldPermanentCard.transform.position.x, 0.05f, fieldPermanentCard.transform.position.z);
        StartCoroutine(DeleteCoroutine(effect, fieldPermanentCard));

        GameObject effect2 = null;

        if (fieldPermanentCard.ThisPermanent.TopCard != null)
        {
            switch (fieldPermanentCard.ThisPermanent.TopCard.BaseCardColorsFromEntity[0])
            {
                case CardColor.Green:
                    effect2 = Instantiate(GreenEvolutionEffect, effectParent);
                    break;

                case CardColor.Red:
                    effect2 = Instantiate(RedEvolutionEffect, effectParent);
                    break;

                case CardColor.Blue:
                    effect2 = Instantiate(BlueEvolutionEffect, effectParent);
                    break;

                case CardColor.Yellow:
                    effect2 = Instantiate(YellowEvolutionEffect, effectParent);
                    break;

                case CardColor.Purple:
                    effect2 = Instantiate(PurpleEvolutionEffect, effectParent);
                    break;

                case CardColor.Black:
                    effect2 = Instantiate(BlackEvolutionEffect, effectParent);
                    break;

                case CardColor.White:
                    effect2 = Instantiate(WhiteEvolutionEffect, effectParent);
                    break;

                case CardColor.None:
                    effect2 = Instantiate(WhiteEvolutionEffect, effectParent);
                    break;
            }

            effect2.transform.position = new Vector3(fieldPermanentCard.transform.position.x, fieldPermanentCard.transform.position.y, fieldPermanentCard.transform.position.z);
            effect2.transform.localScale = new Vector3(4, 1, 4);
            StartCoroutine(DeleteCoroutine(effect2, fieldPermanentCard));

            while (!end)
            {
                yield return null;
            }

            end = false;
        }

        yield return new WaitForSeconds(0.1f);
    }
    #endregion

    #region フィールドユニットカードに重ねてプレイする時のエフェクト
    [Header("進化SE")]
    public AudioClip EvolutionSE;

    [Header("究極体進化SE")]
    public AudioClip EvolutionSE_Ultimate;

    [Header("画面振動")]
    public CinemachineImpulseSource impulseSource;
    public IEnumerator DigivolveFieldPermanentCardEffect(FieldPermanentCard targetFieldPermanentCard, bool isBurst, bool isBlast, bool isAppFusion)
    {
        if (targetFieldPermanentCard != null)
        {
            targetFieldPermanentCard.Parent.SetActive(false);

            if (isBurst)
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.burstEffectObject.EvolutionEffectAnimation(targetFieldPermanentCard.ThisPermanent.TopCard));
            }
            else if (isAppFusion)
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.burstEffectObject.EvolutionEffectAnimation(targetFieldPermanentCard.ThisPermanent.TopCard));
            }
            else if (isBlast)
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.EvolutionEffectObject.EvolutionEffectAnimation(targetFieldPermanentCard.ThisPermanent.TopCard, message: "Blast Digivolution"));
            }

            else if (targetFieldPermanentCard.ThisPermanent.Level >= 6)
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.EvolutionEffectObject.EvolutionEffectAnimation(targetFieldPermanentCard.ThisPermanent.TopCard, message: "Digivolution"));
            }

            ContinuousController.instance.PlaySE(EvolutionSE);

            Permanent permanent = new Permanent(new List<CardSource>() { targetFieldPermanentCard.ThisPermanent.TopCard });

            FieldPermanentCard fieldPermanentCard = Instantiate(GManager.instance.fieldCardPrefab, targetFieldPermanentCard.transform.parent);
            fieldPermanentCard.anim.enabled = false;

            fieldPermanentCard.Parent.SetActive(false);

            fieldPermanentCard.transform.localRotation = targetFieldPermanentCard.transform.localRotation;

            if (fieldPermanentCard.Collider.GetComponent<UnityEngine.EventSystems.EventTrigger>() != null)
            {
                fieldPermanentCard.Collider.GetComponent<UnityEngine.EventSystems.EventTrigger>().enabled = false;
            }

            fieldPermanentCard.SetPermanentData(permanent, false);

            yield return null;

            fieldPermanentCard.Parent.SetActive(true);

            bool end = false;

            float fallTime = 0.1f;

            fieldPermanentCard.transform.localPosition = new Vector3(targetFieldPermanentCard.transform.localPosition.x, targetFieldPermanentCard.transform.localPosition.y, -30);

            var sequence = DOTween.Sequence();

            sequence
                .Append(DOTween.To(() => fieldPermanentCard.transform.localPosition, (x) => fieldPermanentCard.transform.localPosition = x, new Vector3(fieldPermanentCard.transform.localPosition.x, targetFieldPermanentCard.transform.localPosition.y, 0), fallTime).SetEase(Ease.OutBounce))
                .AppendCallback(() => { end = true; });

            sequence.Play();

            //エフェクト生成
            GameObject effect = Instantiate(NewUnitEffect_OnLand, effectParent);
            effect.transform.position = new Vector3(targetFieldPermanentCard.transform.position.x, 0.05f, targetFieldPermanentCard.transform.position.z);
            StartCoroutine(DeleteCoroutine(effect, fieldPermanentCard));

            GameObject effect2 = null;

            if (permanent.TopCard != null)
            {
                switch (permanent.TopCard.BaseCardColorsFromEntity[0])
                {
                    case CardColor.Green:
                        effect2 = Instantiate(GreenEvolutionEffect, effectParent);
                        break;

                    case CardColor.Red:
                        effect2 = Instantiate(RedEvolutionEffect, effectParent);
                        break;

                    case CardColor.Blue:
                        effect2 = Instantiate(BlueEvolutionEffect, effectParent);
                        break;

                    case CardColor.Yellow:
                        effect2 = Instantiate(YellowEvolutionEffect, effectParent);
                        break;

                    case CardColor.Purple:
                        effect2 = Instantiate(PurpleEvolutionEffect, effectParent);
                        break;

                    case CardColor.Black:
                        effect2 = Instantiate(BlackEvolutionEffect, effectParent);
                        break;

                    case CardColor.White:
                        effect2 = Instantiate(WhiteEvolutionEffect, effectParent);
                        break;

                    case CardColor.None:
                        effect2 = Instantiate(WhiteEvolutionEffect, effectParent);
                        break;
                }

                effect2.transform.position = new Vector3(targetFieldPermanentCard.transform.position.x, targetFieldPermanentCard.transform.position.y, targetFieldPermanentCard.transform.position.z);
                effect2.transform.localScale = new Vector3(4, 1, 4);
                StartCoroutine(DeleteCoroutine(effect2, fieldPermanentCard));

                while (!end)
                {
                    yield return null;
                }

                end = false;
            }

            yield return new WaitForSeconds(0.12f);

            targetFieldPermanentCard.ShowPermanentData(true);
            targetFieldPermanentCard.Parent.SetActive(true);

            Destroy(fieldPermanentCard.gameObject);
        }
    }
    #endregion

    #region パーマネントがバウンスされる時のエフェクト
    public IEnumerator BounceEffect(Permanent permanent, bool playSE = true)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (permanent.ShowingPermanentCard != null)
                {
                    permanent.ShowingPermanentCard.Parent.SetActive(false);

                    ContinuousController.instance.PlaySE(EvolutionSE);

                    Permanent newPpermanent = new Permanent(permanent.cardSources);

                    FieldPermanentCard fieldPermanentCard = Instantiate(GManager.instance.fieldCardPrefab, permanent.ShowingPermanentCard.transform.parent);
                    fieldPermanentCard.anim.enabled = false;

                    fieldPermanentCard.Parent.SetActive(false);

                    fieldPermanentCard.transform.localRotation = permanent.ShowingPermanentCard.transform.localRotation;

                    if (fieldPermanentCard.Collider.GetComponent<UnityEngine.EventSystems.EventTrigger>() != null)
                    {
                        fieldPermanentCard.Collider.GetComponent<UnityEngine.EventSystems.EventTrigger>().enabled = false;
                    }

                    fieldPermanentCard.SetPermanentData(newPpermanent, false);

                    yield return null;

                    fieldPermanentCard.Parent.SetActive(true);

                    fieldPermanentCard.transform.localPosition = new Vector3(permanent.ShowingPermanentCard.transform.localPosition.x, permanent.ShowingPermanentCard.transform.localPosition.y, 0);

                    Vector3 targetPos = new Vector3();
                    float targeScale = fieldPermanentCard.transform.localScale.x;

                    if (permanent.TopCard.Owner.isYou)
                    {
                        targetPos = new Vector3(0, -620, 0);
                        targeScale = fieldPermanentCard.transform.localScale.x * 1.1f;
                    }

                    else
                    {
                        targetPos = new Vector3(0, 630, 0);
                        targeScale = fieldPermanentCard.transform.localScale.x * 0.25f;
                    }

                    bool end = false;

                    float animTime = 0.25f;

                    if (playSE)
                    {
                        ContinuousController.instance.PlaySE(DebuffSE);
                    }

                    var sequence = DOTween.Sequence();

                    sequence
                        .Append(fieldPermanentCard.transform.DOLocalMove(targetPos, animTime))
                        .Join(fieldPermanentCard.transform.DOScale(targeScale, animTime))
                        .AppendCallback(() => { end = true; });

                    sequence.Play();

                    yield return new WaitWhile(() => !end);

                    Destroy(fieldPermanentCard.gameObject);

                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
    }
    #endregion

    #region パーマネントがデッキバウンスされる時のエフェクト
    public IEnumerator DeckBounceEffect(Permanent permanent)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (permanent.ShowingPermanentCard != null)
                {
                    permanent.ShowingPermanentCard.Parent.SetActive(false);

                    ContinuousController.instance.PlaySE(EvolutionSE);

                    Permanent newPpermanent = new Permanent(permanent.cardSources);

                    FieldPermanentCard fieldPermanentCard = Instantiate(GManager.instance.fieldCardPrefab, permanent.ShowingPermanentCard.transform.parent);
                    fieldPermanentCard.anim.enabled = false;
                    fieldPermanentCard.skipUpdate = true;
                    fieldPermanentCard.Parent.SetActive(false);

                    fieldPermanentCard.transform.localRotation = Quaternion.Euler(0, 0, 0);

                    if (fieldPermanentCard.Collider.GetComponent<UnityEngine.EventSystems.EventTrigger>() != null)
                    {
                        fieldPermanentCard.Collider.GetComponent<UnityEngine.EventSystems.EventTrigger>().enabled = false;
                    }

                    fieldPermanentCard.SetPermanentData(newPpermanent, false);

                    yield return null;

                    fieldPermanentCard.Parent.SetActive(true);

                    fieldPermanentCard.transform.localPosition = new Vector3(permanent.ShowingPermanentCard.transform.localPosition.x, permanent.ShowingPermanentCard.transform.localPosition.y, 0);

                    Vector3 targetPos = new Vector3();
                    float targeScale = fieldPermanentCard.transform.localScale.x;

                    if (permanent.TopCard.Owner.isYou)
                    {
                        targetPos = new Vector3(823, -190, 0);
                        //targeScale = fieldPermanentCard.transform.localScale.x * 1.1f;
                    }

                    else
                    {
                        targetPos = new Vector3(-823, 180, 0);
                        //targeScale = fieldPermanentCard.transform.localScale.x * 0.25f;
                    }

                    bool end = false;

                    float animTime = 0.25f;

                    ContinuousController.instance.PlaySE(DebuffSE);

                    var sequence = DOTween.Sequence();

                    sequence
                        .Append(fieldPermanentCard.transform.DOLocalMove(targetPos, animTime))
                        .Join(fieldPermanentCard.transform.DOScale(targeScale, animTime))
                        .AppendCallback(() => { end = true; });

                    sequence.Play();

                    yield return new WaitWhile(() => !end);
                    end = false;

                    for (int i = 0; i < fieldPermanentCard.Parent.transform.childCount; i++)
                    {
                        fieldPermanentCard.Parent.transform.GetChild(i).gameObject.SetActive(false);
                    }

                    fieldPermanentCard.CardImage.gameObject.SetActive(true);

                    animTime = 0.16f;

                    sequence = DOTween.Sequence();

                    sequence
                        .Append(DOTween.To(() => fieldPermanentCard.CardImage.color, (x) => fieldPermanentCard.CardImage.color = x, new Color(1, 1, 1, 0), animTime))
                        .AppendCallback(() => { end = true; });

                    sequence.Play();

                    yield return new WaitWhile(() => !end);
                    end = false;

                    Destroy(fieldPermanentCard.gameObject);
                }
            }
        }
    }
    #endregion

    #region カードを公開する
    [Header("カード公開親")]
    public Transform ShowCardParent;

    [Header("カード公開テキスト")]
    public TextMeshProUGUI ShowCardTitleText;
    public bool canCloseByClick { get; set; } = true;
    public IEnumerator ShowCardEffect(List<CardSource> ShownCards, string Title, bool willHide, bool ShowReverseCard)
    {
        if (hideShowCard != null)
        {
            StopCoroutine(hideShowCard);
        }

        OffShowCard();

        ShowCardTitleText.text = Title;

        for (int i = 0; i < ShowCardParent.childCount; i++)
        {
            Destroy(ShowCardParent.GetChild(i).gameObject);
        }

        if (ShowCardParent.transform.parent.gameObject.activeSelf)
        {
            yield return new WaitWhile(() => ShowCardParent.childCount >= 1);
        }

        yield return new WaitForSeconds(Time.deltaTime * 2);

        if (ShownCards.Count == 0)
        {
            yield break;
        }

        canCloseByClick = true;

        foreach (CardSource cardSource in ShownCards)
        {
            HandCard handCard = Instantiate(GManager.instance.handCardPrefab, ShowCardParent);
            yield return new WaitUntil(() => handCard != null);
            handCard.gameObject.name = $"showCard_{cardSource.Owner.PlayerName}";
            handCard.notHideSelectedIndexText = true;
            handCard.SetUpHandCard(cardSource);

            if (ShowReverseCard)
            {
                handCard.SetUpHandCardImage();

                if (ShownCards.Count >= 2)
                {
                    handCard.SetSelectedIndexText(ShownCards.IndexOf(cardSource) + 1);
                }
            }

            else
            {
                if (!cardSource.IsFlipped)
                {
                    handCard.SetUpHandCardImage();

                    if (ShownCards.Count >= 2)
                    {
                        handCard.SetSelectedIndexText(ShownCards.IndexOf(cardSource) + 1);
                    }
                }

                else
                {
                    handCard.SetUpReverseCard();
                    handCard.ShowOpponent = false;
                    handCard.OffSelectedIndexText();
                }
            }
        }



        ShowCardParent.transform.parent.parent.gameObject.SetActive(true);
        ShowCardParent.transform.parent.gameObject.SetActive(true);
        ShowCardParent.transform.parent.GetComponent<Animator>().SetInteger("Close", 0);

        if (willHide)
        {
            hideShowCard = StartCoroutine(HideShowCard());
        }
    }

    Coroutine hideShowCard = null;

    public IEnumerator HideShowCard()
    {
        yield return new WaitForSeconds(2.5f);

        float timer = 0f;

        while (true)
        {
            if (!ShowCardParent.transform.parent.gameObject.activeSelf)
            {
                yield break;
            }

            if (timer >= 2.5f)
            {
                break;
            }

            timer += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        OffShowCard();
    }

    public void OffShowCard()
    {
        ShowCardParent.transform.parent.gameObject.SetActive(false);

        for (int i = 0; i < ShowCardParent.childCount; i++)
        {
            Destroy(ShowCardParent.GetChild(i).gameObject);
        }
    }

    public void OnClickShowCardBackground1()
    {
        if (canCloseByClick)
        {
            OffShowCard();
        }
    }
    #endregion

    #region カードを公開する2
    [Header("カード公開親2")]
    public Transform ShowCardParent2;

    [Header("カード公開テキスト2")]
    public TextMeshProUGUI ShowCardTitleText2;
    public IEnumerator ShowCardEffect2(List<CardSource> ShownCards, string Title, bool willHide, bool ShowReverseCard)
    {
        if (hideShowCard2 != null)
        {
            StopCoroutine(hideShowCard2);
        }

        OffShowCard2();

        ShowCardTitleText2.text = Title;

        for (int i = 0; i < ShowCardParent2.childCount; i++)
        {
            Destroy(ShowCardParent2.GetChild(i).gameObject);
        }

        yield return new WaitForSeconds(Time.deltaTime * 2);

        if (ShownCards.Count == 0)
        {
            yield break;
        }

        foreach (CardSource cardSource in ShownCards)
        {
            HandCard handCard = Instantiate(GManager.instance.handCardPrefab, ShowCardParent2);
            yield return new WaitUntil(() => handCard != null);
            handCard.gameObject.name = $"showCard2_{cardSource.Owner.PlayerName}";
            handCard.notHideSelectedIndexText = true;
            handCard.SetUpHandCard(cardSource);

            if (ShowReverseCard)
            {
                handCard.SetUpHandCardImage();

                if (ShownCards.Count >= 2)
                {
                    handCard.SetSelectedIndexText(ShownCards.IndexOf(cardSource) + 1);
                }
            }

            else
            {
                if (!cardSource.IsFlipped)
                {
                    handCard.SetUpHandCardImage();

                    if (ShownCards.Count >= 2)
                    {
                        handCard.SetSelectedIndexText(ShownCards.IndexOf(cardSource) + 1);
                    }
                }

                else
                {
                    handCard.SetUpReverseCard();
                    handCard.ShowOpponent = false;
                    handCard.OffSelectedIndexText();
                }
            }
        }

        ShowCardParent2.transform.parent.parent.gameObject.SetActive(true);
        ShowCardParent2.transform.parent.gameObject.SetActive(true);
        ShowCardParent2.transform.parent.GetComponent<Animator>().SetInteger("Close", 0);

        if (willHide)
        {
            hideShowCard2 = StartCoroutine(HideShowCard2());
        }
    }

    Coroutine hideShowCard2 = null;

    public IEnumerator HideShowCard2()
    {
        yield return new WaitForSeconds(2.5f);

        float timer = 0f;

        while (true)
        {
            if (!ShowCardParent2.transform.parent.gameObject.activeSelf)
            {
                yield break;
            }

            if (timer >= 2.5f)
            {
                break;
            }

            timer += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        OffShowCard2();
    }

    public void OffShowCard2()
    {
        ShowCardParent2.transform.parent.gameObject.SetActive(false);

        for (int i = 0; i < ShowCardParent2.childCount; i++)
        {
            Destroy(ShowCardParent2.GetChild(i).gameObject);
        }
    }

    public void OnClickShowCardBackground2()
    {
        if (canCloseByClick)
        {
            OffShowCard2();
        }
    }
    #endregion

    #region カードをドローした時のエフェクト
    [Header("中央のカード表示エフェクト")]
    public GameObject ShowUseHandCardEffectPrefab;

    [Header("使用カード拡大表示HandCard2")]
    public HandCard ShowUseHandCard;

    [Header("使用カード拡大表示HandCard親")]
    public Transform ShowUseHandCardParent;
    public IEnumerator AddHandCardEffect(CardSource cardSource)
    {
        ContinuousController.instance.PlaySE(GManager.instance.DrawSE);

        Player player = cardSource.Owner;

        bool end = false;

        var sequence = DOTween.Sequence();
        var sequence2 = DOTween.Sequence();

        ShowUseHandCardParent.parent.gameObject.SetActive(true);
        ShowUseHandCardParent.gameObject.SetActive(true);
        ShowUseHandCard.gameObject.SetActive(true);
        ShowUseHandCard.transform.SetParent(ShowUseHandCardParent);

        ShowUseHandCard.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        ShowUseHandCard.RemoveSelectEffect();

        Vector3 targetPositon = new Vector3(150, 0, 0);

        if (cardSource.Owner.isYou)
        {
            targetPositon -= new Vector3(0, 30, 0);
            ShowUseHandCard.transform.localPosition = new Vector3(210, 10, 0);
            ShowUseHandCard.SetUpHandCard(cardSource);
            ShowUseHandCard.SetUpHandCardImage();
            ShowUseHandCard.CardImage.color = new Color(1, 1, 1, 1);
        }

        else
        {
            targetPositon += new Vector3(0, 30, 0);
            ShowUseHandCard.transform.localPosition = new Vector3(210, 30, 0);
            ShowUseHandCard.SetUpReverseCard();
        }

        ShowUseHandCard.CardImage.color = new Color(0, 0, 0, 1);
        ShowUseHandCard.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -60));

        ShowUseHandCard.CardImage.transform.localRotation = Quaternion.Euler(0, 0, 0);
        ShowUseHandCard.Outline_Select.transform.localRotation = Quaternion.Euler(0, 0, 0);

        float time = 0.06f;

        sequence = DOTween.Sequence();

        sequence
            .Append(ShowUseHandCard.transform.DOLocalMove(targetPositon, time))
            .Join(ShowUseHandCard.transform.DOScale(new Vector3(0.45f, 0.45f, 0.45f), time))
            .Join(ShowUseHandCard.transform.DOLocalRotate(Vector3.zero, time))
            .AppendCallback(() => { end = true; });

        sequence.Play();

        yield return new WaitWhile(() => !end);
        end = false;

        if (player.isYou)
        {
            ShowUseHandCard.SetUpHandCard(cardSource);
            ShowUseHandCard.SetUpHandCardImage();
            ShowUseHandCard.CardImage.color = new Color(0, 0, 0, 1);

            #region エフェクト生成
            GameObject effect = Instantiate(ShowUseHandCardEffectPrefab, ShowUseHandCardParent);
            effect.transform.localScale *= 0.5f;
            effect.transform.localPosition = ShowUseHandCard.transform.localPosition;
            effect.transform.SetSiblingIndex(0);
            StartCoroutine(DeleteCoroutine(effect, null));

            GameObject effect2 = Instantiate(ShowUseHandCardEffectPrefab, ShowUseHandCardParent);
            effect2.transform.localScale *= 0.5f;
            effect2.transform.localPosition = ShowUseHandCard.transform.localPosition;
            effect2.transform.SetSiblingIndex(0);
            effect2.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));
            StartCoroutine(DeleteCoroutine(effect2, null));
            #endregion

            yield return new WaitForSeconds(0.04f);
        }

        yield return new WaitForSeconds(0.05f);

        //縮小して上に上がる
        StartCoroutine(ShrinkUpUseHandCard(ShowUseHandCard));

        yield return new WaitForSeconds(0.06f);

    }
    #endregion

    #region 表示したカードが縮小して上に上がる
    public IEnumerator ShrinkUpUseHandCard(HandCard handCard)
    {
        ShowUseHandCard.OffDP();

        ContinuousController.instance.PlaySE(GManager.instance.DeleteHandSE);

        bool end = false;

        var sequence = DOTween.Sequence();

        handCard.gameObject.SetActive(true);

        #region 縮小
        float shrinkTime2 = 0.07f;

        sequence = DOTween.Sequence();

        sequence
            .Append(handCard.transform.DOScaleX(0.06f, shrinkTime2))
            .Join(handCard.transform.DOScaleY(1.4f, shrinkTime2))
            .Join(DOTween.To(() => handCard.CardImage.color, (x) => handCard.CardImage.color = x, new Color32(205, 205, 205, 255), shrinkTime2))
            .AppendCallback(() => { end = true; });

        sequence.Play();

        yield return new WaitForSeconds(shrinkTime2 / 2);

        handCard.CardImage.sprite = null;

        while (!end)
        {
            yield return null;
        }

        end = false;
        #endregion

        #region 上に上がる
        float upTime = 0.07f;

        handCard.transform.DOLocalMoveY(220, upTime);

        yield return new WaitForSeconds(upTime);

        #endregion

        handCard.gameObject.SetActive(false);
    }
    #endregion

    #region プレイするカードを中央に表示
    public IEnumerator ShowUseHandCardEffect_PlayCard(CardSource card)
    {
        if (card.Owner.HandTransform.GetComponent<HandContoller>() != null)
        {
            card.Owner.HandTransform.GetComponent<HandContoller>().isDragging = true;
        }

        yield return StartCoroutine(ShowUseHandCardEffect(card));

        if (card.Owner.HandTransform.GetComponent<HandContoller>() != null)
        {
            card.Owner.HandTransform.GetComponent<HandContoller>().isDragging = false;
        }
    }
    #endregion

    #region プレイするカードを中央に表示
    public IEnumerator ShowUseHandCardEffect(CardSource card)
    {
        ContinuousController.instance.PlaySE(GManager.instance.ShowPlayCardSE);

        bool end = false;

        var sequence = DOTween.Sequence();

        ShowUseHandCard.OffDP();

        #region エフェクト生成
        GameObject effect = Instantiate(ShowUseHandCardEffectPrefab, ShowUseHandCardParent);
        effect.transform.SetSiblingIndex(0);
        StartCoroutine(DeleteCoroutine(effect, null));

        GameObject effect2 = Instantiate(ShowUseHandCardEffectPrefab, ShowUseHandCardParent);
        effect2.transform.SetSiblingIndex(0);
        effect2.transform.localRotation = Quaternion.EulerAngles(new Vector3(0, 90, 0));
        StartCoroutine(DeleteCoroutine(effect2, null));
        #endregion

        #region 白いカードが回転
        ShowUseHandCard.gameObject.SetActive(true);
        ShowUseHandCard.Outline_Select.SetActive(false);
        ShowUseHandCard.transform.SetParent(ShowUseHandCardParent);
        ShowUseHandCard.transform.parent.gameObject.SetActive(true);

        ShowUseHandCard.transform.localPosition = Vector3.zero;
        ShowUseHandCard.transform.localScale = new Vector3(1, 1, 1);

        ShowUseHandCard.CardImage.sprite = null;

        ShowUseHandCard.CardImage.color = new Color32(255, 255, 255, 140);

        Quaternion startRotate = Quaternion.Euler(0, 35, 0);
        Vector3 targetRotate = new Vector3(0, 0, 0);

        ShowUseHandCard.CardImage.transform.localRotation = startRotate;

        float rotateTime = 0.1f;

        sequence = DOTween.Sequence();

        sequence
            .Append(ShowUseHandCard.CardImage.transform.DOLocalRotate(targetRotate, rotateTime, RotateMode.FastBeyond360))
            .Join(DOTween.To(() => ShowUseHandCard.CardImage.color, (x) => ShowUseHandCard.CardImage.color = x, new Color(1, 1, 1, 0), rotateTime))
            .AppendCallback(() => { end = true; });

        sequence.Play();

        while (!end)
        {
            yield return null;
        }

        end = false;
        #endregion

        #region カードを表示
        ShowUseHandCard.CardImage.color = new Color(0, 0, 0, 0);

        ShowUseHandCard.SetUpHandCard(card);
        ShowUseHandCard.SetUpHandCardImage();
        ShowUseHandCard.CardImage.color = new Color(1, 1, 1, 1);

        ShowUseHandCard.SetOutlineColor(DataBase.CardColor_ColorLightDictionary[card.BaseCardColorsFromEntity[0]]);
        ShowUseHandCard.Outline_Select.transform.localRotation = ShowUseHandCard.CardImage.transform.localRotation;
        ShowUseHandCard.Outline_Select.SetActive(true);

        float showCardTime = 0.16f;

        sequence = DOTween.Sequence();

        sequence
            .Append(DOTween.To(() => ShowUseHandCard.CardImage.color, (x) => ShowUseHandCard.CardImage.color = x, new Color(0, 0, 0, 1), showCardTime))
            .AppendCallback(() => { end = true; });

        sequence.Play();

        while (!end)
        {
            yield return null;
        }

        end = false;
        #endregion

        yield return new WaitForSeconds(0.16f);
    }
    #endregion

    #region バフエフェクト
    [Header("バフエフェクト")]
    [SerializeField] GameObject BuffEffect;

    [Header("バフSE")]
    public AudioClip BuffSE;

    public IEnumerator CreateBuffEffect(Permanent permanent)
    {
        if (permanent != null)
        {
            if (permanent.ShowingPermanentCard != null)
            {
                if (BuffEffect != null)
                {
                    if (permanent.ShowingPermanentCard.IsEffectPlaying)
                    {
                        yield break;
                    }

                    permanent.ShowingPermanentCard.IsEffectPlaying = true;

                    permanent.ShowingPermanentCard.ShowPermanentData(true);

                    Vector3 position = new Vector3(permanent.ShowingPermanentCard.transform.position.x, permanent.ShowingPermanentCard.transform.position.y, permanent.ShowingPermanentCard.transform.position.z);

                    GameObject effect = Instantiate(BuffEffect, effectParent);

                    effect.transform.position = position;

                    StartCoroutine(DeleteCoroutine(effect, permanent.ShowingPermanentCard));

                    ContinuousController.instance.PlaySE(BuffSE);

                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
    }
    #endregion

    #region デバフエフェクト
    [Header("デバフエフェクト")]
    [SerializeField] GameObject DebuffEffect;

    [Header("デバフSE")]
    public AudioClip DebuffSE;

    public IEnumerator CreateDebuffEffect(Permanent permanent)
    {
        if (permanent != null)
        {
            if (permanent.ShowingPermanentCard != null)
            {
                if (DebuffEffect != null)
                {
                    if (permanent.ShowingPermanentCard.IsEffectPlaying)
                    {
                        yield break;
                    }

                    permanent.ShowingPermanentCard.IsEffectPlaying = true;

                    permanent.ShowingPermanentCard.ShowPermanentData(true);

                    Vector3 position = new Vector3(permanent.ShowingPermanentCard.transform.position.x, permanent.ShowingPermanentCard.transform.position.y, permanent.ShowingPermanentCard.transform.position.z);

                    GameObject effect = Instantiate(DebuffEffect, effectParent);

                    effect.transform.position = position;

                    StartCoroutine(DeleteCoroutine(effect, permanent.ShowingPermanentCard));

                    ContinuousController.instance.PlaySE(DebuffSE);

                    if (permanent.DP <= 0)
                    {
                        yield return new WaitForSeconds(0.4f);
                    }

                    else
                    {
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }
    }
    #endregion

    #region デジクロス進化元付与エフェクト
    [Header("デジクロス進化元エフェクト")]
    [SerializeField] GameObject DigiXrosSelectCardEffect;

    [Header("デジクロス進化元SE")]
    public AudioClip DigiXrosSelectCardEffectSE;

    public IEnumerator CreateDigiXrosSelectCardEffect(Permanent permanent, Player player = null)
    {
        if (DigiXrosSelectCardEffect != null)
        {
            Vector3 position = new Vector3();

            if (permanent != null)
            {
                if (permanent.ShowingPermanentCard != null)
                {
                    position = new Vector3(permanent.ShowingPermanentCard.transform.position.x, permanent.ShowingPermanentCard.transform.position.y, permanent.ShowingPermanentCard.transform.position.z);
                }
            }

            else if (player != null)
            {
                position = new Vector3(player.TrashCardImage.transform.position.x, player.TrashCardImage.transform.position.y, player.TrashCardImage.transform.position.z);
            }

            GameObject effect = Instantiate(DigiXrosSelectCardEffect, effectParent);

            effect.transform.position = position;

            StartCoroutine(DeleteCoroutine(effect, permanent.ShowingPermanentCard));

            ContinuousController.instance.PlaySE(DigiXrosSelectCardEffectSE);
        }

        yield return new WaitForSeconds(0.3f);
    }
    #endregion

    #region Assembly Effect
    [Header("Assembly Select Effect")]
    [SerializeField] GameObject AssemblySelectCardEffect;

    [Header("Assembly Select Audio")]
    public AudioClip AssemblySelectCardEffectSE;

    public IEnumerator CreateAssemblySelectCardEffect(Permanent permanent, Player player = null)
    {
        if (AssemblySelectCardEffect != null)
        {
            Vector3 position = new Vector3();

            if (permanent != null)
            {
                if (permanent.ShowingPermanentCard != null)
                {
                    position = new Vector3(permanent.ShowingPermanentCard.transform.position.x, permanent.ShowingPermanentCard.transform.position.y, permanent.ShowingPermanentCard.transform.position.z);
                }
            }

            else if (player != null)
            {
                position = new Vector3(player.TrashCardImage.transform.position.x, player.TrashCardImage.transform.position.y, player.TrashCardImage.transform.position.z);
            }

            GameObject effect = Instantiate(AssemblySelectCardEffect, effectParent);

            effect.transform.position = position;

            StartCoroutine(DeleteCoroutine(effect, permanent.ShowingPermanentCard));

            ContinuousController.instance.PlaySE(AssemblySelectCardEffectSE);
        }

        yield return new WaitForSeconds(0.3f);
    }
    #endregion

    #region 攻撃・ブロック不可付与エフェクト
    [Header("凍結SE")]
    [SerializeField] AudioClip FreezeSE;

    public IEnumerator FreezePermanentEffect(Permanent permanent)
    {
        if (permanent != null)
        {
            if (permanent.ShowingPermanentCard != null)
            {
                bool end = false;
                float animTime = 0.2f;

                ContinuousController.instance.PlaySE(FreezeSE);

                Sequence sequence = DOTween.Sequence();

                sequence
                    .Append(permanent.ShowingPermanentCard.transform.DOShakePosition(animTime, strength: 8f, vibrato: 30, fadeOut: true))
                    .AppendCallback(() => end = true);

                sequence.Play();

                yield return new WaitWhile(() => !end);
                end = false;

                sequence.Kill();
            }
        }
    }
    #endregion

    #region リカバリーフェクト
    [Header("ライフ回復エフェクト")]
    [SerializeField] GameObject HealLifeEffect;

    [Header("ライフ回復SE")]
    [SerializeField] AudioClip HealLifeSE;

    public IEnumerator CreateRecoveryEffect(Player player)
    {
        if (BuffEffect != null)
        {
            Vector3 position = new Vector3(player.securityObject.transform.position.x, player.securityObject.transform.position.y, player.securityObject.transform.position.z);

            GameObject effect = Instantiate(HealLifeEffect, effectParent);

            effect.transform.position = position;

            StartCoroutine(DeleteCoroutine(effect, null));

            ContinuousController.instance.PlaySE(HealLifeSE);

            yield return null;
        }
    }
    #endregion

    #region セキュリティチェックのエフェクト
    public IEnumerator BreakSecurityEffect(Player player)
    {
        ContinuousController.instance.StartCoroutine(player.securityObject.securityBreakGlass.BreakIenumerator());

        yield return new WaitForSeconds(0.06f);

        GameObject effect2 = Instantiate(BlueEvolutionEffect, effectParent);
        effect2.transform.position = new Vector3(player.securityObject.securityBreakGlass.transform.position.x, player.securityObject.securityBreakGlass.transform.position.y, player.securityObject.securityBreakGlass.transform.position.z);

        if (!player.isYou)
        {
            effect2.transform.position -= new Vector3(0, 0, 30);
        }

        else
        {
            effect2.transform.position += new Vector3(0, 0, 15);
        }

        effect2.transform.localScale = new Vector3(5, 1, 5);
        StartCoroutine(DeleteCoroutine(effect2, null));

        yield return new WaitForSeconds(0.17f);
    }
    #endregion

    #region パーマネントが破壊されるときのエフェクト
    [Header("割れるガラス")]
    [SerializeField] BreakGlass permanetBreakGlassPrefab;

    [Header("パーマネント消滅用マテリアル")]
    [SerializeField] Material destroyPermanentMaterial;

    [Header("ガラス描画用パーマネントカード")]
    [SerializeField] FieldPermanentCard renderingFiedlPermanentCard;
    public IEnumerator DestroyPermanentEffect(Permanent permanent)
    {
        if (permanent == null)
        {
            yield break;
        }

        if (permanent.ShowingPermanentCard == null)
        {
            yield break;
        }

        FieldPermanentCard fieldPermanentCard = permanent.ShowingPermanentCard;

        //エフェクト用の割れるガラスを生成
        BreakGlass permanetBreakGlass = Instantiate(permanetBreakGlassPrefab, fieldPermanentCard.transform.parent);

        //場所を指定
        permanetBreakGlass.transform.localPosition = fieldPermanentCard.transform.localPosition;
        permanetBreakGlass.transform.localRotation = Quaternion.Euler(
            fieldPermanentCard.transform.localRotation.eulerAngles.x + 180,
            fieldPermanentCard.transform.localRotation.eulerAngles.y,
            fieldPermanentCard.transform.localRotation.eulerAngles.z * -1);
        permanetBreakGlass.transform.localScale = new Vector3(54, 54, 1);

        //ガラスを初期化
        permanetBreakGlass.Init(destroyPermanentMaterial);

        //レンダリング用のパーマネントカードを設定
        renderingFiedlPermanentCard.gameObject.SetActive(true);
        renderingFiedlPermanentCard.SetPermanentData(fieldPermanentCard.ThisPermanent, true);

        //元パーマネントオブジェクトを非表示
        fieldPermanentCard.Parent.SetActive(false);

        GameObject effect2 = null;

        if (permanent.TopCard != null)
        {
            switch (permanent.TopCard.BaseCardColorsFromEntity[0])
            {
                case CardColor.Green:
                    effect2 = Instantiate(GreenEvolutionEffect, effectParent);
                    break;

                case CardColor.Red:
                    effect2 = Instantiate(RedEvolutionEffect, effectParent);
                    break;

                case CardColor.Blue:
                    effect2 = Instantiate(BlueEvolutionEffect, effectParent);
                    break;

                case CardColor.Yellow:
                    effect2 = Instantiate(YellowEvolutionEffect, effectParent);
                    break;

                case CardColor.Purple:
                    effect2 = Instantiate(PurpleEvolutionEffect, effectParent);
                    break;

                case CardColor.Black:
                    effect2 = Instantiate(BlackEvolutionEffect, effectParent);
                    break;

                case CardColor.White:
                    effect2 = Instantiate(WhiteEvolutionEffect, effectParent);
                    break;

                case CardColor.None:
                    effect2 = Instantiate(WhiteEvolutionEffect, effectParent);
                    break;
            }

            effect2.transform.position = new Vector3(permanent.ShowingPermanentCard.transform.position.x, permanent.ShowingPermanentCard.transform.position.y, permanent.ShowingPermanentCard.transform.position.z);
            effect2.transform.localScale = new Vector3(4, 1, 4);
            StartCoroutine(DeleteCoroutine(effect2, null));
        }

        //爆発させる
        yield return ContinuousController.instance.StartCoroutine(permanetBreakGlass.BreakIenumerator());

        //エフェクトを削除
        DestroyImmediate(permanetBreakGlass.gameObject);

        renderingFiedlPermanentCard.gameObject.SetActive(false);
    }
    #endregion

    #region セキュリティカードが出てくるエフェクト
    [Header("セキュリティカードが出てくるアニメーション")]
    [SerializeField] RuntimeAnimatorController EnterSecurity;
    public IEnumerator EnterSecurityCardEffect(CardSource card)
    {
        ContinuousController.instance.PlaySE(GManager.instance.ShowPlayCardSE);

        bool end = false;

        var sequence = DOTween.Sequence();

        #region 白いカードが回転
        ShowUseHandCard.gameObject.SetActive(true);
        ShowUseHandCard.Outline_Select.SetActive(false);
        ShowUseHandCard.transform.SetParent(ShowUseHandCardParent);
        ShowUseHandCard.transform.parent.gameObject.SetActive(true);

        ShowUseHandCard.transform.localPosition = Vector3.zero;
        ShowUseHandCard.transform.localScale = new Vector3(0.05f, 0.05f, 1);

        ShowUseHandCard.CardImage.sprite = null;
        #endregion

        #region カードを表示
        ShowUseHandCard.SetUpHandCard(card);
        ShowUseHandCard.SetUpHandCardImage();
        ShowUseHandCard.CardImage.color = new Color(0, 0, 0, 1);

        ShowUseHandCard.SetOutlineColor(DataBase.CardColor_ColorLightDictionary[card.BaseCardColorsFromEntity[0]]);
        ShowUseHandCard.Outline_Select.transform.localRotation = ShowUseHandCard.CardImage.transform.localRotation;
        ShowUseHandCard.Outline_Select.SetActive(true);

        ShowUseHandCard.ShowDP();

        float animTime = 0.2f;

        Animator animator = ShowUseHandCard.gameObject.GetComponent<Animator>();

        if (animator == null)
        {
            animator = ShowUseHandCard.gameObject.AddComponent<Animator>();
        }

        animator.runtimeAnimatorController = EnterSecurity;

        if (card.Owner.isYou)
        {
            animator.SetInteger("You", 1);
        }

        else
        {
            animator.SetInteger("You", -1);
        }

        animator.enabled = true;

        yield return new WaitForSeconds(animTime);

        Vector3 position = ShowUseHandCard.transform.localPosition;
        Vector3 scale = ShowUseHandCard.transform.localScale;

        animator.enabled = false;

        ShowUseHandCard.transform.localPosition = position;
        ShowUseHandCard.transform.localScale = scale;

        #region エフェクト生成
        GameObject effect = Instantiate(ShowUseHandCardEffectPrefab, ShowUseHandCardParent);
        effect.transform.SetSiblingIndex(0);
        StartCoroutine(DeleteCoroutine(effect, null));

        GameObject effect2 = Instantiate(ShowUseHandCardEffectPrefab, ShowUseHandCardParent);
        effect2.transform.SetSiblingIndex(0);
        effect2.transform.localRotation = Quaternion.EulerAngles(new Vector3(0, 90, 0));
        StartCoroutine(DeleteCoroutine(effect2, null));
        #endregion

        #endregion

        yield return new WaitForSeconds(0.17f);
    }
    #endregion

    #region セキュリティカードが処理領域に行くエフェクト
    public IEnumerator MoveToExecuteCardEffect_SetPosition(CardSource card, Vector3 startPos)
    {
        ShowUseHandCard.gameObject.SetActive(true);
        ShowUseHandCard.transform.position = startPos;
        ShowUseHandCard.SetUpHandCard(card);
        ShowUseHandCard.SetUpHandCardImage();
        ShowUseHandCard.CardImage.color = new Color(0, 0, 0, 1);
        ShowUseHandCard.Outline_Select.SetActive(true);
        ShowUseHandCard.SetOrangeOutline();

        yield return ContinuousController.instance.StartCoroutine(MoveToExecuteCardEffect(card));
    }

    public IEnumerator MoveToExecuteCardEffect(CardSource card)
    {
        if (ShowUseHandCard.gameObject.activeSelf)
        {
            if (ShowUseHandCard.cardSource == card)
            {
                bool end = false;

                var sequence = DOTween.Sequence();

                float animTime = 0.12f;

                sequence = DOTween.Sequence();

                Vector3 targetPos = Vector3.zero;

                if (card.Owner.isYou)
                {
                    targetPos = new Vector3(-166, -20, 0);
                }

                else
                {
                    targetPos = new Vector3(161, 50.2f, 0);
                }

                if (card.Owner.isYou)
                {
                    ShowUseHandCard.transform.localScale = new Vector3(0.3f, 0.3f, 1);
                }

                else
                {
                    ShowUseHandCard.transform.localScale = new Vector3(0.225f, 0.225f, 1);
                }

                ShowUseHandCard.transform.localRotation = Quaternion.Euler(27.7f, 0, 0);

                sequence
                    .Append(ShowUseHandCard.transform.DOLocalMove(targetPos, animTime))
                    .AppendCallback(() => { end = true; });

                sequence.Play();

                while (!end)
                {
                    yield return null;
                }

                ShowUseHandCard.gameObject.SetActive(false);
                ShowUseHandCard.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }

        yield return ContinuousController.instance.StartCoroutine(card.Owner.brainStormObject.BrainStormCoroutine(card));

        ShowUseHandCard.OffDP();
    }
    #endregion

    #region セキュリティカードが破棄されるエフェクト
    [Header("カード消滅用マテリアル")]
    [SerializeField] Material destroyCardSourceMaterial;

    [Header("ガラス描画用ハンドカード")]
    [SerializeField] HandCard renderingHandCard;
    public IEnumerator DestroySecurityEffect(CardSource destroyedSecurityCard)
    {
        if (destroyedSecurityCard == null)
        {
            yield break;
        }

        if (!ShowUseHandCard.gameObject.activeSelf || ShowUseHandCard.cardSource != destroyedSecurityCard)
        {
            yield break;
        }

        //エフェクト用の割れるガラスを生成
        BreakGlass permanetBreakGlass = Instantiate(permanetBreakGlassPrefab, ShowUseHandCard.transform.parent);

        //場所を指定
        permanetBreakGlass.transform.localPosition = ShowUseHandCard.transform.localPosition;
        permanetBreakGlass.transform.localRotation = Quaternion.Euler(ShowUseHandCard.transform.localRotation.eulerAngles.x + 180, ShowUseHandCard.transform.localRotation.eulerAngles.y, ShowUseHandCard.transform.localRotation.eulerAngles.z * -1);
        permanetBreakGlass.transform.localScale = new Vector3(30.1f, 30.1f, 1);

        //ガラスを初期化
        permanetBreakGlass.Init(destroyCardSourceMaterial);

        //レンダリング用のパーマネントカードを設定
        renderingHandCard.gameObject.SetActive(true);
        renderingHandCard.SetUpHandCard(destroyedSecurityCard);
        renderingHandCard.SetUpHandCardImage();
        renderingHandCard.ShowDP();

        //元パーマネントオブジェクトを非表示
        ShowUseHandCard.gameObject.SetActive(false);

        GameObject effect2 = null;

        if (destroyedSecurityCard != null)
        {
            switch (destroyedSecurityCard.BaseCardColorsFromEntity[0])
            {
                case CardColor.Green:
                    effect2 = Instantiate(GreenEvolutionEffect, effectParent);
                    break;

                case CardColor.Red:
                    effect2 = Instantiate(RedEvolutionEffect, effectParent);
                    break;

                case CardColor.Blue:
                    effect2 = Instantiate(BlueEvolutionEffect, effectParent);
                    break;

                case CardColor.Yellow:
                    effect2 = Instantiate(YellowEvolutionEffect, effectParent);
                    break;

                case CardColor.Purple:
                    effect2 = Instantiate(PurpleEvolutionEffect, effectParent);
                    break;

                case CardColor.Black:
                    effect2 = Instantiate(BlackEvolutionEffect, effectParent);
                    break;

                case CardColor.White:
                    effect2 = Instantiate(WhiteEvolutionEffect, effectParent);
                    break;

                case CardColor.None:
                    effect2 = Instantiate(WhiteEvolutionEffect, effectParent);
                    break;
            }

            effect2.transform.position = new Vector3(ShowUseHandCard.transform.position.x, ShowUseHandCard.transform.position.y, ShowUseHandCard.transform.position.z);
            effect2.transform.localScale = new Vector3(4, 1, 4);
            StartCoroutine(DeleteCoroutine(effect2, null));
        }

        //爆発させる
        yield return ContinuousController.instance.StartCoroutine(permanetBreakGlass.BreakIenumerator());

        //エフェクトを削除
        DestroyImmediate(permanetBreakGlass.gameObject);

        renderingFiedlPermanentCard.gameObject.SetActive(false);
    }
    #endregion

    #region バトルを行うエフェクト
    [Header("バトル時アニメーションオブジェクト")]
    public GameObject BattleAnimationPrefab;

    [Header("バトルSE")]
    public AudioClip BattleSE;
    public IEnumerator BattleEffect(List<Permanent> WinnerPermanents, List<Permanent> LoserPermanents, CardSource LoserCard)
    {
        List<GameObject> BattleAnimationObjects = new List<GameObject>();

        List<GameObject> ShakeObjects = new List<GameObject>();

        foreach (Permanent permanent in LoserPermanents)
        {
            if (permanent != null)
            {
                if (permanent.ShowingPermanentCard != null)
                {
                    GameObject BattleAnimationObject = Instantiate(BattleAnimationPrefab, permanent.ShowingPermanentCard.transform.parent);

                    BattleAnimationObject.transform.localScale = new Vector3(1, 1, 1);

                    BattleAnimationObject.transform.SetParent(GManager.instance.canvas.transform);

                    BattleAnimationObject.transform.position = permanent.ShowingPermanentCard.transform.position;

                    BattleAnimationObjects.Add(BattleAnimationObject);

                    ShakeObjects.Add(permanent.ShowingPermanentCard.gameObject);
                }
            }
        }

        if (LoserCard != null)
        {
            if (GManager.instance.attackProcess.SecurityDigimon == LoserCard)
            {
                if (ShowUseHandCard.gameObject.activeSelf && ShowUseHandCard.cardSource == LoserCard)
                {
                    GameObject BattleAnimationObject = Instantiate(BattleAnimationPrefab, ShowUseHandCard.transform.parent);

                    BattleAnimationObject.transform.localScale = new Vector3(0.3f, 0.3f, 1);

                    BattleAnimationObject.transform.SetParent(GManager.instance.canvas.transform);

                    BattleAnimationObject.transform.position = ShowUseHandCard.transform.position;

                    BattleAnimationObjects.Add(BattleAnimationObject);

                    BattleAnimationObject.transform.SetParent(GManager.instance.canvas2.transform);

                    BattleAnimationObject.transform.transform.SetSiblingIndex(5);

                    ShakeObjects.Add(ShowUseHandCard.gameObject);
                }

                foreach (HandCard handCard in LoserCard.Owner.brainStormObject.BrainStormHandCards)
                {
                    if (handCard.gameObject.activeSelf && handCard.cardSource == LoserCard)
                    {
                        GameObject BattleAnimationObject = Instantiate(BattleAnimationPrefab, handCard.transform.parent.parent);

                        BattleAnimationObject.transform.localScale = new Vector3(1, 1, 1);

                        BattleAnimationObject.transform.SetParent(GManager.instance.canvas.transform);

                        BattleAnimationObject.transform.position = handCard.transform.position;

                        BattleAnimationObjects.Add(BattleAnimationObject);
                        break;
                    }
                }
            }
        }

        if (BattleAnimationObjects.Count >= 1)
        {
            ContinuousController.instance.PlaySE(BattleSE);

            bool end = false;
            float animTime = 0.25f;

            List<Sequence> sequences = new List<Sequence>();

            foreach (GameObject BattleAnimationObject in BattleAnimationObjects)
            {
                GameObject claw = BattleAnimationObject.transform.GetChild(0).transform.GetChild(0).gameObject;

                claw.transform.localPosition = new Vector3(-250, 250, 0);

                Sequence sequence = DOTween.Sequence();

                sequences.Add(sequence);

                sequence
                    .Append(claw.transform.DOLocalMove(Vector3.zero, animTime).SetEase(Ease.InCubic))
                    .AppendCallback(() => end = true);

                sequence.Play();
            }

            foreach (GameObject ShakeObject in ShakeObjects)
            {
                Sequence sequence = DOTween.Sequence();

                sequences.Add(sequence);

                sequence
                    .Append(ShakeObject.transform.DOShakePosition(animTime, strength: 8f, vibrato: 30, fadeOut: true))
                    .AppendCallback(() => end = true);

                sequence.Play();
            }

            yield return new WaitWhile(() => !end);
            end = false;

            foreach (Sequence sequence in sequences)
            {
                sequence.Kill();
            }

            yield return new WaitForSeconds(0.1f);

            for (int i = 0; i < BattleAnimationObjects.Count; i++)
            {
                DestroyImmediate(BattleAnimationObjects[i]);
            }
        }

        //ShowUseHandCard.OffDP();
    }
    #endregion

    #region 進化元が離れるエフェクト
    public IEnumerator RemoveDigivolveRootEffect(CardSource card, Permanent targetPermanent)
    {
        if (card != null)
        {
            if (targetPermanent != null)
            {
                if (targetPermanent.TopCard != null)
                {
                    ContinuousController.instance.PlaySE(GManager.instance.MoveSE);

                    bool end = false;

                    var sequence = DOTween.Sequence();

                    float animTime = 0.17f;

                    sequence = DOTween.Sequence();

                    Vector3 startPos = targetPermanent.ShowingPermanentCard.transform.position;

                    ShowUseHandCard.SetUpHandCard(card);
                    ShowUseHandCard.SetUpHandCardImage();

                    ShowUseHandCard.gameObject.SetActive(true);
                    ShowUseHandCard.Outline_Select.SetActive(false);
                    ShowUseHandCard.transform.SetParent(ShowUseHandCardParent);
                    ShowUseHandCard.transform.parent.gameObject.SetActive(true);

                    ShowUseHandCard.CardImage.color = new Color32(0, 0, 0, 255);

                    ShowUseHandCard.SetOutlineColor(DataBase.CardColor_ColorLightDictionary[card.BaseCardColorsFromEntity[0]]);
                    ShowUseHandCard.Outline_Select.SetActive(true);

                    ShowUseHandCard.transform.localScale = new Vector3(0.2f, 0.2f, 1);
                    ShowUseHandCard.transform.localRotation = Quaternion.Euler(0f, 0, 0);
                    ShowUseHandCard.transform.position = startPos;

                    float targetPos_Y = 0f;

                    if (card.Owner.isYou)
                    {
                        targetPos_Y = ShowUseHandCard.transform.localPosition.y + 22f;
                    }

                    else
                    {
                        targetPos_Y = ShowUseHandCard.transform.localPosition.y - 22f;
                    }

                    float targetPos_X_1 = 0f;

                    if (card.Owner.isYou)
                    {
                        targetPos_X_1 = ShowUseHandCard.transform.localPosition.x + 15f;
                    }

                    else
                    {
                        targetPos_X_1 = ShowUseHandCard.transform.localPosition.x - 15f;
                    }

                    float targetPos_X_2 = ShowUseHandCard.transform.localPosition.x;

                    sequence
                        .Append(ShowUseHandCard.transform.DOLocalMoveY(targetPos_Y, animTime))
                        .Join(ShowUseHandCard.transform.DOLocalMoveX(targetPos_X_1, animTime / 2f))
                        .Append(ShowUseHandCard.transform.DOLocalMoveX(targetPos_X_2, animTime / 2f))
                        .AppendCallback(() => { end = true; });

                    sequence.Play();

                    while (!end)
                    {
                        yield return null;
                    }

                    end = false;

                    yield return new WaitForSeconds(0.17f);

                    ShowUseHandCard.Outline_Select.SetActive(false);

                    sequence = DOTween.Sequence();

                    sequence
                        .Append(DOTween.To(() => ShowUseHandCard.CardImage.color, (x) => ShowUseHandCard.CardImage.color = x, new Color(0, 0, 0, 0), animTime))
                        .AppendCallback(() => { end = true; });

                    sequence.Play();

                    while (!end)
                    {
                        yield return null;
                    }

                    end = false;

                    ShowUseHandCard.gameObject.SetActive(false);
                }
            }
        }
    }
    #endregion

    #region プレイに失敗したときのエフェクト
    public IEnumerator FailedPlayCardEffect(CardSource cardSource)
    {
        if (cardSource != null)
        {
            HandCard targetHandCard = null;

            foreach (HandCard handCard in cardSource.Owner.brainStormObject.BrainStormHandCards)
            {
                if (handCard.gameObject.activeSelf && handCard.cardSource == cardSource)
                {
                    targetHandCard = handCard;
                    break;
                }
            }

            if (targetHandCard != null)
            {
                ContinuousController.instance.PlaySE(FreezeSE);

                bool end = false;
                float animTime = 0.25f;

                Sequence sequence = DOTween.Sequence();

                sequence
                    .Append(targetHandCard.transform.DOShakePosition(animTime, strength: 8f, vibrato: 30, fadeOut: true))
                    .AppendCallback(() => end = true);

                sequence.Play();

                yield return new WaitWhile(() => !end);
                end = false;

                sequence.Kill();

                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    #endregion
}