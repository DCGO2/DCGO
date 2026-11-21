using System.Collections;
using System.Collections.Generic;

// Craniamon
namespace DCGO.CardEffects.EX13
{
    public class EX13_062 : CEntity_Effect 
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Assembly
            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect("Assembly", CanUseCondition, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                    => true;

                bool IsAssemblyCard(CardSource assemblyCard)
                    => assemblyCard != null
                        && assemblyCard.Owner == card.Owner
                        && assemblyCard.HasDigimonColor(CardColor.Black)
                        && assemblyCard.HasBlocker;

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource != card) return null;

                    AssemblyConditionElement level5Element = new AssemblyConditionElement(assemblyCard => IsAssemblyCard(assemblyCard) && assemblyCard.IsLevel5, elementCount: 1);
                    AssemblyConditionElement level4Element = new AssemblyConditionElement(assemblyCard => IsAssemblyCard(assemblyCard) && assemblyCard.IsLevel4, elementCount: 1);
                    AssemblyConditionElement level3Element = new AssemblyConditionElement(assemblyCard => IsAssemblyCard(assemblyCard) && assemblyCard.IsLevel3, elementCount: 1);

                    return new AssemblyCondition(
                        elements: new List<AssemblyConditionElement>() { level5Element, level4Element, level3Element },
                        reduceCost: 5);
                }
            }
            #endregion

            Permanent thisPermanent = card.PermanentOfThisCard();

            #region Shared On Play / When Digivolving
            string SharedEffectName = "Your opponent's effects don't affect this Digimon until their turn ends";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    onPlay: true,
                    whenDigivolving: true);

            string SharedEffectDescription(string tag) => $"[{tag}] Your opponent's effects don't affect this Digimon until their turn ends";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (thisPermanent != null)
                {
                    CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                    canNotAffectedClass.SetUpICardEffect("Your opponent's effects don't affect this Digimon until their turn ends", CanUseCondition1, card);
                    canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                    thisPermanent.UntilOpponentTurnEndEffects.Add((_timing) => canNotAffectedClass);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(thisPermanent));

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return thisPermanent.TopCard != null;
                    }

                    bool CardCondition(CardSource cardSource)
                    {
                        return thisPermanent.TopCard != null
                            && thisPermanent.TopCard.Owner.GetBattleAreaPermanents().Contains(thisPermanent)
                            && cardSource == thisPermanent.TopCard;
                    }

                    bool SkillCondition(ICardEffect cardEffect)
                    {
                        return cardEffect != null
                            && cardEffect.EffectSourceCard != null
                            && cardEffect.EffectSourceCard.Owner == card.Owner.Enemy;
                    }
                }
            }
            #endregion

            #region All Turns - OPT
            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("EX13_062_Delete");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon suspends, you may delete all of your opponent's Digimon with the lowest play cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenSelfPermanentSuspends(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool PermanentCondition(Permanent permanent) => CardEffectCommons.IsMinCost(permanent, card.Owner.Enemy, true);

                    if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                    {
                        List<Permanent> destroyTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(PermanentCondition);
                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(destroyTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                    }

                    yield return null;
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnUnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain +3000 DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When this Digimon unsuspends, it gets +3000 DP until your turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenSelfPermanentUnsuspends(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (thisPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(thisPermanent));

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            thisPermanent,
                            +3000,
                            EffectDuration.UntilOwnerTurnEnd,
                            activateClass
                        ));
                    }

                    yield return null;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
