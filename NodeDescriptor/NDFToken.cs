using System;

namespace NodeDescriptor
{
    class NDFToken
    {
        public string Value = "";
        public int Line = 0;
        public int Character = 0;
        public NDFTokenType Type = NDFTokenType.None;
    }

    enum NDFTokenType
    {
        None,
        Identifier,
        Number,
        String,
        Dollar,
        Hashtag,
        BraceOpen,
        BraceClose,
        SqBracketOpen,
        SqBracketClose,
        Semicolon,
        Colon,
        Equals,
        Comma,
        Boolean
    }
}
