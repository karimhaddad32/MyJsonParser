﻿
namespace MyJsonParser
{
    internal class JsonParser
    {
        internal static bool TryParse(string input, out object? jsonObject)
        {
            jsonObject = null;

            if (string.IsNullOrWhiteSpace(input)) return false;

            try
            {
                (jsonObject, int position) = JsonParser.InternalParse(input, 0, 0);

                if (position < input.Length) {
                    throw new InvalidOperationException();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        private static (object? JsonObject, int) InternalParse(string input, int position, int deepness)
        {
            int i = position;

            while (i < input.Length)
            {
                var indexCharacter = input[i];

                switch (indexCharacter)
                {
                    case ' ' or '\n':
                        i++;
                        continue;
                    case '{':
                        var (result, inc) = ParseObject(input, i);
                        return (result, i - position + inc);
                    case '[':
                        var (arrResult, arrInc) = ParseArray(input, i, deepness);
                        return (arrResult, i - position + arrInc);
                    default:
                        throw new InvalidOperationException();

                }
            }

            throw new InvalidOperationException($"unknown character {input[i]}");
        }

        private static (Dictionary<string, object?>, int) ParseObject(string input, int position)
        {
            var result = new Dictionary<string, object?>();

            int i = position + 1;
            var commaFoundWithoutClosing = false;
            while(i < input.Length)
            {
                var indexCharacter = input[i];

                if(indexCharacter is ' ' or '\n') {
                    i++;
                    continue;
                }

                if(indexCharacter is ',')
                {
                    commaFoundWithoutClosing = true;
                    i++;
                    continue;
                }

                if (indexCharacter is '}' && !commaFoundWithoutClosing)
                {
                    i++;
                    i += CleanRestOfSpaces(input, i);
                    break;
                }


                if (indexCharacter is ']')
                {
                    i++;
                    i += CleanRestOfSpaces(input, i);
                    break;
                }

                if (indexCharacter is '}' && commaFoundWithoutClosing)
                {
                    throw new InvalidOperationException("syntax error. Comma found in a wrong position"); 
                }

                if (indexCharacter is '"')
                {
                    commaFoundWithoutClosing = false;
                    var (keyVal, val, inc) = ParseObjectProperty(input, i);
                    i += inc;
                    result[keyVal] = val;
                    continue;
                }

                throw new InvalidOperationException($"Character {input[i]}");
            }

            if (commaFoundWithoutClosing)
            {
                throw new InvalidOperationException("syntax error. Comma found in a wrong position");
            }

            return (result, i - position);
        }

        private static int CleanRestOfSpaces(string input, int position)
        {
            int i = position;
            while(i < input.Length)
            {
                if (input[i] is ' ' or '\n')
                {
                    i++;
                }

                break;
            }

            return i - position;
        }

        private static (string key, object? objectVal, int increment) ParseObjectProperty(string input, int position)
        {
            int i = position;
  
            var (key, keyInc) = ParseString(input, i);

            i += keyInc;

            var indexCharacter = input[i];

            while (indexCharacter != ':')
            {
                i++;
                indexCharacter = input[i];
            }

            i++;

            var (value, valueInc) = ParseValue(input, i, 0);
            i += valueInc;

            return (key, value, i - position);
            
        }

        private static (string keyVal, int increment) ParseString(string input, int position)
        {
            int i = position + 1;
            string keyVal = "";

            while (i < input.Length)
            {
                var indexCharacter = input[i];

                if (indexCharacter is '"')
                {
                    i++;
                    break;
                }

                if (indexCharacter is '\t' or '\n')
                {
                    throw new InvalidOperationException("Invalid Token");
                }

                if(indexCharacter is '\"')
                {
                    keyVal += "\"";
                    i++;
                    continue;
                }

                if (indexCharacter is '\\')
                {

                    int increment = 2;
                    var peek = input[i + 1];

                    keyVal += peek switch
                    {
                        '"' or 'b' or 't' or '\\' or 'f' or 'n' or 'r' or '/' => $"{indexCharacter}{peek}",
                        'u' => $"{indexCharacter}{peek}{GetHex(input.Substring(i + 2, 4), out increment)}",
                        _ => throw new InvalidOperationException("Invalid Escape Character Usage")
                    };
                  
                    Console.WriteLine(keyVal.Length);

                    i += increment;
                    continue;
                }

                keyVal += input[i++];
            }

            while(i < input.Length)
            {
                if (input[i] is ' ' or '\n')
                {
                    i++;
                    continue;
                }

                if (input[i] is '}' or ']' or ',' or ':')
                {
                    break;
                }

                i++;
            }

            return (keyVal, i - position);
        }

        private static string GetHex(string hex, out int increment)
        {
            for(int i = 0; i< hex.Length; i++)
            {
                if (IsDigit(hex[i]) || IsSmallLetterHex(hex[i]) || IsCapitalLetterHex(hex[i]))
                {
                    increment = 6;
                    return hex;
                }
            }

            throw new InvalidOperationException("Invalid Hex with escape Character!");
        }

        private static bool IsCapitalLetterHex(char c)
        {
            return (c >= 'A' && c <= 'F');
        }  
        
        private static bool IsSmallLetterHex(char c)
        {
            return (c >= 'a' && c <= 'f');
        }

        private static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private static (object?[], int) ParseArray(string input, int position, int initialDeepness)
        {
            int i = position + 1;
            int deepness = 1 + initialDeepness;

            if(deepness >= 20)
            {
                throw new InvalidOperationException("Array Too Deep!");
            }

            var list = new List<object?>();

            bool arrayClosed = false;
            var commaFoundWithoutClosing = false;

            while (i < input.Length)
            {
                var indexedCharacter = input[i];

                if (indexedCharacter is ' ' or '\n')
                {
                    i++;
                    continue;
                }

                if (indexedCharacter is ']')
                {
                    arrayClosed = true;
                    i++;
                    i += CleanRestOfSpaces(input, i);

                    break;
                }

                if (indexedCharacter is ',' && list.Count > 0)
                {
                    commaFoundWithoutClosing = true;
                    i++;
                    continue;
                }

                if (indexedCharacter is ',' && list.Count == 0)
                {
                    throw new InvalidOperationException("Invalid Array");
                }

                commaFoundWithoutClosing = false;

                var (valueObj, inc) = ParseValue(input, i, deepness);
                list.Add(valueObj);
                i += inc; 
            }

            if (!arrayClosed || commaFoundWithoutClosing)
            {
                throw new InvalidOperationException("Invalid Array");
            }

            return (list.ToArray(), i - position);
        }

        private static (object?, int) ParseValue(string input, int position, int deepness)
        {
            int i = position;

            while(i < input.Length)
            {
                var indexCharacter = input[i];

                if(indexCharacter is ',')
                {
                    throw new InvalidOperationException("Invalid Array");
                }

                if (indexCharacter is ' ' or '\n' or '\t') {
                    i++;
                    continue;
                }

                if ((indexCharacter is '{' or '['))
                {
                    var (valResult, valInc) = InternalParse(input, i, deepness);
                    return (valResult, i - position + valInc);
                }

                if (indexCharacter == '"')
                {
                    var (str, inc) = ParseString(input, i);
                    return (str, i - position + inc);
                }

                if (IsDigitsOrSigns(indexCharacter))
                {
                    var(num, inc) = ParseNumber(input, i);
                    return (num, i - position + inc);
                }

                if(indexCharacter == 'n') {

                    TryParseExactValue(input, i, "null");

                    return (null, i - position + 4);
                }

                if (indexCharacter == 't')
                {
                    TryParseExactValue(input, i, "true");

                    return (true, i - position + 4);
                } 
                
                if (indexCharacter == 'f')
                {
                    TryParseExactValue(input, i, "false");

                    return (false, i - position + 5);
                }

                throw new InvalidOperationException($"unknown character {indexCharacter}");
            }

            throw new InvalidOperationException($"unknown character {input[i]}");
        }

       

        private static (object? nullVal, int inc) TryParseExactValue(string input, int position, string exactValue)
        {
            int i = position;
            var value = "";
            while(i < input.Length)
            {
                if(input[i] is ',' or '}' or ']')
                {
                    break;
                }

                value += input[i++];
            }

            if (value.Trim() != exactValue)
            {
                throw new InvalidOperationException("Invalid Token");
            }

            return (null, i - position);
        }

        private static bool IsNumerical(char indexCharacter)
        {
            return indexCharacter is '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or '0' or 'e' or 'E' or '-' or '.' or '+';
        }

        private static bool IsDigitsOrSigns(char indexCharacter)
        {
            return indexCharacter is '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or '0' or '-' or '+';
        }

        private static (object?, int) ParseNumber(string input, int position)
        {
            int i = position;
            string number = "";

            while (i < input.Length && IsNumerical(input[i]))
            {
                number += input[i++];
            }

            if (number.Trim().Length > 1 &&  number.Trim().StartsWith("0") && !number.Trim().StartsWith("0.")) {
                throw new InvalidOperationException("Invalid Number");
            }

            if (number.Contains('.') || number.Contains('E') || number.Contains('e'))
            {
                return (double.Parse(number), i - position);
            }
            return (int.Parse(number), i - position);
        }

        internal static Dictionary<string, object?>? ToDictionary(object? jsonObject)
        {
            return jsonObject as Dictionary<string, object?>;
        }

        internal static T ToClass<T>(object? jsonObject) where T : new()
        {
            var dict = ToDictionary(jsonObject);

            if (dict == null) return new();

            T obj = new();

            foreach (var property in typeof(T).GetProperties())
            {
                if (dict.ContainsKey(property.Name))
                {
                    // Check if the property type matches the value type in the dictionary
                    property.SetValue(obj, Convert.ChangeType(dict[property.Name], property.PropertyType));
                }
            }
            return obj;
        }

        internal static T[]? ToClassArray<T>(object? jsonObject) where T : new()
        {
            var array = ToJsonArray(jsonObject);

            if (array == null) return default;

            List<T?> result = [];

            foreach(var item in array)
            {
                result.Add(ToClass<T>(item));
            }
      
            return [.. result];
        }

        internal static object?[]? ToJsonArray(object? jsonArray)
        {
            return jsonArray as object?[];
        }
    }
}