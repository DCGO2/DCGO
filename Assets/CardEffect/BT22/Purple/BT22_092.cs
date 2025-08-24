using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Jimmy KEN
namespace DCGO.CardEffects.BT22
{
    public class BT22_092 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of your turn

            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }

            #endregion

            #region Your Turn

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Activate 1 [Main] effect. if you do, gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] When your Digimon are played or digivolve, if any of them have the [Flame] or [CS] trait, by suspending this Tamer, activate 1 of those Digimon's [Main] effects. If this activated any effect, gain 1 memory.";
                }

                bool PermanentConditionNSoEnterField(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.EqualsTraits("Flame") || permanent.TopCard.HasCSTraits)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentConditionNSoEnterField))
                            {
                                return true;
                            }

                            if (CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentConditionNSoEnterField))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    List<Permanent> permanents = CardEffectCommons.GetPlayedPermanentsFromEnterFieldHashtable(
                        hashtable: _hashtable,
                        rootCondition: null)
                        .Filter(HasMainEffect);


                    bool HasMainEffect(Permanent permanent)
                    {
                        foreach (ICardEffect effect in permanent.EffectList(EffectTiming.OnDeclaration))
                        {
                            if (effect.EffectDiscription.Contains("[Main]"))
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    if (permanents.Count > 0)
                    {
                        bool CanSelectPermanentCondition(Permanent permanent)
                        {
                            return permanents.Contains(permanent);
                        }

                        Permanent selectedPermanent = null;

                        if (card.Owner.GetBattleAreaDigimons().Count(CanSelectPermanentCondition) == 1)
                        {
                            selectedPermanent = card.Owner.GetBattleAreaDigimons().Find(CanSelectPermanentCondition);
                        }
                        else
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will activate a [Main] effect.", "The opponent is selecting 1 Digimon that will activate a [Main] effect.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;

                                yield return null;
                            }
                        }

                        if (selectedPermanent != null)
                        {
                            List<ICardEffect> candidateEffects = new List<ICardEffect>();

                            List<ICardEffect> effects = selectedPermanent.EffectList(EffectTiming.OnDeclaration)
                                .Clone()
                                .Filter(cardEffect => cardEffect != null && cardEffect is ActivateICardEffect && cardEffect.EffectDiscription.Contains("[Main]"));

                            candidateEffects.AddRange(effects);

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
                                    if (selectedEffect.EffectSourceCard != null)
                                    {
                                        if (selectedEffect.EffectSourceCard.PermanentOfThisCard() != null)
                                        {
                                            if (selectedEffect.CanUse(null))
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));

                                                yield return ContinuousController.instance.StartCoroutine(((ActivateICardEffect)selectedEffect).Activate_Optional_Effect_Execute(null));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}