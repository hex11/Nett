﻿using System.Collections.Generic;
using System.Linq;
using Nett.Util;

namespace Nett.Writer
{
    internal sealed partial class ParseInfoTomlTableWriter
    {
        private sealed class ParseInfoTomlInlineTableWriter : ParseInfoTomlWriter
        {
            public ParseInfoTomlInlineTableWriter(FormattingStreamWriter writer, TomlSettings settings)
                : base(writer, settings)
            {
            }

            public void WriteInlineTable(TomlKey key, TomlTable table, int level)
            {
                this.WritePrependComments(table, level);

                this.writer.Write(key.ToString());
                this.writer.Write(" = ");
                this.WriteInlineTableBody(table);

                this.WriteAppendComments(table);
            }

            public void WriteTomlTableArray(TomlKey key, TomlTableArray tableArray, int level)
            {
                this.WritePrependComments(tableArray, level);

                const string assignment = " = [ ";
                this.writer.Write(key.ToString());
                this.writer.Write(assignment);

                int indentLen = key.ToString().Length + assignment.Length;
                string indent = new string(' ', indentLen);

                for (int i = 0; i < tableArray.Items.Count; i++)
                {
                    this.WriteInlineTableBody(tableArray.Items[i]);

                    if (i < tableArray.Items.Count - 1)
                    {
                        this.writer.Write(",");
                        this.writer.WriteLine();
                        this.writer.Write(indent);
                    }
                }

                this.writer.WriteLine(" ]");
            }

            private void WriteInlineTableBody(TomlTable table)
            {
                this.writer.Write("{ ");
                var rows = table.InternalRows.ToArray();

                for (int i = 0; i < rows.Length - 1; i++)
                {
                    this.WriteTableRow(rows[i]);
                    this.writer.Write(", ");
                }

                if (rows.Length > 0)
                {
                    this.WriteTableRow(rows[rows.Length - 1]);
                }

                this.writer.Write(" }");
            }

            private void WriteTableRow(KeyValuePair<TomlKey, TomlObject> r)
            {
                this.WriteKey(r.Key);

                if (r.Value.TomlType == TomlObjectType.Array)
                {
                    this.WriteArray((TomlArray)r.Value);
                }
                else if (r.Value.TomlType == TomlObjectType.Table)
                {
                    this.WriteInlineTable(r.Key, (TomlTable)r.Value, level: 0);
                }
                else if (r.Value.TomlType == TomlObjectType.ArrayOfTables)
                {
                    this.WriteTomlTableArray(r.Key, (TomlTableArray)r.Value, level: 0);
                }
                else
                {
                    this.WriteValue((TomlValue)r.Value);
                }
            }

            private void WriteKey(TomlKey key)
            {

            }
        }
    }
}
