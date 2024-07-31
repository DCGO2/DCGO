using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX7
{
    public class Zephagamon_EX7_036 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Security Attack +1

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card,
                    condition: null));
            }

            #endregion

            #region Vortex

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Vortex", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "<Vortex> [End of Your Turn] This Digimon may attack an opponent's Digimon. With this effect, it can attack the turn it was played.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           card.PermanentOfThisCard().CanAttack(activateClass, isVortex: true) &&
                           CardEffectCommons.HasMatchConditionOpponentsPermanent(card, permanent =>
                               permanent.IsDigimon &&
                               card.PermanentOfThisCard().CanAttackTargetDigimon(permanent, activateClass, isVortex: true));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    if (selectedPermanent.CanAttack(activateClass, isVortex: true))
                    {
                        SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                        selectAttackEffect.SetUp(
                            attacker: selectedPermanent,
                            canAttackPlayerCondition: () => false,
                            defenderCondition: _ => true,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                    }
                }
            }

            #endregion

            #region When Digivolving/ When Attacking Shared

            bool CanSelectPermanentSharedCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnBattleArea(permanent) &&
                       permanent.IsDigimon;
            }

            bool CanSelectSuspendedPermanentSharedCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                       permanent.IsSuspended;
            }
            
            bool CanActivateSharedCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] Suspend 1 Digimon. If this effect suspended your Digimon, return 1 of your opponent's suspended Digimon to the bottom of the deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentSharedCondition))
                    {
                        bool ownDigimon = false;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentSharedCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            ownDigimon = permanents.Some(permanent =>
                                CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card));

                            yield return null;
                        }

                        if (ownDigimon && CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendedPermanentSharedCondition))
                        {
                            selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition_ByPreSelecetedList: null,
                                canTargetCondition: CanSelectSuspendedPermanentSharedCondition,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to bottom deck.",
                                "The opponent is selecting 1 Digimon to bottom deck.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Attacking] Suspend 1 Digimon. If this effect suspended your Digimon, return 1 of your opponent's suspended Digimon to the bottom of the deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentSharedCondition))
                    {
                        bool ownDigimon = false;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentSharedCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            ownDigimon = permanents.Some(permanent =>
                                CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card));

                            yield return null;
                        }

                        if (ownDigimon && CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendedPermanentSharedCondition))
                        {
                            selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition_ByPreSelecetedList: null,
                                canTargetCondition: CanSelectSuspendedPermanentSharedCondition,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to bottom deck.",
                                "The opponent is selecting 1 Digimon to bottom deck.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }
                }
            }

            #endregion

            #region Rule Text

            if (timing == EffectTiming.None)
            {
                ChangeTraitsClass changeTraitsClass = new ChangeTraitsClass();
                changeTraitsClass.SetUpICardEffect("Trait: Has the [Bird Dragon] type", CanUseCondition, card);
                changeTraitsClass.SetUpChangeTraitsClass(changeeTraits: ChangeTraits);
                cardEffects.Add(changeTraitsClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                List<string> ChangeTraits(CardSource cardSource, List<string> cardTraits)
                {
                    if (cardSource == card)
                    {
                        cardTraits.Add("Bird Dragon");
                    }

                    return cardTraits;
                }
            }

            #endregion

            return cardEffects;
        }
    }
}