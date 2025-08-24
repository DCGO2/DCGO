using System;
using System.Collections;
using System.Collections.Generic;

// MarineBullmon
namespace DCGO.CardEffects.BT22
{
    public class BT22_024 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Decode

            if (timing == EffectTiming.WhenRemoveField)
            {
                bool SourceCondition(CardSource source)
                {
                    return source.IsDigimon
                    && source.HasLevel && source.IsLevel4
                    && (source.ContainsTraits("Aqua") || source.ContainsTraits("Sea Animal"));
                }

                string[] decodeStrings = { "(Lv.4 w/[Aqua]/[Sea Animal] trait)", "Level 4 Digimon card with [Aqua] or [Sea Animal] " };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(card: card, isInheritedEffect: false, decodeStrings: decodeStrings, sourceCondition: SourceCondition, condition: null));
            }

            #endregion

            #region Hand Main

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [Shellmon] from trash under 1 [Sangomon], to digivolve for 3", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Hand] [Main] If you have [Yao Qinglan], by placing 1 [Shellmon] from your trash as any of your [Sangomon]'s bottom digivolution card, it digivolves into this card for a digivolution cost of 3, ignoring digivolution requirements.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnHand(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsYaoQinglan)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsSangomon)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsShellmon);
                }

                bool IsYaoQinglan(Permanent permanent)
                {
                    return permanent.IsTamer && permanent.TopCard.EqualsCardName("Yao Qinglan");
                }

                bool IsSangomon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) && 
                           permanent.TopCard.EqualsCardName("Sangomon");
                }

                bool IsShellmon(CardSource source)
                {
                    return source.IsDigimon && source.EqualsCardName("Shellmon");
                }

                bool IsMarineBullmon(CardSource cardSource)
                {
                    return CardEffectCommons.IsExistOnHand(cardSource) && cardSource == card;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsYaoQinglan))
                    {
                        Permanent sangomon = null;
                        CardSource shellmon = null;

                        #region Select Shellmon From Trash

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, IsShellmon));
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                        selectCardEffect.SetUp(
                                    canTargetCondition: IsShellmon,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 [Shellmon] to add as digivolution source",
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
                            shellmon = cardSource;
                            yield return null;
                        }

                        selectCardEffect.SetUpCustomMessage("Select 1 [Shellmon] to add as digivolution source.", "The opponent is selecting 1 [Shellmon] to add as digivolution source.");
                        yield return StartCoroutine(selectCardEffect.Activate());

                        #endregion

                        if (shellmon != null)
                        {
                            #region Select Sangomon Permanent

                            int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, IsSangomon));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsSangomon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                sangomon = permanent;
                                yield return null;
                            }

                            selectPermanentEffect.SetUpCustomMessage("Select 1 [Sangomon] to add digivolution sources.", "The opponent is selecting 1 [Sangomon] to add digivolution sources.");
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            #endregion

                            if (sangomon != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(sangomon.AddDigivolutionCardsBottom(new List<CardSource>() { shellmon }, activateClass));
                                if (sangomon.DigivolutionCards.Contains(shellmon))
                                {
                                    #region Digivolve into MarineBullmon

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                        sangomon,
                                        IsMarineBullmon,
                                        payCost: true,
                                        reduceCostTuple: null,
                                        fixedCostTuple: null,
                                        ignoreDigivolutionRequirementFixedCost: 3,
                                        isHand: true,
                                        activateClass: activateClass,
                                        successProcess: null,
                                        ignoreSelection:true,
                                        failedProcess:FailureProcess()));

                                    #endregion
                                }

                                IEnumerator FailureProcess()
                                {
                                    List<IDiscardHand> discardHands = new List<IDiscardHand>() { new IDiscardHand(card, null) };
                                    yield return ContinuousController.instance.StartCoroutine(new IDiscardHands(discardHands, null).DiscardHands());
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 level 4 or lower [Aqua]/[Sea Animal] digimon from sources", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT22_024_OnEndAttack");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Attack] [Once Per Turn] You may play 1 level 4 or lower Digimon card with [Aqua] or [Sea Animal] in any of its traits from this Digimon's digivolution cards without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnEndAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.PermanentOfThisCard().DigivolutionCards.Filter(x => CanSelectCardCondition(x)).Count >= 1;
                }

                bool CanSelectCardCondition(CardSource source)
                {
                    return source.IsDigimon
                        && source.HasLevel && source.Level <= 4
                        && (source.ContainsTraits("Sea Animal") || source.ContainsTraits("Aqua"));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.PermanentOfThisCard().DigivolutionCards.Filter(x => CanSelectCardCondition(x)).Count >= 1)
                    {
                        CardSource selectedCard = null;

                        #region Select Digimon From Digivolution Cards

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 digimon to play",
                                    maxCount: 1,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        selectCardEffect.SetUpCustomMessage("Select 1 digimon to play.", "The opponent is selecting 1 digimon to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");
                        yield return StartCoroutine(selectCardEffect.Activate());

                        #endregion

                        if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: new List<CardSource>() { selectedCard }, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.DigivolutionCards, activateETB: true));
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}