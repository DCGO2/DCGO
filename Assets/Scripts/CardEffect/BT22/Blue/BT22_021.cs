using System;
using System.Collections;
using System.Collections.Generic;

// Shellmon
namespace DCGO.CardEffects.BT22
{
    public class BT22_021 : CEntity_Effect
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
                    && source.HasLevel && source.IsLevel3
                    && (source.ContainsTraits("Sea Animal") || source.ContainsTraits("Aqua"));
                }

                string[] decodeStrings = { "(Lv.3 w/[Aqua]/[Sea Animal] in any trait)", "Level 3 Digimon card with [Aqua] or [Sea Animal]" };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(card: card, isInheritedEffect: false, decodeStrings: decodeStrings, sourceCondition: SourceCondition, condition: null));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 level 5 or lower [Aqua]/[Sea Animal] digimon from hand under any digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] You may place 1 level 5 or lower Digimon card with [Aqua] or [Sea Animal] in any of its traits from your hand as any of your Digimon's bottom digivolution card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsValidDigimonCondition)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, IsValidHandCardCondition);
                }

                bool IsValidDigimonCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool IsValidHandCardCondition(CardSource source)
                {
                    return source.IsDigimon
                        && source.HasLevel && source.Level <= 5
                        && (source.HasSeaAnimalTraits || source.HasAquaTraits);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, IsValidHandCardCondition) && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsValidDigimonCondition))
                    {
                        #region Select Hand Card

                        CardSource selectedCard = null;
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, IsValidHandCardCondition));
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsValidHandCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }
                        selectHandEffect.SetUpCustomMessage("Select 1 card to add as digivolution card,", "The opponent is selecting 1 card to add as digivolution card,");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");
                        yield return StartCoroutine(selectHandEffect.Activate());

                        #endregion

                        if (selectedCard != null)
                        {
                            #region Select Permanent

                            Permanent selectedPermanent = null;
                            int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsValidDigimonCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsValidDigimonCondition,
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
                                selectedPermanent = permanent;
                                yield return null;
                            }
                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to add digivolution source.", "The opponent is selecting 1 Digimon to add digivolution source.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            #endregion

                            if (selectedPermanent != null && selectedCard != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass));
                            }
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 level 5 or lower [Aqua]/[Sea Animal] digimon from hand under any digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolging] You may place 1 level 5 or lower Digimon card with [Aqua] or [Sea Animal] in any of its traits from your hand as any of your Digimon's bottom digivolution card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsValidDigimonCondition)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, IsValidHandCardCondition);
                }

                bool IsValidDigimonCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool IsValidHandCardCondition(CardSource source)
                {
                    return source.IsDigimon
                        && source.HasLevel && source.Level <= 5
                        && (source.HasSeaAnimalTraits || source.HasAquaTraits);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, IsValidHandCardCondition) && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsValidDigimonCondition))
                    {
                        #region Select Hand Card

                        CardSource selectedCard = null;
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, IsValidHandCardCondition));
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsValidHandCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }
                        selectHandEffect.SetUpCustomMessage("Select 1 card to add as digivolution card,", "The opponent is selecting 1 card to add as digivolution card,");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");
                        yield return StartCoroutine(selectHandEffect.Activate());

                        #endregion

                        if (selectedCard != null)
                        {
                            #region Select Permanent

                            Permanent selectedPermanent = null;
                            int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsValidDigimonCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsValidDigimonCondition,
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
                                selectedPermanent = permanent;
                                yield return null;
                            }
                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to add digivolution source.", "The opponent is selecting 1 Digimon to add digivolution source.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            #endregion

                            if (selectedPermanent != null && selectedCard != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass));
                            }
                        }
                    }
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(isInheritedEffect: true, card: card, condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}