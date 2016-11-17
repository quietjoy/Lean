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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using QuantConnect.Configuration;
using QuantConnect.Logging;

namespace QuantConnect.ToolBox.QuoteBarConverter
{
    public class Program
    {
        /// <summary>
        /// QuoteBar Converter: Convert QuantConnect Ticks into QuantConnect QuoteBars 
        /// at second, minute, hour and daily resolution
        /// </summary>
        public static void Main(string[] args)
        {
            var dataDirectory = Config.Get("data-directory", "C:/Data");
            string sourceDirectory;
            
            if (args.Length == 0)
                sourceDirectory = Config.Get("data-source-directory", "C:/ConvertedData");
            else
                sourceDirectory = Config.Get("data-source-directory", args[0]);

            var errorLogPath = @"C:/ConvertedData/error.log";

            // FXCM
            //var fxcmSourceDirectory      = Path.Combine(dataDirectory, @"forex\fxcm\tick");
            //var fxcmDestinationDirectory = sourceDirectory;
            //var fxcmTickZipFiles = Directory.GetFiles(fxcmSourceDirectory, "*.*", SearchOption.AllDirectories);
            //var topLevelFXCMTickDirectories = Directory.GetDirectories(fxcmSourceDirectory, "*.*", SearchOption.TopDirectoryOnly);

            // OANDA
            var oandaSourceDirectory = Path.Combine(dataDirectory, @"forex\oanda\tick");
            var oandaDestinationDirectory = sourceDirectory;
            var oandaTickZipFiles = Directory.GetFiles(oandaSourceDirectory, "*.*", SearchOption.AllDirectories);
            var oandaTopLevelTickDirectories = Directory.GetDirectories(oandaSourceDirectory, "*.*", SearchOption.TopDirectoryOnly);

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 5
            };

            //Log.Trace("QuoteBarConverter.Main(): Beginning to convert FXCM tick data into minute and second quotebars.");
            //Parallel.ForEach(fxcmTickZipFiles, options, file =>
            //{
            //    var fileNameData = file.Split('\\');
            //    var permtick = fileNameData[fileNameData.Length - 2];
            //    var symbol = new Symbol(SecurityIdentifier.GenerateForex(permtick, "fxcm"), permtick);
            //    var quoteBarConverter = new QuoteBarMinuteSecondConverter(file, fxcmDestinationDirectory, symbol);
            //    quoteBarConverter.Convert();
            //});
            //Log.Trace("QuoteBarConverter.Main(): Done converting FXCM minute and second resolution data.");


            //Log.Trace("QuoteBarConverter.Main(): Beginning to create FXCM hour and daily resolution quotebars.");
            //Parallel.ForEach(topLevelFXCMTickDirectories, options, directory =>
            //{
            //    var fileNameData = directory.Split('\\');
            //    var permtick = fileNameData[fileNameData.Length - 1];
            //    var symbol = new Symbol(SecurityIdentifier.GenerateForex(permtick, "fxcm"), permtick);
            //    var quoteBarConverter = new QuoteBarHourDailyConverter(fxcmDestinationDirectory, symbol);
            //    quoteBarConverter.Convert();
            //});
            //Log.Trace("QuoteBarConverter.Main(): Done converting FXCM minute data to hour and daily resolution data.");


            Log.Trace("QuoteBarConverter.Main(): Beginning to convert OANDA tick data into minute and second quotebars.");
            Parallel.ForEach(oandaTickZipFiles, options, file =>
            {
                var fileNameData = file.Split('\\');
                var permtick = fileNameData[fileNameData.Length - 2];
                var symbol = new Symbol(SecurityIdentifier.GenerateForex(permtick, "oanda"), permtick);

                try
                {
                    var quoteBarConverter = new QuoteBarMinuteSecondConverter(file, oandaDestinationDirectory, symbol);
                    quoteBarConverter.Convert();
                }
                catch (Exception e)
                {
                    File.AppendAllLines(errorLogPath, new string[]
                    {
                        "#############",
                        DateTime.Now.ToString("u"),
                        e.Message.ToString(),
                        e.GetBaseException().ToString(),
                        "min/sec resolution",
                        symbol
                    } );
                }
            });
            Log.Trace("QuoteBarConverter.Main(): Done converting OANDA minute and second resolution data.");


            Log.Trace("QuoteBarConverter.Main(): Beginning to create OANDA hour and daily resolution quotebars.");
            Parallel.ForEach(oandaTopLevelTickDirectories, options, directory =>
            {
                var fileNameData = directory.Split('\\');
                var permtick = fileNameData[fileNameData.Length - 1];
                var symbol = new Symbol(SecurityIdentifier.GenerateForex(permtick, "oanda"), permtick);
                try
                {
                    var quoteBarConverter = new QuoteBarHourDailyConverter(oandaDestinationDirectory, symbol);
                    quoteBarConverter.Convert();
                }
                catch (Exception e)
                {
                    File.AppendAllLines(errorLogPath, new string[]
                    {
                        "#############",
                        DateTime.Now.ToString("u"),
                        e.Message.ToString(),
                        e.GetBaseException().ToString(),
                        "daily/hour resolution",
                        symbol
                    });
                }
            });
            Log.Trace("QuoteBarConverter.Main(): Done converting OANDA minute data to hour and daily resolution data.");


            Log.Trace("QuoteBarConverter.Main(): Done converting tick data. Exiting.");

            Console.ReadLine();
        }
    }
}
