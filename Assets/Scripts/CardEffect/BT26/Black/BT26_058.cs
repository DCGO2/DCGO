using System.Collections;
using System.Collections.Generic;

// HiAndromon
namespace DCGO.CardEffects.BT26
{
    public class BT26_058 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("CS");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

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

            #region Shared When Digivolving / When Attacking
            string SharedEffectName()
                => "1 of your [CS] Digimon isn't affected by opponent's Digimon effects";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] Your opponent's Digimon effects don't affect 1 of your Digimon with the [CS] trait until their turn ends.";

            bool CanSelectPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.EqualsTraits("CS");

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 [CS] Digimon that isn't affected by opponent's Digimon effects.", "The opponent is selecting 1 [CS] Digimon that isn't affected by opponent's Digimon effects.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                    canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's Digimon effects", CanUseCondition1, card);
                    canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition1, SkillCondition: SkillCondition1);
                    permanent.UntilOpponentTurnEndEffects.Add((_timing) => canNotAffectedClass);

                    bool CanUseCondition1(Hashtable _hashtable) => CardEffectCommons.IsPermanentExistsOnBattleArea(permanent);

                    bool CardCondition1(CardSource cardSource) => CardEffectCommons.IsPermanentExistsOnBattleArea(permanent) && cardSource == permanent.TopCard;

                    bool SkillCondition1(ICardEffect cardEffect)
                        => cardEffect != null
                            && cardEffect.EffectSourceCard != null
                            && cardEffect.EffectSourceCard.Owner == card.Owner.Enemy
                            && cardEffect.EffectSourceCard.IsDigimon;

                    yield return null;
                }
            }
            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: 1,
                hashValue: "BT26_058_WD_WA",
                whenDigivolving: true,
                whenAttacking: true);

            #region All Turns - Protect CS Digimon
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing this Digimon's top stacked card under the leaving Digimon, it doesn't leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] When any of your Digimon with the [CS] trait would leave the battle area, by placing this Digimon's top stacked card as its bottom digivolution card, they don't leave.";

                bool ProtectedPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsTraits("CS");

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, ProtectedPermanentCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.PermanentOfThisCard().DigivolutionCards.Count >= 1;

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> removedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable).Filter(ProtectedPermanentCondition);
                    CardSource topCard = card.PermanentOfThisCard().TopCard;

                    yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { topCard }, activateClass));

                    foreach (Permanent permanent in removedPermanents)
                    {
                        permanent.willBeRemoveField = false;
                        permanent.HideDeleteEffect();
                        permanent.HideHandBounceEffect();
                        permanent.HideDeckBounceEffect();
                        permanent.HideWillRemoveFieldEffect();
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
