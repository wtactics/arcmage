﻿using System;
using Arcmage.DAL;

namespace Arcmage.Seed
{
    class Program
    {
        static void Main(string[] args)
        {
        
            Console.WriteLine("Adding initial data to the database...");
            using (var repository = new Repository())
            {
                repository.FillPredefinedData();
            }
            Console.WriteLine("Finished!");
        }
    }
}
