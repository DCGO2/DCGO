using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX5
{
    public class EX5_053 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardTraits.Contains("Deva") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 5;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }

            if (timing == EffectTiming.OnSecurityCheck)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play the security Digimon without battle", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("PlaySecurity_EX5_053");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Opponent's Turn] [Once Per Turn] When your security is checked, if that card is a Digimon card with the [Deva] trait, play it without battling and without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            CardSource securityCard = CardEffectCommons.GetCardFromHashtable(hashtable);

                            if (securityCard != null)
                            {
                                if (securityCard.Owner == card.Owner)
                                {
                                    if (securityCard.IsDigimon)
                                    {
                                        if (securityCard.CardTraits.Contains("Deva"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    CardSource securityCard = CardEffectCommons.GetCardFromHashtable(_hashtable);

                    if (securityCard != null)
                    {
                        DontBattleSecurityDigimonClass dontBattleSecurityDigimonClass = new DontBattleSecurityDigimonClass();
                        dontBattleSecurityDigimonClass.SetUpICardEffect("Ignore Battle", CanUseCondition, card);
                        dontBattleSecurityDigimonClass.SetUpDontBattleSecurityDigimonClass(CardSourceCondition: CardSourceCondition);
                        card.Owner.UntilSecurityCheckEndEffects.Add(_timing => dontBattleSecurityDigimonClass);

                        bool CanUseCondition(Hashtable hashtable)
                        {
                            return CardEffectCommons.CanUseIgnoreBattle(hashtable, securityCard);
                        }

                        bool CardSourceCondition(CardSource cardSource)
                        {
                            return cardSource == securityCard;
                        }
                    }

                    if (securityCard != null)
                    {
                        if (CardEffectCommons.CanPlayAsNewPermanent(
                            cardSource: securityCard,
                            payCost: false,
                            cardEffect: activateClass,
                            root: SelectCardEffect.Root.Security))
                        {
                            GManager.instance.attackProcess.SecurityDigimon = null;

                            yield return ContinuousController.instance.StartCoroutine(securityCard.Owner.brainStormObject.CloseBrainstrorm(securityCard));

                            if (GManager.instance.GetComponent<Effects>().ShowUseHandCard.gameObject.activeSelf)
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShrinkUpUseHandCard(GManager.instance.GetComponent<Effects>().ShowUseHandCard));
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: new List<CardSource>() { securityCard },
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.Security,
                                activateETB: true));
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete opponent's all Digimons with the highest DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] Delete 1 of your opponent's highest play cost Digimon.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (CardEffectCommons.IsMaxCost(permanent, card.Owner.Enemy, true))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanActivateOnDeletion(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
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
                }
            }

            return cardEffects;
        }
    }
}