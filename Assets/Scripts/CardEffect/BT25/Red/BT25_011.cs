using System.Collections;
using System.Collections.Generic;

// Aquilamon
namespace DCGO.CardEffects.BT25
{
    public class BT25_011 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.EqualsCardName("Hawkmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 2, true, card, null));
            }
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.EqualsTraits("TS");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 2, true, card, null, level: 3));
            }
            #endregion

            #region Raid
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region OP/WD Shared
            string SharedEffectName = "Suspend 1 enemy digimon, then may DNA into [Silphymon]";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: -1,
                onPlay: true,
                whenDigivolving: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] Suspend 1 of your opponent's Digimon. Then, if it's your turn, 2 of your Digimon may DNA digivolve into [Silphymon] in the hand.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanSelectDNACardCondition(CardSource cardSource)
            {
                return cardSource.IsDigimon
                    && cardSource.CanPlayJogress(true)
                    && cardSource.EqualsCardName("Silphymon");
            }

            IEnumerator SharedActivateCoroutine(Hashtable _hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
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
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

                if (CardEffectCommons.IsOwnerTurn(card))
                {
                    yield return ContinuousController.instance.StartCoroutine(
                     CardEffectCommons.DNADigivolvePermanentsIntoHandOrTrashCard(
                         CanSelectDNACardCondition,
                         payCost: true,
                         isHand: true,
                         activateClass
                     ));
                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 2000,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}
