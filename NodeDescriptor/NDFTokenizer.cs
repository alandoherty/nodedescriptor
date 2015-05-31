using System;
using System.Collections.Generic;

namespace NodeDescriptor
{
    class NDFTokenizer
    {
        #region Fields
        private static Dictionary<char, NDFTokenType> symbols;
        private List<NDFToken> tokens = null;
        private NDFToken[] tokensArr = null;
        private string input = "";
        private string source = "[Unknown]";

        private NDFTokenizerState state = NDFTokenizerState.Start;
        private int pos = 0;
        private string token = "";
        private int line = 1;
        private int character = 1;
        private bool escape = false;
        private char c = '\0';
        #endregion

        #region Properties        
        /// <summary>
        /// Gets the input.
        /// </summary>
        /// <value>The input.</value>
        public string Input {
            get {
                return input;
            }
        }

        /// <summary>
        /// Gets the tokens.
        /// </summary>
        /// <value>The tokens.</value>
        public NDFToken[] Output {
            get {
                return tokensArr;
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
        /// Determines whether [is identifier character] [the specified c].
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns></returns>
        private bool IsIdentifierChar(char c) {
            return (Char.IsLetterOrDigit(c) || c == '_' || c == '-');
        }

        /// <summary>
        /// Peek the next character.
        /// </summary>
        /// <returns></returns>
        private char Peek() {
            // check bounds
            if (pos == input.Length)
                return '\0';

            return input[pos];
        }

        /// <summary>
        /// Gets the next character and shifts the position by one.
        /// </summary>
        /// <returns></returns>
        private char Next() {
            // check bounds
            if (pos == input.Length)
                return '\0';

            return input[pos++];
        }

        /// <summary>
        /// Parses the input.
        /// </summary>
        public void Tokenize() {
            // initial values
            pos = 0;
            state = NDFTokenizerState.Start;

            // process
            c = Next();

            while (c != '\0') {
                // index management
                if (c == '\n') {
                    line++;
                    character = 0;
                }

                // state machine
                if (state == NDFTokenizerState.Start)
                    TokenizeStart();
                else if (state == NDFTokenizerState.String)
                    TokenizeString();
                else if (state == NDFTokenizerState.Comment)
                    TokenizeComment();
                else if (state == NDFTokenizerState.LineComment)
                    TokenizeLineComment();
                else if (state == NDFTokenizerState.MultilineComment)
                    TokenizeMultilineComment();
                else if (state == NDFTokenizerState.Identifier)
                    TokenizeIdentifier();
                else if (state == NDFTokenizerState.Number)
                    TokenizeNumber();

                // next
                c = Next();
                character++;
            }

            // output
            tokensArr = tokens.ToArray();
        }

        /// <summary>
        /// The initial state to discover what token to parse next.
        /// </summary>
        /// <exception cref="Exception">Unexpected symbol ' + c + ' on line  + line + : + character</exception>
        private void TokenizeStart() {
            // clear token
            token = "";

            // check character
            if (symbols.ContainsKey(c)) {
                Token(c.ToString(), symbols[c]);
            } else if (c == '\"') {
                State(NDFTokenizerState.String);
            } else if (c == '/') {
                State(NDFTokenizerState.Comment);
            } else if (Char.IsLetter(c)) {
                token = new string(new char[] { c });

                if (IsIdentifierChar(Peek())) {
                    State(NDFTokenizerState.Identifier);
                } else {
                    Token(token, NDFTokenType.Identifier);
                }
            } else if (Char.IsDigit(c)) {
                token = new string(new char[] { c });

                if (Char.IsDigit(Peek()) || Peek() == '.') {
                    State(NDFTokenizerState.Number);
                } else {
                    Token(token, NDFTokenType.Number);
                }
            } else if (Char.IsWhiteSpace(c)) {
            } else
                Error(c);
        }

        /// <summary>
        /// Parses a string.
        /// </summary>
        private void TokenizeString() {
            if (escape) {
                // escape character
                if (c == 'n')
                    token += '\n';
                else if (c == 't')
                    token += '\t';
                else if (c == 'r')
                    token += '\r';
                else
                    token += c;

                // clear escape
                escape = false;
            } else {
                if (c == '\"') {
                    Token(token, NDFTokenType.String);
                    State(NDFTokenizerState.Start);
                }  else if (c == '\\')
                    escape = true;
                else
                    token += c;
            }
        }

        /// <summary>
        /// Parses a comment.
        /// </summary>
        /// <exception cref="Exception">Unexpected symbol ' + c + ' at line  + line + : + character</exception>
        private void TokenizeComment() {
            if (c == '/')
                State(NDFTokenizerState.LineComment);
            else if (c == '*')
                State(NDFTokenizerState.MultilineComment);
            else
                Error();
        }

        /// <summary>
        /// Parses a line comment.
        /// </summary>
        private void TokenizeLineComment() {
            if (c == '\n')
                State(NDFTokenizerState.Start);
        }

        /// <summary>
        /// Parses a multiline comment.
        /// </summary>
        private void TokenizeMultilineComment() {
            if (c == '*' && Peek() == '/')
                State(NDFTokenizerState.Start);
        }

        /// <summary>
        /// Parses an identifier token.
        /// </summary>
        private void TokenizeIdentifier() {
            token += c;
            char p = Peek();

            if (!IsIdentifierChar(Peek())) {
                if (token.ToLower() == "true")
                    Token(token, NDFTokenType.Boolean);
                else if (token.ToLower() == "false")
                    Token(token, NDFTokenType.Boolean);
                else
                    Token(token, NDFTokenType.Identifier);
                State(NDFTokenizerState.Start);
            }
        }

        /// <summary>
        /// Parses a number token.
        /// </summary>
        /// <exception cref="Exception">Unexpected symbol '.' in number at line  + line + : + character</exception>
        private void TokenizeNumber() {
            token += c;

            if (!Char.IsDigit(Peek()) && Peek() != '.') {
                Token(token, NDFTokenType.Number);
                State(NDFTokenizerState.Start);
            }

            // only one dot allowed
            if (Peek() == '.' && token.Contains("."))
                Error('.');
        }

        /// <summary>
        /// Adds a token.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        private void Token(string value, NDFTokenType type) {
            tokens.Add(new NDFToken() { Value = value, Type = type, Character = character, Line = line });
        }

        /// <summary>
        /// Sets the state.
        /// </summary>
        /// <param name="state">The state.</param>
        private void State(NDFTokenizerState state) {
            this.state = state;
        }

        /// <summary>
        /// Throws an unexpected symbol error for the specified character.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <exception cref="Exception">Unexpected symbol '.' in  + source + : + line</exception>
        private void Error(char c) {
            throw new Exception("Unexpected symbol '.' in " + source + ":" + line);
        }

        /// <summary>
        /// Throws an unexpected symbol error for the current character.
        /// </summary>
        private void Error() {
            Error(c);
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="NDFTokenizer"/> class.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="source">The source.</param>
        public NDFTokenizer(string input, string source) {
            this.tokens = new List<NDFToken>();
            this.input = input;
            this.source = source;
        }

        /// <summary>
        /// Initializes the <see cref="NDFTokenizer"/> class statically.
        /// </summary>
        static NDFTokenizer() {
            // symbols
            symbols = new Dictionary<char, NDFTokenType>() {
                //{'#', NDFTokenType.Hashtag},
                //{'$', NDFTokenType.Dollar},
                {'{', NDFTokenType.BraceOpen},
                {'}', NDFTokenType.BraceClose},
                {';', NDFTokenType.Semicolon},
                {'[', NDFTokenType.SqBracketOpen},
                {']', NDFTokenType.SqBracketClose},
                {':', NDFTokenType.Colon},
                {'=', NDFTokenType.Equals},
                {',', NDFTokenType.Comma}
            };
        }
        #endregion
    }

    enum NDFTokenizerState
    {
        Start,
        String,
        Comment,
        LineComment,
        MultilineComment,
        Identifier,
        Number
    }
}
