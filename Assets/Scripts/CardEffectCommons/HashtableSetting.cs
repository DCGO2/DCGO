using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CardEffectCommons
{
    public static Hashtable CardEffectHashtable(ICardEffect cardEffect) => new Hashtable() { { "CardEffect", cardEffect } };

    #region Hashtable used when check whether the permanent can trigger [Pierce]
    public static Hashtable PierceCheckHashtableOfPermanent(Permanent permanent)
    {
        Hashtable hashtable = new Hashtable();

        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                CardSource opponentCard = GManager.instance.turnStateMachine.gameContext.ActiveCardList.Find(cardSource => cardSource.Owner == permanent.TopCard.Owner.Enemy);

                if (opponentCard != null)
                {
                    IBattle battle = new IBattle(null, null, null);
                    hashtable.Add("battle", battle);

                    Hashtable battleHashtable = new Hashtable();
                    Permanent WinnerPermanent = new Permanent(permanent.cardSources);
                    battleHashtable.Add("WinnerPermanents", new List<Permanent>() { WinnerPermanent });
                    battleHashtable.Add("WinnerPermanents_real", new List<Permanent>() { WinnerPermanent });
                    Permanent LoserPermanent = new Permanent(new List<CardSource>() { opponentCard });
                    battleHashtable.Add("LoserPermanents", new List<Permanent>() { LoserPermanent });
                    Permanent LoserPermanents_real = new Permanent(new List<CardSource>() { opponentCard }) { IsDestroyedByBattle = true };
                    battleHashtable.Add("LoserPermanents_real", new List<Permanent>() { LoserPermanents_real });

                    battle.hashtable = battleHashtable;
                }

            }
        }

        return hashtable;
    }
    #endregion

    #region Hashtable used when check whether the permanent can trigger [On Deletion] effect
    public static Hashtable OnDeletionCheckHashtableOfPermanent(Permanent permanent)
    {
        Hashtable hashtable = new Hashtable();

        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                List<Hashtable> hashtables = new List<Hashtable>();

                Hashtable hashtable1 = new Hashtable()
                {
                    {"Permanent", new Permanent(permanent.cardSources)}
                };
                hashtables.Add(hashtable1);

                hashtable.Add("hashtables", hashtables);
            }
        }

        return hashtable;
    }
    #endregion

    #region Hashtable used when check whether the permanent would remove field effect
    public static Hashtable WhenPermanentWouldRemoveFieldCheckHashtable(List<Permanent> permanents, ICardEffect cardEffect, IBattle battle)
    {
        Hashtable hashtable = new Hashtable()
        {
            {"CardEffect", cardEffect},
            {"Permanents", permanents},
            {"battle", battle}
        };

        return hashtable;
    }
    #endregion

    #region Hashtable used when check whether the permanent can activate [On Deletion] or "Permanent returned to hand" effect
    public static Hashtable OnDeletionHashtable(List<Permanent> permanents, ICardEffect cardEffect, IBattle battle, bool isDPZero)
    {
        Hashtable hashtable = new Hashtable();

        if (cardEffect != null)
        {
            hashtable.Add("CardEffect", cardEffect);
        }

        if (battle != null)
        {
            hashtable.Add("battle", battle);
        }

        if (isDPZero)
        {
            hashtable.Add("DPZero", isDPZero);
        }

        List<Hashtable> hashtables = permanents
            .Clone()
            .Filter(permanent => permanent != null && permanent.TopCard != null)
            .Map(permanent =>
            {
                List<CardSource> cardSources = permanent.cardSources.Clone();
                List<string> cardNames = permanent.TopCard.CardNames.Clone();
                List<CardColor> cardColors = permanent.TopCard.CardColors.Clone();

                Hashtable hashtableOfPermanent = new Hashtable()
                {
                    {"Permanent", permanent},
                    {"TopCard", permanent.TopCard},
                    {"CardSources", cardSources},
                    {"DigivolutionSources", permanent.DigivolutionCards.Clone()},
                    {"CardNames", cardNames},
                    {"CardColors", cardColors},
                    {"HasSaveText", permanent.TopCard.HasSaveText},
                    {"Level", permanent.TopCard.Level},
                };

                return hashtableOfPermanent;
            });

        hashtable.Add("hashtables", hashtables);

        return hashtable;
    }
    #endregion

    #region Hashtable used when check whether the permanent can activate [On Play] [When Digivolving] or "Permanent enters the field" effect
    public static Hashtable OnEnterFieldHashtable(List<OnEnterFieldHashtableParams> hashtableParams, bool isEvolution, bool isJogress, int digiXrosCount, int assemblyCount,
    ICardEffect cardEffect)
    {
        Hashtable hashtable = new Hashtable()
        {
            {"isEvolution", isEvolution},
            {"isJogress", isJogress},
            {"DigiXrosCount", digiXrosCount },
            {"AssemblyCount", assemblyCount },
            {"isFromDigimonDigivolutionCards", hashtableParams.Some(param => param.IsFromDigimonDigivolutionCards)}
        };

        if (cardEffect != null)
        {
            hashtable.Add("CardEffect", cardEffect);
        }

        List<Hashtable> hashtables = hashtableParams
            .Clone()
            .Filter(hashtableParam => hashtableParam != null)
            .Map(hashtableParam =>
            {
                Hashtable hashtableOfPermanent = new Hashtable()
                {
                    {"Permanent", hashtableParam.Permanent},
                    {"evoRoots", hashtableParam.EvoRoots.Clone()},
                    {"evoRootTops", hashtableParam.EvoRootTops.Clone()},
                    {"Root", hashtableParam.Root},
                    {"oldLevels", hashtableParam.OldLevels.Clone()},
                };

                return hashtableOfPermanent;
            });

        hashtable.Add("hashtables", hashtables);

        return hashtable;
    }
    #endregion

    #region Hashtable used when check whether the permanent would enter the field" effect
    public static Hashtable WouldEnterFieldHashtable(bool payCost, CardSource card, SelectCardEffect.Root root, bool isEvolution, PlayCardClass playCardClass,
    ICardEffect cardEffect, bool isJogress, List<Permanent> targetPermanents)
    {
        Hashtable hashtable = new Hashtable()
        {
            {"PayCost", payCost},
            {"Card", card},
            {"Root", root},
            {"isEvolution", isEvolution},
            {"PlayCardClass", playCardClass},
            {"CardEffect", cardEffect},
            {"isJogress", isJogress},
            {"Permanents", targetPermanents},
        };

        return hashtable;
    }
    #endregion

    #region Hashtable used when check whether the card can trigger [On Play] effect
    public static Hashtable OnPlayCheckHashtableOfCard(CardSource cardSource)
    {
        return new Hashtable()
        {
            {"isEvolution", false},
            {
                "hashtables", new List<Hashtable>()
                {
                    new Hashtable()
                    {
                        {"Permanent", new Permanent(new List<CardSource>(){cardSource} ) },
                    }
                }
            },
        };
    }
    #endregion

    #region Hashtable used when check whether the card can trigger [When Digivolving] effect
    public static Hashtable WhenDigivolvingCheckHashtableOfCard(CardSource cardSource)
    {
        return new Hashtable()
        {
            {"isEvolution", true},
            {
                "hashtables", new List<Hashtable>()
                {
                    new Hashtable()
                    {
                        {"Permanent", new Permanent(new List<CardSource>(){cardSource} ) },
                    }
                }
            },
        };
    }
    #endregion

    #region Hashtable used when check whether the card can trigger option [Main] effect
    public static Hashtable OptionMainCheckHashtable(CardSource cardSource)
    {
        return new Hashtable()
        {
            {"Card", cardSource },
        };
    }
    #endregion

    #region Hashtable used when check whether the permanent can trigger [On Play] effect
    public static Hashtable OnPlayCheckHashtableOfPermanent(Permanent permanent)
    {
        return new Hashtable()
        {
            {"isEvolution", false},
            {
                "hashtables", new List<Hashtable>()
                {
                    new Hashtable()
                    {
                        {"Permanent", permanent },
                    }
                }
            },
        };
    }
    #endregion

    #region Hashtable used when check whether the permanent can trigger [When Digivolving] effect
    public static Hashtable WhenDigivolutionCheckHashtableOfPermanent(Permanent permanent)
    {
        return new Hashtable()
        {
            {"isEvolution", true},
            {
                "hashtables", new List<Hashtable>()
                {
                    new Hashtable()
                    {
                        {"Permanent", permanent },
                    }
                }
            },
        };
    }
    #endregion

    #region Hashtable used when check whether the card can trigger [When Attacking] effect
    public static Hashtable OnAttackCheckHashtableOfCard(CardSource cardSource, ICardEffect cardEffect)
    {
        return new Hashtable()
        {
            {"AttackingPermanent", cardSource.PermanentOfThisCard() ?? new Permanent(new List<CardSource>(){cardSource} )},
            {"CardEffect", cardEffect},
        };
    }
    #endregion

    #region Hashtable used when check whether the permanent can trigger [When Attacking] effect
    public static Hashtable OnAttackCheckHashtableOfPermanent(Permanent attackingPermanent, ICardEffect cardEffect)
    {
        return new Hashtable()
        {
            {"AttackingPermanent", attackingPermanent},
            {"CardEffect", cardEffect},
        };
    }
    #endregion

    #region Hashtable used when check whether the permanent would remove field effect
    public static Hashtable WhenDigivolutionCardWouldDiscardedCheckHashtable(Permanent targetPermanent, List<CardSource> cardSources, ICardEffect cardEffect)
    {
        Hashtable hashtable = new Hashtable()
        {
            {"CardEffect", cardEffect},
            {"Permanent", targetPermanent},
            {"DiscardedCards", cardSources},
        };

        return hashtable;
    }
    #endregion
}
