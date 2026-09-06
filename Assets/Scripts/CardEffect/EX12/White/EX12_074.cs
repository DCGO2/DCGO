using System.Collections;
using System.Collections.Generic;

// Genshi Continent & Ashino Island
namespace DCGO.CardEffects.EX12
{
    public class EX12_074 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Use Req
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("Shambala");
                }
            }
            #endregion

            #region Your Turn Security
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May digivolve to [Shambala] for 1 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("EX12_074_YT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Security] [Your Turn] [Once Per Turn] When one of your [Shambala] trait Digimon attacks, it may digivolve into a [Shambala] trait Digimon card in the hand with the cost reduced by 1.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistInSecurity(card, false)
                    && CardEffectCommons.IsOwnerTurn(card)
                    && CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, PermanentCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistInSecurity(card, false)
                    && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);

                bool PermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.EqualsTraits("Shambala");

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.EqualsTraits("Shambala");

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: GManager.instance.attackProcess.AttackingPermanent,
                        cardCondition: CanSelectCardCondition,
                        payCost: true,
                        reduceCostTuple: (reduceCost: 1, reduceCostCardCondition: null),
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: true,
                        activateClass: activateClass,
                        successProcess: null));
                }
            }
            #endregion

            #region Main Effect
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Replace your bottom sec with this face-up card, play a [Shambala] Digimon for -3", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] Add your bottom security card to the hand and place this card face up as the bottom security card. Then, you may play 1 [Shambala] trait Digimon card from your hand with the play cost reduced by 3.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("Shambala")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, true, activateClass, fixedCost: cardSource.GetCostItself - 3);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectFactory.ReplaceBottomSecurityWithFaceUpOptionEffect(card, activateClass));

                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                            canTargetCondition: CanSelectCardCondition,
                            SelectCardEffect.Root.Hand,
                            activateClass,
                            payCost: true,
                            reduceCostTuple: (3, null)
                        ));
                    }
                }
            }
            #endregion

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Play 1 play cost 5 or lower [Shambala] Digimon card from hand/trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                 => "[Security] You may play 1 play cost 5 or lower [Shambala] trait Digimon card from your hand or trash without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                bool CanPlayCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 5
                        && cardSource.EqualsTraits("Shambala")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanPlayCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanPlayCondition);

                    if (canSelectHand || canSelectTrash)
                    {
                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<int>> selectionElements1 = new List<SelectionElement<int>>()
                        {
                            new (message: $"From hand", value : 1, spriteIndex: 0),
                            new (message: $"From trash", value : 2, spriteIndex: 0),
                            new (message: $"Don't play", value: 3, spriteIndex: 1)
                        };

                            string selectPlayerMessage1 = "From which area will you play a card?";
                            string notSelectPlayerMessage1 = "The opponent is choosing from which area to select a card.";

                            GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage1, notSelectPlayerMessage: notSelectPlayerMessage1);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetInt(canSelectHand ? 1 : 2);
                        }
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        SelectCardEffect.Root root = GManager.instance.userSelectionManager.SelectedIntValue == 1 ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                        bool doSelect = GManager.instance.userSelectionManager.SelectedIntValue != 3;

                        if (doSelect)
                        {
                            #region Hand/Trash Card Selection & Play
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                                canTargetCondition: CanPlayCondition,
                                root,
                                activateClass,
                                payCost: false
                            ));
                            #endregion
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
