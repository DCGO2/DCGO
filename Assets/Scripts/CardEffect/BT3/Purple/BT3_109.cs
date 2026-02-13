using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT3_109 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OptionSkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] 1 of your Digimon gains \"[On Deletion] Play this card without paying its memory cost. Any [On Play] effects on Digimon played with this effect don't activate\" for the turn.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get effects.", "The opponent is selecting 1 Digimon that will get effects.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        Permanent selectedPermanent = permanent;

                        if (selectedPermanent != null)
                        {
                            CardSource _topCard = selectedPermanent.TopCard;

                            ActivateClass activateClass1 = new ActivateClass();
                            activateClass1.SetUpICardEffect("Play this card from trash", CanUseCondition1, selectedPermanent.TopCard);
                            activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                            activateClass1.SetEffectSourcePermanent(selectedPermanent);
                            CardEffectCommons.AddEffectToPermanent(targetPermanent: selectedPermanent, effectDuration: EffectDuration.UntilEachTurnEnd, card: card, cardEffect: activateClass1, timing: EffectTiming.OnDestroyedAnyone);

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(selectedPermanent));

                            string EffectDiscription1()
                            {
                                return "[On Deletion] Play this card without paying its memory cost. Any [On Play] effects on Digimon played with this effect don't activate";
                            }

                            bool CanUseCondition1(Hashtable hashtable1)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                {
                                    if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable1, (permanent) => permanent == selectedPermanent))
                                    {
                                        if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
                            }

                            bool CanActivateCondition1(Hashtable hashtable1)
                            {
                                if (CardEffectCommons.IsTopCardInTrashOnDeletion(hashtable1))
                                {
                                    CardSource TopCard = CardEffectCommons.GetTopCardFromEffectHashtable(hashtable1);

                                    if (TopCard != null)
                                    {
                                        if (CardEffectCommons.IsExistOnTrash(TopCard))
                                        {
                                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: TopCard, payCost: false, cardEffect: activateClass1))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }

                                return false;
                            }

                            IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                            {
                                CardSource TopCard = CardEffectCommons.GetTopCardFromEffectHashtable(_hashtable1);

                                if (TopCard != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                        cardSources: new List<CardSource>() { TopCard },
                                        activateClass: activateClass1,
                                        payCost: false,
                                        isTapped: false,
                                        root: SelectCardEffect.Root.Trash,
                                        activateETB: false));
                                }
                            }
                        }
                    }
                }
            }
        }

        return cardEffects;
    }
}
