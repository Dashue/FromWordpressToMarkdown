﻿/**
* Ported from: html2markdown by
* @author Himanshu Gilani
* @author Kates Gasis (original author)
*
*/

using HtmlParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HtmlConverters
{
    public partial class HtmlToMarkdownConverter : BaseParser, IHtmlConverter
    {
        Stack<string> listTagStack = new Stack<string>();
        private Stack<Dictionary<string, HtmlAttribute>> linkAttrStack = new Stack<Dictionary<string, HtmlAttribute>>();
        Stack<string> blockquoteStack = new Stack<string>();
        Stack<bool> preStack = new Stack<bool>();
        Stack<string> codeStack = new Stack<string>();
        internal Stack<string> nodeStack = new Stack<string>();
        List<string> links = new List<string>();


        private bool inlineStyle;


        public HtmlToMarkdownConverter(bool inlineStyle = false)
        {
            this.inlineStyle = inlineStyle;
        }

        public string sliceText(string start)
        {
            var text = new LinkedList<string>();
            while (nodeStack.Count > 0 && (nodeStack.Peek() != start))
            {
                var t = nodeStack.Pop();
                text.AddFirst(t);
            }

            return string.Join("", text);
        }

        public bool removeIfEmptyTag(string start)
        {
            var cleaned = false;
            if (start == HtmlToMarkdownConverterHelper.peekTillNotEmpty(nodeStack.ToList()))
            {
                while (nodeStack.Peek() != start)
                {
                    nodeStack.Pop();
                }
                nodeStack.Pop();
                cleaned = true;
            }
            return cleaned;
        }

        private void block(bool isEndBlock)
        {
            if (nodeStack.Count == 0)
            {
                return;
            }
            var lastItem = nodeStack.Pop();
            if (string.IsNullOrWhiteSpace(lastItem))
            {
                return;
            }

            if (!isEndBlock)
            {
                string block;
                if (Regex.IsMatch(lastItem, @"\s*\n\n\s*$"))
                {
                    lastItem = Regex.Replace(lastItem, @"\s*\n\n\s*$", "\n\n");
                    block = "";
                }
                else if (Regex.IsMatch(@"\s*\n\s*$", lastItem))
                {
                    lastItem = Regex.Replace(lastItem, @"\s*\n\s*$", "\n");
                    block = "\n";
                }
                else if (Regex.IsMatch(@"\s+$", lastItem))
                {
                    block = "\n\n";
                }
                else
                {
                    block = "\n\n";
                }

                nodeStack.Push(lastItem);
                nodeStack.Push(block);
            }
            else
            {
                nodeStack.Push(lastItem);
                if (!HtmlToMarkdownConverterHelper.endsWith(lastItem, "\n"))
                {
                    nodeStack.Push("\n\n");
                }
            }
        }

        public void listBlock()
        {
            if (nodeStack.Count > 0)
            {
                var li = nodeStack.Peek();

                if (false == li.EndsWith("\n"))
                {
                    nodeStack.Push("\n");
                }
            }
            else
            {
                nodeStack.Push("\n");
            }
        }

        public string getListMarkdownTag()
        {
            var listItem = "";

            for (var i = 0; i < listTagStack.Count - 1; i++)
            {
                listItem += "  ";
            }

            if (listTagStack.Count > 0)
            {
                listItem += listTagStack.Peek();
            }

            return listItem;
        }

        public string Convert(string html)
        {

            Parse(html);

            if (!inlineStyle)
            {
                for (var i = 0; i < links.Count; i++)
                {
                    if (i == 0)
                    {
                        var lastItem = nodeStack.Pop();
                        nodeStack.Push(Regex.Replace(lastItem, @"\s+$/g", ""));
                        nodeStack.Push("\n\n[" + i + "]: " + links[i]);
                    }
                    else
                    {
                        nodeStack.Push("\n[" + i + "]: " + links[i]);
                    }
                }
            }

            var stringBulder = new StringBuilder();
            Console.WriteLine(nodeStack.Count);
            var array = nodeStack.ToArray();

            Array.Reverse(array);
            return string.Join("", array);

        }

        protected override void comment(string text)
        {
            throw new NotImplementedException();
        }

        protected override void completed(List<string> htmlStack)
        {
            throw new NotImplementedException();
        }

        protected override List<string> ExcludedTags
        {
            get { return Markdown.ExcludedTags; }
        }
    }
}
