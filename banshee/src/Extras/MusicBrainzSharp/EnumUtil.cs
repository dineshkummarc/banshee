using System;
using System.Collections.Generic;
using System.Reflection;

namespace MusicBrainzSharp
{
    // C# doesn't support string enums? Riiickeeeee!
    internal static class EnumUtil
    {
        static Dictionary<string, string> string_values = new Dictionary<string, string>();
        public static string EnumToString(Enum enumeration)
        {
            Type type = enumeration.GetType();

            // If we've cached the value, return it!
            if(string_values.ContainsKey(Enum.GetName(type, enumeration)))
                return string_values[Enum.GetName(type, enumeration)];

            string output = null;

            // See if the enum has a super-special StringValueAttribute
            FieldInfo field_info = type.GetField(enumeration.ToString());
            StringValueAttribute[] attrs =
                field_info.GetCustomAttributes(typeof(StringValueAttribute),
                false) as StringValueAttribute[];
            if(attrs.Length > 0)
                output = attrs[0].Value;
            
            // If it doesn't, determin the string based on the enum's name
            else {
                string enum_name = Enum.GetName(type, enumeration);
                for(int i = 1; i < enum_name.Length; i++)
                    if(enum_name[i] >= 'A' && enum_name[i] <= 'Z')
                        enum_name = enum_name.Insert(i++, "-");

                // *IncType enums must be made lower-case
                output = type.Name.EndsWith("IncType")
                    ? enum_name.ToLower()
                    : enum_name;
            }
            
            // Cache the result and return FTW!
            string_values.Add(Enum.GetName(type, enumeration), output);
            return output;
        }
    }

    internal class StringValueAttribute : Attribute
    {
        string value;
        public StringValueAttribute(string value)
        {
            this.value = value;
        }
        public string Value { get { return value; } }
    }
}
