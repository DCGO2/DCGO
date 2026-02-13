using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Akihiro Kurata
namespace DCGO.CardEffects.EX10
{
    public class EX10_066 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Turn
            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }
            #endregion

            #region End of Your Turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [Belphemon] in name", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn] If you have 6 or fewer cards in your hand, by placing this Tamer as the bottom digivolution card of any of your Digimon with [Belphemon] in their names, that Digimon may digivolve into a Digimon card with [Belphemon] in its name in its name in the trash without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           card.Owner.HandCards.Count <= 6;
                }

                bool CanSelectBelphemon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.ContainsCardName("Belphemon");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent digivolutionCardAdded = null;

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectBelphemon))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectBelphemon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get digivolution cards.", "The opponent is selecting 1 Digimon that will get digivolution cards.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent oldThisCardPermanent = card.PermanentOfThisCard();

                            CardSource topCard = oldThisCardPermanent.TopCard;

                            yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(new List<Permanent[]>() { new Permanent[] { oldThisCardPermanent, permanent } }, false, activateClass).PlacePermanentToDigivolutionCards());

                            if (oldThisCardPermanent.TopCard == null && CardEffectCommons.IsExistOnBattleArea(card))
                            {
                                if (permanent.DigivolutionCards.Contains(topCard))
                                {
                                    digivolutionCardAdded = permanent;
                                }
                            }
                        }
                    }

                    if(digivolutionCardAdded != null)
                    {
                        bool CanSelectEvoBelphemon(CardSource source)
                        {
                            return source.ContainsCardName("Belphemon") &&
                                   source.CanPlayCardTargetFrame(digivolutionCardAdded.PermanentFrame, false, activateClass, SelectCardEffect.Root.Trash);
                        }

                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectEvoBelphemon))
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: digivolutionCardAdded,
                            cardCondition: CanSelectEvoBelphemon,
                            payCost: false,
                            reduceCostTuple: null,
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: false,
                            activateClass: activateClass,
                            successProcess: null));
                        }
                    }

                    yield return null;
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