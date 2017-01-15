﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using NUnit.Framework;
using QuantConnect.Data;
using QuantConnect.Data.Market;

namespace QuantConnect.Tests.Common.Data.Market
{
    [TestFixture]
    public class QuoteBarTests
    {
        [Test]
        public void DoesntGenerateCorruptedPricesIfBidOrAskAreMissing()
        {
            var bar = new QuoteBar();
            bar.UpdateAsk(10, 15);
            Assert.AreEqual(10, bar.Open);
            Assert.AreEqual(10, bar.High);
            Assert.AreEqual(10, bar.Low);
            Assert.AreEqual(10, bar.Close);

            bar = new QuoteBar();
            bar.Ask = new Bar(11,11,11,11);
            Assert.AreEqual(11, bar.Open);
            Assert.AreEqual(11, bar.High);
            Assert.AreEqual(11, bar.Low);
            Assert.AreEqual(11, bar.Close);
        }

        [Test]
        public void QuoteBarReader_CanParseTradeBarFormattedData_Successfully()
        {
            var config = new SubscriptionDataConfig(typeof(TradeBar), Symbols.SPY, Resolution.Second, TimeZones.NewYork, TimeZones.NewYork, false, false, false);
            var line = "14280000,1.10905,1.10909,1.10904,1.10908";
            var date = DateTime.MaxValue;
            var isLiveMode = false;

            var quoteBar = new QuoteBar();
            var parsedQuoteBar = (QuoteBar) quoteBar.Reader(config, line, date, isLiveMode);

            Assert.AreEqual(parsedQuoteBar.Symbol, Symbols.SPY);

            Assert.AreEqual(parsedQuoteBar.Ask.Open, 1.10905);
            Assert.AreEqual(parsedQuoteBar.Ask.High, 1.10909);
            Assert.AreEqual(parsedQuoteBar.Ask.Low, 1.10904);
            Assert.AreEqual(parsedQuoteBar.Ask.Close, 1.10908);

            Assert.AreEqual(parsedQuoteBar.Bid.Open, 1.10905);
            Assert.AreEqual(parsedQuoteBar.Bid.High, 1.10909);
            Assert.AreEqual(parsedQuoteBar.Bid.Low, 1.10904);
            Assert.AreEqual(parsedQuoteBar.Bid.Close, 1.10908);
        }

        [Test]
        public void QuoteBarReader_CanParseMalformattedData_Successfully()
        {
            var config = new SubscriptionDataConfig(typeof(TradeBar), Symbols.SPY, Resolution.Second, TimeZones.NewYork, TimeZones.NewYork, false, false, false);
            // Neither a quote or a trade
            var line = "14340000,1.10907,1.109075,1.108985,1.1090214400000,1.109005,1.109005,1.10884,1.10887";
            var date = DateTime.MaxValue;
            var isLiveMode = false;

            var quoteBar = new QuoteBar();
            var parsedQuoteBar = (QuoteBar)quoteBar.Reader(config, line, date, isLiveMode);

            Assert.AreEqual(parsedQuoteBar.Symbol, Symbols.SPY);

            Assert.AreEqual(parsedQuoteBar.Ask.Open, 0);
            Assert.AreEqual(parsedQuoteBar.Ask.High, 0);
            Assert.AreEqual(parsedQuoteBar.Ask.Low, 0);
            Assert.AreEqual(parsedQuoteBar.Ask.Close, 0);

            Assert.AreEqual(parsedQuoteBar.Bid.Open, 0);
            Assert.AreEqual(parsedQuoteBar.Bid.High, 0);
            Assert.AreEqual(parsedQuoteBar.Bid.Low, 0);
            Assert.AreEqual(parsedQuoteBar.Bid.Close, 0);
        }

        [Test]
        public void QuoteBarReader_CanParseQuoteBar_Successfully()
        {
            var config = new SubscriptionDataConfig(typeof(TradeBar), Symbols.SPY, Resolution.Second, TimeZones.NewYork, TimeZones.NewYork, false, false, false);
            // Neither a quote or a trade
            var line = "14340000,11090,11090,11089,11090,100,11090,11088,11088,11090,10000";
            var date = DateTime.MaxValue;
            var isLiveMode = false;

            var quoteBar = new QuoteBar();
            var parsedQuoteBar = (QuoteBar)quoteBar.Reader(config, line, date, isLiveMode);

            Assert.AreEqual(parsedQuoteBar.Symbol, Symbols.SPY);

            Assert.AreEqual(parsedQuoteBar.Bid.Open, 1.1090);
            Assert.AreEqual(parsedQuoteBar.Bid.High, 1.1090);
            Assert.AreEqual(parsedQuoteBar.Bid.Low, 1.1089);
            Assert.AreEqual(parsedQuoteBar.Bid.Close, 1.1090);

            Assert.AreEqual(parsedQuoteBar.Ask.Open, 1.10900);
            Assert.AreEqual(parsedQuoteBar.Ask.High, 1.1088);
            Assert.AreEqual(parsedQuoteBar.Ask.Low, 1.1088);
            Assert.AreEqual(parsedQuoteBar.Ask.Close, 1.1090);
        }
    }
}
