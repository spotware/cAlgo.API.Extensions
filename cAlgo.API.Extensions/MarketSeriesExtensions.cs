﻿using cAlgo.API.Internals;
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
            return marketSeries.Close.Count - 1;
        }

        /// <summary>
        /// Returns the volume profile of x latest values in a market series
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="periods">The number of latest values</param>
        /// <param name="priceStep">The step (Pips) that is used for increament of price</param>
        /// <param name="symbol">The market series symbol</param>
        /// <returns>List<PriceVolume></returns>
        public static List<PriceVolume> GetPriceVolume(this MarketSeries marketSeries, int periods, double priceStep, Symbol symbol)
        {
            List<PriceVolume> result = new List<PriceVolume>();

            int index = marketSeries.GetIndex();

            double priceStepInPrice = symbol.ToPips(priceStep);

            for (int i = index; i > index - periods; i--)
            {
                double barRange = symbol.ToPips(marketSeries.High[i] - marketSeries.Low[i]) / priceStepInPrice;

                long volumePerPriceLevel = (long)(marketSeries.TickVolume[i] / barRange);

                for (double price = marketSeries.Low[i]; price <= marketSeries.High[i]; price += priceStep)
                {
                    price = Math.Round(price, symbol.Digits);

                    PriceVolume priceVolume = result.FirstOrDefault(pVolume => pVolume.Price == price);

                    if (priceVolume == null)
                    {
                        priceVolume = new PriceVolume() { Price = price };

                        result.Add(priceVolume);
                    }

                    if (marketSeries.Close[i] > marketSeries.Open[i])
                    {
                        priceVolume.BullishVolume += volumePerPriceLevel;
                    }
                    else if (marketSeries.Close[i] < marketSeries.Open[i])
                    {
                        priceVolume.BearishVolume += volumePerPriceLevel;
                    }
                    else
                    {
                        priceVolume.NeutralVolume += volumePerPriceLevel;
                    }
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
            return marketSeries.Close[index] > marketSeries.Open[index] ? BarType.Up : BarType.Down;
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
            return useOpenClose ? marketSeries.Open[index] - marketSeries.Close[index] : marketSeries.High[index] - marketSeries.Low[index];
        }

        /// <summary>
        /// Returns the range of a bar in a market series in Pips
        /// </summary>
        /// <param name="marketSeries"></param>
        /// <param name="index">Bar index in market series</param>
        /// <param name="useOpenClose">Use bar open and close price instead of high and low?</param>
        /// <returns>double</returns>
        public static double GetBarRangeInPips(this MarketSeries marketSeries, Symbol symbol, int index, bool useOpenClose = false)
        {
            return useOpenClose ? symbol.ToPips(marketSeries.Open[index] - marketSeries.Close[index]) :
                symbol.ToPips(marketSeries.High[index] - marketSeries.Low[index]);
        }
    }
}