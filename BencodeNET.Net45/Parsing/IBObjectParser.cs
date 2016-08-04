﻿using System.IO;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    public interface IBObjectParser
    {
        IBObject Parse(string bencodedString);
        IBObject Parse(Stream stream);
        IBObject Parse(BencodeStream stream);

        Task<IBObject> ParseAsync(Stream stream);
        Task<IBObject> ParseAsync(BencodeStream stream);
    }

    public interface IBObjectParser<T> : IBObjectParser where T : IBObject
    {
        T Parse(string bencodedString);
        T Parse(Stream stream);
        T Parse(BencodeStream stream);

        Task<T> ParseAsync(Stream stream);
        Task<T> ParseAsync(BencodeStream stream);
    }
}
