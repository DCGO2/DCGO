using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Dark Field
namespace DCGO.CardEffects.BT26
{
    public class BT26_100 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Ignore Color Requirement
            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);
                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                    => card.Owner.SecurityCards.Count(cardSource => !cardSource.IsFlipped) == 0;

                bool CardCondition(CardSource cardSource) => cardSource == card;
            }
            #endregion

            #region Security - All Turns

            bool IsOwnTitanDigimon(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.EqualsTraits("Titan");

            bool HasPlutomonOrTitamon(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && (permanent.TopCard.ContainsCardName("Plutomon") || permanent.TopCard.ContainsCardName("Titamon"));

            #region Blocker
            if (timing == EffectTiming.None)
            {
                bool CanUseCondition() => CardEffectCommons.IsExistInSecurity(card, false);

                cardEffects.Add(CardEffectFactory.BlockerStaticEffect(IsOwnTitanDigimon, false, card, CanUseCondition));
            }
            #endregion

            #region DP +3000
            if (timing == EffectTiming.None)
            {
                bool CanUseCondition()
                    => CardEffectCommons.IsExistInSecurity(card, false)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasPlutomonOrTitamon);

                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(IsOwnTitanDigimon, 3000, false, card, CanUseCondition, effectName: () => "All of your [Titan] trait Digimon get +3000 DP."));
            }
            #endregion

            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Replace bottom security with this face-up card, play 1 lvl 4 or lower [Titan] from hand/trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] Add your bottom security card to the hand and place this card face up as the bottom security card. Then, you may play 1 level 4 or lower [Titan] trait card from your hand or trash without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.IsDigimon && cardSource.HasLevel && cardSource.Level <= 4
                        && cardSource.EqualsTraits("Titan")
                        && cardSource.HasPlayCost
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectFactory.ReplaceBottomSecurityWithFaceUpOptionEffect(card, activateClass));

                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    if (canSelectHand || canSelectTrash)
                    {
                        SelectCardEffect.Root root;

                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                            {
                                new(message: "From hand", value: 1, spriteIndex: 0),
                                new(message: "From trash", value: 2, spriteIndex: 0),
                                new(message: "Don't play", value: 3, spriteIndex: 1),
                            };

                            GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: "From which area will you play a card?", notSelectPlayerMessage: "The opponent is choosing from which area to select a card.");
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            if (GManager.instance.userSelectionManager.SelectedIntValue == 3) yield break;

                            root = GManager.instance.userSelectionManager.SelectedIntValue == 1 ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                        }
                        else
                        {
                            root = canSelectHand ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                            canTargetCondition: CanSelectCardCondition,
                            root: root,
                            cardEffect: activateClass,
                            payCost: false));
                    }
                }
            }
            #endregion

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 lvl 4 or lower [Titan] Digimon card from hand or trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Security] You may play 1 level 4 or lower [Titan] trait Digimon card from your hand or trash without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);

                bool CanPlayCondition(CardSource cardSource)
                    => cardSource.IsDigimon && cardSource.HasLevel && cardSource.Level <= 4
                        && cardSource.EqualsTraits("Titan")
                        && cardSource.HasPlayCost
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanPlayCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanPlayCondition);

                    if (canSelectHand || canSelectTrash)
                    {
                        SelectCardEffect.Root root;

                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<int>> selectionElements1 = new List<SelectionElement<int>>()
                            {
                                new(message: "From hand", value: 1, spriteIndex: 0),
                                new(message: "From trash", value: 2, spriteIndex: 0),
                                new(message: "Don't play", value: 3, spriteIndex: 1),
                            };

                            GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: "From which area will you play a card?", notSelectPlayerMessage: "The opponent is choosing from which area to select a card.");
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            if (GManager.instance.userSelectionManager.SelectedIntValue == 3) yield break;

                            root = GManager.instance.userSelectionManager.SelectedIntValue == 1 ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                        }
                        else
                        {
                            root = canSelectHand ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                            canTargetCondition: CanPlayCondition,
                            root: root,
                            cardEffect: activateClass,
                            payCost: false));
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
