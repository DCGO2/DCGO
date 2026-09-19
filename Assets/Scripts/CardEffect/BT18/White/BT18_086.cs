using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT18
{
    public class BT18_086 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play [Lucemon] card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Security] You may play 1 [Lucemon] from your trash without paying the cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Lucemon")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return card.Owner.ExecutingCards.Contains(card)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                        canTargetCondition: CanSelectCardCondition,
                        SelectCardEffect.Root.Trash,
                        activateClass,
                        payCost: false
                    ));
                }
            }
            #endregion

            #region Breeding
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Prevent Removal", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Breeding] [All Turns] When any of your [Lucemon: Satan Mode] would leave the battle area, by moving this Digimon to battle area, they don't leave.";
                }

                bool IsLucemonRemoved(Permanent permanent)
                {
                    return permanent.TopCard.Owner == card.Owner &&
                           permanent.TopCard.EqualsCardName("Lucemon: Satan Mode");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBreedingArea(card) &&
                           CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, IsLucemonRemoved);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBreedingArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.MovePermanent(card.Owner.GetBreedingAreaPermanents()[0].PermanentFrame));

                    foreach (Permanent removed in CardEffectCommons.GetPermanentsFromHashtable(hashtable).Filter(IsLucemonRemoved))
                    {
                        removed.willBeRemoveField = false;

                        removed.HideHandBounceEffect();
                        removed.HideDeckBounceEffect();
                        removed.HideWillRemoveFieldEffect();
                    }
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           (permanent.DP == 0);
                }

                bool NonWhiteDigimon(Permanent permanent)
                {
                    return permanent.IsDigimon &&
                           !permanent.TopCard.CardColors.Contains(CardColor.White) &&
                           permanent.TopCard.ContainsCardName("Lucemon");
                }

                bool CanUseCondition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionOwnersPermanent(card, NonWhiteDigimon);
                }

                string effectName = "[All Turns] While you have a non-white Digimon with [Lucemon] in its name, none of your 0 DP Digimon can be deleted.";

                cardEffects.Add(CardEffectFactory.CanNotBeDestroyedStaticEffect(
                    permanentCondition: PermanentCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition,
                    effectName: effectName
                ));
            }
            #endregion

            return cardEffects;
        }
    }
}