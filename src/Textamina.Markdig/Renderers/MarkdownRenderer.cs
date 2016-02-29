﻿using System;
using System.Collections.Generic;
using Textamina.Markdig.Syntax;
using Textamina.Markdig.Syntax.Inlines;

namespace Textamina.Markdig.Renderers
{
    public abstract class MarkdownRenderer
    {
        private readonly Dictionary<Type, MarkdownObjectRenderer> renderersPerType;

        protected MarkdownRenderer()
        {
            Renderers = new List<MarkdownObjectRenderer>();
            renderersPerType = new Dictionary<Type, MarkdownObjectRenderer>();
        }

        public List<MarkdownObjectRenderer> Renderers { get; }

        public void WriteChildren(ContainerBlock containerBlock)
        {
            if (containerBlock == null)
            {
                return;
            }

            foreach (var block in containerBlock.Children)
            {
                Write(block);
            }
        }

        public void WriteChildren(ContainerInline containerInline)
        {
            if (containerInline == null)
            {
                return;
            }

            var inline = containerInline.FirstChild;
            while (inline != null)
            {
                Write(inline);
                inline = inline.NextSibling;
            }
        }

        public void Write<T>(T obj) where T : MarkdownObject
        {
            if (obj == null)
            {
                return;
            }

            MarkdownObjectRenderer renderer;
            if (!renderersPerType.TryGetValue(obj.GetType(), out renderer))
            {
                foreach (var testRenderer in Renderers)
                {
                    if (testRenderer.Accept(this, obj.GetType()))
                    {
                        renderersPerType[obj.GetType()] = renderer = testRenderer;
                        break;
                    }
                }
            }
            if (renderer != null)
            {
                renderer.Write(this, obj);
            }
            else
            {
                var containerBlock = obj as ContainerBlock;
                if (containerBlock != null)
                {
                    WriteChildren(containerBlock);
                }
                else
                {
                    var containerInline = obj as ContainerInline;
                    if (containerInline != null)
                    {
                        WriteChildren(containerInline);
                    }
                }
            }
        }
    }
}