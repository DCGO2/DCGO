using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// PrinceMamemon (X Antibody)
namespace DCGO.CardEffects.BT27
{
    public class BT27_072 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.ContainsCardName("Mamemon") && !permanent.TopCard.EqualsTraits("X Antibody");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 1, false, card, null, level: 6));
            }
            #endregion

            #region Shared WD / WA
            string SharedEffectName = "Activate [On deletion] effect from 1 of this card's digivolution cards";

            string SharedHashString = "BT27_072_WD_WA";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] Activate 1 [On Deletion] effect on 1 Digimon card with [Mamemon] in its name in this Digimon's digivolution cards as this Digimon's effect.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                List<ICardEffect> candidateEffects = new List<ICardEffect>();

                foreach(CardSource source in card.PermanentOfThisCard().DigivolutionCards)
                {
                    List<ICardEffect> onDeletions = source.EffectList(EffectTiming.OnDestroyedAnyone)
                        .Filter(cardEffect => cardEffect is ActivateICardEffect
                            && !cardEffect.IsInheritedEffect
                            && !cardEffect.IsLinkedEffect
                            && cardEffect.IsOnDeletion);

                    candidateEffects.AddRange(onDeletions);
                }

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
                            canNoSelect: () => false,
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

                        ActivateClass effectToActivate = CardEffectFactory.CopyActivateClassToCard(card, (ActivateClass)selectedEffect);

                        if (effectToActivate.CanUse(effectHashtable))
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                effectToActivate.Activate_Optional_Effect_Execute(effectHashtable));
                        }
                    }
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: 1,
                hashValue: SharedHashString,
                whenDigivolving: true,
                whenAttacking: true);

            #endregion

            #region All Turns

            bool AllTurnsCondition() => CardEffectCommons.IsExistOnBattleArea(card);

            bool IsYourMamemon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) && permanent.TopCard.ContainsCardName("Mamemon");

            #region Alliance
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceStaticEffect(IsYourMamemon, false, card, AllTurnsCondition));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerStaticEffect(permanentCondition: IsYourMamemon, isInheritedEffect: false, card: card, condition: AllTurnsCondition));             
            }
            #endregion

            #region Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootStaticEffect(permanentCondition: IsYourMamemon, isInheritedEffect: false, card: card, AllTurnsCondition));
            }
            #endregion

            #endregion

            return cardEffects;
        }
    }
}
