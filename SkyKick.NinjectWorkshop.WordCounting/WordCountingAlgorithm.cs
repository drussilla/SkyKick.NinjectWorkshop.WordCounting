﻿using System;

namespace SkyKick.NinjectWorkshop.WordCounting
{
    public interface IWordCountingAlgorithm
    {
        int CountWordsInString(string content);
    }

    internal class WordCountingAlgorithm : IWordCountingAlgorithm
    {
        public int CountWordsInString(string content)
        {
            return 
            content
                .Replace("\n", " ")
                .Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries)
                .Length;
        }
    }
}
