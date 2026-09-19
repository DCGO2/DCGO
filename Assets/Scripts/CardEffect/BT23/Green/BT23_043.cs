using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// CannonBeemon
namespace DCGO.CardEffects.BT23
{
    public class BT23_043 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.IsLevel4
                        && (targetPermanent.TopCard.HasCSTraits || targetPermanent.TopCard.HasRoyalBaseTraits);
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion Alternative Digivolution Condition

            #region Opponents Turn - Security

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistInSecurity(card, false) &&
                           CardEffectCommons.IsOpponentTurn(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.EqualsTraits("Royal Base"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.BlockerStaticEffect(
                    permanentCondition: PermanentCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition));
            }

            #endregion

            #region All Turns - OPT

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By flipping your top face up security card face down, prevent remove from field", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT23_AT");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon would leave the battle area other than by your effects, by flipping your top face-up security card face down, it doesn't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card)
                        && !CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOwnerEffect(cardEffect, card));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersSecurity(card, _ => true, false);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    var flippedSecurity = false;
                    foreach (CardSource source in card.Owner.SecurityCards)
                    {
                        if (source.IsFlipped) continue;

                        source.SetReverse();
                        GManager.OnSecurityStackChanged?.Invoke(card.Owner.Enemy);
                        flippedSecurity = true;
                        break;
                    }

                    if (flippedSecurity)
                    {
                        var thisPermament = card.PermanentOfThisCard();

                        thisPermament.HideDeleteEffect();
                        thisPermament.HideHandBounceEffect();
                        thisPermament.HideDeckBounceEffect();
                        thisPermament.HideWillRemoveFieldEffect();

                        thisPermament.DestroyingEffect = null;
                        thisPermament.IsDestroyedByBattle = false;
                        thisPermament.HandBounceEffect = null;
                        thisPermament.LibraryBounceEffect = null;
                        thisPermament.willBeRemoveField = false;
                    }

                    yield return null;

                }
            }

            #endregion

            #region All Turns - ESS - OPT

            if (timing == EffectTiming.WhenRemoveField)
            {
                List<Permanent> removedPermanents = new List<Permanent>();

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By flipping your top face up security card face down, prevent remove from field", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT23_ESS_AT");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When any of your [Royal Base] trait Digimon would leave the battle area other than by your effects, by flipping your top face-up security card face down, 1 of those Digimon doesn't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, CanSelectPermanentCondition)
                        && !CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOwnerEffect(cardEffect, card));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.Owner.SecurityCards.Count(source => !source.IsFlipped) > 0;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasRoyalBaseTraits;
                }

                bool CanSelectPermament(Permanent permanent)
                {
                    return removedPermanents.Contains(permanent);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    removedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable).Filter(CanSelectPermanentCondition);

                    var flippedSecurity = false;
                    foreach (CardSource source in card.Owner.SecurityCards)
                    {
                        if (source.IsFlipped) continue;

                        source.SetReverse();
                        GManager.OnSecurityStackChanged?.Invoke(card.Owner.Enemy);
                        flippedSecurity = true;
                        break;
                    }

                    if (flippedSecurity)
                    {
                        Permanent selectedPermanent = null;
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermament));

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermament,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
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

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to prevent removal.", "The opponent is selecting 1 Digimon to prevent removal.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedPermanent != null)
                        {
                            selectedPermanent.HideDeleteEffect();
                            selectedPermanent.HideHandBounceEffect();
                            selectedPermanent.HideDeckBounceEffect();
                            selectedPermanent.HideWillRemoveFieldEffect();

                            selectedPermanent.DestroyingEffect = null;
                            selectedPermanent.IsDestroyedByBattle = false;
                            selectedPermanent.HandBounceEffect = null;
                            selectedPermanent.LibraryBounceEffect = null;
                            selectedPermanent.willBeRemoveField = false;
                        }

                    }

                    yield return null;

                }
            }

            #endregion

            return cardEffects;
        }
    }
}