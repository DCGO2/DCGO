using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace GameStats.Tests
{
    public class GameStatsUtilityTests
    {
        // Local player is player2, so the opponent is player1.
        static string Anonymize(string log, string localName) =>
            GameStatsUtility.AnonymizeLog(log, localName, "Player2", "Player1");

        // 1. Phase lines put the name on its own line.
        [Fact]
        public void AnonymizeLog_ReplacesPhaseLines()
        {
            string log =
                "\n-------------Turn: 1-------------\n\nActive Phase:\nAlice\n" +
                "\nDraw Phase:\nAlice\n" +
                "\nBreeding Phase:\nOpponent\n" +
                "\nMain Phase:\nOpponent\n" +
                "\nEnd Turn:\nAlice\n" +
                "\nMulligan Hand\nOpponent\n" +
                "\nDraw 1 card\nAlice\n";

            string expected =
                "\n-------------Turn: 1-------------\n\nActive Phase:\nPlayer2\n" +
                "\nDraw Phase:\nPlayer2\n" +
                "\nBreeding Phase:\nPlayer1\n" +
                "\nMain Phase:\nPlayer1\n" +
                "\nEnd Turn:\nPlayer2\n" +
                "\nMulligan Hand\nPlayer1\n" +
                "\nDraw 1 card\nPlayer2\n";

            Assert.Equal(expected, Anonymize(log, "Alice"));
        }

        // 2. A short name must not be replaced inside card text or other lines.
        [Fact]
        public void AnonymizeLog_ShortNameInsideCardText_IsLeftAlone()
        {
            string log = "\nPlay Card:Agumon (BT1-010)\nMain Phase:\na\nDigivolve: a to Greymon\n";
            string expected = "\nPlay Card:Agumon (BT1-010)\nMain Phase:\nPlayer2\nDigivolve: a to Greymon\n";

            Assert.Equal(expected, Anonymize(log, "a"));
        }

        // 3. Winner line written by ResultObject.
        [Fact]
        public void AnonymizeLog_ReplacesWinnerLine()
        {
            Assert.Equal("\nEnd Game\nWinner:Player2", Anonymize("\nEnd Game\nWinner:Alice", "Alice"));
            Assert.Equal("\nEnd Game\nWinner:Player1", Anonymize("\nEnd Game\nWinner:Opponent", "Alice"));
        }

        // 4. Security suffix written by CardController.
        [Fact]
        public void AnonymizeLog_ReplacesSecuritySuffix()
        {
            string log = "\nAdd 2 cards to security:Alice\n\nAdd 1 cards to security:Opponent\n";
            string expected = "\nAdd 2 cards to security:Player2\n\nAdd 1 cards to security:Player1\n";

            Assert.Equal(expected, Anonymize(log, "Alice"));
        }

        // 5. Name text that is not a whole line or known suffix is not touched.
        [Fact]
        public void AnonymizeLog_NameAsPartOfLine_IsLeftAlone()
        {
            string log = "\nAlice's Agumon\nWinner:Alice2\nAdd 1 cards to security:Alice2\n";

            Assert.Equal(log, Anonymize(log, "Alice"));
        }

        // 6. The local player named "Opponent" cannot be told apart, so both sides become Unknown.
        [Fact]
        public void AnonymizeLog_LocalNameIsOpponent_UsesUnknownLabel()
        {
            string log = "\nMain Phase:\nOpponent\n\nEnd Game\nWinner:Opponent\nAdd 1 cards to security:Opponent";
            string expected = "\nMain Phase:\nUnknown\n\nEnd Game\nWinner:Unknown\nAdd 1 cards to security:Unknown";

            string anonymized = Anonymize(log, "Opponent");

            Assert.Equal(expected, anonymized);
            Assert.DoesNotContain("Opponent", anonymized);
        }

        // 7. Windows line endings are kept.
        [Fact]
        public void AnonymizeLog_KeepsCrlfLineEndings()
        {
            string log = "\r\nMain Phase:\r\nAlice\r\n\r\nEnd Game\r\nWinner:Opponent\r\n";
            string expected = "\r\nMain Phase:\r\nPlayer2\r\n\r\nEnd Game\r\nWinner:Player1\r\n";

            Assert.Equal(expected, Anonymize(log, "Alice"));
        }

        // 8. A log with no line breaks still works.
        [Fact]
        public void AnonymizeLog_NoLineBreaks()
        {
            Assert.Equal("Player2", Anonymize("Alice", "Alice"));
            Assert.Equal("Winner:Player1", Anonymize("Winner:Opponent", "Alice"));
            Assert.Equal("Agumon", Anonymize("Agumon", "Alice"));
        }

        // 9. Empty input and missing names.
        [Fact]
        public void AnonymizeLog_EmptyLogOrName()
        {
            Assert.Equal("", Anonymize(null, "Alice"));
            Assert.Equal("", Anonymize("", "Alice"));

            // An empty name must not turn blank lines into labels.
            Assert.Equal("\n\nMain Phase:\n\nPlayer1\n", Anonymize("\n\nMain Phase:\n\nOpponent\n", ""));
            Assert.Equal("\n \nMain Phase:\nPlayer1\n", Anonymize("\n \nMain Phase:\nOpponent\n", " "));
            Assert.Equal("\nMain Phase:\nPlayer1\n", Anonymize("\nMain Phase:\nOpponent\n", null));
        }

        // 10. A name with a line break is replaced as a whole.
        [Fact]
        public void AnonymizeLog_NameWithLineBreak()
        {
            Assert.Equal("\nMain Phase:\nPlayer2\n", Anonymize("\nMain Phase:\nAl\nice\n", "Al\nice"));
        }

        // 11. Player1 as local side uses the right labels.
        [Fact]
        public void AnonymizeLog_LocalPlayer1()
        {
            string anonymized = GameStatsUtility.AnonymizeLog("\nMain Phase:\nBob\nWinner:Opponent", "Bob", "Player1", "Player2");

            Assert.Equal("\nMain Phase:\nPlayer1\nWinner:Player2", anonymized);
        }

        // 12. Match ID is the same on both clients and matches the contract.
        [Fact]
        public void MatchId_IsDeterministicLowercaseSha256()
        {
            string id1 = GameStatsUtility.MatchId("AbCd1234", -1234567890123L);
            string id2 = GameStatsUtility.MatchId("AbCd1234", -1234567890123L);

            Assert.Equal(id1, id2);
            Assert.Equal(64, id1.Length);
            Assert.Matches("^[0-9a-f]{64}$", id1);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes("AbCd1234:-1234567890123"));
                Assert.Equal(Convert.ToHexString(hash).ToLowerInvariant(), id1);
            }

            Assert.NotEqual(id1, GameStatsUtility.MatchId("AbCd1234", 1234567890123L));
            Assert.NotEqual(id1, GameStatsUtility.MatchId("AbCd1235", -1234567890123L));
        }

        // 13. Known vector so the collector can check the same value.
        [Fact]
        public void MatchId_KnownVector()
        {
            // sha256("AbCd1234:-1234567890123"), computed outside .NET
            Assert.Equal("2f23a4c635dae00f4d9b7ea81c763e626a4a4b063868bac4491602cb3cf251c4", GameStatsUtility.MatchId("AbCd1234", -1234567890123L));
        }

        // 14. Seed formatting does not depend on the current culture.
        [Fact]
        public void MatchId_IgnoresCurrentCulture()
        {
            CultureInfo original = CultureInfo.CurrentCulture;

            try
            {
                string invariant = GameStatsUtility.MatchId("room", -42);

                // This culture uses U+2212 as the minus sign.
                CultureInfo.CurrentCulture = new CultureInfo("sv-SE");

                Assert.Equal(invariant, GameStatsUtility.MatchId("room", -42));
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        // 15. End reason mapping.
        [Theory]
        [InlineData(true, "", true, true, "surrender")]
        [InlineData(true, "Some Effect", true, true, "surrender")]
        [InlineData(false, "Some Effect", true, true, "effect")]
        [InlineData(false, "", true, true, "normal")]
        [InlineData(false, null, true, false, "normal")]
        [InlineData(false, "", false, true, "draw")]
        [InlineData(false, "", false, false, "disconnect")]
        public void EndReason_Mapping(bool surrendered, string effectName, bool hasWinner, bool bothPlayersInRoom, string expected)
        {
            Assert.Equal(expected, GameStatsUtility.EndReason(surrendered, effectName, hasWinner, bothPlayersInRoom));
        }

        // 15b. Reconnect endings are disconnects, even with a winner or a surrender.
        [Theory]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void EndReason_ReconnectIsDisconnect(bool surrendered, bool hasWinner)
        {
            Assert.Equal("disconnect", GameStatsUtility.EndReason(surrendered, "", hasWinner, false, true));
        }

        // 19. Retry-After is honored within bounds.
        [Theory]
        [InlineData("60", 1000, 60000)]
        [InlineData("30", 1000, 30000)]
        [InlineData("600", 1000, 60000)]
        [InlineData("0", 1000, 1000)]
        [InlineData("", 2000, 2000)]
        [InlineData(null, 2000, 2000)]
        [InlineData("Wed, 21 Oct 2026 07:28:00 GMT", 2000, 2000)]
        public void RetryDelay_UsesRetryAfter(string retryAfter, int fallbackMilliseconds, int expected)
        {
            Assert.Equal(expected, GameStatsUtility.RetryDelayMilliseconds(retryAfter, fallbackMilliseconds));
        }

        // 16. Timestamps are UTC ISO-8601 with a Z suffix.
        [Fact]
        public void FormatUtc_WritesIso8601()
        {
            DateTime time = new DateTime(2026, 11, 5, 12, 34, 56, DateTimeKind.Utc);

            Assert.Equal("2026-11-05T12:34:56Z", GameStatsUtility.FormatUtc(time));
        }

        // 17. Side helpers.
        [Fact]
        public void SideHelpers()
        {
            Assert.Equal("player2", GameStatsUtility.OtherSide("player1"));
            Assert.Equal("player1", GameStatsUtility.OtherSide("player2"));
            Assert.Equal("Player1", GameStatsUtility.SideLabel("player1"));
            Assert.Equal("Player2", GameStatsUtility.SideLabel("player2"));
        }

        // 18. The player-written deck name is removed from the deck code.
        [Fact]
        public void StripDeckName_RemovesLeadingName()
        {
            Assert.Equal(",0a1b,2c,-1,", GameStatsUtility.StripDeckName("John's Deck,0a1b,2c,-1,"));
            Assert.Equal(",0a1b,2c,-1,", GameStatsUtility.StripDeckName(",0a1b,2c,-1,"));
            Assert.Equal("", GameStatsUtility.StripDeckName(null));
        }
    }
}
