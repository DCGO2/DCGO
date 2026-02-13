using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// PolarBearmon
namespace DCGO.CardEffects.EX11
{
    public class EX11_016 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Ice-Snow")
                        && targetPermanent.TopCard.IsLevel4;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Iceclad

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.IcecladSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region Shared OP / WD

            string SharedEffectName()
            {
                return "Trash any 2 digivolution cards from your opponents Digimon, may send opponents sourceless digimon to top or bottom security.";
            }

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] Trash any 2 digivolution cards from your opponent's Digimon. Then, you may place 1 of their Digimon with no digivolution cards as the top or bottom security card.";
            }

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            bool OpponentsDigimon(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool OpponentsDigimonWithoutSources(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                       permanent.DigivolutionCards.Count == 0;
            }
            
            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (card.Owner.Enemy.GetBattleAreaDigimons().Count() > 0)
                {
                    #region Strip 2 sources

                    bool CanSelectCardCondition(CardSource cardSource)
                    {
                        return !cardSource.CanNotTrashFromDigivolutionCards(activateClass);
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimon)) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                                permanentCondition: OpponentsDigimon,
                                cardCondition: CanSelectCardCondition,
                                maxCount: 2,
                                canNoTrash: false,
                                isFromOnly1Permanent: false,
                                activateClass: activateClass
                    ));

                    #endregion

                    #region Send to Security

                    if (CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimonWithoutSources)
                        && card.Owner.CanAddSecurity(activateClass))
                    {
                        Permanent selectedPermanent = null;
                        SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect1.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentsDigimonWithoutSources,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine1,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect1.SetUpCustomMessage("Select 1 Digimon to place in security", "The opponent is selecting 1 Digimon place in security.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                        IEnumerator SelectPermanentCoroutine1(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                                    {
                                        new SelectionElement<bool>(message: $"Security Top", value : true, spriteIndex: 0),
                                        new SelectionElement<bool>(message: $"Security Bottom", value : false, spriteIndex: 1),
                                    };

                            string selectPlayerMessage = "Choose security position?";
                            string notSelectPlayerMessage = "The opponent is choosing security position";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                            bool position = GManager.instance.userSelectionManager.SelectedBoolValue;

                            yield return ContinuousController.instance.StartCoroutine(new IPutSecurityPermanent(selectedPermanent, CardEffectCommons.CardEffectHashtable(activateClass), toTop: position).PutSecurity());
                        }
                    }

                    #endregion

                }
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, (hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card)
                        && CardEffectCommons.IsExistOnBattleArea(card);
                }
            }

            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, (hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card)
                        && CardEffectCommons.IsExistOnBattleArea(card);
                }
            }

            #endregion
            
            #region Inherited Effect - Your Turn

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && card.PermanentOfThisCard().TopCard.EqualsTraits("Ice-Snow")
                        && !CardEffectCommons.HasMatchConditionOpponentsPermanent(card, (permanent) =>
                                permanent.IsDigimon && !permanent.HasNoDigivolutionCards);                   
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(1, true, card, Condition));
            }

            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && card.PermanentOfThisCard().TopCard.EqualsTraits("Ice-Snow")
                        && !CardEffectCommons.HasMatchConditionOpponentsPermanent(card, (permanent) =>
                                permanent.IsDigimon && !permanent.HasNoDigivolutionCards);
                }

                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: true, card: card, condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}
