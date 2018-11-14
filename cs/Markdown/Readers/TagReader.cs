﻿using System.Collections.Generic;
using System.Linq;
using Markdown.Tags;
using Markdown.Tokens;

namespace Markdown.Readers
{
    class TagReader : IReader
    {
        private IEnumerable<IReader> readers;
        private IEnumerable<TagReader> skippedReaders;
        private string mdTag;
        private string htmlTag;

        public TagReader(string mdTag, string htmlTag,
            IEnumerable<IReader> readers,
            IEnumerable<TagReader> skippedReaders)
        {
            this.mdTag = mdTag;
            this.readers = readers;
            this.htmlTag = htmlTag;
            this.skippedReaders = skippedReaders;
        }

        private bool IsOpenTag(string text, int position)
        {
            return position < text.Length - mdTag.Length - 1 &&
                   CanReadTag(text, position) && 
                   IsLetterOrSlash(text[position + mdTag.Length]);
        }

        private bool IsLetterOrSlash(char symbol)
        {
            return char.IsLetter(symbol) || symbol == '\\';
        }

        private bool CanReadTag(string text, int position)
        {
            return position <= text.Length - mdTag.Length &&
                   text.Substring(position, mdTag.Length) == mdTag;
        }

        private bool IsClosedTag(string text, int position)
        {
            return CanReadTag(text, position) && 
                   !char.IsWhiteSpace(text[position - 1]);
        }

        private IToken GetToken(string text, int index)
        {
            return readers.Select(reader => reader.ReadToken(text, index))
                .FirstOrDefault(token => token != null);
        }

        public IToken ReadToken(string text, int position)
        {
            if (!IsOpenTag(text, position))
                return null;
            var tokens = new List<IToken>();

            for (int i = position + mdTag.Length; i <= text.Length - mdTag.Length; i++)
            {
                IToken token = GetSkippedToken(text, i);
                if (string.IsNullOrEmpty(token.Text))
                {

                    if (IsClosedTag(text, i))
                    {
                        var rightPosition = tokens.Select(t => t.Position).Max();
                        return new Tag(text.Substring(position, i - position + mdTag.Length), htmlTag, tokens, rightPosition + mdTag.Length);
                    }

                    token = GetToken(text, i);
                }
                if (token.Text.Any(char.IsDigit)) break;
                tokens.Add(token);
                i = token.Position;
            }

            return null;
        }

        private IToken GetSkippedToken(string text, int i)
        {
            var maxTokenTagLength = skippedReaders.Where(reader => reader.CanReadTag(text, i))
                .Select(reader => reader.mdTag.Length).Concat(new []{0}).Max();
            return new TextToken(text.Substring(i, maxTokenTagLength), i + maxTokenTagLength - 1);
        }
    }
}