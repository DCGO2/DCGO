using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects
{
    public class SnowAgumon_EX7_017 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] Trash 1 digivolution card at the top of 1 of your opponent's Digimon.";
                }
                
                bool CanSelectDigimonCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DigivolutionCards.Count((cardSource) => !cardSource.CanNotTrashFromDigivolutionCards(activateClass)) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }
                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDigimonCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDigimonCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectDigimonCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDigimonCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will trash digivolution cards.", "The opponent is selecting 1 Digimon that will trash digivolution cards.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedPermanent, trashCount: 1, isFromTop: true, activateClass: activateClass));
                        }
                    }
                }
            }
            return cardEffects;
        }
    }
}