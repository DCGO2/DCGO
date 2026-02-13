using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT19
{
    public class BT19_084 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("+1 Memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Start of Your Main Phase] If you have face up security cards, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return isExistOnField(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                bool HasFaceUpSecurity()
                {
                    return card.Owner.SecurityCards.Count(source => !source.IsFlipped) > 0;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return isExistOnField(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if(HasFaceUpSecurity())
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnDeclaration)
            {
                List<CardSource> cards = new List<CardSource>();
                CardSource selectedCard = null;
                Permanent selectedParmanent = null;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("One of your Digimon digivolves into Digimon card in security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] By suspending this Tamer, 1 of your Digimon digivolves into a Digimon card in your face up security cards. If this effect digivolved, you may place 1 Digimon card with the [Royal Base] trait from your hand face up as your bottom security card.";
                }

                bool OwnersDigivolveTarget(Permanent permanent)
                {
                    if(CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if(card.Owner.SecurityCards.Count(cardSource => 
                            !cardSource.IsFlipped && 
                            cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, PayCost: false, activateClass, root: SelectCardEffect.Root.Security)) > 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool DigivolveTarget(CardSource cardSource)
                {
                    return !cardSource.IsFlipped &&
                            cardSource.CanPlayCardTargetFrame(selectedParmanent.PermanentFrame, PayCost: false, activateClass, root: SelectCardEffect.Root.Security);
                }

                bool IsRoyalBaseDigimon(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           cardSource.EqualsTraits("Royal Base");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return isExistOnField(card) &&
                           CardEffectCommons.IsOwnerTurn(card) &&
                           CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return isExistOnField(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() },
                            CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, OwnersDigivolveTarget))
                    {                       
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OwnersDigivolveTarget,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectDigivolveTarget,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    IEnumerator SelectDigivolveTarget(Permanent permanent)
                    {
                        selectedParmanent = permanent;

                        yield return null;
                    }

                    if (selectedParmanent != null)
                    {
                        cards = card.Owner.SecurityCards.Filter(source => !source.IsFlipped);

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: DigivolveTarget,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to digivolve.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: cards,
                            canLookReverseCard: false,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());



                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;

                            yield return null;
                        }

                        if (selectedCard != null)
                        {
                            PlayCardClass playCardClass = new PlayCardClass(
                                        cardSources: new List<CardSource>() { selectedCard },
                                        hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                        payCost: true,
                                        targetPermanent: selectedParmanent,
                                        isTapped: false,
                                        root: SelectCardEffect.Root.Security,
                                        activateETB: true);

                            yield return ContinuousController.instance.StartCoroutine(playCardClass.PlayCard());

                            if (CardEffectCommons.IsDigivolvedByTheEffect(selectedParmanent, selectedCard, activateClass))
                            {
                                if (card.Owner.CanAddSecurity(activateClass))
                                {
                                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                    selectHandEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: IsRoyalBaseDigimon,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: 1,
                                        canNoSelect: true,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        selectCardCoroutine: null,
                                        afterSelectCardCoroutine: null,
                                        mode: SelectHandEffect.Mode.PutSecurityBottom,
                                        cardEffect: activateClass);

                                    selectHandEffect.SetIsFaceup();

                                    yield return StartCoroutine(selectHandEffect.Activate());
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Security Effect

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}