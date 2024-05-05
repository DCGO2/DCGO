using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class DoruGreymon_BT16_061 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.CardNames.Contains("Dorugamon"))
                    {
                        return true;
                    }

                    if(targetPermanent.TopCard.Level == 4 && targetPermanent.TopCard.ContainsTraits("SoC"))
                    {
                        return true;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region Collision
            if(timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.CollisionSelfStaticEffect(false,card));
            }
            #endregion

            #region All Turns

            #endregion

            #region All Turns - ESS
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 digimon 5 cost or less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] (Once Per Turn) When this Digimon deletes another Digimon, you may play 1 card with the [X Antibody] or [SoC] trait and a play cost of 5 or less from your trash without paying the cost.";
                }

                bool SelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.HasPlayCost && cardSource.BasePlayCostFromEntity <= 5)
                    {
                        if (cardSource.CardTraits.Contains("X Antibody") || cardSource.CardTraits.Contains("XAntibody"))
                        {
                            return true;
                        }

                        if (cardSource.ContainsTraits("SoC"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        bool WinnerCondition(Permanent permanent) => permanent.cardSources.Contains(card);
                        bool LoserCondition(Permanent permanent) => CardEffectCommons.IsOpponentPermanent(permanent, card);

                        if (CardEffectCommons.CanTriggerWhenDeleteOpponentDigimon(hashtable: hashtable, winnerCondition: WinnerCondition, loserCondition: LoserCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Exists(SelectCardCondition))
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: SelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: null,
                            message: "",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent:false,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass
                        );

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }
                    yield return null;
                }
            }
            #endregion

            if (timing == EffectTiming.None)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return true;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return null;
                }
            }

            return cardEffects;
        }
    }
}