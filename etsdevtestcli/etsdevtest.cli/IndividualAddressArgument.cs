using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace etsdevtest.cli;

public class IndividualAddressArgument : Argument<ushort>
{
    public IndividualAddressArgument(string description = "Individual address in the format x.x.x", bool aDefault = false) : base(
        name: "ia",
        description: description,
        parse: result =>
        {
            if (result.Tokens.SingleOrDefault() is Token token)
            {
                string[] parts = token.Value.Split('.');
                if (parts.Length != 3)
                {
                    throw new ArgumentException("Input must be in the format x.x.x");
                }

                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);
                int z = int.Parse(parts[2]);

                if (x < 0 || x > 15 || y < 0 || y > 15 || z < 0 || z > 255)
                {
                    throw new ArgumentOutOfRangeException("Values must be within the range: x (0-15), y (0-15), z (0-255)");
                }

                return (ushort)((x << 12) | (y << 8) | z); ;
            }
            return 0x0000;
        },
        isDefault: aDefault
    )
    { }
}
