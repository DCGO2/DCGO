using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Net;
using System.Text;
public class OfficialCardListUtility
{
    public static List<string[]> GetCardDatas(List<TextAsset> textAssets)
    {
        List<string[]> CardDatas = new List<string[]>();

        foreach (TextAsset textAsset in textAssets)
        {
            string sourceString = textAsset.text;

            for (int i = 0; i < 20; i++)
            {
                sourceString = sourceString.Replace($"<liclass=\"image_lists_itemdatapage-{i + 1}\">", "��");
            }

            string[] CardDataArray = sourceString.Split("��");

            for (int i = 0; i < CardDataArray.Length; i++)
            {
                string[] CardData = CardDataArray[i].Split('\n');

                if (CardData.Length == 1)
                {
                    continue;
                }

                CardDatas.Add(CardData);
            }
        }

        return CardDatas;
    }

    public static void AttachCardData(CEntity_Base cEntity_Base, List<string[]> CardDatas_JPN, List<string[]> CardDatas_ENG, ref List<int> evoCosts_level, ref List<int> evoCosts_memory)
    {
        string[] targetCardData = null;

        int value;

        bool hasAceName = false;

        foreach (string[] CardData in CardDatas_JPN)
        {
            if (CardData.Length >= 3)
            {
                if (!string.IsNullOrEmpty(CardData[2]))
                {
                    if (CardData[2].Contains(cEntity_Base.CardID))
                    {
                        targetCardData = CardData;
                        break;
                    }
                }
            }
        }

        if (targetCardData != null)
        {
            for (int i = 0; i < targetCardData.Length; i++)
            {
                if (!string.IsNullOrEmpty(targetCardData[i]))
                {
                    //Card Color
                    if (targetCardData[i].Contains("<divclass=\"card_detailcard_detail_"))
                    {
                        targetCardData[i] = targetCardData[i].Replace("<divclass=\"card_detailcard_detail_", "").Replace("\">", "").Replace("multicolor", "");

                        string[] parseByUnderBar = targetCardData[i].Split('_');

                        foreach (string cardColorName in parseByUnderBar)
                        {
                            foreach (string cardColorNameValues in DataBase.CardColorNameDictionary.Values)
                            {
                                if (cardColorName.Trim() == cardColorNameValues)
                                {
                                    cEntity_Base.cardColors.Add(DictionaryUtility.GetCardColor(cardColorName.Trim(), DataBase.CardColorNameDictionary));
                                }
                            }
                        }
                    }

                    //Card Type
                    if (targetCardData[i].Contains("cardtype"))
                    {
                        foreach (string cardTypeName in DataBase.CardKindJPNameDictionary.Values)
                        {
                            if (targetCardData[i].Contains(cardTypeName))
                            {
                                cEntity_Base.cardKind = DictionaryUtility.GetCardKind(cardTypeName, DataBase.CardKindJPNameDictionary);
                            }
                        }
                    }

                    //Card Level
                    if (targetCardData[i].Contains("cardlv"))
                    {
                        targetCardData[i] = targetCardData[i].Replace("<liclass=\"cardlv\">Lv.", "").Replace("</li>", "");

                        if (int.TryParse(targetCardData[i], out value))
                        {
                            cEntity_Base.Level = value;
                        }
                    }

                    //Card Name
                    if (targetCardData[i].Contains("card_name"))
                    {
                        targetCardData[i] = targetCardData[i].Replace("<divclass=\"card_name\">", "").Replace("</div>", "")
                            .Replace("?", "o").Replace("?", "e");

                        cEntity_Base.CardName_JPN = targetCardData[i];

                        if (cEntity_Base.CardName_JPN.Contains("ACE"))
                        {
                            hasAceName = true;
                        }
                    }

                    //Form?
                    if (targetCardData[i].Contains("<dt>形態</dt>"))
                    {
                        targetCardData[i + 1] = targetCardData[i + 1].Replace("<dd>", "").Replace("</dd>", "");

                        if (!string.IsNullOrEmpty(targetCardData[i + 1]) && targetCardData[i + 1] != "-")
                        {
                            cEntity_Base.Form_JPN.Add(targetCardData[i + 1]);
                        }
                    }

                    //Attribute
                    if (targetCardData[i].Contains("<dt>属性</dt>"))
                    {
                        targetCardData[i + 1] = targetCardData[i + 1].Replace("<dd>", "").Replace("</dd>", "");

                        if (!string.IsNullOrEmpty(targetCardData[i + 1]) && targetCardData[i + 1] != "-")
                        {
                            cEntity_Base.Attribute_JPN.Add(targetCardData[i + 1]);
                        }
                    }

                    //Type
                    if (targetCardData[i].Contains("<dt>タイプ</dt>"))
                    {
                        targetCardData[i + 1] = targetCardData[i + 1].Replace("<dd>", "").Replace("</dd>", "");

                        if (!string.IsNullOrEmpty(targetCardData[i + 1]) && targetCardData[i + 1] != "-")
                        {
                            string[] typeStrings = targetCardData[i + 1].Split('/');

                            foreach (string typeString in typeStrings)
                            {
                                if (!string.IsNullOrEmpty(typeString))
                                {
                                    cEntity_Base.Type_JPN.Add(typeString);
                                }
                            }
                        }
                    }

                    //DP
                    if (targetCardData[i].Contains("<dt>DP</dt>"))
                    {
                        targetCardData[i + 1] = targetCardData[i + 1].Replace("<dd>", "").Replace("</dl>", "");

                        if (!string.IsNullOrEmpty(targetCardData[i + 1]))
                        {
                            if (int.TryParse(targetCardData[i + 1], out value))
                            {
                                cEntity_Base.DP = value;
                            }
                        }
                    }

                    //Play Cost
                    if (targetCardData[i].Contains("<dt>登場コスト</dt>"))
                    {
                        targetCardData[i + 1] = targetCardData[i + 1].Replace("<dd>", "").Replace("</dd>", "");

                        if (!string.IsNullOrEmpty(targetCardData[i + 1]))
                        {
                            if (int.TryParse(targetCardData[i + 1].Trim(), out value))
                            {
                                cEntity_Base.PlayCost = value;
                            }
                        }
                    }

                    //Rarity
                    if (targetCardData[i].Contains("<li>") && targetCardData[i].Contains("</li>"))
                    {
                        targetCardData[i] = targetCardData[i].Replace("<li>", "").Replace("</li>", "");

                        if (targetCardData[i] == "�o")
                        {
                            targetCardData[i] = "P";
                        }
                        if (!string.IsNullOrEmpty(targetCardData[i]))
                        {
                            cEntity_Base.rarity = (Rarity)Enum.Parse(typeof(Rarity), targetCardData[i]);
                        }
                    }

                    //Evo Cost 1
                    if (targetCardData[i].Contains("<dt>進化コスト1</dt>"))
                    {
                        targetCardData[i + 1] = targetCardData[i + 1].Replace("<dd>", "").Replace("</dd>", "").Replace("Lv.", "").Replace("から", "��");

                        string[] evoCostStrings = targetCardData[i + 1].Split("��");

                        if (evoCostStrings.Length >= 2)
                        {
                            if (int.TryParse(evoCostStrings[0], out value))
                            {
                                int x = value;

                                evoCosts_level.Add(x);
                            }

                            if (int.TryParse(evoCostStrings[1], out value))
                            {
                                int x = value;

                                if (cEntity_Base.CardID == "BT1-054")
                                {
                                    x = 3;
                                }

                                if (cEntity_Base.CardID == "BT6-050")
                                {
                                    x = 3;
                                }

                                evoCosts_memory.Add(x);
                            }
                        }
                    }

                    //Evo Cost 2
                    if (targetCardData[i].Contains("<dt>進化コスト2</dt>"))
                    {
                        targetCardData[i + 1] = targetCardData[i + 1].Replace("<dd>", "").Replace("</dd>", "").Replace("Lv.", "").Replace("から", "��");

                        string[] evoCostStrings = targetCardData[i + 1].Split("��");

                        if (evoCostStrings.Length >= 2)
                        {
                            if (int.TryParse(evoCostStrings[0], out value))
                            {
                                int x = value;

                                evoCosts_level.Add(x);
                            }

                            if (int.TryParse(evoCostStrings[1], out value))
                            {
                                int x = value;

                                evoCosts_memory.Add(x);
                            }
                        }
                    }

                    //Effect Description
                    if (targetCardData[i].Contains("<dt>����</dt>") || targetCardData[i].Contains("<dt>��i�e�L�X�g</dt>"))
                    {
                        targetCardData[i + 1] = targetCardData[i + 1].Replace("<dd>", "").Replace("</dd>", "");

                        if (!string.IsNullOrEmpty(targetCardData[i + 1]) && targetCardData[i + 1] != "-")
                        {
                            cEntity_Base.EffectDiscription_JPN += targetCardData[i + 1].Replace("<br>", "");
                        }
                    }

                    //Inherited Effect Description
                    if (targetCardData[i].Contains("<dt>�i��������</dt>") || (!(cEntity_Base.cardKind == CardKind.Digimon && hasAceName) && targetCardData[i].Contains("<dt>下段テキスト</dt>")))
                    {
                        targetCardData[i + 1] = targetCardData[i + 1].Replace("<dd>", "").Replace("</dd>", "");

                        if (!string.IsNullOrEmpty(targetCardData[i + 1]) && targetCardData[i + 1] != "-")
                        {
                            cEntity_Base.InheritedEffectDiscription_JPN += targetCardData[i + 1].Replace("<br>", "");
                        }
                    }

                    //Security Effect Description
                    if (targetCardData[i].Contains("<dt>�Z�L�����e�B����</dt>") || (!(cEntity_Base.cardKind == CardKind.Digimon || cEntity_Base.cardKind == CardKind.DigiEgg) && targetCardData[i].Contains("<dt>下段テキスト</dt>")))
                    {
                        targetCardData[i + 1] = targetCardData[i + 1].Replace("<dd>", "").Replace("</dd>", "");

                        if (!string.IsNullOrEmpty(targetCardData[i + 1]) && targetCardData[i + 1] != "-")
                        {
                            cEntity_Base.SecurityEffectDiscription_JPN += targetCardData[i + 1].Replace("<br>", "");
                        }
                    }

                    //Overflow Memory
                    if (cEntity_Base.cardKind == CardKind.Digimon && hasAceName && targetCardData[i].Contains("<dt>下段テキスト</dt>"))
                    {
                        if (targetCardData[i + 1].Contains("overflow-5"))
                        {
                            cEntity_Base.OverflowMemory = 4;
                        }
                        else if (targetCardData[i + 1].Contains("overflow-4"))
                        {
                            cEntity_Base.OverflowMemory = 4;
                        }
                        else if (targetCardData[i + 1].Contains("overflow-3"))
                        {
                            cEntity_Base.OverflowMemory = 3;
                        }
                    }
                }
            }
        }

        targetCardData = null;

        foreach (string[] CardData in CardDatas_ENG)
        {
            if (CardData.Length >= 3)
            {
                if (!string.IsNullOrEmpty(CardData[2]))
                {
                    if (CardData[2].Contains(cEntity_Base.CardID))
                    {
                        targetCardData = CardData;
                        break;
                    }
                }
            }
        }

        if (targetCardData == null)
        {
            AttachEnglishCardDataFromDigimonDev(cEntity_Base);
        }

        else
        {
            for (int i = 0; i < targetCardData.Length; i++)
            {
                if (!string.IsNullOrEmpty(targetCardData[i]))
                {
                    //Card Name
                    if (targetCardData[i].Contains("card_name"))
                    {
                        targetCardData[i] = targetCardData[i].Replace(" ", "").Replace("<divclass=\"card_name\">", "").Replace("</div>", "")
                        .Replace("&amp;", "&");

                        cEntity_Base.CardName_ENG = targetCardData[i].Replace("�I", "!");
                    }

                    //Form
                    if (targetCardData[i].Contains("<dt>Form</dt>"))
                    {
                        targetCardData[i + 1] = targetCardData[i + 1].Replace("<dd>", "").Replace("</dd>", "");

                        if (!string.IsNullOrEmpty(targetCardData[i + 1]) && targetCardData[i + 1] != "-")
                        {
                            cEntity_Base.Form_ENG.Add(targetCardData[i + 1]);
                        }
                    }

                    //Attribute
                    if (targetCardData[i].Contains("<dt>Attribute</dt>"))
                    {
                        targetCardData[i + 1] = targetCardData[i + 1].Replace("<dd>", "").Replace("</dd>", "");

                        if (!string.IsNullOrEmpty(targetCardData[i + 1]) && targetCardData[i + 1] != "-")
                        {
                            cEntity_Base.Attribute_ENG.Add(targetCardData[i + 1]);
                        }
                    }

                    //Type
                    if (targetCardData[i].Contains("<dt>Type</dt>"))
                    {
                        targetCardData[i + 1] = targetCardData[i + 1].Replace("<dd>", "").Replace("</dd>", "");

                        if (!string.IsNullOrEmpty(targetCardData[i + 1]) && targetCardData[i + 1] != "-")
                        {
                            string[] typeStrings = targetCardData[i + 1].Split('/');

                            foreach (string typeString in typeStrings)
                            {
                                if (!string.IsNullOrEmpty(typeString))
                                {
                                    cEntity_Base.Type_ENG.Add(typeString);
                                }
                            }
                        }
                    }

                    //Effect Description
                    if (targetCardData[i].Contains("<dt>Effect</dt>") || targetCardData[i].Contains("<dt>��i�e�L�X�g</dt>"))
                    {
                        targetCardData[i + 1] = targetCardData[i + 1].Replace("<dd>", "").Replace("</dd>", "");

                        if (!string.IsNullOrEmpty(targetCardData[i + 1]) && targetCardData[i + 1] != "-")
                        {
                            cEntity_Base.EffectDiscription_ENG += targetCardData[i + 1].Replace("<br>", "");
                        }

                        if (cEntity_Base.CardID == "BT2-010")
                        {
                            cEntity_Base.EffectDiscription_ENG += "[On Deletion] If it's your turn, gain 1 memory.";
                        }
                    }

                    //Inherited Effect Description
                    if (targetCardData[i].Contains("<dt>Digivolveeffect</dt>") && !(cEntity_Base.cardKind == CardKind.Digimon && cEntity_Base.IsACE))
                    {
                        targetCardData[i + 1] = targetCardData[i + 1].Replace("<dd>", "").Replace("</dd>", "");

                        if (!string.IsNullOrEmpty(targetCardData[i + 1]) && targetCardData[i + 1] != "-")
                        {
                            cEntity_Base.InheritedEffectDiscription_ENG += targetCardData[i + 1].Replace("<br>", "");
                        }
                    }

                    //Security Effect Description
                    if (targetCardData[i].Contains("<dt>Securityeffect</dt>"))
                    {
                        targetCardData[i + 1] = targetCardData[i + 1].Replace("<dd>", "").Replace("</dd>", "");

                        if (!string.IsNullOrEmpty(targetCardData[i + 1]) && targetCardData[i + 1] != "-")
                        {
                            if (cEntity_Base.CardName_JPN == "X�R��")
                            {
                                cEntity_Base.InheritedEffectDiscription_ENG += targetCardData[i + 1].Replace("<br>", "");
                            }

                            else
                            {
                                cEntity_Base.SecurityEffectDiscription_ENG += targetCardData[i + 1].Replace("<br>", "");
                            }
                        }
                    }
                }
            }
        }

        if (cEntity_Base.IsACE)
        {
            if (cEntity_Base.CardName_JPN.Contains("ACE"))
            {
                cEntity_Base.CardName_JPN = cEntity_Base.CardName_JPN.Replace("ACE", "");
            }

            if (cEntity_Base.CardName_ENG.Contains("ACE"))
            {
                cEntity_Base.CardName_ENG = cEntity_Base.CardName_ENG.Replace("ACE", "");
            }

            if (cEntity_Base.CardName_ENG.Contains("Ace"))
            {
                cEntity_Base.CardName_ENG = cEntity_Base.CardName_ENG.Replace("Ace", "");
            }
        }

        #region Card erratas
        if (cEntity_Base.Type_JPN.Contains("X�R��"))
        {
            if (!cEntity_Base.Type_ENG.Contains("XAntibody"))
            {
                cEntity_Base.Type_ENG.Add("XAntibody");
            }
        }

        if (cEntity_Base.CardID == "BT2-024" || cEntity_Base.CardID == "BT2-029" || cEntity_Base.CardID == "BT4-024" || cEntity_Base.CardID == "BT4-028" || cEntity_Base.CardID == "BT4-034")
        {
            while (cEntity_Base.Type_ENG.Contains("SeaAnimal") || cEntity_Base.Type_ENG.Contains("Sea Animal"))
            {
                cEntity_Base.Type_ENG.Remove("SeaAnimal");
                cEntity_Base.Type_ENG.Remove("Sea Animal");
            }

            cEntity_Base.Type_ENG.Add("Aquatic");
        }

        if (cEntity_Base.CardID == "BT1-042")
        {
            cEntity_Base.CardName_ENG = "LoaderLeomon";
        }

        if (cEntity_Base.CardID == "P-063")
        {
            cEntity_Base.CardName_ENG = "RuliTsukiyono";
        }

        if (cEntity_Base.CardID == "ST12-09")
        {
            if (!cEntity_Base.cardColors.Contains(CardColor.Black))
            {
                cEntity_Base.cardColors.Add(CardColor.Black);
            }
        }

        if (cEntity_Base.CardID == "BT9-067")
        {
            cEntity_Base.EffectDiscription_ENG = "[On Play][When Digivolving] Place 1 [Raijinmon], 1 [Fujinmon], and 1 [Suijinmon] from your trash under this Digimon in any order as its bottom digivolution cards. Gain 1 memory for each card placed.[When Attacking] If the level 6 cards in this Digimon's digivolution cards have 3 or more colors among them, this Digimon gets +3000 DP until the end of your opponent's turn. If they have 4 or more colors, <De-Digivolve 1> 1 of your opponent's Digimon.";
        }

        if (cEntity_Base.CardID == "BT8-097")
        {
            cEntity_Base.EffectDiscription_ENG = "Reduce the memory cost of this card in your hand by 1 for each Digimon your opponent has in play.[Main] Your opponent can't play Digimon by effects until the end of their turn. Delete all of your opponent's Digimon with 6000 DP or less.";
        }

        if (cEntity_Base.CardID == "BT10-086")
        {
            cEntity_Base.EffectDiscription_ENG = "Digivolve: 3 from [Omnimon]When a Digimon with [X Antibody] in its digivolution cards would digivolve into this card, reduce the digivolution cost by 2.[When Digivolving] Return all of your opponent's Digimon with the highest level to the bottom of their owners' decks in any order.[When Digivolving][When Attacking][Once Per Turn] By placing 1 [X Antibody] or level 6 card from this Digimon's digivolution cards at the bottom of its owner's deck, reveal all of your opponent's security cards, and trash 1 of them. Place the rest in your opponent's security stack face down. Then, your opponent shuffles their security stack.";
        }

        if (cEntity_Base.CardID == "BT10-097")
        {
            cEntity_Base.EffectDiscription_ENG = "[Main] Reveal the top 6 cards of your deck. You may add 2 cards with [Blue Flare] in their traits among them to your hand, and play 1 [Kiriha Aonuma] among them without paying its memory cost. Place the rest at the bottom of your deck in any order. Then, place this card in your Battle Area.[Main] <Delay> (By trashing this card in your battle area, activate the effect below. You can't activate this effect the turn this card enters play.) - Gain 2 memory.";
        }

        if (cEntity_Base.CardID == "BT10-096")
        {
            cEntity_Base.SecurityEffectDiscription_ENG = "[Security] Reveal the top 3 cards of your deck. You may add 1 Digimon card with [Xros Heart] in its traits among them to your hand and play 1 [Taiki Kudo] among them without paying its memory cost. Place the rest at the bottom of your deck in any order.";
        }

        if (cEntity_Base.CardID == "BT10-093")
        {
            cEntity_Base.EffectDiscription_ENG = "[All Turns][Once Per Turn] When a purple card is placed under this Tamer, <Draw 1> (Draw 1 card from your deck), and gain 1 memory.[Your turn] [Once per turn] When you would play 1 level 4 or higher Digimon card with [Bagra Army] in its traits, by placing up to 3 purple Digimon cards from under your Tamers in the digivolution cards of the Digimon card played, reduce the memory cost of that Digimon by 2 for each card placed.";
        }

        if (cEntity_Base.CardID == "BT10-004")
        {
            cEntity_Base.InheritedEffectDiscription_ENG = "[Your Turn][Once Per Turn] When an effect suspends a Digimon, this Digimon gets +1000 DP for the turn.";
        }

        if (cEntity_Base.CardID == "EX3-068 ")
        {
            cEntity_Base.EffectDiscription_ENG = "[Main] 1 of your opponent's Digimon gets -6000 DP for the turn. Then, you may return 1 card with the [Four Great Dragons] trait from your trash to your hand.";
        }

        if (cEntity_Base.CardID == "P-060")
        {
            cEntity_Base.InheritedEffectDiscription_ENG = "[When Attacking][Once Per Turn] If you have a [Ruli Tsukiyono] in play, gain 1 memory.";
        }

        if (cEntity_Base.CardID == "P-029")
        {
            cEntity_Base.InheritedEffectDiscription_ENG = "[Your Turn] When digivolving this Digimon into an [AncientGreymon] in your hand, reduce its digivolution cost by 2.";
        }

        if (cEntity_Base.CardID == "P-030")
        {
            cEntity_Base.InheritedEffectDiscription_ENG = "[Your Turn] When digivolving this Digimon into an [AncientGarurumon] in your hand, reduce its digivolution cost by 2.";
        }

        if (cEntity_Base.CardID == "P-012")
        {
            cEntity_Base.EffectDiscription_ENG = "[Main] If you have a Digimon with [Veedramon] in its name, you may suspend this Tamer to activate one of the following effects: - Trigger <Draw 1>. (Draw 1 card from your deck. - 1 of your Digimon gets +1000 DP for the turn.";
        }

        if (cEntity_Base.CardID == "P-045")
        {
            cEntity_Base.InheritedEffectDiscription_ENG = "[All Turns] All of your other Digimon with the same name as this Digimon gain <Decoy (Black/White)>. (When one of your other black or white Digimon would be deleted by an opponent's effect, you may delete this Digimon to prevent that deletion.)";
        }

        if (cEntity_Base.CardID == "EX1-073")
        {
            cEntity_Base.EffectDiscription_ENG = "[On Play] You may place up to 5 level 5 red and black cards with [Cyborg] in their traits and different card numbers from your hand and trash in this Digimon's digivolution cards to gain 1 memory for each card placed.[All Turns]This Digimon's DP can't be reduced.[All Turns]When this Digimon would be deleted, you may trash 2 level5 Digimon cards in this Digimon's digivolution cards to prevent this Digimon from being deleted.";
        }

        if (cEntity_Base.CardID == "EX3-063")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("When DNA", "If DNA");
        }

        if (cEntity_Base.CardID == "EX3-058")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("for its digivolution cost", "for the cost").Replace("for its DNA digivolve cost", "for the cost").Replace("[Free] in its traits", "the [Free] trait");
        }

        if (cEntity_Base.CardID == "EX3-057")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("[On Deletion]", "[When Digivolving]").Replace("the top 2 cards of their decks", "the top 2 cards of both players' decks");
        }

        if (cEntity_Base.CardID == "EX3-055")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("trash 1 card among them", "trash 1 such card among them");
        }

        if (cEntity_Base.CardID == "EX3-045")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("You may suspend 1 of you or your opponent�fs Digimon", "You may suspend 1 Digimon").Replace("[Fairy] in its traits", "[Fairy] in one of their traits").Replace("[Fairy] in their traits", "[Fairy] in one of their traits");
        }

        if (cEntity_Base.CardID == "EX3-035")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("Return 1 card with", "You may return 1 card with").Replace("Then, you may return 1 [Magnadramon]", "Then, by returning 1 [Magnadramon]").Replace("in any order to trash the top 2 cards of your opponent's security stack", "in any order, trash the top 2 cards of your opponent's security stack").Replace("with [Four Great Dragons] in its trait", "with the [Four Great Dragons] trait");
        }

        if (cEntity_Base.CardID == "EX3-031")
        {
            cEntity_Base.InheritedEffectDiscription_ENG = cEntity_Base.InheritedEffectDiscription_ENG.Replace("with [Four Great Dragons] in its traits, it gains gains", "with the [Four Great Dragons] trait, 1 of those Digimon gains");
        }

        if (cEntity_Base.CardID == "EX3-030")
        {
            cEntity_Base.EffectDiscription_ENG = "[On Play] Reveal the top 4 cards of your deck. Add 1 yellow card with [Angel], [Cherub], [Throne], [Authority], [Seraph] or [Virtue], other than [Three Great Angels], in one of its traits and 1 card with the [Four Great Dragons] trait among them to your hand. Place the rest at the bottom of your deck in any order.";

            cEntity_Base.InheritedEffectDiscription_ENG = "[Your Turn] [Once Per Turn] When you play a Digimon with the [Four Great Dragons] trait, 1 of those Digimon gains <Rush> for the turn. (This Digimon may attack the turn it was played.)";
        }

        if (cEntity_Base.CardID == "EX3-028")
        {
            cEntity_Base.EffectDiscription_ENG = "[On Play] Reveal the top 4 cards of your deck. Add 1 yellow card with [Angel], [Cherub], [Throne], [Authority], [Seraph] or [Virtue], other than [Three Great Angels], in one of its traits and 1 card with the [Four Great Dragons] trait among them to your hand. Place the rest at the bottom of your deck in any order.";
        }

        if (cEntity_Base.CardID == "EX3-026")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("or [Aquatic] in its traits", "or [Aqua] or [Sea Animal] in one of its traits");
        }

        if (cEntity_Base.CardID == "EX3-024")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("You may suspend 1 of your Digimon with [Dramon] or [Examon] in its name to force 1 of your opponent�fs Digimon to attack", "By suspending 1 of your Digimon with [Dramon] or [Examon] in its name, your opponent attacks with 1 of their Digimon");

            cEntity_Base.InheritedEffectDiscription_ENG = cEntity_Base.InheritedEffectDiscription_ENG.Replace("You may suspend 1 of your Digimon with [Dramon] or [Examon] in its name to force 1 of your opponent�fs Digimon to attack", "By suspending 1 of your Digimon with [Dramon] or [Examon] in its name, your opponent attacks with 1 of their Digimon");
        }

        if (cEntity_Base.CardID == "EX3-023")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("with [Aquatic] in its traits", "with [Aqua] or [Sea Animal] in one of its traits");

            cEntity_Base.InheritedEffectDiscription_ENG = cEntity_Base.InheritedEffectDiscription_ENG.Replace("return 1", "you may return 1");
        }

        if (cEntity_Base.CardID == "EX3-022")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("from one of your", "from 1 of your");

            cEntity_Base.InheritedEffectDiscription_ENG = cEntity_Base.InheritedEffectDiscription_ENG.Replace("play a blue level 3 Digimon card from 1 of your blue Digimon without", "play 1 blue level 3 Digimon card from 1 of your blue Digimon's digivolution cards without");
        }

        if (cEntity_Base.CardID == "EX3-014")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("each card with [Dragon] in its traits", "each card with [Dragon], [saur] or [Ceratopsian] in one of its traits");

            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("[Dragon] traits", "[Dragon], [saur], or [Ceratopsian] in one of its traits");
        }

        if (cEntity_Base.CardID == "EX3-008")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("[Free] in its traits from your trash for its play cost", "the [Free] trait from your trash for the cost");

            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("for its DNA digivolve cost", "for the cost");
        }

        if (cEntity_Base.CardID == "EX3-006")
        {
            cEntity_Base.EffectDiscription_ENG = "[On Play] Reveal the top 4 cards of your deck. Add 1 Digimon card with [Rock Dragon], [Earth Dragon], [Bird Dragon], [Machine Dragon] or [Sky Dragon] in its traits and 1 [Hina Kurihara] among them to your hand. Place the rest at the bottom of your deck in any order.";
        }

        if (cEntity_Base.CardID == "EX3-005")
        {
            cEntity_Base.EffectDiscription_ENG = "[Your Turn][Once Per Turn] When you play a [Hina Kurihara], delete 1 of your opponent's Digimon with 3000 DP or less.";
        }

        if (cEntity_Base.CardID == "EX3-004")
        {
            cEntity_Base.EffectDiscription_ENG = "[On Play] You may trash 1 card with [Imperialdramon] in its name or [Free] in its traits from your hand to <Draw 2>. (Draw 2 cards from your deck.)";
        }

        if (cEntity_Base.CardID == "EX3-003")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("with [Dragon] in its traits", "with [Dragon], [saur] or [Ceratopsian] in one of its traits");
        }

        if (cEntity_Base.CardID == "EX3-001")
        {
            cEntity_Base.InheritedEffectDiscription_ENG = cEntity_Base.InheritedEffectDiscription_ENG.Replace("When a Digimon", "When this Digimon");
        }

        if (cEntity_Base.CardID == "P-071")
        {
            cEntity_Base.EffectDiscription_ENG = "[Security] At the end of the battle, you may play 1 purple level 3 Digimon card from your trash without paying its memory cost. Then, add this card to its owner's hand.";
        }

        if (cEntity_Base.CardID == "BT12-094")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("under this Tamer, 1 of your Digimon gets +2000 DP for the turn", "under this Tamer, gain 1 memory");
        }

        if (cEntity_Base.CardID == "BT11-009")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("place 2 cards in this Digimon's", "place 1 card in this Digimon's");
        }

        if (cEntity_Base.CardID == "EX3-030")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("it gains <Rush>", "1 of those Digimon gains <Rush>");
        }

        if (cEntity_Base.CardID == "ST13-09")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("you may reveal", "reveal");
        }

        if (cEntity_Base.CardID == "ST13-02")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("you may reveal", "reveal");
        }

        if (cEntity_Base.CardID == "EX2-037")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("that Digimon", "1 of those Digimon");
        }

        if (cEntity_Base.CardID == "BT9-071")
        {
            cEntity_Base.EffectDiscription_ENG = "[On Play] Reveal the top 3 cards of your deck. Add 1 card with [Undead] or [Dark Animal] in its traits among them to your hand and trash 1 card with [Undead] or [Dark Animal] in its traits among them. Place the rest at the bottom of your deck in any order.";
        }

        if (cEntity_Base.CardID == "BT9-106")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("You may digivolve", "Digivolve");
        }

        if (cEntity_Base.CardID == "BT9-100")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("Then, unsuspend", "Then, you may unsuspend");
        }

        if (cEntity_Base.CardID == "BT9-056")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("unsuspend this Digimon", "you may unsuspend this Digimon");
        }

        if (cEntity_Base.CardID == "BT9-014")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("choose any number", "you may choose any number");
        }

        if (cEntity_Base.CardID == "BT8-093")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("you may delete this Tamer to play", "by deleting this Tamer , you may play");
        }

        if (cEntity_Base.CardID == "BT6-097")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("Trash up to 2", "Trash 2");
        }

        if (cEntity_Base.CardID == "BT6-033")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("trash cards", "trash any number of cards").Replace("there are 3 remaining", "you have 3 or more remaining");
        }

        if (cEntity_Base.CardID == "BT4-114")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("You may unsuspend", "Unsuspend").Replace("their names", "their names (other than [KendoGarurumon])");
        }

        if (cEntity_Base.CardID == "BT3-090")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("play 1 purple", "you may play 1 purple");
        }

        if (cEntity_Base.CardID == "BT2-112")
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("If you attack", "When you attack");
        }

        if (cEntity_Base.CardID == "EX4-005")
        {
            cEntity_Base.EffectDiscription_ENG = "[Digivolve][Koromon]: Cost 0" + cEntity_Base.EffectDiscription_ENG;
        }

        if (cEntity_Base.CardID == "EX4-006")
        {
            cEntity_Base.EffectDiscription_ENG = "[Digivolve][Gigimon]: Cost 0" + cEntity_Base.EffectDiscription_ENG;
        }

        if (cEntity_Base.CardID == "EX4-007")
        {
            cEntity_Base.EffectDiscription_ENG = "Digivolve: 2 from Lv.3 w/[Agumon] in name and [Dinosaur] trait" + cEntity_Base.EffectDiscription_ENG;
        }

        if (cEntity_Base.CardID == "EX4-008")
        {
            cEntity_Base.EffectDiscription_ENG = "Digivolve: 2 from Lv.3 w/[Guilmon] in name" + cEntity_Base.EffectDiscription_ENG;
        }

        if (cEntity_Base.CardID == "EX4-009")
        {
            cEntity_Base.EffectDiscription_ENG = "[Digivolve][GeoGreymon]: Cost 3" + cEntity_Base.EffectDiscription_ENG;
        }

        if (cEntity_Base.CardID == "EX4-010")
        {
            cEntity_Base.EffectDiscription_ENG = "Digivolve: 3 from Lv.4 w/[Growlmon] in name" + cEntity_Base.EffectDiscription_ENG;
        }

        if (cEntity_Base.CardID == "EX4-011")
        {
            cEntity_Base.EffectDiscription_ENG = "Digivolve: 3 from Lv.5 w/[WarGrowlmon] in name" + cEntity_Base.EffectDiscription_ENG;
        }

        if (cEntity_Base.CardID == "EX4-024")
        {
            cEntity_Base.EffectDiscription_ENG = "[Digivolve][Viximon]: Cost 0" + cEntity_Base.EffectDiscription_ENG;
        }

        if (cEntity_Base.CardID == "EX4-025")
        {
            cEntity_Base.EffectDiscription_ENG = "Digivolve: 2 from Lv.3 w/[Lopmon] or [Terriermon] in name" + cEntity_Base.EffectDiscription_ENG;
        }

        if (cEntity_Base.CardID == "EX4-026")
        {
            cEntity_Base.EffectDiscription_ENG = "[Digivolve][Renamon]: Cost 2" + cEntity_Base.EffectDiscription_ENG;
        }

        if (cEntity_Base.CardID == "EX4-027")
        {
            cEntity_Base.EffectDiscription_ENG = "[Digivolve][Veemon]: Cost 2" + cEntity_Base.EffectDiscription_ENG;
        }

        if (cEntity_Base.CardID == "EX4-028")
        {
            cEntity_Base.EffectDiscription_ENG = "[Digivolve][Kyubimon]: Cost 3" + cEntity_Base.EffectDiscription_ENG;
        }

        if (cEntity_Base.CardID == "EX4-029")
        {
            cEntity_Base.EffectDiscription_ENG = "Digivolve: 3 from Lv.4 2-color w/green" + cEntity_Base.EffectDiscription_ENG;
        }

        if (cEntity_Base.CardID == "EX4-031")
        {
            cEntity_Base.EffectDiscription_ENG = "Digivolve: 3 from Lv.5 2-color w/green" + cEntity_Base.EffectDiscription_ENG;
        }

        if (cEntity_Base.CardID == "EX4-035")
        {
            cEntity_Base.EffectDiscription_ENG = "Digivolve: 2 from Lv.3 w/[Lopmon] or [Terriermon] in name" + cEntity_Base.EffectDiscription_ENG;
        }

        if (cEntity_Base.CardID == "EX4-036")
        {
            cEntity_Base.EffectDiscription_ENG = "Digivolve: 3 from Lv.4 w/[Gargomon] in name or 2-color w/green" + cEntity_Base.EffectDiscription_ENG;
        }

        if (cEntity_Base.CardID == "EX4-037")
        {
            cEntity_Base.EffectDiscription_ENG = "Digivolve: 4 from Lv.5 w/[Rapidmon] in name or 2-color w/green" + cEntity_Base.EffectDiscription_ENG;
        }

        if (cEntity_Base.CardID == "EX5-015")
        {
            if (!cEntity_Base.EffectDiscription_ENG.Contains("Gabumon") || !cEntity_Base.EffectDiscription_ENG.Contains("Tsunomon"))
            {
                cEntity_Base.EffectDiscription_ENG = "Digivolve: 0 from [Gabumon] or [Tsunomon]" + cEntity_Base.EffectDiscription_ENG;
            }
        }

        if (cEntity_Base.CardID == "EX5-018")
        {
            if (!cEntity_Base.EffectDiscription_ENG.Contains("Garurumon"))
            {
                cEntity_Base.EffectDiscription_ENG = "[Digivolve][Garurumon]: Cost 0" + cEntity_Base.EffectDiscription_ENG;
            }
        }

        if (cEntity_Base.CardID == "EX5-027")
        {
            if (!cEntity_Base.EffectDiscription_ENG.Contains("Frimon"))
            {
                cEntity_Base.EffectDiscription_ENG = "[Digivolve][Frimon]: Cost 0" + cEntity_Base.EffectDiscription_ENG;
            }
        }

        if (cEntity_Base.CardID == "EX5-030")
        {
            if (!cEntity_Base.EffectDiscription_ENG.Contains("Liollmon") || !cEntity_Base.EffectDiscription_ENG.Contains("Elecmon"))
            {
                cEntity_Base.EffectDiscription_ENG = "Digivolve: 2 from [Liollmon] or [Elecmon]" + cEntity_Base.EffectDiscription_ENG;
            }
        }

        if (cEntity_Base.CardID == "EX5-032")
        {
            if (!cEntity_Base.EffectDiscription_ENG.Contains("Leomon"))
            {
                cEntity_Base.EffectDiscription_ENG = "[Digivolve] Lv.4 w/[Leomon] in name: Cost 3" + cEntity_Base.EffectDiscription_ENG;
            }
        }

        if (cEntity_Base.CardID == "EX5-044")
        {
            if (!cEntity_Base.EffectDiscription_ENG.Contains("Frimon"))
            {
                cEntity_Base.EffectDiscription_ENG = "[Digivolve][Frimon]: Cost 0" + cEntity_Base.EffectDiscription_ENG;
            }
        }

        if (cEntity_Base.CardID == "EX5-047")
        {
            if (!cEntity_Base.EffectDiscription_ENG.Contains("Liollmon") || !cEntity_Base.EffectDiscription_ENG.Contains("Elecmon"))
            {
                cEntity_Base.EffectDiscription_ENG = "Digivolve: 2 from [Liollmon] or [Elecmon]" + cEntity_Base.EffectDiscription_ENG;
            }
        }

        if (cEntity_Base.CardID == "EX5-048")
        {
            if (!cEntity_Base.EffectDiscription_ENG.Contains("Sukamon"))
            {
                cEntity_Base.EffectDiscription_ENG = "[Digivolve] Lv.4 w/[Sukamon] in name: Cost 3" + cEntity_Base.EffectDiscription_ENG;
            }
        }

        if (cEntity_Base.CardID == "EX5-049")
        {
            if (!cEntity_Base.EffectDiscription_ENG.Contains("Leomon"))
            {
                cEntity_Base.EffectDiscription_ENG = "[Digivolve] Lv.4 w/[Leomon] in name: Cost 3" + cEntity_Base.EffectDiscription_ENG;
            }
        }

        if (cEntity_Base.CardID == "EX5-055")
        {
            if (!cEntity_Base.EffectDiscription_ENG.Contains("Leomon"))
            {
                cEntity_Base.EffectDiscription_ENG = "[Digivolve] Lv.5 w/[Leomon] in name: Cost 4" + cEntity_Base.EffectDiscription_ENG;
            }
        }

        if (cEntity_Base.CardID == "EX5-073")
        {
            if (!cEntity_Base.EffectDiscription_ENG.Contains("Apollomon") || !cEntity_Base.EffectDiscription_ENG.Contains("Dianamon"))
            {
                cEntity_Base.EffectDiscription_ENG = "��DNA Digivolution: 0 from [Apollomon] + [Dianamon]��" + cEntity_Base.EffectDiscription_ENG;
            }
        }

        if (cEntity_Base.CardID == "BT7-085")
        {
            cEntity_Base.EffectDiscription_JPN = "�y�o�ꎞ�z����̃f�W����1�̂̐i�������A������3���j������B";
            cEntity_Base.EffectDiscription_ENG = "[On Play] Trash 3 digivolution cards from the bottom of 1 of your opponent's Digimon.";
            cEntity_Base.InheritedEffectDiscription_JPN = "�y�A�^�b�N���z�m�^�[����1��n���̑���̃^�[���I�����܂ŁA�i�����������Ȃ�����̃f�W����1�̂̓A�^�b�N�ƃu���b�N���ł��Ȃ��B";
            cEntity_Base.InheritedEffectDiscription_ENG = "[When Attacking][Once Per Turn] Until the end of your opponent's next turn, 1 of your opponent's Digimon with no digivolution cards can't attack or block.";
            cEntity_Base.SecurityEffectDiscription_JPN = "�y�Z�L�����e�B�z���̃J�[�h���R�X�g���x���킸�ɓo�ꂳ����B";
            cEntity_Base.SecurityEffectDiscription_ENG = "[Security] Play this card without paying its memory cost.";
        }

        if (cEntity_Base.CardID == "BT7-086")
        {
            cEntity_Base.EffectDiscription_JPN = "�y���C���z�m�^�[����1��n�����̃g���b�V������A�����Ɂu�n�C�u���b�h�́v�����J�[�h5�����D���ȏ��Ԃł��̃e�C�}�[�̉��ɒu�����ƂŁA���̃e�C�}�[��Ԃ�Lv.5�̃f�W�����Ƃ��Ĉ�����D�́u�J�C�[���O���C�����v�ɐi���R�X�g���x�����Đi���ł���B";
            cEntity_Base.EffectDiscription_ENG = "[Main][Once Per Turn] You may place 5 cards with [Hybrid] in their traits from your trash under this Tamer in any order to digivolve it into an [EmperorGreymon] in your hand for its digivolution cost as if this Tamer is a level 5 red Digimon.";
            cEntity_Base.InheritedEffectDiscription_JPN = "�y�����̃^�[���z���̃f�W������DP��+2000����B���̃f�W������DP��10000�ȏ�̊ԁA���̃f�W�����́�Z�L�����e�B�A�^�b�N+1��i���̃f�W�������`�F�b�N����Z�L�����e�B�̖���+1�j�𓾂�B";
            cEntity_Base.InheritedEffectDiscription_ENG = "[Your Turn] This Digimon gets +2000 DP. While this Digimon has 10000 DP or more, it gains <Security Attack +1>. (This Digimon checks 1 additional security card.)";
            cEntity_Base.SecurityEffectDiscription_JPN = "�y�Z�L�����e�B�z���̃J�[�h���R�X�g���x���킸�ɓo�ꂳ����B";
            cEntity_Base.SecurityEffectDiscription_ENG = "[Security] Play this card without paying its memory cost.";
        }

        if (cEntity_Base.CardID == "BT7-087")
        {
            cEntity_Base.EffectDiscription_JPN = "�y���C���z�m�^�[����1��n�����̎�D����A�����Ɂu�n�C�u���b�h�́v�����J�[�h5�����D���ȏ��Ԃł��̃e�C�}�[�̉��ɒu�����ƂŁA���̃e�C�}�[���Lv.5�̃f�W�����Ƃ��Ĉ�����D�́u�}�O�i�K���������v�ɐi���R�X�g���x�����Đi���ł���B";
            cEntity_Base.EffectDiscription_ENG = "[Main][Once Per Turn] You may place 5 cards with [Hybrid] in their traits from your hand under this Tamer in any order to digivolve it into a [MagnaGarurumon] in your hand for its digivolution cost as if this Tamer is a level 5 blue Digimon.";
            cEntity_Base.InheritedEffectDiscription_JPN = "�y�����̃^�[���z�m�^�[����1��n�����̎�D�����ʂő������Ƃ��A�������[��+1����B���̌�A���̃^�[���̊ԁA���̃f�W�����̓u���b�N����Ȃ��B";
            cEntity_Base.InheritedEffectDiscription_ENG = "[Your Turn][Once Per Turn] When an effect adds a card to your hand, gain 1 memory. Then, this Digimon can't be blocked for the turn.";
            cEntity_Base.SecurityEffectDiscription_JPN = "�y�Z�L�����e�B�z���̃J�[�h���R�X�g���x���킸�ɓo�ꂳ����B";
            cEntity_Base.SecurityEffectDiscription_ENG = "[Security] Play this card without paying its memory cost.";
        }

        if (cEntity_Base.CardID == "BT7-088")
        {
            cEntity_Base.EffectDiscription_JPN = "�y�o�ꎞ�z�����̃Z�L�����e�B�̓��e��S�Ċm�F���āA���̒��̓����Ɂu�n�C�u���b�h�́v���u�/���m�v�����J�[�h1�����I�[�v�����Ď�D�ɉ�������B�������Ƃ�, �჊�J�o���[+1�s�f�b�L�t��i�����̓f�b�L�̏ォ��J�[�h��1���Z�L�����e�B�̏�ɒu���j�B���̌�A�����̃Z�L�����e�B���V���b�t������B";
            cEntity_Base.EffectDiscription_ENG = "[On Play] You may search your security stack for 1 card with [Hybrid] or [Ten Warriors] in its traits, reveal it, and add it to your hand. If you added a card to your hand, <Recovery +1 (Deck)>. (Place the top card of your deck on top of your security stack.) Then, shuffle your security stack.";
            cEntity_Base.InheritedEffectDiscription_JPN = "�y����̃^�[���z�����̃Z�L�����e�B�f�W�����S�Ă�DP��+3000����B";
            cEntity_Base.InheritedEffectDiscription_ENG = "[Opponent's Turn] All of your Security Digimon get +3000 DP.";
            cEntity_Base.SecurityEffectDiscription_JPN = "�y�Z�L�����e�B�z���̃J�[�h���R�X�g���x���킸�ɓo�ꂳ����B";
            cEntity_Base.SecurityEffectDiscription_ENG = "[Security] Play this card without paying its memory cost.";
        }

        if (cEntity_Base.CardID == "BT7-089")
        {
            cEntity_Base.EffectDiscription_JPN = "�y�����̃^�[���z���̃e�C�}�[���΂̃f�W�����ɐi������Ƃ��A�x�����i���R�X�g��-1����B";
            cEntity_Base.EffectDiscription_ENG = "[Your Turn] When this Tamer digivolves into a green Digimon, reduce the memory cost of the digivolution by 1.";
            cEntity_Base.InheritedEffectDiscription_JPN = "��ђʁ�i���̃f�W�������A�^�b�N�����o�g���ő���̃f�W�������������ł������Ƃ��A���̃f�W�����̓Z�L�����e�B���`�F�b�N����j";
            cEntity_Base.InheritedEffectDiscription_ENG = "<Piercing> (When this Digimon attacks and deletes an opponent's Digimon and survives the battle, it performs any security checks it normally would.)";
            cEntity_Base.SecurityEffectDiscription_JPN = "�y�Z�L�����e�B�z���̃J�[�h���R�X�g���x���킸�ɓo�ꂳ����B";
            cEntity_Base.SecurityEffectDiscription_ENG = "[Security] Play this card without paying its memory cost.";
        }

        if (cEntity_Base.CardID == "BT7-091")
        {
            cEntity_Base.EffectDiscription_JPN = "�y�o�ꎞ�z��1�h���[��i�����̃f�b�L����J�[�h��1�������j�B���̌�A�����̎�D��1���j������B";
            cEntity_Base.EffectDiscription_ENG = " [On Play] <Draw 1>. (Draw 1 card from your deck.) Then, trash 1 card in your hand.";
            cEntity_Base.InheritedEffectDiscription_JPN = "�y���Ŏ��z�������[��+1����B";
            cEntity_Base.InheritedEffectDiscription_ENG = "[On Deletion] Gain 1 memory.";
            cEntity_Base.SecurityEffectDiscription_JPN = "�y�Z�L�����e�B�z���̃J�[�h���R�X�g���x���킸�ɓo�ꂳ����B";
            cEntity_Base.SecurityEffectDiscription_ENG = "[Security] Play this card without paying its memory cost.";
        }

        if (cEntity_Base.CardID == "BT12-088")
        {
            cEntity_Base.EffectDiscription_JPN = "�y�����̃^�[���J�n���z�������[��2�ȉ��̂Ƃ��A3�ɂ���B";
            cEntity_Base.EffectDiscription_ENG = "[Start of Your Turn] If you have 2 memory or less, set your memory to 3.";
            cEntity_Base.InheritedEffectDiscription_JPN = "�y�����̃^�[���z���̃f�W������DP+2000�B���̃f�W������DP��10000�ȏ�̊ԁA���̃f�W�����́u�y�����̃^�[���z[�^�[���ɂP��]���̃f�W����������̃Z�L�����e�B���`�F�b�N�����Ƃ��A�������[+2�B�v�̌��ʂ𓾂�B";
            cEntity_Base.InheritedEffectDiscription_ENG = "[Your Turn] This Digimon gets +2000 DP. While this Digimon has 10000 or more DP, it gains \"[Your Turn][Once per Turn] When this Digimon checks an opponent's security, gain 2 memory.\"";
            cEntity_Base.SecurityEffectDiscription_JPN = "�y�Z�L�����e�B�z���̃J�[�h���R�X�g���x���킸�ɓo�ꂳ����B";
            cEntity_Base.SecurityEffectDiscription_ENG = "[Security] Play this card without paying its memory cost.";
        }

        if (cEntity_Base.CardID == "BT14-086")
        {
            cEntity_Base.EffectDiscription_JPN = "�y�����̃��C���t�F�C�Y�J�n���z����̃f�W����������Ȃ�A�������[+1�B�y���C���z���̂Ɂu�k�������v/�u���񂴂������v���܂ނ������Ɂu�f�W�΁v���������̃f�W����1�̂Ɂ�}�C���h�����N��i���̃e�C�}�[���A�i�����Ƀe�C�}�[�J�[�h���������̃f�W�����̐i�����̉��ɒu���j�B";
            cEntity_Base.EffectDiscription_ENG = "[Start of Your Main Phase] If your opponent has a Digimon, gain 1 memory. [Main] <Mind Link> with 1 of your Digimon with [Numemon] or [Monzaemon] in its name, or the [DigiPolice] trait. (Place this Tamer as that Digimon's bottom digivolution card if there are no Tamer cards in its digivolution cards.)";
            cEntity_Base.InheritedEffectDiscription_JPN = "�y���݂��̃^�[���z���̃f�W���������̂Ɂu�k�������v/�u���񂴂������v���܂ނ������Ɂu�f�W�΁v�����ԁA���̃f�W�����́�W���~���O��Ɓ�ċN����𓾂�B�y���݂��̃^�[���I�����z���̃f�W�����̐i��������A�u�ʕP�ь��v1�����R�X�g���x���킸�ɓo��ł���B";
            cEntity_Base.InheritedEffectDiscription_ENG = "[All Turns] While this Digimon has [Numemon] or [Monzaemon] in its name, or the [DigiPolice] trait, it gains <Jamming> and <Reboot>. [End of All Turns] You may play 1 [Satsuki Tamahime] from this Digimon's digivolution cards without paying the cost.";
            cEntity_Base.SecurityEffectDiscription_JPN = "�y�Z�L�����e�B�z���̃J�[�h���R�X�g���x���킸�ɓo�ꂳ����B";
            cEntity_Base.SecurityEffectDiscription_ENG = "[Security] Play this card without paying its memory cost.";
        }

        if (cEntity_Base.CardID == "BT14-087")
        {
            cEntity_Base.EffectDiscription_JPN = "�y�����̃��C���t�F�C�Y�J�n���z����̃f�W����������Ȃ�A�������[+1�B�y���C���z�����Ɂu���b�^�v/�uSoC�v���������̃f�W����1�̂Ɂ�}�C���h�����N��i���̃e�C�}�[���A�i�����Ƀe�C�}�[�J�[�h���������̃f�W�����̐i�����̉��ɒu���j�B";
            cEntity_Base.EffectDiscription_ENG = "[Start of Your Main Phase] If your opponent has a Digimon, gain 1 memory. [Main] <Mind Link> with 1 of your Digimon with the [Dark Animal] or [SoC] trait. (Place this Tamer as that Digimon's bottom digivolution card if there are no Tamer cards in its digivolution cards.)";
            cEntity_Base.InheritedEffectDiscription_JPN = "�y���݂��̃^�[���z���̃f�W�����������Ɂu���b�^�v/�uSoC�v�����ԁA���̃f�W�����́�A�g��Ɓ�u���b�J�[��𓾂�B�y���݂��̃^�[���I�����z���̃f�W�����̐i��������A�u�i�Z�l�m�v1�����R�X�g���x���킸�ɓo��ł���B";
            cEntity_Base.InheritedEffectDiscription_ENG = "[All Turns] While this Digimon has the [Dark Animal] or [SoC] trait, it gains <Alliance> and <Blocker>. [End of All Turns] You may play 1 [Eiji Nagasumi] from this Digimon's digivolution cards without paying the cost.";
            cEntity_Base.SecurityEffectDiscription_JPN = "�y�Z�L�����e�B�z���̃J�[�h���R�X�g���x���킸�ɓo�ꂳ����B";
            cEntity_Base.SecurityEffectDiscription_ENG = "[Security] Play this card without paying its memory cost.";
        }

        if (cEntity_Base.CardID == "EX4-012")
        {
            cEntity_Base.EffectDiscription_JPN = "�y�i�����zDP6000�ȉ��̑���̃f�W����1�̂����ł�����B����̃f�W����1�̂��ƂɁA����DP���Ō��ʂ̏���{2000�B�y���݂��̃^�[���z�m�^�[����1��n����̃f�W���������ł����Ƃ��A�����̃e�C�}�[������Ȃ�A�ł�DP�̍�������̃f�W����1�̂����ł�����B";
        }

        if (cEntity_Base.CardID == "BT13-007")
        {
            cEntity_Base.EffectDiscription_ENG = "[Breeding] [Your Turn] All of your Digimon can't digivolve.\n[Breeding][Your Turn][Once Per Turn] When a [Royal Knight] trait Digimon card would be played, you may reduce the play cost by 4. Further reduce it by 1 for each of this Digimon's digivolution cards.\n[Breeding][Start of Your Main Phase] Reveal the top card of your Digi - Egg deck, then pllace that card and all of your [Royal Knight] trait Digimon as this Digimon as its bottom digivolution cards.";
        }

        if (cEntity_Base.CardID == "EX5-070")
        {
            cEntity_Base.EffectDiscription_JPN = "�����̃f�W����������ԁA���̃J�[�h�͐F�����𖳎��ł���B�y�Z�L�����e�B�z���̃J�[�h����D�ɉ�����B�y���C���z�i�����ɁuX�R�́v���Ȃ������̃f�W����1�̂���D�̓����ɁuX�R�́v�����f�W�����J�[�h�Ɏx�����i���R�X�g-1�Ői���ł���B���������Ƃ��A���̃J�[�h�����̃f�W�����̐i�����̉��ɒu���B�q���[���r����:�uX�R�́v�Ƃ��Ă������B";
            cEntity_Base.EffectDiscription_ENG = "While you have a Digimon, you may ignore this card's color requirements.[Security] Add this card to its owner's hand.[Main] 1 of your Digimon without [X Antibody] in its digivolution cards may digivolve into a Digimon card with the [X Antibody] trait in your hand with the digivolution cost reduced by 1. If it did, place this card as its bottom digivolution card.[Rule] Name: Also treated as [X Antibody].";
            cEntity_Base.InheritedEffectDiscription_JPN = "�y���݂��̃^�[���z���̃f�W�����������̌��ʈȊO�Ńo�g���G���A�𗣂��Ƃ��A���̃f�W�����̐i��������A�f�W�����J�[�h1������D�ɖ߂��A�uX�R�́v1���������̃Z�L�����e�B�̏�ɒu���B";
            cEntity_Base.InheritedEffectDiscription_ENG = "[All Turns] When this Digimon would leave the battle area other than by one of your effects, from this Digimon's digivolution cards, return 1 Digimon card to the hand and place 1 [X Antibody] on top of your security stack.";
            cEntity_Base.SecurityEffectDiscription_JPN = "";
            cEntity_Base.SecurityEffectDiscription_ENG = "";
        }

        if (!string.IsNullOrEmpty(cEntity_Base.EffectDiscription_ENG))
        {
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("and/or", "and");
            cEntity_Base.EffectDiscription_ENG = cEntity_Base.EffectDiscription_ENG.Replace("<br>", "");
        }

        if (!string.IsNullOrEmpty(cEntity_Base.InheritedEffectDiscription_ENG))
        {
            cEntity_Base.InheritedEffectDiscription_ENG = cEntity_Base.InheritedEffectDiscription_ENG.Replace("and/or", "and");
            cEntity_Base.InheritedEffectDiscription_ENG = cEntity_Base.InheritedEffectDiscription_ENG.Replace("<br>", "");
        }

        if (!string.IsNullOrEmpty(cEntity_Base.SecurityEffectDiscription_ENG))
        {
            cEntity_Base.SecurityEffectDiscription_ENG = cEntity_Base.SecurityEffectDiscription_ENG.Replace("and/or", "and");
            cEntity_Base.SecurityEffectDiscription_ENG = cEntity_Base.SecurityEffectDiscription_ENG.Replace("<br>", "");
        }
        #endregion

        #region �^�O���폜
        cEntity_Base.EffectDiscription_ENG = GetTagRemovedString(cEntity_Base.EffectDiscription_ENG);
        cEntity_Base.InheritedEffectDiscription_ENG = GetTagRemovedString(cEntity_Base.InheritedEffectDiscription_ENG);
        cEntity_Base.SecurityEffectDiscription_ENG = GetTagRemovedString(cEntity_Base.SecurityEffectDiscription_ENG);
        cEntity_Base.EffectDiscription_JPN = GetTagRemovedString(cEntity_Base.EffectDiscription_JPN);
        cEntity_Base.InheritedEffectDiscription_JPN = GetTagRemovedString(cEntity_Base.InheritedEffectDiscription_JPN);
        cEntity_Base.SecurityEffectDiscription_JPN = GetTagRemovedString(cEntity_Base.SecurityEffectDiscription_JPN);

        Debug.LogWarning(cEntity_Base.cardColors.Count);
        #endregion
    }

    static void AttachEnglishCardDataFromDigimonDev(CEntity_Base cEntity_Base)
    {
        string url = $"https://digimoncardgame.fandom.com/wiki/{cEntity_Base.CardID}";

        WebClient wc = new WebClient();
        wc.Encoding = Encoding.UTF8;
        string resultText = wc.DownloadString(url);
        List<string> parseByEnter = resultText.Split('\n').ToList();

        for (int i = 0; i < parseByEnter.Count; i++)
        {
            if (!string.IsNullOrEmpty(parseByEnter[i]))
            {
                bool replaceEmpty = true;

                if (i >= 3)
                {
                    if (!string.IsNullOrEmpty(parseByEnter[i]) && (!string.IsNullOrEmpty(parseByEnter[i - 1]) || !string.IsNullOrEmpty(parseByEnter[i - 3])))
                    {
                        if (parseByEnter[i - 1].Contains("<td>Name</td>"))
                        {
                            replaceEmpty = false;
                        }

                        if (parseByEnter[i - 1].Contains("<td>Form</td>"))
                        {
                            replaceEmpty = false;
                        }

                        if (parseByEnter[i - 1].Contains("<td>Attribute</td>"))
                        {
                            replaceEmpty = false;
                        }

                        if (parseByEnter[i - 1].Contains("<td>Type</td>"))
                        {
                            replaceEmpty = false;
                        }

                        if ((parseByEnter[i - 3].Contains("Card Effect(s)") || parseByEnter[i - 3].Contains("CardEffect(s)")) && parseByEnter[i - 3].Contains("sectionheader"))
                        {
                            replaceEmpty = false;
                        }

                        if ((parseByEnter[i - 3].Contains("Inherited Effect") || parseByEnter[i - 3].Contains("InheritedEffect")) && parseByEnter[i - 3].Contains("sectionheader"))
                        {
                            replaceEmpty = false;
                        }

                        if ((parseByEnter[i - 3].Contains("Security Effect") || parseByEnter[i - 3].Contains("SecurityEffect")) && parseByEnter[i - 3].Contains("sectionheader"))
                        {
                            replaceEmpty = false;
                        }
                    }
                }

                parseByEnter[i] = parseByEnter[i].Replace("\t", "").Replace("\n", "").Trim();

                if (replaceEmpty)
                {
                    parseByEnter[i] = parseByEnter[i].Replace(" ", "").Trim();
                }

                else
                {
                    parseByEnter[i] = GetTagRemovedString(parseByEnter[i]);
                }
            }
        }

        for (int i = 0; i < parseByEnter.Count; i++)
        {
            if (!string.IsNullOrEmpty(parseByEnter[i]))
            {
                if (i >= 3)
                {
                    if (!string.IsNullOrEmpty(parseByEnter[i]) && (!string.IsNullOrEmpty(parseByEnter[i - 1]) || !string.IsNullOrEmpty(parseByEnter[i - 3])))
                    {
                        //�J�[�h��
                        if (parseByEnter[i - 1].Contains("<td>Name</td>"))
                        {
                            cEntity_Base.CardName_ENG = parseByEnter[i];
                        }

                        //�`��
                        if (parseByEnter[i - 1].Contains("<td>Form</td>"))
                        {
                            if (!string.IsNullOrEmpty(parseByEnter[i]))
                            {
                                if (parseByEnter[i] != "-")
                                {
                                    cEntity_Base.Form_ENG.Add(parseByEnter[i]);
                                }
                            }
                        }

                        //����
                        if (parseByEnter[i - 1].Contains("<td>Attribute</td>"))
                        {
                            if (!string.IsNullOrEmpty(parseByEnter[i]))
                            {
                                if (parseByEnter[i] != "-")
                                {
                                    cEntity_Base.Attribute_ENG.Add(parseByEnter[i]);
                                }
                            }
                        }

                        //�^�C�v
                        if (parseByEnter[i - 1].Contains("<td>Type</td>"))
                        {
                            if (!string.IsNullOrEmpty(parseByEnter[i]))
                            {
                                if (parseByEnter[i] != "-")
                                {
                                    string[] typeStrings = parseByEnter[i].Split('/');

                                    foreach (string typeString in typeStrings)
                                    {
                                        if (!string.IsNullOrEmpty(typeString))
                                        {
                                            cEntity_Base.Type_ENG.Add(typeString);
                                        }
                                    }
                                }
                            }
                        }

                        //����
                        if ((parseByEnter[i - 3].Contains("Card Effect(s)") || parseByEnter[i - 3].Contains("CardEffect(s)")) && parseByEnter[i - 3].Contains("sectionheader"))
                        {
                            if (!string.IsNullOrEmpty(parseByEnter[i]))
                            {
                                if (parseByEnter[i] != "-")
                                {
                                    cEntity_Base.EffectDiscription_ENG += parseByEnter[i];
                                }
                            }
                        }

                        //�i��������
                        if ((parseByEnter[i - 3].Contains("Inherited Effect") || parseByEnter[i - 3].Contains("InheritedEffect")) && parseByEnter[i - 3].Contains("sectionheader"))
                        {
                            if (!string.IsNullOrEmpty(parseByEnter[i]))
                            {
                                if (parseByEnter[i] != "-")
                                {
                                    cEntity_Base.InheritedEffectDiscription_ENG += parseByEnter[i];
                                }
                            }
                        }

                        //�Z�L�����e�B����
                        if ((parseByEnter[i - 3].Contains("Security Effect") || parseByEnter[i - 3].Contains("SecurityEffect")) && parseByEnter[i - 3].Contains("sectionheader"))
                        {
                            if (!string.IsNullOrEmpty(parseByEnter[i]))
                            {
                                if (parseByEnter[i] != "-")
                                {
                                    cEntity_Base.SecurityEffectDiscription_ENG += parseByEnter[i];
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    static string GetTagRemovedString(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            while (text.Contains("<") && text.Contains(">"))
            {
                int startIndex = text.IndexOf("<");

                int endIndex = text.IndexOf(">");

                string s = text.Substring(startIndex, endIndex - startIndex + 1);

                text = text.Replace(s, "");
            }

            text = DataBase.ReplaceToASCII(text);
        }

        return text;
    }
}
