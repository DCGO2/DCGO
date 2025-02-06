using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT14
{
    public class BT14_095 : CEntity_Effect
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
                    return "[Main] 1 of your opponent's Digimon gains \"[All Turns] When this Digimon becomes suspended, lose 2 memory.\" until the end of their turn.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
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
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get effects.", "The opponent is selecting 1 Digimon that will get effects.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                            }

                            if (selectedPermanent != null)
                            {
                                ActivateClass activateClass1 = new ActivateClass();
                                activateClass1.SetUpICardEffect("Memory -2", CanUseCondition2, selectedPermanent.TopCard);
                                activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                                activateClass1.SetEffectSourcePermanent(selectedPermanent);

                                CardEffectCommons.AddEffectToPermanent(
                                    targetPermanent: selectedPermanent,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                    card: card,
                                    cardEffect: activateClass1,
                                    timing: EffectTiming.OnTappedAnyone);

                                string EffectDiscription1()
                                {
                                    return "[All Turns] When this Digimon becomes suspended, lose 2 memory.";
                                }

                                bool CanUseCondition2(Hashtable hashtable1)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                    {
                                        if (CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable1, (permanent) => permanent == selectedPermanent))
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                bool CanActivateCondition1(Hashtable hashtable1)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                    {
                                        return true;
                                    }

                                    return false;
                                }

                                IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(selectedPermanent.TopCard.Owner.AddMemory(-2, activateClass));
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                string EffectDiscription()
                {
                    return "[Security] Opponent's 1 Digimon gains effect. Then, add this card to your hand.";
                }

                IEnumerator AfterMainEffect(ICardEffect activateClass)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
                }

                CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects, effectName: $"Opponent's 1 Digimon gains effect and add this card to hand", effectDiscription: EffectDiscription(), afterMainEffect: AfterMainEffect);
            }

            return cardEffects;
        }
    }
}