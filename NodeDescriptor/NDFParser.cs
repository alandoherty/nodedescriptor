using System;
using System.Collections.Generic;

namespace NodeDescriptor
{
    class NDFParser
    {
        #region Fields
        private static List<NDFTokenType> valueTokens = null;

        private NDFToken[] input = null;
        private List<NDFNode> nodes = new List<NDFNode>();
        private NDFNode[] nodesArr = null;
        private string source = "[Unknown]";

        private int pos = 0;
        private NDFToken c = null;
        private NDFToken last = null;
        #endregion

        #region Properties        
        /// <summary>
        /// Gets the input.
        /// </summary>
        /// <value>The input.</value>
        public NDFToken[] Input {
            get {
                return input;
            }
        }

        /// <summary>
        /// Gets the output.
        /// </summary>
        /// <value>The output.</value>
        public NDFNode[] Output {
            get {
                return nodesArr;
            }
        }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>The source.</value>
        public string Source {
            get {
                return source;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Peek the next token.
        /// </summary>
        /// <returns></returns>
        private NDFToken Peek() {
            // check bounds
            if (pos == input.Length)
                return null;

            return input[pos];
        }

        /// <summary>
        /// Gets the next token and shifts the position by one.
        /// </summary>
        /// <returns></returns>
        private void Next() {
            // check bounds
            if (pos == input.Length) {
                c = null;
                return;
            }

            c = input[pos++];
            last = c;
        }

        /// <summary>
        /// Expects the current token to be of a specific type.
        /// </summary>
        /// <param name="type">The type.</param>
        private void Expect(NDFTokenType type) {
            if (c == null)
                Error("Expected " + type.ToString().ToLower() + " but reached end of stream");

            if (c.Type != type)
                Error(c, type);
        }

        /// <summary>
        /// Expects the next token to be of a specific type.
        /// </summary>
        /// <param name="type">The type.</param>
        private void ExpectNext(NDFTokenType type) {
            Next();
            Expect(type);
        }

        /// <summary>
        /// Expects the peeked token to be of a specific type.
        /// </summary>
        /// <param name="type">The type.</param>
        private void ExpectPeek(NDFTokenType type) {
            NDFToken t = Peek();

            if (t == null)
                Error("Expected " + type.ToString().ToLower() + " but reached end of stream");

            if (t.Type != type)
                Error(t, type);
        }

        /// <summary>
        /// Accepts the current token to be of a specific type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private bool Accept(NDFTokenType type) {
            if (c == null)
                return false;

            if (c.Type == type)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Accepts the next token to be of a specific type shifting forward if found.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private bool AcceptNext(NDFTokenType type) {
            NDFToken n = Peek();

            if (n == null)
                return false;

            if (n.Type == type) {
                Next();
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Accepts the next token to be of a specific type without shifting forward.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private bool AcceptPeek(NDFTokenType type) {
            NDFToken n = Peek();

            if (n == null)
                return false;

            if (n.Type == type) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Parses the input.
        /// </summary>
        public void Parse() {
            // initial values
            pos = 0;

            // process
            Next();

            while (c != null) {
                // start
                ParseStart();

                // next
                Next();
            }

            // output
            nodesArr = nodes.ToArray();
        }

        /// <summary>
        /// The initial state to discover what token to parse next.
        /// </summary>
        /// <exception cref="Exception">Unexpected symbol ' + c + ' on line  + line + : + character</exception>
        private void ParseStart() {
            // expect initial token
            Expect(NDFTokenType.Identifier);

            if (c.Value == "object") {
                nodes.Add(ParseObject());
                ExpectNext(NDFTokenType.Semicolon);
            } else if (c.Value == "val") {
                nodes.Add(ParseKeyValue());
                ExpectNext(NDFTokenType.Semicolon);
            } else {
                Error();
            }
        }

        /// <summary>
        /// Parses an object.
        /// [identifier] ((colon) (identifier)) 
        /// [braceopen] ([identifier] [identifier] [equals] {value} [semicolon]) [braceclose]
        /// </summary>
        /// <returns></returns>
        private NDFObjectNode ParseObject() {
            // create node
            NDFObjectNode node = new NDFObjectNode();

            // object name
            ExpectNext(NDFTokenType.Identifier);
            node.Key = c.Value;

            // colon
            ExpectNext(NDFTokenType.Colon);

            // object type
            ExpectNext(NDFTokenType.Identifier);
            node.Type = c.Value;

            // object table
            ExpectPeek(NDFTokenType.BraceOpen);
            Dictionary<string, NDFKeyValueNode> table = new Dictionary<string, NDFKeyValueNode>();

            // open brace
            ExpectNext(NDFTokenType.BraceOpen);

            while (AcceptNext(NDFTokenType.Identifier)) {
                if (c.Value == "val") {
                    // value
                    NDFKeyValueNode valNode = ParseKeyValue();

                    // semicolon
                    ExpectNext(NDFTokenType.Semicolon);

                    table.Add(valNode.Key, valNode);
                } else {
                    Error();
                }
            }

            // close brace
            ExpectNext(NDFTokenType.BraceClose);
            node.Table = table;

            return node;
        }

        /// <summary>
        /// Parses a table.
        /// [braceopen] ([identifier/string] [equals] {value} (comma)) [braceclose]
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, NDFKeyValueNode> ParseTable() {
            // table
            Dictionary<string, NDFKeyValueNode> table = new Dictionary<string, NDFKeyValueNode>();

            // open brace
            ExpectNext(NDFTokenType.BraceOpen);

            while (AcceptPeek(NDFTokenType.Identifier) || AcceptPeek(NDFTokenType.String)) {
                // keyvalue
                NDFKeyValueNode node = ParseKeyValue();

                table.Add(node.Key, node);

                // comma
                if (!AcceptNext(NDFTokenType.Comma))
                    break;
            }

            // close brace
            ExpectNext(NDFTokenType.BraceClose);

            return table;
        }

        /// <summary>
        /// Parses a key value.
        /// [identifier/string] [equals] {value}
        /// </summary>
        /// <returns></returns>
        private NDFKeyValueNode ParseKeyValue() {
            // key/value
            NDFKeyValueNode node = new NDFKeyValueNode();

            // key
            if (AcceptNext(NDFTokenType.Identifier))
                node.Key = c.Value;
            else if (AcceptNext(NDFTokenType.String))
                node.Key = c.Value;
            else
                ExpectNext(NDFTokenType.Identifier);

            // equals
            ExpectNext(NDFTokenType.Equals);

            // value
            node.Value = ParseValue();

            return node;
        }

        /// <summary>
        /// Parses a value.
        /// {value}
        /// </summary>
        /// <returns></returns>
        private object ParseValue() {
            if (AcceptNext(NDFTokenType.Number))
                return double.Parse(c.Value);
            else if (AcceptNext(NDFTokenType.String))
                return c.Value;
            else if (AcceptPeek(NDFTokenType.SqBracketOpen))
                return ParseArray();
            else if (AcceptPeek(NDFTokenType.BraceOpen))
                return ParseTable();
            else if (AcceptNext(NDFTokenType.Boolean))
                return bool.Parse(c.Value);
            else {
                Next();
                Error();
                return null;
            }
        }

        /// <summary>
        /// Peeks a value, returning if found.
        /// {value}
        /// </summary>
        /// <returns></returns>
        private bool PeekValue() {
            NDFTokenType t = Peek().Type;
            return valueTokens.Contains(t);
        }

        /// <summary>
        /// Parses an array.
        /// [sqbracketopen] ([identifier] [equals] {value} (comma)) [sqbracketclose]
        /// </summary>
        /// <returns></returns>
        private object[] ParseArray() {
            // sq bracket open
            ExpectNext(NDFTokenType.SqBracketOpen);

            // values
            List<object> values = new List<object>();

            while (PeekValue()) {
                values.Add(ParseValue());

                if (!AcceptNext(NDFTokenType.Comma))
                    break;
            }

            // sq bracket close
            ExpectNext(NDFTokenType.SqBracketClose);

            return values.ToArray();
        }

        /// <summary>
        /// Throws a custom error for the specified token.
        /// </summary>
        /// <param name="c">The token.</param>
        /// <param name="err">The error.</param>
        private void Error(NDFToken c, string err) {
            throw new Exception(err + " near " + source + ":" + c.Line);
        }
        
        /// <summary>
        /// Throws an unexpected token error for the specified token, expecting another type.
        /// </summary>
        /// <param name="c">The token.</param>
        /// <param name="type">The expected type.</param>
        private void Error(NDFToken c, NDFTokenType type) {
            if (c == null)
                Error(last, "Expected " + type.ToString().ToLower() + " but reached end of stream");

            throw new Exception("Unexpected " + c.Type.ToString().ToLower() + " '" + c.Value + "', expected " + type.ToString() + " near " + source + ":" + c.Line);
        }

        /// <summary>
        /// Throws an unexpected token error for the specified token.
        /// </summary>
        /// <param name="c">The token.</param>
        private void Error(NDFToken c) {
            if (c == null)
                Error(last, "Unexpected end of stream");

            throw new Exception("Unexpected " + c.Type.ToString().ToLower() + " '" + c.Value + "' near " + source + ":" + c.Line);
        }

        /// <summary>
        /// Throws a custom error for the specified token.
        /// </summary>
        /// <param name="err">The error.</param>
        private void Error(string err) {
            Error(c, err);
        }

        /// <summary>
        /// Throws an unexpected token error for the current token, expecting another type.
        /// </summary>
        /// <param name="type">The expected type.</param>
        private void Error(NDFTokenType type) {
            Error(c, type);
        }

        /// <summary>
        /// Throws an unexpected token error for the current token.
        /// </summary>
        private void Error() {
            Error(c);
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="NDFParser"/> class.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="source">The source.</param>
        public NDFParser(NDFToken[] input, string source) {
            this.input = input;
            this.source = source;
        }

        /// <summary>
        /// Initializes the <see cref="NDFParser"/> class.
        /// </summary>
        static NDFParser() {
            // value tokens
            valueTokens = new List<NDFTokenType>() {
                NDFTokenType.String,
                NDFTokenType.Boolean,
                NDFTokenType.SqBracketOpen,
                NDFTokenType.BraceOpen,
                NDFTokenType.Number
            };
        }
        #endregion
    }

    enum NDFParserState
    {
        Start,
        Object,
        Value,
        Table
    }
}
