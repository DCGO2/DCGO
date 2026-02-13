using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

//MasterBlimpmon
namespace DCGO.CardEffects.BT24
{
    public class BT24_062 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel4
                        && (targetPermanent.TopCard.HasTSTraits
                            || targetPermanent.TopCard.EqualsTraits("Machine")
                            || targetPermanent.TopCard.EqualsTraits("Cyborg"));
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, false, card, null));
            }

            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region Armor Purge
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.ArmorPurgeEffect(card));
            }
            #endregion

             #region Assembly
            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect($"Assembly", CanUseCondition, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        AssemblyConditionElement element = new AssemblyConditionElement(CanSelectCardCondition);

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            return cardSource.EqualsCardName("Blimpmon")
                                || (cardSource.IsTamer
                                    && cardSource.HasTSTraits);
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element: element,
                            CanTargetCondition_ByPreSelecetedList: null,
                            selectMessage: "[Blimpmon]/Tamer Card w/ [TS] trait",
                            elementCount: 1,
                            reduceCost: 2);

                        return assemblyCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Shared EOA / EOOT OPT

            string SharedEffectName = "Play a 5- Cost [Machine]/[Cyborg]/[TS] from digivolution cards.";

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] You may play 1 play cost 5 or lower card with the [Machine], [Cyborg] or [TS] trait from this Digimon's digivolution cards without paying the cost.";

            string SharedEffectHash = "BT24_062_EOT_EOOT";

            bool SharedCanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return CardEffectCommons.IsExistOnBattleArea(card)
                    && card.PermanentOfThisCard().DigivolutionCards.Any(cardSource => CanSelectCardCondition(cardSource));
            } 

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.HasPlayCost
                    && cardSource.GetCostItself <= 5
                    && (cardSource.HasTSTraits
                        || cardSource.EqualsTraits("Machine")
                        || cardSource.EqualsTraits("Cyborg"));
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent selectedPermanent = card.PermanentOfThisCard();
                
                List<CardSource> selectedCards = new List<CardSource>();

                int maxCount = Math.Min(1, selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition));

                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to play.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: selectedPermanent.DigivolutionCards,
                            canLookReverseCard: false,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to play.", "The opponent is selecting 1 digivolution card to play.");
                selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                yield return StartCoroutine(selectCardEffect.Activate());

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCards.Add(cardSource);
                    yield return null;
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                    cardSources: selectedCards,
                    activateClass: activateClass,
                    payCost: false,
                    isTapped: false,
                    root: SelectCardEffect.Root.DigivolutionCards,
                    activateETB: true));
            }

            #endregion

            #region End of Attack

            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(hash => SharedCanActivateCondition(hash, activateClass), hash => SharedActivateCoroutine(hash, activateClass), 1, true, SharedEffectDescription("End of Attack"));
                activateClass.SetHashString(SharedEffectHash);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
            }

            #endregion

            #region End of Opponent's Turn

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(hash => SharedCanActivateCondition(hash, activateClass), hash => SharedActivateCoroutine(hash, activateClass), 1, true, SharedEffectDescription("End of Opponent's Turn"));
                activateClass.SetHashString(SharedEffectHash);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOpponentTurn(card);
                }
            }

            #endregion

            #region Your Turn - ESS

            if (timing == EffectTiming.None)
            {
                CanNotSwitchAttackTargetClass canNotSwitchAttackTargetClass = new CanNotSwitchAttackTargetClass();
                canNotSwitchAttackTargetClass.SetUpICardEffect("This Digimon's attack target can't be switched.", CanUseCondition, card);
                canNotSwitchAttackTargetClass.SetUpCanNotSwitchAttackTargetClass(PermanentCondition: PermanentCondition);
                canNotSwitchAttackTargetClass.SetIsInheritedEffect(true);
                cardEffects.Add(canNotSwitchAttackTargetClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent != null && permanent.TopCard != null && permanent == card.PermanentOfThisCard();
                }
            }

            #endregion

            return cardEffects;
        }
    }
}