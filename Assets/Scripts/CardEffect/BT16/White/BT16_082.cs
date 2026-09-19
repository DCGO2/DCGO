using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class BT16_082 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnMove)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Hatch and reveal top 3 and add a Tamer or Digimon to hand.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] [Once Per Turn] When one of your Digimon moves from the breeding area to the battle area, reveal the top 3 cards of your deck. Add 1 Digimon or Tamer card among them to your hand. Return the rest to the bottom of the deck. and you may hatch in your breeding area.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanSelectCardCondition(CardSource card)
                {
                    if (card.IsDigimon || card.IsTamer)
                    {
                        return true;
                    }
                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnMove(hashtable, PermanentCondition))
                            {
                                return true;
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
                    List<CardSource> selectedCards = new List<CardSource>();

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndSelect(
                    revealCount: 3,
                    selectCardConditions:
                    new SelectCardConditionClass[]
                    {
                        new SelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList:null,
                            canEndSelectCondition:null,
                            canNoSelect:false,
                            selectCardCoroutine: null,
                            message: "Select 1 Digimon or Tamer card.",
                            maxCount: 1,
                            canEndNotMax:false,
                            mode: SelectCardEffect.Mode.AddHand
                            ),
                    },
                    remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                    activateClass: activateClass,
                    canNoAction: false));

                    if (card.Owner.CanHatch)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Hatch", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Not hatch", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Will you hatch in Breeding Area?";
                        string notSelectPlayerMessage = "The opponent is selecting whether to hatch in Breeding Area.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool doHatch = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (doHatch)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new HatchDigiEggClass(player: card.Owner, hashtable: CardEffectCommons.CardEffectHashtable(activateClass)).Hatch());
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}