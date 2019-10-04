using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Service
{
    public class JsonGenerator
    {
        private StringBuilder output;
        private bool atStartOfLine = true;
        private StringBuilder indent = new StringBuilder();

        public JsonGenerator(StringBuilder output)
        {
            this.output = output;
        }

        /**
         * Indent text by two spaces. After calling Indent(), two spaces will be inserted at the
         * beginning of each line of text. Indent() may be called multiple times to produce deeper
         * indents.
         */
        public void Indent()
        {
            this.indent.Append("  ");
        }

        /**
         * Reduces the current indent level by two spaces, or crashes if the indent level is zero.
         */
        public void Outdent()
        {
            int length = indent.Length;
            if (length == 0)
            {
                throw new ArgumentException(" Outdent() without matching Indent().");
            }
            this.indent.Remove(length - 2, length);
        }

        /**
         * Print text to the output stream.
         */
        public void Print(string text)
        {
            int size = text.Length;
            int pos = 0;

            for (int i = 0; i < size; i++)
            {
                if (text[i] == '\n')
                {
                    Write(text.Substring(pos, size), i - pos + 1);
                    pos = i + 1;
                    atStartOfLine = true;
                }
            }
            Write(text.Substring(pos, size), size - pos);
        }

        private void Write(string data, int size)
        {
            if (size == 0)
            {
                return;
            }
            if (atStartOfLine)
            {
                atStartOfLine = false;
                output.Append(indent);
            }
            output.Append(data);
        }
    }
}
