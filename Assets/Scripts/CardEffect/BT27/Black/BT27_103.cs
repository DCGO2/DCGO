using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Alphamon: Ouryuken // Ultimate Oryuken
namespace DCGO.CardEffects.BT27
{
    public class BT27_103 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared Conditions
            bool IsEnemyDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            #endregion

            #region Digimon Card effects

            #region Static Effects

            #region DNA
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.GetJogressConditionClass(
                    PermanentCondition1, 
                    "Alphamon", 
                    PermanentCondition2, 
                    "Ouryumon", 
                    card
                ));

                bool PermanentCondition1(Permanent permanent)
                {
                    return permanent.TopCard.EqualsCardName("Alphamon");
                }

                bool PermanentCondition2(Permanent permanent)
                {
                    return permanent.TopCard.EqualsCardName("Ouryumon");
                }
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Security A. +1
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Partition
            List<PartitionCondition> partitionConditions = new List<PartitionCondition>
            {
                new PartitionCondition("Alphamon"),
                new PartitionCondition("Ouryumon")
            };

            if (timing == EffectTiming.WhenRemoveField)
            {
                cardEffects.Add(CardEffectFactory.PartitionSelfEffect
                    (isInheritedEffect: false,
                    card: card,
                    condition: null,
                    cardSourceConditions: partitionConditions));
            }
            #endregion

            #endregion

            #region Shared WD / WA

            string SharedEffectName = "May delete an enemy Digimon. If DNA: Recovery +2";

            string SharedHashString = "BT27_103_WD_WA";

            string SharedEffectDescription(string tag) 
                => $"[{tag}] [Once Per Turn] You may delete 1 of your opponent's Digimon. Then, if DNA Digivolving, <Recovery +2>";

            bool SharedCanActivateCondition(Hashtable hashtable, ActivateClass activateClass) => CardEffectCommons.HasMatchConditionPermanent(IsEnemyDigimon) || CardEffectCommons.IsJogress(hashtable);

            bool IsSkippableFunction(Hashtable hashtable) => !CardEffectCommons.IsJogress(hashtable);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool used = false;

                if (CardEffectCommons.HasMatchConditionPermanent(IsEnemyDigimon))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsEnemyDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        if(permanents.Count > 0) used = true;

                        yield return null;
                    }
                }

                if (CardEffectCommons.IsJogress(hashtable))
                {
                    used = true;

                    yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 2, activateClass).Recovery());
                }

                if (!used) activateClass.RemoveUse();
            }

            CardEffectFactory.ActivateClassesForSharedEffects(ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                false,
                additionalActivateCondition: SharedCanActivateCondition,
                isSkippableFunction: IsSkippableFunction,
                maxCountPerTurn: 1,
                hashValue: SharedHashString,
                whenDigivolving: true,
                whenAttacking: true
                );

            #endregion

            #region All Turns
            if (timing == EffectTiming.None)
            {
                CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                canNotAffectedClass.SetUpICardEffect("During attacks, immune to opponent's effects", CanUseCondition, card);
                canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                canNotAffectedClass.SetIsBackgroundProcess(true);
                cardEffects.Add(canNotAffectedClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && GManager.instance.attackProcess.IsAttacking;
                }

                bool CardCondition(CardSource cardSource) => cardSource == card.PermanentOfThisCard()?.TopCard;

                bool SkillCondition(ICardEffect cardEffect)
                {
                    return cardEffect != null
                        && cardEffect.EffectSourceCard != null
                        && CardEffectCommons.IsOpponentEffect(cardEffect, card);
                }
            }
            #endregion

            #endregion

            #region Option Card Effects

            #region Option Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 of your opponent's digimon, then may DNA Digivolve into this", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] Delete 1 of your opponent's Digimon. Then, 2 of your Digimon may DNA digivolve into this card.";

                bool CanUseCondition(Hashtable hashtable) => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsEnemyDigimon))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsEnemyDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DNADigivolvePermanentsIntoHandOrTrashCard(
                        canSelectDNACardCondition: cardSource => cardSource == card,
                        payCost: true,
                        isHand: true,
                        activateClass,
                        ignoreSelection: true
                    ));
                }
            }
            #endregion

            #region Arts Digivolution
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }
            #endregion
            
            #endregion

            return cardEffects;
        }
    }
}