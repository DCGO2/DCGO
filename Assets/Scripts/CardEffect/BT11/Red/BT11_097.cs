using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT11
{
    public class BT11_097 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] Delete 1 of your opponent's Digimon with 8000 DP or less. Then, if you have a red Tamer in play, activate 1 of the [On Deletion] effects of 1 of your red Digimon with [Vaccine] in its traits.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(8000, activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.CardColors.Contains(CardColor.Red))
                        {
                            if (permanent.TopCard.CardTraits.Contains("Vaccine"))
                            {
                                List<ICardEffect> candidateEffects = permanent.EffectList(EffectTiming.OnDestroyedAnyone)
                                    .Clone()
                                    .Filter(cardEffect => cardEffect != null && cardEffect is ActivateICardEffect && !cardEffect.IsSecurityEffect && cardEffect.IsOnDeletion);

                                if (candidateEffects.Count >= 1)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.TopCard.CardColors.Contains(CardColor.Red) && permanent.IsTamer))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                        {
                            Permanent selectedPermanent = null;

                            int maxCount = 1;

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition1,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon whose [On Deletion] effect will activate.", "The opponent is selecting 1 Digimon whose [On Deletion] effect will activate.");
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                            {
                                foreach (Permanent permanent in permanents)
                                {
                                    selectedPermanent = permanent;
                                }

                                yield return null;
                            }

                            if (selectedPermanent != null)
                            {
                                List<ICardEffect> candidateEffects = selectedPermanent.EffectList(EffectTiming.OnDestroyedAnyone)
                                    .Clone()
                                    .Filter(cardEffect => cardEffect != null && cardEffect is ActivateICardEffect && !cardEffect.IsSecurityEffect && cardEffect.IsOnDeletion);

                                if (candidateEffects.Count >= 1)
                                {
                                    ICardEffect selectedEffect = null;

                                    if (candidateEffects.Count == 1)
                                    {
                                        selectedEffect = candidateEffects[0];
                                    }
                                    else
                                    {
                                        List<SkillInfo> skillInfos = candidateEffects
                                            .Map(cardEffect => new SkillInfo(cardEffect, null, EffectTiming.None));

                                        List<CardSource> cardSources = candidateEffects
                                            .Map(cardEffect => cardEffect.EffectSourceCard);

                                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                        selectCardEffect.SetUp(
                                            canTargetCondition: (cardSource) => true,
                                            canTargetCondition_ByPreSelecetedList: null,
                                            canEndSelectCondition: null,
                                            canNoSelect: () => false,
                                            selectCardCoroutine: null,
                                            afterSelectCardCoroutine: null,
                                            message: "Select 1 effect to activate.",
                                            maxCount: 1,
                                            canEndNotMax: false,
                                            isShowOpponent: false,
                                            mode: SelectCardEffect.Mode.Custom,
                                            root: SelectCardEffect.Root.Custom,
                                            customRootCardList: cardSources,
                                            canLookReverseCard: true,
                                            selectPlayer: card.Owner,
                                            cardEffect: activateClass);

                                        selectCardEffect.SetNotShowCard();
                                        selectCardEffect.SetUpSkillInfos(skillInfos);
                                        selectCardEffect.SetUpAfterSelectIndexCoroutine(AfterSelectIndexCoroutine);

                                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                        IEnumerator AfterSelectIndexCoroutine(List<int> selectedIndexes)
                                        {
                                            if (selectedIndexes.Count == 1)
                                            {
                                                selectedEffect = candidateEffects[selectedIndexes[0]];
                                                yield return null;
                                            }
                                        }
                                    }

                                    if (selectedEffect != null)
                                    {
                                        Hashtable effectHashtable = CardEffectCommons.OnDeletionHashtable(
                                            new List<Permanent>() { selectedPermanent },
                                            null,
                                            null,
                                            false);

                                        card.Owner.TrashCards.Add(selectedPermanent.TopCard);
                                        card.Owner.TrashCards.Add(selectedEffect.EffectSourceCard);
                                        selectedEffect.EffectSourceCard.PermanentJustBeforeRemoveField = selectedPermanent;
                                        selectedPermanent.TopCard.PermanentJustBeforeRemoveField = selectedPermanent;
                                        bool canUse = selectedEffect.CanUse(effectHashtable);
                                        card.Owner.TrashCards.Remove(selectedPermanent.TopCard);
                                        card.Owner.TrashCards.Remove(selectedEffect.EffectSourceCard);
                                        selectedEffect.EffectSourceCard.PermanentJustBeforeRemoveField = null;
                                        selectedPermanent.TopCard.PermanentJustBeforeRemoveField = null;

                                        if (canUse)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(((ActivateICardEffect)selectedEffect).Activate_Optional_Effect_Execute(effectHashtable));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(
                    card: card,
                    cardEffects: ref cardEffects,
                    effectName: $"Delete 1 Digimon with 8000 DP or less and activate 1 [On Deletion] effect");
            }

            return cardEffects;
        }
    }
}