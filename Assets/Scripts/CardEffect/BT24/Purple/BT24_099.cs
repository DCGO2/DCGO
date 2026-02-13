using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Super Hacking
namespace DCGO.CardEffects.BT24
{
    public class BT24_099 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Ignore Color Requirment
            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);
                cardEffects.Add(ignoreColorConditionClass);

                bool HasAppmonTrait(Permanent permanent)
                {
                    if (CardEffectCommons.IsOwnerPermanent(permanent, card))
                    {
                        if (permanent.TopCard.EqualsTraits("Appmon"))
                        {
                            if (permanent.IsDigimon || permanent.IsTamer)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionPermanent(HasAppmonTrait, true);
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }
            }
            #endregion

            #region Main

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1, Draw 2", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] By trashing 1 [Appmon] trait card from your hand, <Draw 2>. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.HasAppmonTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                    {
                        bool discarded = false;

                        int discardCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: discardCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                discarded = true;

                                yield return null;
                            }
                        }

                        if (discarded)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 2, activateClass).Draw());
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }

            #endregion

            #region All Turns - Delay

            if(timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Link 1 [Appmon] from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() => "[All Turns] When Digimon are deleted, <Delay>. You may link 1 [Appmon] trait Digimon card from your trash to 1 of your Digimon without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanDeclareOptionDelayEffect(card)
                        && CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermamentCondition);
                }

                bool PermamentCondition(Permanent permanent)
                {
                    return permanent.IsDigimon;
                }

                bool OwnPermamentCondition(CardSource cardsource, Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) && cardsource.CanLinkToTargetPermanent(permanent, false);
                bool LinkCardCondition(CardSource cardSource) => cardSource.IsDigimon && cardSource.HasAppmonTraits && cardSource.CanLink(false);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool delaySuccessful = false;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        delaySuccessful = true;
                        yield return null;
                    }

                    if (delaySuccessful)
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, LinkCardCondition))
                        {
                            CardSource selectedTrashCard = null;

                            #region Select Trash card

                            int maxCount1 = Math.Min(1, card.Owner.TrashCards.Count(LinkCardCondition));
                            
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: LinkCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 [Appmon] to link",
                                maxCount: maxCount1,
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

                            selectCardEffect.SetUpCustomMessage("Select 1 [Appmon] to link", "The opponent is selecting 1 [Appmon] to link");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Selected card");
                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            #endregion

                            if (selectedTrashCard != null)
                            {
                                Permanent selectedDigimon = null;

                                #region Select Permament Link Target

                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, (permanent) => OwnPermamentCondition(selectedTrashCard, permanent)));
                                
                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: (permanent) => OwnPermamentCondition(selectedTrashCard, permanent),
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
                                    selectedDigimon = permanent;
                                    yield return null;
                                }

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to link to", "The opponent is selecting 1 Digimon to link to");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                #endregion

                                if (selectedDigimon != null) yield return ContinuousController.instance.StartCoroutine(selectedDigimon.AddLinkCard(selectedTrashCard, activateClass));
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
                activateClass.SetUpICardEffect("place in battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Security] place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}