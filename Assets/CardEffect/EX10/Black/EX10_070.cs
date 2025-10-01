using System.Collections;
using System.Collections.Generic;

//EX10 God Grade Unleashed
namespace DCGO.CardEffects.EX10
{
    public class EX10_070 : CEntity_Effect
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
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] <Draw 1>. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }

            #endregion

            #region Delay - link when link card trashed

            if (timing == EffectTiming.OnLinkCardDiscarded)
            {
                Permanent trashedFrom = null;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Link 1 card from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);                

                string EffectDescription()
                {
                    return "[All Turns] When effects trash any of your Digimon's link cards, <Delay>.\r\n • You may link 1 Digimon card with the [Appmon] trait from your trash to 1 of those Digimon without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanDeclareOptionDelayEffect(card))
                    {
                        if (CardEffectCommons.CanTriggerOnTrashLinkedCard(hashtable, perm => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(perm, card), cardEffect => cardEffect != null, source => source != null))
                        {
                            trashedFrom = CardEffectCommons.GetPermanentFromHashtable(hashtable);

                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().CanBeDestroyedBySkill(activateClass))
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, cardSource => CanLinkToTrashedFromDigimon(cardSource, hashtable)))
                            {
                                return true;
                            }                            
                        }
                    }

                    return false;
                }

                bool CanLinkToTrashedFromDigimon(CardSource cardSource, Hashtable hashtable)
                {
                    return cardSource.CanLinkToTargetPermanent(CardEffectCommons.GetPermanentFromHashtable(hashtable), false);
                }

                bool CanSelectLinkTarget(CardSource cardSource)
                {
                    return cardSource.HasAppmonTraits && cardSource.CanLinkToTargetPermanent(trashedFrom, false);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool delaySuccussful = false;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                    targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                    activateClass: activateClass,
                    successProcess: permanents => SuccessProcess(),
                    failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        delaySuccussful = true;

                        yield return null;
                    }

                    if (delaySuccussful)
                    {
                        Permanent selectedPermanent = null;
                        CardSource cardForLinking = null;

                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectLinkTarget))
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectLinkTarget,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to link.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to link.", "The opponent is selecting 1 digivolution card to link.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Linked Card");

                            yield return StartCoroutine(selectCardEffect.Activate());
                        }

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            cardForLinking = cardSource;

                            yield return null;
                        }

                        if (cardForLinking != null)
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
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to link.", "The opponent is selecting 1 Digimon to link.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            bool CanSelectPermanentCondition(Permanent permanent)
                            {
                                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                                       permanent == trashedFrom &&
                                       cardForLinking.CanLinkToTargetPermanent(permanent, false);
                            }

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;

                                yield return null;
                            }

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddLinkCard(cardForLinking, activateClass));
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