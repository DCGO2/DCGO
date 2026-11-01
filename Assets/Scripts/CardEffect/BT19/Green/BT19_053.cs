using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// QueenBeemon
namespace DCGO.CardEffects.BT19
{
    public class BT19_053 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Royal Base");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false,
                    card: card, condition: null, level: 5));
            }
            #endregion

            #region Alliance
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play [Royal Base] Digimon from face up security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT19_053_WA");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] [Once Per Turn] You may play 1 [Royal Base] trait Digimon card from your face up security cards with the play cost reduced by 8.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.EqualsTraits("Royal Base")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: true, cardEffect: activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.Owner.SecurityCards.Count(CanSelectCardCondition) > 0;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    #region reduce play cost
                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect($"Play Cost -8", CanUseCondition1, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                    Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                    card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                    ICardEffect GetCardEffect(EffectTiming _timing)
                    {
                        if (_timing == EffectTiming.None)
                        {
                            return changeCostClass;
                        }

                        return null;
                    }

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return true;
                    }

                    int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                    {
                        if (CardSourceCondition(cardSource)
                        && RootCondition(root)
                        && PermanentsCondition(targetPermanents))
                        {
                            Cost -= 8;
                        }
                        
                        return Cost;
                    }

                    bool PermanentsCondition(List<Permanent> targetPermanents)
                    {
                        return targetPermanents == null || targetPermanents.Count(targetPermanent => targetPermanent != null) == 0;
                    }

                    bool CardSourceCondition(CardSource cardSource)
                    {
                        return cardSource.IsDigimon
                            && cardSource.HasPlayCost
                            && cardSource.EqualsTraits("Royal Base");
                    }

                    bool RootCondition(SelectCardEffect.Root root)
                    {
                        return true;
                    }

                    bool isUpDown()
                    {
                        return true;
                    }
                    #endregion

                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => false,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 card to play.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Security,
                        customRootCardList: card.Owner.SecurityCards.Filter(source => !source.IsFlipped),
                        canLookReverseCard: false,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
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
                        payCost: true,
                        isTapped: false,
                        root: SelectCardEffect.Root.Security,
                        activateETB: true));

                    #region release effect reducing play cost 
                    card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);
                    #endregion
                }
            }
            #endregion

            #region All Turns - When Removed
            if (timing == EffectTiming.WhenRemoveField)
            {
                List<Permanent> removedPermanents = new List<Permanent>();

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place those Digimon face up as your bottom security cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT19_053_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When any of your [Royal Base] trait Digimon would leave the battle area other than in battle, you may place those Digimon face up as your bottom security cards.";
                }

                bool HasRoyalBaseDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsTraits("Royal Base")
                        && permanent.willBeRemoveField;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, HasRoyalBaseDigimon)
                        && !CardEffectCommons.IsByBattle(hashtable);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    removedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable).Filter(HasRoyalBaseDigimon);

                    return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && card.Owner.CanAddSecurity(activateClass)
                        && removedPermanents.Some(permanent => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    foreach(Permanent permanent in removedPermanents)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IPutSecurityPermanent(
                        permanent,
                        CardEffectCommons.CardEffectHashtable(activateClass), toTop: false, isFaceup: true).PutSecurity());

                        permanent.HideHandBounceEffect();
                        permanent.HideDeckBounceEffect();
                    }

                    activateClass.RemoveUse();
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
