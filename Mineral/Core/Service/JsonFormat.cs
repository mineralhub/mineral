using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using static Google.Protobuf.Reflection.MessageDescriptor;

namespace Mineral.Core.Service
{
    public class JsonFormat
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        //protected static void Print(IMessage message, JsonGenerator generator, bool self_type)
        //{
        //    foreach (var field in message.Descriptor.Fields.InFieldNumberOrder())
        //    {
        //        PrintField(field, generator, self_type)

        //        //    Map.Entry<FieldDescriptor, Object> field = iter.next();
        //        //    printField(field.getKey(), field.getValue(), generator, selfType);
        //        //    if (iter.hasNext())
        //        //    {
        //        //        generator.print(",");
        //        //    }
        //        //}
        //        //if (message.Descriptor...getUnknownFields().asMap().size() > 0)
        //        //{
        //        //    generator.print(", ");
        //        //}
        //        //printUnknownFields(message.getUnknownFields(), generator, selfType);
        //        }
        //}

        //private static void PrintSingleField(FieldDescriptor field, JsonGenerator generator, bool self_type)
        //{
        //    if (field.is .isExtension())
        //    {
        //        generator.Print("\"");
        //        // We special-case MessageSet elements for compatibility with proto1.
        //        if (field.ContainingType.CustomOptions.getMessageSetWireFormat()
        //            && (field.getType() == FieldDescriptor.Type.MESSAGE) && (field.isOptional())
        //            // object equality
        //            && (field.getExtensionScope() == field.getMessageType()))
        //        {
        //            generator.print(field.getMessageType().getFullName());
        //        }
        //        else
        //        {
        //            generator.print(field.getFullName());
        //        }
        //        generator.print("\"");
        //    }
        //    else
        //    {
        //        generator.print("\"");
        //        if (field.getType() == FieldDescriptor.Type.GROUP)
        //        {
        //            // Groups must be serialized with their original capitalization.
        //            generator.print(field.getMessageType().getName());
        //        }
        //        else
        //        {
        //            generator.print(field.getName());
        //        }
        //        generator.print("\"");
        //    }

        //    // Done with the name, on to the value

        //    if (field.getJavaType() == FieldDescriptor.JavaType.MESSAGE)
        //    {
        //        generator.print(": ");
        //        generator.indent();
        //    }
        //    else
        //    {
        //        generator.print(": ");
        //    }

        //    if (field.isRepeated())
        //    {
        //        // Repeated field. Print each element.
        //        generator.print("[");
        //        for (Iterator <?> iter = ((List <?>) value).iterator(); iter.hasNext(); ) {
        //            printFieldValue(field, iter.next(), generator, selfType);
        //            if (iter.hasNext())
        //            {
        //                generator.print(",");
        //            }
        //        }
        //        generator.print("]");
        //    }
        //    else
        //    {
        //        printFieldValue(field, value, generator, selfType);
        //        if (field.getJavaType() == FieldDescriptor.JavaType.MESSAGE)
        //        {
        //            generator.outdent();
        //        }
        //    }
        //}
        #endregion


        #region External Method
        public static void Print(IMessage message, StringBuilder output, bool self_type)
        {
            //JsonGenerator generator = new JsonGenerator(output);
            //generator.Print("{");
            //Print(message, generator, self_type);
            //generator.Print("}");
        }

        public static string PrintToString(IMessage message, bool self_type)
        {
            //try
            //{
            //    StringBuilder text = new StringBuilder();
            //    print(message, text, selfType);
            //    return text.toString();
            //}
            //catch (IOException e)
            //{
            //    throw new System.Exception(
            //        "Writing to a StringBuilder threw an IOException (should never happen).",
            //        e);
            //}
            return "";
        }

        public static string PrintToString(UnknownFieldSet fields, bool self_type)
        {
            //try
            //{
            //    StringBuilder text = new StringBuilder();
            //    print(fields, text, selfType);
            //    return text.toString();
            //}
            //catch (IOException e)
            //{
            //    throw new System.Exception(
            //        "Writing to a StringBuilder threw an IOException (should never happen).",
            //        e);
            //}

            return "";
        }

        public static void PrintField(FieldDescriptor field, JsonGenerator generator, bool selfType)
        {
            //PrintSingleField(field, generator, selfType);
        }
        #endregion
    }
}
