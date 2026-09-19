using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Skadimon
namespace DCGO.CardEffects.EX11
{
    public class EX11_017
        : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Ice-Snow") && targetPermanent.TopCard.IsLevel5;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Iceclad

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.IcecladSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region Barrier
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP / WD / WA

            string SharedEffectName = "Play [Suzune Kazuki] or a level 4 or lower [Ice-Snow] Digimon from hand.";

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] You may play 1 [Suzune Kazuki] or level 4 or lower [Ice-Snow] trait Digimon card from your hand without paying the cost.";

            string SharedHashString = "EX11_017_OP_WD_WA_Play_Card";

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card)
                    && CardEffectCommons.HasMatchConditionOwnersHand(card, ValidCardToPlay);
            }

            bool ValidCardToPlay(CardSource cardSource)
            {
                return cardSource.EqualsCardName("Suzune Kazuki")
                    || (cardSource.IsDigimon
                        && cardSource.EqualsTraits("Ice-Snow")
                        && cardSource.HasLevel
                        && cardSource.Level <= 4);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                List<CardSource> selectedCards = new List<CardSource>();

                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: ValidCardToPlay,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    selectCardCoroutine: SelectCardCoroutine,
                    afterSelectCardCoroutine: null,
                    mode: SelectHandEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                yield return StartCoroutine(selectHandEffect.Activate());

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
                    root: SelectCardEffect.Root.Hand,
                    activateETB: true));
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition,(hash) => SharedActivateCoroutine(hash, activateClass), 1, true, SharedEffectDescription("On Play"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card)
                        && CardEffectCommons.IsExistOnBattleArea(card);
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition,(hash) => SharedActivateCoroutine(hash, activateClass), 1, true, SharedEffectDescription("When Digivolving"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card)
                        && CardEffectCommons.IsExistOnBattleArea(card);
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition,(hash) => SharedActivateCoroutine(hash, activateClass), 1, true, SharedEffectDescription("When Attacking"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card)
                        && CardEffectCommons.IsExistOnBattleArea(card);
                }
            }
            #endregion

            #region All Turns
            if(timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash any 3 sources from opponent's Digimon. 1 of their Digimon with no sources can't suspend until their turn ends.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("EX11_017_AT_Trash_3_Stun");
                cardEffects.Add(activateClass);

                string EffectDescription() => "[All Turns] [Once Per Turn] When other Digimon are played or digivolve, trash any 3 digivolution cards from your opponent's Digimon. Then, 1 of their Digimon with no digivolution cards can't suspend until their turn ends.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition)
                            || CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition));
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent != card.PermanentOfThisCard()
                        && CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);
                }

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleArea(card);

                bool CanSelectTrashSourceCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1;
                }

                bool CanSelectCardCondition(CardSource cardSource) => !cardSource.CanNotTrashFromDigivolutionCards(activateClass);

                bool StunPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.HasNoDigivolutionCards;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                                permanentCondition: CanSelectTrashSourceCondition,
                                cardCondition: CanSelectCardCondition,
                                maxCount: 3,
                                canNoTrash: false,
                                isFromOnly1Permanent: false,
                                activateClass: activateClass
                            ));

                    if(CardEffectCommons.HasMatchConditionOpponentsPermanent(card, StunPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: StunPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will not be able to suspend.",
                            "The opponent is selecting 1 Digimon that will not be able to suspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                            canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCanNotSuspendCondition, card);
                            canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCanNotSuspendCondition);
                            permanent.UntilOwnerTurnEndEffects.Add(_ => canNotSuspendClass);

                            if (!permanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                                    .CreateDebuffEffect(permanent));
                            }

                            bool CanUseCanNotSuspendCondition(Hashtable hashtableCanNotSuspend)
                            {
                                return permanent.TopCard != null && !permanent.TopCard.CanNotBeAffected(activateClass);
                            }

                            bool PermanentCanNotSuspendCondition(Permanent permanentCanNotSuspend)
                            {
                                return permanentCanNotSuspend == permanent;
                            }
                        }
                    }
                }
            }
            #endregion            

            return cardEffects;
        }
    }
}