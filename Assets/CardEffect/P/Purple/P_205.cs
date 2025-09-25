using System;
using System.Collections;
using System.Collections.Generic;

// Insane Synthetic Monster
namespace DCGO.CardEffects.P
{
    public class P_205 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region ignoring colours

            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);

                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.HasMatchConditionPermanent((permanent) => permanent.TopCard.Owner == card.Owner && (permanent.IsTamer || permanent.IsDigimon) && permanent.TopCard.HasDMTraits, true);

                bool CardCondition(CardSource cardSource)
                    => cardSource == card;
            }

            #endregion

            #region Main/Security Shared

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DrawAndDiscardCards(
                    player: (card.Owner, card.Owner),
                    drawAmount: 2,
                    trashAmount: 2,
                    card: card,
                    activateClass: activateClass));

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
            }

            #endregion

            #region Main

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 2, Trash 2.", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, hash => SharedActivateCoroutine(hash, activateClass), -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] <Draw 2> and trash 2 cards in your hand. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }
            }

            #endregion

            #region Main - Delay

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By delete 1 7 play cost or less digimon, play 1 [Kimeramon]/[Millenniummon] in its name digimon from trash for 3 reduced play cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] <Delay>. By deleting 1 of your play cost 7 or lower Digimon, you may play 1 Digimon card with [Kimeramon] or [Millenniummon] in its name from your trash with the play cost reduced by 3.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanDeclareOptionDelayEffect(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnField(card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasPlayCost && permanent.TopCard.BasePlayCostFromEntity <= 7;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.ContainsCardName("Kimeramon") || cardSource.ContainsCardName("Millenniummon")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass,
                        successProcess: permanents => DeleteOptionSuccessProcess(),
                        failureProcess: null)
                    );

                    IEnumerator DeleteOptionSuccessProcess()
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            Permanent selectedPermanent = null;
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));
                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;
                                yield return null;
                            }

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete", "The opponent is selecting 1 Digimon to delete");
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                targetPermanents: new List<Permanent>() { selectedPermanent },
                                activateClass: activateClass,
                                successProcess: permanents => DeleteDigimonSuccessProcess(),
                                failureProcess: null)
                            );

                            IEnumerator DeleteDigimonSuccessProcess()
                            {
                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                                {
                                    CardSource selectedTrashCard = null;
                                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CanSelectCardCondition));

                                    selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectCardCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => true,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 [Kimeramon]/[Millenniummon] to play",
                                        maxCount: maxCount,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Trash,
                                        customRootCardList: null,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                                    {
                                        selectedTrashCard = cardSource;
                                        yield return null;
                                    }

                                    selectCardEffect.SetUpCustomMessage("Select 1 [Kimeramon]/[Millenniummon] to play", "The opponent is selecting 1 [Kimeramon]/[Millenniummon] to play");
                                    selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");
                                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                    if (selectedTrashCard != null)
                                    {
                                        int cost = selectedTrashCard.BasePlayCostFromEntity - 3;
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                            cardSources: new List<CardSource>() { selectedTrashCard },
                                            activateClass: activateClass,
                                            payCost: true,
                                            isTapped: false,
                                            root: SelectCardEffect.Root.Trash,
                                            activateETB: true,
                                            fixedCost: cost));
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
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 2, Trash 2.", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, hash => SharedActivateCoroutine(hash, activateClass), -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Security] <Draw 2> and trash 2 cards in your hand. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }
            }

            #endregion

            return cardEffects;
        }
    }
}