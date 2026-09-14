using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AISelection.Tests
{
    public class AISelectionUtilityTests
    {
        static List<int> Ints(params int[] values) => values.ToList();

        // 1. Exact-count selection (maxCount must be met, cannot end early).
        [Fact]
        public void Choose_SelectsExactlyMaxCount_WhenCanEndNotMaxIsFalse()
        {
            List<int> chosen = AISelectionUtility.Choose(
                totalCount: 5,
                canEndSelect: (indexes) => indexes.Count == 3,
                canAdd: null,
                maxCount: 3,
                canNoSelect: false,
                canEndNotMax: false);

            Assert.NotNull(chosen);
            Assert.Equal(3, chosen.Count);
            Assert.Equal(chosen.Count, chosen.Distinct().Count());
            Assert.All(chosen, (index) => Assert.InRange(index, 0, 4));
        }

        // 2. P-094 Destromon regression: maxCount equals ALL eligible permanents but the
        //    effect only allows deleting up to a total play cost (3 + Vemmon count).
        //    The old bot always selected maxCount targets, whose total cost always
        //    exceeded the cap, so it deadlocked. The selection must be able to end early.
        [Fact]
        public void Choose_P094CostCapped_EndsEarlyWithinTotalCostCap()
        {
            int[] costs = { 4, 4, 1, 1, 1 };
            const int totalCostLimit = 3;

            List<int> chosen = AISelectionUtility.Choose(
                totalCount: costs.Length,
                canEndSelect: (indexes) => indexes.Sum((i) => costs[i]) <= totalCostLimit,
                canAdd: null,
                maxCount: costs.Length,
                canNoSelect: false,
                canEndNotMax: true);

            Assert.NotNull(chosen);
            Assert.NotEmpty(chosen);
            Assert.True(chosen.All((i) => i >= 0 && i < costs.Length));
            Assert.Equal(chosen.Count, chosen.Distinct().Count());
            Assert.True(chosen.Sum((i) => costs[i]) <= totalCostLimit);
        }

        // 3. BT19-096 Hornet Eraser shape: "choose up to X play cost total" against a list
        //    of enemy Digimon; the bot may select fewer targets than the count allows.
        [Fact]
        public void Choose_DeleteUpToTotalCost_MaySelectFewerThanMaxCount()
        {
            int[] costs = { 6, 6, 5, 2, 2 };
            const int totalCostLimit = 8;

            List<int> chosen = AISelectionUtility.Choose(
                totalCount: costs.Length,
                canEndSelect: (indexes) => indexes.Sum((i) => costs[i]) <= totalCostLimit,
                canAdd: null,
                maxCount: costs.Length,
                canNoSelect: false,
                canEndNotMax: true);

            Assert.NotNull(chosen);
            Assert.NotEmpty(chosen);
            Assert.True(chosen.Sum((i) => costs[i]) <= totalCostLimit);
        }

        // 4. Declining: when nothing satisfies the end condition and the effect allows
        //    choosing nothing, the AI returns null (mirrors the human "No Selection" path).
        [Fact]
        public void Choose_ReturnsNull_WhenNothingValidAndCanNoSelect()
        {
            List<int> chosen = AISelectionUtility.Choose(
                totalCount: 3,
                canEndSelect: (indexes) => false,
                canAdd: null,
                maxCount: 2,
                canNoSelect: true,
                canEndNotMax: true);

            Assert.Null(chosen);
        }

        // 5. No valid set and declining is not allowed: best-effort single target so the
        //    game never hangs (the effect fizzles instead of freezing).
        [Fact]
        public void Choose_FallsBackToSingleTarget_WhenNothingValidAndCannotDecline()
        {
            List<int> chosen = AISelectionUtility.Choose(
                totalCount: 3,
                canEndSelect: (indexes) => false,
                canAdd: null,
                maxCount: 2,
                canNoSelect: false,
                canEndNotMax: true);

            Assert.NotNull(chosen);
            Assert.Equal(new List<int>() { 0 }, chosen);
        }

        // 6. Empty candidate list: no targets at all, decline (do not fabricate targets).
        [Fact]
        public void Choose_ReturnsNull_WhenNoCandidates()
        {
            List<int> chosen = AISelectionUtility.Choose(
                totalCount: 0,
                canEndSelect: (indexes) => indexes.Count == 1,
                canAdd: null,
                maxCount: 1,
                canNoSelect: false,
                canEndNotMax: true);

            Assert.Null(chosen);
        }

        // 7. Incremental canAdd validation is applied while building the selection.
        //    Index 1 is never acceptable, so the largest valid set is drawn from {0,2}.
        [Fact]
        public void Choose_RespectsIncrementalCanAddValidation()
        {
            List<int> chosen = AISelectionUtility.Choose(
                totalCount: 3,
                canEndSelect: (indexes) => indexes.Count == 2,
                canAdd: (prefix, index) => index != 1,
                maxCount: 2,
                canNoSelect: false,
                canEndNotMax: false);

            Assert.NotNull(chosen);
            Assert.Equal(2, chosen.Count);
            Assert.DoesNotContain(1, chosen);
        }
    }
}