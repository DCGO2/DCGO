using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Meramon
namespace DCGO.CardEffects.BT27
{
    public class BT27_010 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By paying 1 cost, you may activate an On Deletion of this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT27_010_Main");
                cardEffects.Add(activateClass);

                string EffectDescription() => "[Main] [Once Per Turn] By paying 1 cost, you may activate 1 of this Digimon's [On Deletion] effects.";

                bool CanUseCondition(Hashtable hashtable) 
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                    && card.Owner.MaxMemoryCost >= 1
                    && CardEffectCommons.IsOwnerTurn(card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(-1, activateClass));

                    List<ICardEffect> candidateEffects = card.PermanentOfThisCard().EffectList(EffectTiming.OnDestroyedAnyone)
                        .Filter(cardEffect => cardEffect is ActivateICardEffect && cardEffect.IsOnDeletion);

                    if (candidateEffects.Count > 0)
                    {
                        ICardEffect selectedEffect = null;

                        if (candidateEffects.Count == 1)
                        {
                            selectedEffect = candidateEffects[0];
                        }
                        else
                        {
                            List<SkillInfo> skillInfos = candidateEffects
                                .Map(cardEffect => new SkillInfo(cardEffect, null, EffectTiming.None));

                            List<CardSource> cardSources = candidateEffects
                                .Map(cardEffect => cardEffect.EffectSourceCard);

                            SelectCardEffect selectSourceCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectSourceCardEffect.SetUp(
                                canTargetCondition: (cardSource) => true,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 effect to activate.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: false,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: cardSources,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectSourceCardEffect.SetNotShowCard();
                            selectSourceCardEffect.SetUpSkillInfos(skillInfos);
                            selectSourceCardEffect.SetUpAfterSelectIndexCoroutine(AfterSelectIndexCoroutine);

                            yield return ContinuousController.instance.StartCoroutine(selectSourceCardEffect.Activate());

                            IEnumerator AfterSelectIndexCoroutine(List<int> selectedIndexes)
                            {
                                if (selectedIndexes.Count == 1)
                                {
                                    selectedEffect = candidateEffects[selectedIndexes[0]];
                                    yield return null;
                                }
                            }
                        }

                        if (selectedEffect != null)
                        {
                            Hashtable effectHashtable = CardEffectCommons.OnDeletionCheckHashtableOfPermanent(card.PermanentOfThisCard());

                            if (selectedEffect.CanUse(effectHashtable))
                            {
                                yield return ContinuousController.instance.StartCoroutine(
                                    ((ActivateICardEffect)selectedEffect).Activate_Optional_Effect_Execute(effectHashtable));
                            }
                        }
                    }
                }
            }
            #endregion

            #region Shared On Deletion
            
            string OnDeletionEffectName = "Delete 1 of your opponent's Digimon with 6000 DP or less";

            string OnDeletionEffectDescription() => "[On Deletion] Delete 1 of your opponent's Digimon with 6000 DP or less.";

            bool OnDeletionUseCondition(Hashtable hashtable, ActivateClass activateClass) => CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);

            bool OnDeletionActivateCondition(Hashtable hashtable, ActivateClass activateClass) => CardEffectCommons.CanActivateOnDeletion(card, activateClass);

            IEnumerator OnDeletionActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool OpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.HasDP
                        && permanent.DP <= card.Owner.MaxDP_DeleteEffect(6000, activateClass);
                }
                
                if (CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimon))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: OpponentsDigimon,
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
            }

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                foreach(bool isInheritedEffect in new[]{true, false})
                {
                    ActivateClass activateClass = new();
                    activateClass.SetUpICardEffect(OnDeletionEffectName, hash => OnDeletionUseCondition(hash, activateClass), card);
                    activateClass.SetUpActivateClass(hash => OnDeletionActivateCondition(hash, activateClass), hash => OnDeletionActivateCoroutine(hash, activateClass), -1, false, OnDeletionEffectDescription());
                    activateClass.SetIsInheritedEffect(isInheritedEffect);
                    cardEffects.Add(activateClass);
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
