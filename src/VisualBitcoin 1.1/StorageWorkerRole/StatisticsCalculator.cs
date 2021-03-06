﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using Storage;
using Storage.Models;


namespace StorageWorkerRole
{
	class StatisticsCalculator
	{
        // Number of seconds between 3 January 2009 (first BTC ?) and 1 January 1970.
        private const ulong InitialTime = 1230940800;
        private Blob blob;
        private Queue queue;
        private List<Block> blocklist;
	    private List<int> intlist;
        private List<int> intlist2;
        private List<int> intlist3;
        private List<double> intlist4;

        public StatisticsCalculator(Queue queue, Blob blob)
        {
            this.queue = queue;
            this.blob = blob;

            if (blob.GetStatistics<List<Block>>("Last_Blocks") == null)
            {
                Block[] list = { null, null, null, null, null, null, null, null, null, null };
                blocklist = new List<Block>(list);
                blob.UploadStatistics<List<Block>>("Last_Blocks", blocklist);
            }
            if (blob.GetStatistics<Statistics>("General_Statistics") == null)
            {
                Statistics statini = new Statistics();
                blob.UploadStatistics("General_Statistics", statini);
            }
            if (blob.GetStatistics<List<int>>("Charts_Number_Of_Transactions") == null)
            {
                int[] list2 = {};
                intlist = new List<int>(list2);
                blob.UploadStatistics("Charts_Number_Of_Transactions", intlist);
            }
            if (blob.GetStatistics<List<int>>("Charts_Height_Block") == null)
            {
                int[] list3 = { };
                intlist2 = new List<int>(list3);
                blob.UploadStatistics("Charts_Height_Block", intlist2);
            }
            if (blob.GetStatistics<List<int>>("Charts_Time") == null)
            {
                int[] list4 = { };
                intlist3 = new List<int>(list4);
                blob.UploadStatistics("Charts_Time", intlist3);
            }
            if (blob.GetStatistics<List<double>>("Charts_Size_Block") == null)
            {
                double[] list5 = { };
                intlist4 = new List<double>(list5);
                blob.UploadStatistics("Charts_Size_Block", intlist4);
            }
            this.blocklist = blob.GetStatistics<List<Block>>("Last_Blocks");
        }

        public void PerformBlockStatistics()
        {
            var hash = queue.PopMessage<string>();
            var block = blob.GetBlock(hash);
            UpdateStatitistics(block);
            SortBlocks(block);
        }

		private static int CompareBlock(Block x, Block y)
		{
            if (x == null)
                return -1;

            if (y == null)   
                return 1;  
                            
            if (x.Time <= y.Time) 
                return -1;

            return 1;
		}

		private void SortBlocks(Block block)
        {
            this.blocklist = blob.GetStatistics<List<Block>>("Last_Blocks");
			blocklist.Add(block);
			blocklist.Sort(CompareBlock);
			blocklist.RemoveAt(0);
            blob.UploadStatistics<List<Block>>("Last_Blocks", blocklist);
		}

		void UpdateStatitistics(Block x)
		{
            Statistics stat = blob.GetStatistics<Statistics>("General_Statistics");
            //Charts
            List<int> chartsNbTrans = blob.GetStatistics<List<int>>("Charts_Number_Of_Transactions");
            List<double> chartsSizeBlock = blob.GetStatistics<List<double>>("Charts_Size_Block");
            List<int> chartsHeighBlock = blob.GetStatistics<List<int>>("Charts_Height_Block");
            List<int> chartsTime = blob.GetStatistics<List<int>>("Charts_Time"); //abscisse

            stat.NumberOfBlocks += 1;
            stat.NumberOfTransactions += Convert.ToUInt64(x.NumberOfTransactions);

            //Time
            stat.TotalTime = (stat.TotalTime + Convert.ToUInt64(x.Time) - InitialTime);
            stat.AverageTime = stat.TotalTime / stat.NumberOfBlocks;
            stat.VarianceTime = Variance(stat.TotalTime, stat.AverageTime, stat.NumberOfBlocks);
            stat.StandardDeviationTime = Math.Sqrt(stat.VarianceTime);

            // Statistics blocks (BTC)
            stat.SumBtc += Convert.ToUInt64(x.Amount);
            stat.AverageBtc = stat.SumBtc / stat.NumberOfBlocks;
            stat.VarianceBtc = Variance(stat.SumBtc, stat.AverageBtc, stat.NumberOfBlocks);
            stat.StandardDeviationBtc = Math.Sqrt(stat.VarianceBtc);

            // Statistics blocks (Transactions).
            stat.NumberOfTransactions += Convert.ToUInt64(x.NumberOfTransactions);
            stat.AverageTrans = stat.NumberOfTransactions / stat.NumberOfBlocks;
            stat.VarianceTrans = Variance(stat.NumberOfTransactions, stat.AverageTrans, stat.NumberOfBlocks);
            stat.StandardDevTrans = Math.Sqrt(stat.VarianceTrans);

            chartsNbTrans.Add(x.NumberOfTransactions);
            chartsSizeBlock.Add(x.Amount);
            chartsHeighBlock.Add(x.Height);
            chartsTime.Add(x.Time);

            blob.UploadStatistics("Charts_Number_Of_Transactions", chartsNbTrans);
            blob.UploadStatistics("Charts_Size_Block", chartsSizeBlock);
            blob.UploadStatistics("Charts_Height_Block", chartsHeighBlock);
            blob.UploadStatistics("Charts_Time", chartsTime);//abscisse


            blob.UploadStatistics("General_Statistics", stat);
        }

		static double Variance(ulong sum, double average, ulong nb)
		{
			return sum * (sum - 2 * average) / nb + average * average;
		}

	}
}
