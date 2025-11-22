using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Protein
namespace DCGO.CardEffects.EX9
{
    public class EX9_071 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Ignore Color Requirements
            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);
                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return card.Owner.GetFieldPermanents().Some(permanet =>
                        permanet.TopCard.HasDMTraits &&
                        !permanet.TopCard.IsOption);
                }

                bool CardCondition(CardSource cardSource)
                    => cardSource == card;
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] <Draw 1>. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }
            #endregion

            #region Main - Delay
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] <Delay>.\r\n・By trashing 1 of your [DM] trait Digimon's bottom 2 face-down digivolution card, it unsuspends.";
                }

                bool DMTraitDigimon(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        return permanent.IsDigimon &&
                               permanent.TopCard.HasDMTraits &&
                               permanent.HasFaceDownDigivolutionCards &&
                               permanent.DigivolutionCards.Count(x => x.IsFlipped) > 1;
                    }

                    return false;
                }

                bool FaceDownDigivolutionSource(CardSource source)
                {
                    return source.IsFlipped;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(DMTraitDigimon))
                        return CardEffectCommons.CanDeclareOptionDelayEffect(card);

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = null;


                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: DMTraitDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to trash sources.", "The opponent is selecting 1 digimon to trash sources.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if(selectedPermanent != null)
                        {
                            int startingSources = selectedPermanent.DigivolutionCards.Count;

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedPermanent, trashCount: 2, isFromTop: false, activateClass: activateClass, FaceDownDigivolutionSource));

                            if (selectedPermanent.DigivolutionCards.Count == startingSources - 2)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent> { selectedPermanent }, activateClass).Unsuspend());
                            }
                        }                        
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Memory +1, Place in battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] Gain 1 memory. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));

                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: card, payCost: false, cardEffect: activateClass, isPlayOption: true))
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}