using System.Collections;
using System.Collections.Generic;

//Reina Oumi
namespace DCGO.CardEffects.EX11
{
    public class EX11_059 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared Card Condition
            bool IsNSo(CardSource cardSource)
            {
                return cardSource.EqualsTraits("NSo");
            }
            #endregion

            #region Shared SOYMP / OP
            string SharedEffectName = "Trash 1 [NSo] card from hand to Draw 1 and gain Memory +1";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                additionalActivateCondition: AdditionalActivateCoroutine,
                optional: false,
                isSkippable: true,
                onPlay: true,
                startOfYourMainPhase: true);

            string SharedEffectDescription(string tag)=> $"[{tag}] By trashing 1 [NSo] trait card from your hand, <Draw 1> and gain 1 memory.";

            bool AdditionalActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                return CardEffectCommons.HasMatchConditionOwnersHand(card, IsNSo);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: IsNSo,
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
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DNA into [NSo] Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When any of your [NSo] trait Digimon are deleted, by suspending this Tamer, 1 of your [NSo] trait Digimon and 1 [NSo] trait Digimon card in the trash may DNA digivolve into a Digimon card with the [NSo] trait in the hand.";
                }

                bool IsNSoPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) 
                        && IsNSo(permanent.TopCard);
                }

                bool IsNSoDigimonCard(CardSource cardSource)
                {
                    return cardSource.IsDigimon 
                        && IsNSo(cardSource);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, IsNSoPermanent, activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass) 
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() },
                            CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DNADigivolveWithHandOrTrashCardIntoHandOrTrash(
                        targetCardCondition: IsNSoDigimonCard, 
                        permanentCondition: IsNSoPermanent, 
                        digivolutionCardCondition: IsNSoDigimonCard,
                        payCost: true,
                        isWithHandCard: false, 
                        isIntoHandCard: true,
                        activateClass: activateClass,
                        successProcess: null,
                        failedProcess: null,
                        isOptional: true));
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
