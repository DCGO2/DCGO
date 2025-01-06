using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX8
{
    public class SuzuneKazuki_EX8_066 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }

            #region All Turns
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash any 1 digivolution card from opponents digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When one of your Digimon are played or digivolve, if any of them have the [Ice-Snow] trait, by suspending this Tamer, trash any 1 digivolution card from your opponent's Digimon.";
                }

                bool EnterFieldPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsOwnerPermanent(permanent, card) &&
                           permanent.IsDigimon && permanent.TopCard.EqualsTraits("Ice-Snow");
                }

                bool OpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return !cardSource.CanNotTrashFromDigivolutionCards(activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, EnterFieldPermanent) || CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, EnterFieldPermanent)) &&
                           CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return isExistOnField(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                        permanentCondition: OpponentsDigimon,
                        cardCondition: CanSelectCardCondition,
                        maxCount: 1,
                        canNoTrash: false,
                        isFromOnly1Permanent: false,
                        activateClass: activateClass
                    ));
                }
            }
            #endregion

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            return cardEffects;
        }
    }
}