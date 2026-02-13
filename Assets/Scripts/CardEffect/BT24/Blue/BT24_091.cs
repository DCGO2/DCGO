using System;
using System.Collections;
using System.Collections.Generic;

// Tidal Stream
namespace DCGO.CardEffects.BT24
{
    public class BT24_091 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Ignore Colour Requirement

            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);

                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.HasMatchConditionPermanent((permanent) => permanent.TopCard.Owner == card.Owner && (permanent.IsTamer || permanent.IsDigimon) && permanent.TopCard.HasTSTraits, true);

                bool CardCondition(CardSource cardSource)
                    => cardSource == card;
            }

            #endregion

            #region Link Condition

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.IsDigimon && targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: PermanentCondition, linkCost: 3, card: card));
            }

            #endregion

            #region Link

            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.LinkEffect(card));
            }

            #endregion

            #endregion

            #region Security Effect

            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(
                    card: card,
                    cardEffects: ref cardEffects,
                    effectName: "Bounce all opponent's lowest level. Unsuspend a Digimon. Then, you may link this card.");
            }

            #endregion

            #region Main

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Bounce all opponent's lowest level. Unsuspend a Digimon. Then, you may link this card.", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] Return all of your opponent's lowest level Digimon to the hand. If this effect returned, 1 of your [TS] trait Digimon unsuspends. Then, you may link this card to 1 of your Digimon on the field without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return permanent.IsDigimon
                        && CardEffectCommons.IsOwnerPermanent(permanent, card)
                        && card.CanLinkToTargetPermanent(permanent, false, true);
                }

                bool CanSelectTargetCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.HasLevel)
                        {
                            if (CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy))
                            {
                                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectDigimonCondition(Permanent permament)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permament, card) && permament.TopCard.HasTSTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    #region Return all Lowest Level Digimon to Hand

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTargetCondition))
                    {
                        List<Permanent> bouncePermanents = new List<Permanent>();

                        foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaDigimons())
                        {
                            if (CanSelectTargetCondition(permanent))
                            {
                                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    bouncePermanents.Add(permanent);
                                }
                            }
                        }

                        if (bouncePermanents.Count >= 1)
                        {
                            Hashtable _hashtable = new Hashtable();
                            _hashtable.Add("CardEffect", activateClass);

                            yield return ContinuousController.instance.StartCoroutine(new HandBounceClaass(bouncePermanents, _hashtable).Bounce());

                            #region if returned, unsuspend

                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDigimonCondition))
                            {
                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectDigimonCondition));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectDigimonCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.UnTap,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will unsuspend", "The opponent is selecting 1 Digimon to unsuspend.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                            }

                            #endregion
                        }
                    }

                    #endregion

                    #region Select Digimon To Link

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition, true))
                    {
                        Permanent selectedPermanent = null;
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to link.", "The opponent is selecting 1 Digimon to link.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddLinkCard(card, activateClass));
                    }

                    #endregion
                }
            }

            #endregion

            #region Link ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Bounce 1 opponent's lowest level Digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsLinkedEffect(true);
                activateClass.SetHashString("WA_BT24-091");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] [Once Per Turn] Return 1 of your opponent's lowest level Digimon to the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanSelectOpponentDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectOpponentDigimon);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectOpponentDigimon));
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOpponentDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Bounce,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to bounce.", "The opponent is selecting a digimon to bounce.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
