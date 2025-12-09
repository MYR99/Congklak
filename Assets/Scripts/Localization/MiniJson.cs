using System.Collections;
using System.Collections.Generic;
using System.Text;

public static class MiniJson
{
    public static object Deserialize(string json)
    {
        if (string.IsNullOrEmpty(json)) return null;
        return Parser.Parse(json);
    }

    private sealed class Parser
    {
        private string json;
        private int index;

        private Parser(string json)
        {
            this.json = json;
            index = 0;
        }

        public static object Parse(string json)
        {
            var p = new Parser(json);
            return p.ParseValue();
        }

        private char Peek()
        {
            return json[index];
        }

        private char Next()
        {
            return json[index++];
        }

        private void SkipWhitespace()
        {
            while (index < json.Length && char.IsWhiteSpace(Peek()))
                index++;
        }

        private object ParseValue()
        {
            SkipWhitespace();

            if (index >= json.Length)
                return null;

            char c = Peek();

            if (c == '"') return ParseString();
            if (c == '{') return ParseObject();
            if (c == '[') return ParseArray();

            return ParseNumberOrLiteral();
        }

        private Dictionary<string, object> ParseObject()
        {
            var dict = new Dictionary<string, object>();
            Next(); // skip {

            while (true)
            {
                SkipWhitespace();
                if (Peek() == '}')
                {
                    Next();
                    break;
                }

                string key = ParseString();

                SkipWhitespace();
                Next(); // skip :

                object value = ParseValue();
                dict[key] = value;

                SkipWhitespace();
                char c = Peek();

                if (c == ',')
                {
                    Next();
                    continue;
                }

                if (c == '}')
                {
                    Next();
                    break;
                }
            }

            return dict;
        }

        private List<object> ParseArray()
        {
            var list = new List<object>();
            Next(); // skip [

            while (true)
            {
                SkipWhitespace();
                if (Peek() == ']')
                {
                    Next();
                    break;
                }

                list.Add(ParseValue());

                SkipWhitespace();
                char c = Peek();

                if (c == ',')
                {
                    Next();
                    continue;
                }

                if (c == ']')
                {
                    Next();
                    break;
                }
            }

            return list;
        }

        private string ParseString()
        {
            var sb = new StringBuilder();
            Next(); // skip "

            while (true)
            {
                if (index >= json.Length) break;

                char c = Next();
                if (c == '"') break;

                sb.Append(c);
            }

            return sb.ToString();
        }

        private object ParseNumberOrLiteral()
        {
            int start = index;

            while (index < json.Length &&
                   !" \t\r\n,]}".Contains(Peek()))
            {
                index++;
            }

            string token = json.Substring(start, index - start);

            if (token == "true") return true;
            if (token == "false") return false;
            if (token == "null") return null;

            if (double.TryParse(token, out var num))
                return num;

            return token; // fallback
        }
    }
}