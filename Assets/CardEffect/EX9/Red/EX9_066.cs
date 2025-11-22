using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Tai Kamiya & Matt Ishida
namespace DCGO.CardEffects.EX9
{
    public class EX9_066 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 card, or Draw 1.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] You may return 1 Digimon card with [Greymon], [Garurumon] or [Omnimon] in its name from your trash to the hand. If this effect didn't return, <Draw 1>";
                }

                bool ReturnCard(CardSource source)
                {
                    return source.HasGreymonName ||
                           source.HasGarurumonName ||
                           source.ContainsCardName("Omnimon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool cardAdded = false;

                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, ReturnCard))
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: ReturnCard,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: SelectCardCoroutine,
                            message: "Select 1 Digimon card with [Greymon], [Garurumon] or [Omnimon] in its name",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.AddHand,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(List<CardSource> sources)
                        {
                            if (sources.Count > 0)
                                cardAdded = true;

                            yield return null;
                        }
                    }

                    if (!cardAdded)
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend this tamer to gain memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When any of your Digimon are played or digivolve, by suspending this Tamer, gain 1 memory if you have a Digimon with [Greymon] in its name. Then, gain 1 memory if you have a Digimon with [Garurumon] in its name.";
                }

                bool EnterFieldDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool DigimonWithGreymon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.HasGreymonName;
                }

                bool DigimonWithGarurumon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.HasGarurumonName;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return isExistOnField(card) &&
                           (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, EnterFieldDigimon) ||
                           CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, EnterFieldDigimon));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return isExistOnField(card) && 
                           CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int plusMemory = 0;

                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    if (CardEffectCommons.HasMatchConditionPermanent(DigimonWithGreymon))
                        plusMemory++;

                    if (CardEffectCommons.HasMatchConditionPermanent(DigimonWithGarurumon))
                        plusMemory++;

                    if (card.Owner.CanAddMemory(activateClass))
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(plusMemory, activateClass));
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