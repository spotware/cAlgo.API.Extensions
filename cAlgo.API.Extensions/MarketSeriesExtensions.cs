﻿using cAlgo.API.Extensions.Types;
using cAlgo.API.Internals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo.API.Extensions
{
    public static class MarketSeriesExtensions
    {
        /// <summary>
        /// Returns the last bar index in a market series
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <returns>int</returns>
        public static int GetIndex(this MarketSeries marketSeries)
        {
            return marketSeries.Close.Count > 0 ? marketSeries.Close.Count - 1 : marketSeries.Close.Count;
        }

        /// <summary>
        /// Returns the volume profile of x latest bars in a market series
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="index">Last Bar Index</param>
        /// <param name="periods">Number of previous bars before provided index</param>
        /// <param name="symbol">The market series symbol</param>
        /// <returns>List<PriceVolume></returns>
        public static List<PriceLevel> GetVolumeProfile(this MarketSeries marketSeries, int index, int periods, Symbol symbol, double stepInPips)
        {
            List<PriceLevel> result = new List<PriceLevel>();

            double step = stepInPips * symbol.PipSize;

            for (int i = index; i > index - periods; i--)
            {
                double barRange = marketSeries.GetBarRange(i);

                double barVolume = symbol.ToTicks(barRange);

                if (barRange <= 0 || barVolume <= 0)
                {
                    continue;
                }

                double percentageAboveBarClose = (marketSeries.High[i] - marketSeries.Close[i]) / barRange;
                double percentageBelowBarClose = (marketSeries.Close[i] - marketSeries.Low[i]) / barRange;

                double bullishVolume = barVolume * percentageBelowBarClose;
                double bearishVolume = barVolume * percentageAboveBarClose;

                double barRangeInPips = symbol.ToPips(barRange);

                double bullishVolumePerPips = bullishVolume / barRangeInPips;
                double bearishVolumePerPips = bearishVolume / barRangeInPips;

                long bullishVolumePerLevel = (long)(bullishVolumePerPips * stepInPips);
                long bearishVolumePerLevel = (long)(bearishVolumePerPips * stepInPips);

                for (double level = marketSeries.Low[i]; level <= marketSeries.High[i]; level += step)
                {
                    level = Math.Round(level, symbol.Digits);

                    PriceLevel priceLevel = result.FirstOrDefault(pLevel => pLevel.Level == level);

                    if (priceLevel == null)
                    {
                        priceLevel = new PriceLevel
                        {
                            Level = level
                        };

                        result.Add(priceLevel);
                    }

                    priceLevel.BullishVolume += bullishVolumePerLevel;
                    priceLevel.BearishVolume += bearishVolumePerLevel;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the amount of percentage change of a value in comparison with it's previous value in a market series
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="index">The value index</param>
        /// <returns>double</returns>
        public static double GetPercentageChange(this MarketSeries marketSeries, int index)
        {
            return -((marketSeries.Open[index] - marketSeries.Close[index]) / marketSeries.Open[index]) * 100;
        }

        /// <summary>
        /// Returns the bar type
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="index">Index of bar</param>
        /// <returns>BarType</returns>
        public static BarType GetBarType(this MarketSeries marketSeries, int index)
        {
            if (marketSeries.Close[index] > marketSeries.Open[index])
            {
                return BarType.Up;
            }
            else if (marketSeries.Close[index] < marketSeries.Open[index])
            {
                return BarType.Down;
            }
            else
            {
                return BarType.Neutral;
            }
        }

        /// <summary>
        /// Returns the range of a bar in a market series
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="index">Bar index in market series</param>
        /// <param name="useOpenClose">Use bar open and close price instead of high and low?</param>
        /// <returns>double</returns>
        public static double GetBarRange(this MarketSeries marketSeries, int index, bool useOpenClose = false)
        {
            return useOpenClose ? Math.Abs(marketSeries.Open[index] - marketSeries.Close[index])
                : marketSeries.High[index] - marketSeries.Low[index];
        }

        /// <summary>
        /// Returns the range of a bar in a market series
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="index">Bar index in market series</param>
        /// <param name="symbol">The market series symbol</param>
        /// <param name="type">The return type</param>
        /// <param name="useOpenClose">Use bar open and close price instead of high and low?</param>
        /// <returns>double</returns>
        public static double GetBarRange(this MarketSeries marketSeries, int index, Symbol symbol, BarRangeType type, bool useOpenClose = false)
        {
            double range = marketSeries.GetBarRange(index, useOpenClose);

            if (type == BarRangeType.Normal)
            {
                return range;
            }
            else if (symbol == null)
            {
                throw new NullReferenceException("The symbol is null");
            }

            switch (type)
            {
                case BarRangeType.Pips:
                    return symbol.ToPips(range);

                case BarRangeType.Ticks:
                    return symbol.ToTicks(range);

                default:
                    throw new InvalidOperationException("Invalid BarRangeType");
            }
        }

        /// <summary>
        /// Returns the maximum bar range in a market series
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="index">The start bar index</param>
        /// <param name="periods">The number of previous bars</param>
        /// <param name="useOpenClose">Use bar open and close price instead of high and low?</param>
        /// <returns>double</returns>
        public static double GetMaxBarRange(this MarketSeries marketSeries, int index, int periods, bool useOpenClose = false)
        {
            double maxRange = double.MinValue;

            for (int i = index; i >= index - periods; i--)
            {
                maxRange = Math.Max(maxRange, marketSeries.GetBarRange(i, useOpenClose: useOpenClose));
            }

            return maxRange;
        }

        /// <summary>
        /// Returns the minimum bar range in a market series
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="index">The start bar index</param>
        /// <param name="periods">The number of previous bars</param>
        /// <param name="useOpenClose">Use bar open and close price instead of high and low?</param>
        /// <returns>double</returns>
        public static double GetMinBarRange(this MarketSeries marketSeries, int index, int periods, bool useOpenClose = false)
        {
            double minRange = double.MaxValue;

            for (int i = index; i >= index - periods; i--)
            {
                minRange = Math.Min(minRange, marketSeries.GetBarRange(i, useOpenClose: useOpenClose));
            }

            return minRange;
        }

        /// <summary>
        /// Returns the mean bar range in a market series
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="index">The start bar index</param>
        /// <param name="periods">The number of previous bars</param>
        /// <param name="useOpenClose">Use bar open and close price instead of high and low?</param>
        /// <returns>double</returns>
        public static double GetMeanBarRange(this MarketSeries marketSeries, int index, int periods, bool useOpenClose = false)
        {
            List<double> ranges = new List<double>();

            for (int i = index; i >= index - periods; i--)
            {
                ranges.Add(marketSeries.GetBarRange(i, useOpenClose: useOpenClose));
            }

            return ranges.Average();
        }

        /// <summary>
        /// Returns True if the index bar is an engulfing bar
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="index">The bar index number in a market series</param>
        /// <returns>bool</returns>
        public static bool IsEngulfingBar(this MarketSeries marketSeries, int index)
        {
            double barBodyRange = marketSeries.GetBarRange(index, useOpenClose: true);
            double previousBarRange = marketSeries.GetBarRange(index - 1);

            BarType barType = marketSeries.GetBarType(index);
            BarType previousBarType = marketSeries.GetBarType(index - 1);

            return barBodyRange > previousBarRange && barType != previousBarType ? true : false;
        }

        /// <summary>
        /// Returns True if the index bar is a rejection bar
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="index">The bar index number in a market series</param>
        /// <returns>bool</returns>
        public static bool IsRejectionBar(this MarketSeries marketSeries, int index)
        {
            double barBodyRange = marketSeries.GetBarRange(index, useOpenClose: true);
            double barRange = marketSeries.GetBarRange(index);

            BarType barType = marketSeries.GetBarType(index);

            double meanBarRange = marketSeries.GetMeanBarRange(index - 1, 50);

            if (barBodyRange / barRange < 0.3 && barRange > meanBarRange)
            {
                double barMiddle = (barRange * 0.5) + marketSeries.Low[index];
                double barFirstQuartile = (barRange * 0.25) + marketSeries.Low[index];
                double barThirdQuartile = (barRange * 0.75) + marketSeries.Low[index];

                if ((marketSeries.Open[index] > barMiddle && marketSeries.Close[index] > barThirdQuartile && barType == BarType.Up) ||
                    (marketSeries.Open[index] < barMiddle && marketSeries.Close[index] < barFirstQuartile && barType == BarType.Down))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns True if the index bar is a doji bar
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="index">The bar index number in a market series</param>
        /// <returns>bool</returns>
        public static bool IsDojiBar(this MarketSeries marketSeries, int index)
        {
            double barBodyRange = marketSeries.GetBarRange(index, useOpenClose: true);
            double barRange = marketSeries.GetBarRange(index);

            double meanBarRange = marketSeries.GetMeanBarRange(index - 1, 50);

            return barRange < meanBarRange / 3 && barBodyRange / barRange < 0.5 ? true : false;
        }

        /// <summary>
        /// Returns True if the index bar is an inside bar
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="index">The bar index number in a market series</param>
        /// <returns>bool</returns>
        public static bool IsInsideBar(this MarketSeries marketSeries, int index)
        {
            BarType barType = marketSeries.GetBarType(index);
            BarType previousBarType = marketSeries.GetBarType(index - 1);

            if (marketSeries.High[index] < marketSeries.High[index - 1] &&
                marketSeries.Low[index] > marketSeries.Low[index - 1] &&
                barType != previousBarType)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns True if the index bar is a three bar reversal
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="index">The bar index number in a market series</param>
        /// <returns>bool</returns>
        public static bool IsThreeBarReversal(this MarketSeries marketSeries, int index)
        {
            bool result = false;

            BarType barType = marketSeries.GetBarType(index);
            BarType previousBarType = marketSeries.GetBarType(index - 1);

            if (barType == BarType.Up && previousBarType == BarType.Down && marketSeries.GetBarType(index - 2) == BarType.Down)
            {
                if (marketSeries.Low[index - 1] < marketSeries.Low[index - 2] && marketSeries.Low[index - 1] < marketSeries.Low[index])
                {
                    if (marketSeries.Close[index] > marketSeries.Open[index - 1])
                    {
                        result = true;
                    }
                }
            }
            else if (barType == BarType.Down && previousBarType == BarType.Up && marketSeries.GetBarType(index - 2) == BarType.Up)
            {
                if (marketSeries.High[index - 1] > marketSeries.High[index - 2] && marketSeries.High[index - 1] > marketSeries.High[index])
                {
                    if (marketSeries.Close[index] < marketSeries.Open[index - 1])
                    {
                        result = true;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the candle type of given bar index
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="index">The bar index number in a market series</param>
        /// <returns>List<CandlePattern></returns>
        public static List<CandlePattern> GetCandlePatterns(this MarketSeries marketSeries, int index)
        {
            List<CandlePattern> patterns = new List<CandlePattern>();

            // Engulfing
            if (marketSeries.IsEngulfingBar(index))
            {
                patterns.Add(CandlePattern.Engulfing);
            }

            // Rejection
            if (marketSeries.IsRejectionBar(index))
            {
                patterns.Add(CandlePattern.Rejection);
            }

            // Doji
            if (marketSeries.IsDojiBar(index))
            {
                patterns.Add(CandlePattern.Doji);
            }

            // InsideBar
            if (marketSeries.IsInsideBar(index))
            {
                patterns.Add(CandlePattern.InsideBar);
            }

            // Three Reversal Bars
            if (marketSeries.IsThreeBarReversal(index))
            {
                patterns.Add(CandlePattern.ThreeBarReversal);
            }

            return patterns;
        }

        /// <summary>
        /// Returns True if the provided bar matches any of the provided patterns otherwise false
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="index">The bar index number in a market series</param>
        /// <param name="patternsToMatch">List of candle patterns to match</param>
        /// <returns>bool</returns>
        public static bool IsCandlePatternMatchesAny(this MarketSeries marketSeries, int index, List<CandlePattern> patternsToMatch)
        {
            List<CandlePattern> barPatterns = marketSeries.GetCandlePatterns(index);

            return patternsToMatch.Any(pattern => barPatterns.Contains(pattern));
        }

        /// <summary>
        /// Returns True if the provided bar matches all of the provided patterns otherwise false
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="index">The bar index number in a market series</param>
        /// <param name="patternsToMatch">List of candle patterns to match</param>
        /// <returns>bool</returns>
        public static bool IsCandlePatternMatchesAll(this MarketSeries marketSeries, int index, List<CandlePattern> patternsToMatch)
        {
            List<CandlePattern> barPatterns = marketSeries.GetCandlePatterns(index);

            return patternsToMatch.All(pattern => barPatterns.Contains(pattern));
        }

        /// <summary>
        /// Returns the largest bar index number between an interval in a market series
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="startIndex">Start index</param>
        /// <param name="endIndex">End index</param>
        /// <returns>int</returns>
        public static int GetLargestBarIndex(this MarketSeries marketSeries, int startIndex, int endIndex)
        {
            double maxBarRange = double.MinValue;

            int result = 0;

            for (int i = startIndex; i <= endIndex; i++)
            {
                double currentBarRange = marketSeries.High[i] - marketSeries.Low[i];

                if (currentBarRange > maxBarRange)
                {
                    maxBarRange = currentBarRange;

                    result = i;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the smallest bar index number between an interval in a market series
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="startIndex">Start index</param>
        /// <param name="endIndex">End index</param>
        /// <returns>int</returns>
        public static int GetSmallestBarIndex(this MarketSeries marketSeries, int startIndex, int endIndex)
        {
            double minBarRange = double.MinValue;

            int result = 0;

            for (int i = startIndex; i <= endIndex; i++)
            {
                double currentBarRange = marketSeries.High[i] - marketSeries.Low[i];

                if (currentBarRange < minBarRange)
                {
                    minBarRange = currentBarRange;

                    result = i;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the range between an index interval in a market series
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="startIndex">Start index</param>
        /// <param name="endIndex">End index</param>
        /// <param name="useBarBody">Use bar body (open and close) instead of shadows (high and low)</param>
        /// <returns>double</returns>
        public static double GetRange(this MarketSeries marketSeries, int startIndex, int endIndex, bool useBarBody = false)
        {
            double min = double.MinValue, max = double.MaxValue;

            for (int i = startIndex; i <= endIndex; i++)
            {
                double barLow, barHigh;

                if (useBarBody)
                {
                    barLow = marketSeries.GetBarType(i) == BarType.Up ? marketSeries.Open[i] : marketSeries.Close[i];
                    barHigh = marketSeries.GetBarType(i) == BarType.Up ? marketSeries.Close[i] : marketSeries.Open[i];
                }
                else
                {
                    barLow = marketSeries.Low[i];
                    barHigh = marketSeries.High[i];
                }

                min = Math.Min(min, barLow);
                max = Math.Max(max, barHigh);
            }

            return max - min;
        }

        /// <summary>
        /// Returns true if the bars on provided index interval is flat
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="startIndex">Start index</param>
        /// <param name="endIndex">End index</param>
        /// <param name="maxStd">Maximum allowed standard deviation in range high and low</param>
        /// <returns>bool</returns>
        public static bool IsFlat(this MarketSeries marketSeries, int startIndex, int endIndex, double maxStd)
        {
            double highStd = marketSeries.High.GetStandardDeviation(startIndex, endIndex);
            double lowStd = marketSeries.Low.GetStandardDeviation(startIndex, endIndex);

            return highStd <= maxStd && lowStd <= maxStd;
        }

        /// <summary>
        /// Returns a market series specific data series based on provided series type
        /// </summary>
        /// <param name="marketSeries">The market series</param>
        /// <param name="seriesType">Series type</param>
        /// <returns>DataSeries</returns>
        public static DataSeries GetSeries(this MarketSeries marketSeries, SeriesType seriesType)
        {
            switch (seriesType)
            {
                case SeriesType.Open:
                    return marketSeries.Open;

                case SeriesType.High:
                    return marketSeries.High;

                case SeriesType.Low:
                    return marketSeries.Low;

                case SeriesType.Close:
                    return marketSeries.Close;

                case SeriesType.Median:
                    return marketSeries.Median;

                case SeriesType.TickVolume:
                    return marketSeries.TickVolume;

                case SeriesType.Typical:
                    return marketSeries.Typical;

                case SeriesType.WeightedClose:
                    return marketSeries.WeightedClose;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns a bar open time by giving its index, it supports both past and future bars but the future bars provided
        /// open time is an approximation based on previous bars time differences not exact open time
        /// </summary>
        /// <param name="marketSeries">The market series</param>
        /// <param name="barIndex">The bar index</param>
        /// <returns>DateTime</returns>
        public static DateTime GetOpenTime(this MarketSeries marketSeries, double barIndex)
        {
            int currentIndex = marketSeries.GetIndex();

            TimeSpan timeDiff = marketSeries.GetTimeDiff();

            double indexDiff = barIndex - currentIndex;

            double indexDiffAbs = Math.Abs(indexDiff);

            double indexDiffFloor = Math.Floor(indexDiffAbs);

            double indexDiffFraction = indexDiffAbs - indexDiffFloor;

            DateTime result = indexDiff <= 0 ? marketSeries.OpenTime[(int)barIndex] : marketSeries.OpenTime[currentIndex];

            if (indexDiff > 0)
            {
                for (int i = 1; i <= indexDiffAbs; i++)
                {
                    do
                    {
                        result = result.Add(timeDiff);
                    }
                    while (result.DayOfWeek == DayOfWeek.Saturday || result.DayOfWeek == DayOfWeek.Sunday);
                }
            }

            double indexDiffFractionInMinutes = timeDiff.TotalMinutes * indexDiffFraction;

            result = result.AddMinutes(indexDiff > 0 ? indexDiffFractionInMinutes : -indexDiffFractionInMinutes);

            return result;
        }

        /// <summary>
        /// Returns the most common time difference between two bar of a market series
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <returns>TimeSpan</returns>
        public static TimeSpan GetTimeDiff(this MarketSeries marketSeries)
        {
            int index = marketSeries.GetIndex();

            if (index < 5)
            {
                throw new InvalidOperationException("Not enough data in market series to calculate the time difference");
            }

            List<TimeSpan> timeDiffs = new List<TimeSpan>();

            for (int i = index; i >= index - 5; i--)
            {
                timeDiffs.Add(marketSeries.OpenTime[i] - marketSeries.OpenTime[i - 1]);
            }

            return timeDiffs.GroupBy(diff => diff).OrderBy(diffGroup => diffGroup.Count()).Last().First();
        }

        /// <summary>
        /// Returns the market profile of x latest bars in a market series
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="index">Last Bar Index</param>
        /// <param name="periods">Number of previous bars before provided index</param>
        /// <param name="symbol">The market series symbol</param>
        /// <param name="step">The price increment step in Pips</param>
        /// <returns>List<PriceVolume></returns>
        public static List<PriceLevel> GetMarketProfile(this MarketSeries marketSeries, int index, int periods, Symbol symbol, double stepInPips)
        {
            double step = stepInPips * symbol.PipSize;

            List<PriceLevel> result = new List<PriceLevel>();

            for (int i = index; i > index - periods; i--)
            {
                for (double level = marketSeries.Low[i]; level <= marketSeries.High[i]; level += step)
                {
                    level = Math.Round(level, symbol.Digits);

                    PriceLevel priceLevel = result.FirstOrDefault(pLevel => pLevel.Level == level);

                    if (priceLevel == null)
                    {
                        priceLevel = new PriceLevel
                        {
                            Level = level,
                            Profile = new List<int>()
                        };

                        result.Add(priceLevel);
                    }

                    priceLevel.Profile.Add(i);
                }
            }

            return result;
        }

        /// <summary>
        /// Combines the input price levels volume profile and market profile data based on provided width
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="data">The input data</param>
        /// <param name="width">Width in term of price not Pips</param>
        /// <param name="symbol">The symbol of price levels</param>
        /// <returns>List<PriceLevel></returns>
        public static List<PriceLevel> GetCombinedPriceLevels(this MarketSeries marketSeries, List<PriceLevel> data, double width)
        {
            List<PriceLevel> ordered = data.OrderBy(priceLevel => priceLevel.Level).ToList();

            List<PriceLevel> dataCombined = new List<PriceLevel>();

            PriceLevel currentLevel = new PriceLevel
            {
                Level = ordered.First().Level,
                Profile = new List<int>()
            };

            ordered.ForEach(priceLevel =>
            {
                if (priceLevel.Level >= currentLevel.Level && priceLevel.Level <= currentLevel.Level + width)
                {
                    // Market profile
                    if (priceLevel.Profile != null)
                    {
                        currentLevel.Profile.AddRange(priceLevel.Profile);
                    }

                    // Volume profile
                    currentLevel.BearishVolume += priceLevel.BearishVolume;
                    currentLevel.BullishVolume += priceLevel.BullishVolume;
                }
                else
                {
                    currentLevel = new PriceLevel
                    {
                        Level = priceLevel.Level,
                        Profile = new List<int>()
                    };
                }

                if (!dataCombined.Contains(currentLevel))
                {
                    dataCombined.Add(currentLevel);
                }
            });

            return dataCombined;
        }
    }
}