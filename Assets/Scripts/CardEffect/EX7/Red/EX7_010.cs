using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX7
{
    public class EX7_010 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasText("Three Musketeers") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 3 || targetPermanent.TopCard.HasText("ThreeMusketeers") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 3;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Shared WD / WA

            string EffectDescription(string tag)
            {
                return $"[{tag}] [Once Per Turn] You may trash 1 Option card in 1 Digimon's digivolution cards.";
            }

            

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);
            }

            bool CanActivateConditionShared(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            IEnumerator ActivateCoroutineShared(Hashtable _hashtable, ActivateClass activateClass)
            {
                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (!cardSource.CanNotTrashFromDigivolutionCards(activateClass))
                    {
                        if (cardSource.IsOption)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                    permanentCondition: CanSelectPermanentCondition,
                    cardCondition: CanSelectCardCondition,
                    maxCount: 1,
                    canNoTrash: false,
                    isFromOnly1Permanent: true,
                    activateClass: activateClass
                ));
            }

            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 Option from the digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, (hashtable) => ActivateCoroutineShared(hashtable, activateClass), 1, true, EffectDescription("When Digivolving"));
                activateClass.SetHashString("TrashOption_EX7_010");
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if(CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 Option from the digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, (hashtable) => ActivateCoroutineShared(hashtable, activateClass), 1, true, EffectDescription("When Attacking"));
                activateClass.SetHashString("TrashOption_EX7_010");
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerOnAttack(hashtable, card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.None)
            {
                ChangeTraitsClass changeTraitsClass = new ChangeTraitsClass();
                changeTraitsClass.SetUpICardEffect("This Digimon gains the [Three Musketeers] trait.", CanUseCondition, card);
                changeTraitsClass.SetUpChangeTraitsClass(changeeTraits: changeTraits);
                cardEffects.Add(changeTraitsClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        {
                            if(card.PermanentOfThisCard().TopCard == card)
                            { 
                                return true;
                            }
                        }
                    }

                    return false;
                }

                List<string> changeTraits(CardSource cardSource, List<string> CardTraits)
                {
                    if (cardSource == card)
                    {
                        CardTraits.Add("Three Musketeers");
                    }

                    return CardTraits;
                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if(CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        {
                            return true;
                        }                      
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 2000,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}