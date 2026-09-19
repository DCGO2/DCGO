using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Unchained
namespace DCGO.CardEffects.EX11
{
    public class EX11_070 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            #region Start of turn set to 3
            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }
            #endregion

            #region End of your Turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DNA digivolve into [ExMaquinamon]. Mind Link to [Maquinamon] in text.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[End of Your Turn] 2 of your Digimon may DNA digivolve into [ExMaquinamon] in the hand. Then, this Tamer may <Mind Link> with 1 of your Digimon with [Maquinamon] in its text.";
                }

                bool CanSelectDNACardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.CanPlayJogress(true)
                        && cardSource.EqualsCardName("ExMaquinamon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool IsMyProperDigimon(Permanent permanent)
                {
                    return permanent.IsDigimon
                        && permanent.TopCard.HasText("Maquinamon");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {

                    #region DNA digivolve
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DNADigivolvePermanentsIntoHandOrTrashCard(
                            CanSelectDNACardCondition,
                            payCost: true,
                            isHand: true,
                            activateClass
                        ));
                    #endregion
                    #region Mind Link
                    yield return ContinuousController.instance.StartCoroutine(
                        new MindLinkClass(
                            tamer: card.PermanentOfThisCard(),
                            digimonCondition: IsMyProperDigimon,
                            activateClass: activateClass
                        ).MindLink()
                    );
                    #endregion
                }
            }
            #endregion

            #region ESS - All turns
            if(timing == EffectTiming.None)
            {
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.PermanentOfThisCard().TopCard.HasText("Maquinamon");
                }

                #region Can't have less than 1000 DP
                {
                    ChangeDPClass changeDPClass = new ChangeDPClass();
                    changeDPClass.SetUpICardEffect("Can't have less than 1000 DP", CanUseCondition, card);
                    changeDPClass.SetUpChangeDPClass(ChangeDP: ChangeDP, permanentCondition: PermanentCondition, isUpDown: () => false, isMinusDP: () => false);
                    changeDPClass.SetIsInheritedEffect(true);
                    cardEffects.Add(changeDPClass);

                    

                    int ChangeDP(Permanent permanent, int DP)
                    {
                        if (PermanentCondition(permanent) && DP < 1000)
                        {
                            DP = 1000;
                        }

                        return DP;
                    }

                    bool PermanentCondition(Permanent permanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent)
                            && !permanent.TopCard.CanNotBeAffected(changeDPClass)
                            && permanent.DigivolutionCards.Contains(card)
                            && permanent.TopCard.HasText("Maquinamon");
                    }
                }
                #endregion
                #region Immunity to Stack Trashing
                {
                    ImmuneStackTrashingClass immuneFromStackTrashingClass = new ImmuneStackTrashingClass();
                    immuneFromStackTrashingClass.SetUpICardEffect("Isn't affected by trashing any stacked card", CanUseCondition, card);
                    immuneFromStackTrashingClass.SetUpImmuneFromStackTrashingClass(PermanentCondition: PermanentCondition, EffectCondition: EffectCondition);
                    immuneFromStackTrashingClass.SetIsInheritedEffect(true);
                    cardEffects.Add(immuneFromStackTrashingClass);

                    bool EffectCondition(ICardEffect effect)
                    {
                        return CardEffectCommons.IsOpponentEffect(effect, card);
                    }

                    bool PermanentCondition(Permanent permanent)
                    {
                        return permanent.cardSources.Contains(card);
                    }
                }
                #endregion
            }
            #endregion

            #region ESS - De-MindLink
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.PlayMindLinkTamerFromDigivolutionCards(
                                                    card, 
                                                    "Unchained", 
                                                    "[End of All Turns] You may play 1 [Unchained] from this Digimon's digivolution cards without paying the cost."));
            }
            #endregion

            return cardEffects;
        }
    }
}