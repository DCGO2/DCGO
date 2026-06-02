using System.Collections;
using System.Collections.Generic;

// Nyaromon
namespace DCGO.CardEffects.EX12
{
    public class EX12_001 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Inherit
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May DNA this [VB] Digimon and 1 other into a [VB] in hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When any of your [ME] trait Digimon would leave the battle area other than by your effects, 1 of them and any of your other Digimon may DNA digivolve into an [ME] trait Digimon card in the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectDNACondition)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent != card.PermanentOfThisCard())
                        && card.PermanentOfThisCard().TopCard.EqualsTraits("VB");
                }

                bool CanSelectDNACondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("VB")
                        && cardSource.jogressCondition.Count > 0;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedDNA = null;
                    List<Permanent> allowedPermanents = new List<Permanent>();
                    Permanent selectedPermanent1 = null;
                    Permanent selectedPermanent2 = null;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectDNACondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: SelectDNACoroutine,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectHandEffect.SetUpCustomMessage("Select 1 Digimon to DNA digivolve.",
                        "The opponent is selecting 1 Digimon to DNA digivolve.");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectDNACoroutine(CardSource cardSource)
                    {
                        selectedDNA = cardSource;
                        yield return null;
                    }

                    if (selectedDNA != null)
                    {
                        JogressCondition dnaCondition = selectedDNA.jogressCondition[0];

                        if (selectedDNA.jogressCondition.Count > 1)
                        {
                            #region select DNA condition
                            SelectDNACondition selectDNACondition = GManager.instance.GetComponent<SelectDNACondition>();
                            selectDNACondition.SetUp(selectedDNA.Owner, selectedDNA, SelectDNA);

                            yield return ContinuousController.instance.StartCoroutine(selectDNACondition.Activate());

                            IEnumerator SelectDNA(int dnaSelection)
                            {
                                dnaCondition = selectedDNA.jogressCondition[dnaSelection];

                                yield return null;
                            }
                            #endregion
                        }

                        JogressConditionElement[] elements = (JogressConditionElement[])dnaCondition.elements.Clone();

                        for (int i = 0; i < elements.Length; i++)
                        {
                            foreach (Permanent permanent in card.Owner.GetBattleAreaPermanents())
                            {
                                if (elements[i].EvoRootCondition(permanent))
                                {
                                    allowedPermanents.Add(permanent);
                                }
                            }
                        }

                        if (allowedPermanents.Count >= 2)
                        {
                            #region Selecting First Permanent for DNA
                            if (allowedPermanents.Contains(card.PermanentOfThisCard()))
                            {
                                selectedPermanent1 = card.PermanentOfThisCard();
                            }
                            #endregion

                            if (selectedPermanent1 != null)
                            {
                                #region Selecting Second Permanent for DNA
                                bool PermanentDNASelection2(Permanent permanent)
                                {
                                    return allowedPermanents.Contains(permanent)
                                        && permanent != selectedPermanent1;
                                }

                                SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect1.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: PermanentDNASelection2,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine2,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect1.SetUpCustomMessage("Select second Digimon to DNA digivolve.",
                                    "The opponent is selecting second Digimon to DNA digivolve.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                                IEnumerator SelectPermanentCoroutine2(Permanent permanent)
                                {
                                    selectedPermanent2 = permanent;

                                    yield return null;
                                }
                                #endregion
                            }
                        }

                        if (selectedDNA != null
                        && selectedPermanent1
                        != null
                        && selectedPermanent2 != null
                        && selectedDNA.CanJogressFromTargetPermanent(selectedPermanent1, false))
                        {
                            int[] jogressEvoRootsFrameIDs = {selectedPermanent1.PermanentFrame.FrameID, selectedPermanent2.PermanentFrame.FrameID};

                            PlayCardClass playCard = new PlayCardClass(
                                cardSources: new List<CardSource>() { selectedDNA },
                                hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                payCost: true,
                                targetPermanent: null,
                                isTapped: false,
                                root: SelectCardEffect.Root.Hand,
                                activateETB: true);

                            playCard.SetJogress(jogressEvoRootsFrameIDs);

                            yield return ContinuousController.instance.StartCoroutine(playCard.PlayCard());

                            if (card.PermanentOfThisCard().CanAttack(activateClass))
                            {
                                SelectAttackEffect selectAttackEffect =
                                        GManager.instance.GetComponent<SelectAttackEffect>();

                                selectAttackEffect.SetUp(
                                    attacker: card.PermanentOfThisCard(),
                                    canAttackPlayerCondition: () => true,
                                    defenderCondition: (permanent) => true,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                            }
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
