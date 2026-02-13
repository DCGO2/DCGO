using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT19
{
    public class BT19_092 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main Effect

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 of your opponent's Digimon to the bottom of the deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Main] Return 1 of your opponent's level 4 or lower Digimon to the bottom of the deck. By returning 1 of your blue Digimon to the bottom of the deck, return 1 of their level 6 or lower Digimon to the bottom of the deck instead.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool CanSelectOwnerPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.CardColors.Contains(CardColor.Blue);
                }

                bool CanSelectOpponentPermanentLevelCondition(Permanent permanent, int level)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.HasLevel && permanent.Level <= level;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool bottomDeckOwnDigimon = false;

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnerPermanentCondition))
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>
                        {
                            new(message: "Yes", value: true, spriteIndex: 0),
                            new(message: "No", value: false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Return 1 of your digimon to the bottom of the deck?";
                        string notSelectPlayerMessage =
                            "The opponent is choosing whether to return 1 of their digimon to the bottom of the deck";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements,
                            selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                            notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                            .WaitForEndSelect());

                        bottomDeckOwnDigimon = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (bottomDeckOwnDigimon)
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition_ByPreSelecetedList: null,
                                canTargetCondition: CanSelectOwnerPermanentCondition,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 of your Digimon to bottom deck.",
                                "The opponent is selecting 1 of your Digimon to bottom deck.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }

                    int targetLevel = bottomDeckOwnDigimon ? 6 : 4;

                    if (CardEffectCommons.HasMatchConditionPermanent(permanent =>
                            CanSelectOpponentPermanentLevelCondition(permanent, targetLevel)))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition_ByPreSelecetedList: null,
                            canTargetCondition: permanent => CanSelectOpponentPermanentLevelCondition(permanent, targetLevel),
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 opponent's Digimon to bottom deck.",
                            "The opponent is selecting 1 opponent's Digimon to bottom deck.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            #endregion

            #region Security Effect

            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects,
                    effectName: "Return 1 of your opponent's Digimon to the bottom of the deck");
            }

            #endregion

            return cardEffects;
        }
    }
}