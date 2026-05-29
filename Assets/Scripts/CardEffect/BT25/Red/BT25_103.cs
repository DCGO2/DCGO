using System.Collections;
using System.Collections.Generic;
using System.Linq;

// GraceNovamon
namespace DCGO.CardEffects.BT25
{
    public class BT25_103 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolve Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 6, permanentCondition: PermanentCondition, digivolutionCost: 5, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region DNA
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.GetJogressConditionClass(
                    PermanentCondition1, 
                    "a level 6 Red Digimon", 
                    PermanentCondition2, 
                    "a level 6 Blue Digimon", 
                    card
                ));

                bool PermanentCondition1(Permanent permanent)
                {
                    return permanent.TopCard.CardColors.Contains(CardColor.Red)
                        && permanent.Levels_ForJogress(card).Contains(6);
                }

                bool PermanentCondition2(Permanent permanent)
                {
                    return permanent.TopCard.CardColors.Contains(CardColor.Blue)
                        && permanent.Levels_ForJogress(card).Contains(6);
                }
            }
            #endregion

            #region Security +1
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Iceclad
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.IcecladSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Partition
            if (timing == EffectTiming.WhenRemoveField)
            {
                List<PartitionCondition> partitionConditions = new List<PartitionCondition>
                {
                    new PartitionCondition("Apollomon"),
                    new PartitionCondition("Dianamon")
                };

                cardEffects.Add(CardEffectFactory.PartitionSelfEffect
                    (isInheritedEffect: false,
                    card: card,
                    condition: null,
                    cardSourceConditions: partitionConditions));
            }
            #endregion

            #region Shared OP / WA

            string BounceEffectName = "Bottom deck 1 enemy Digimon with equal or fewer digivolution cards than this";

            string BounceEffectDescription(string tag)
                => $"[{tag}] Return 1 of your opponent's Digimon with as many or fewer digivolution cards as this Digimon to the bottom of the deck.";

            bool IsEnemyWithLessSources(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.DigivolutionCards.Count() <= card.PermanentOfThisCard().DigivolutionCards.Count;
            }

            IEnumerator BounceActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsEnemyWithLessSources))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsEnemyWithLessSources,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    BounceEffectName,
                    BounceActivateCoroutine,
                    BounceEffectDescription,
                    optional: false,
                    whenDigivolving: true,
                    whenAttacking: true);

            #endregion

            #region Shared WA / Counter

            string AttackingHashString = "BT25_103_WA_COUNTER";
            
            string AttackingEffectName = "May trash a digivolution card per this digimon's digivolution cards, then may end the attack.";

            string AttackingEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] For each of this Digimon's Digivolution cards, you may trash any 1 digivolution card from your opponent's Digimon. Then, you may end this attack.";

            bool IsEnemyDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            IEnumerator AttackingActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool used = false;

                if (CardEffectCommons.HasMatchConditionPermanent(IsEnemyDigimon))
                {
                    int count = card.PermanentOfThisCard().DigivolutionCards.Count;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                        permanentCondition: IsEnemyDigimon,
                        cardCondition: null,
                        maxCount: count,//SelectTrashDigivolutionCards takes care of Math.Min possible cards
                        canNoTrash: true,
                        isFromOnly1Permanent: false,
                        activateClass: activateClass,
                        canEndNotMax: false,
                        afterSelectionCoroutine: AfterSelectCoroutine
                    ));

                    IEnumerator AfterSelectCoroutine(Permanent permanent, List<CardSource> cardSources)
                    {
                        if (cardSources.Count > 0)
                            used = true;

                        yield return null;
                    }
                }

                List<SelectionElement<bool>> selectionElements1 = new List<SelectionElement<bool>>()
                {
                    new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                    new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                };

                string selectPlayerMessage1 = "Will you end the attack?";
                string notSelectPlayerMessage1 = "The opponent is choosing if they will end the attack.";

                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage1, notSelectPlayerMessage: notSelectPlayerMessage1);

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                if (GManager.instance.userSelectionManager.SelectedBoolValue)
                {
                    used = true;

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.EndAttack());
                }

                if (!used) activateClass.RemoveUse();
            }

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    AttackingEffectName,
                    AttackingActivateCoroutine,
                    AttackingEffectDescription,
                    optional: false,
                    isSkippable: true,
                    maxCountPerTurn: 1,
                    hashValue: AttackingHashString,
                    whenAttacking: true,
                    counter: true);
            #endregion

            return cardEffects;
        }
    }
}
