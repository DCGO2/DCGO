using System.Collections;
using System.Collections.Generic;

// Rei Katsura
namespace DCGO.CardEffects.P
{
    public class P_242 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 [System]/[Life]/[Transmutation] to <Draw 1> and gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Start of Your Main Phase] By trashing 1 card with the [System], [Life] or [Transmutation] trait card from your hand, <Draw 1> and gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("System")
                        || cardSource.EqualsTraits("Life")
                        || cardSource.EqualsTraits("Transmutation");
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
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
                            yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                            yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                        }
                    }
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspending, you may link 1 [System]/[Life]/[Transmutation] from trash for 1 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() 
                    => "[Main] By suspending this Tamer, you may link 1 [System], [Life] or [Transmutation] trait Digimon card from your trash to 1 of your Digimon with the cost reduced by 1.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass);
                }
    
                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return (cardSource.EqualsTraits("System")
                            || cardSource.EqualsTraits("Life")
                            || cardSource.EqualsTraits("Transmutation"))
                        && cardSource.CanLink(true);
                }

                bool CanSelectPermanentCondition(Permanent permanent, CardSource cardSource)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && cardSource.CanLinkToTargetPermanent(permanent, true);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());
                    
                    #region Link Cost Reduction
                    ICardEffect GetCardEffect(EffectTiming _timing)
                    {
                        if (_timing == EffectTiming.None)
                        {
                            return CardEffectFactory.GrantedReduceLinkCostClass(
                                card: card, 
                                reducedCost: 1,
                                cardSourceCondition: _ => true,
                                permanentCondition: _ => true,
                                rootCondition: _ => true
                            );
                        }

                        return null;
                    }

                    card.Owner.UntilCalculateFixedCostEffect.Add(GetCardEffect);
                    #endregion

                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to link",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage(
                                "Select 1 card to link.",
                                "The opponent is selecting 1 card to link.");

                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, permanent => CanSelectPermanentCondition(permanent, cardSource)))
                            {
                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: permanent => CanSelectPermanentCondition(permanent, cardSource),
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to link to", "Your opponent is choosing 1 digimon to link to");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new ILinkCard(true, cardSource, permanent, activateClass).LinkCard());
                                }
                            }
                        }
                    }

                    #region Remove Link Cost Reduction
                    card.Owner.UntilCalculateFixedCostEffect.Remove(GetCardEffect);
                    #endregion
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}
