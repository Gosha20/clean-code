﻿using System;
using System.Collections.Generic;
using Markdown.Readers;

namespace Markdown
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = "___abc___";

            var readers = ReaderCreator.Create();
            var md = new Md(readers);
            Console.WriteLine(md.Render(input));
        }
    }
}
