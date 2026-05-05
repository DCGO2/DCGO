using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT25
{
    // Onmon
    public class BT25_045 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasAppmonTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 2, permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Link Condition

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasAppmonTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: PermanentCondition, linkCost: 1, card: card));
            }

            #endregion

            #region Your Turn
            if (timing == EffectTiming.WhenWouldLink)
            {
                ActivateClass activateClass = new ();
                activateClass.SetUpICardEffect("May reduce Link cost by 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT25_045_YT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] [Once Per Turn] When a [Social], [Tool] or [Game] trait card would link to this Digimon, you may reduce the cost by 1.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerWhenWouldLink(hashtable, CardCondition, PermanentCondition);
                }

                bool PermanentCondition(Permanent permanent) => permanent == card.PermanentOfThisCard();

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("Social")
                        || cardSource.EqualsTraits("Tool")
                        || cardSource.EqualsTraits("Game");
                }

                bool RootCondition(SelectCardEffect.Root root) => true;

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleArea(card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    card.Owner.UntilCalculateFixedCostEffect.Add((timing) => CardEffectFactory.GrantedReduceLinkCostClass(
                        card: card, 
                        reducedCost: 1,
                        cardSourceCondition: CardCondition,
                        permanentCondition: PermanentCondition,
                        rootCondition: RootCondition
                    ));

                    yield return null;
                }
            }
            #endregion

            #region Link
            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.LinkEffect(card));
            }
            #endregion

            #region When Linking

            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 enemy Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsLinkedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Linking] Suspend 1 of your opponent's Digimon.";
                }

                bool CanSelectPermanentCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenLinking(hashtable, null, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
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
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
