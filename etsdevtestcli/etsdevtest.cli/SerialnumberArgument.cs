
using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;

namespace etsdevtest.cli;

public class Serialnumber
{
    public Serialnumber(byte[] aValue = null, string aString = "")
    {
        Value = aValue;
        String = aString;
    }

    public string String
    {
        get;
        set;
    }

    public byte[] Value
    {
        get;
        set;
    }

    static public byte[] FromHex(string aSerialnumber)
    {
        byte[] sn = new byte[6];
        string serialnumber = "";

        foreach (var ch in aSerialnumber)
        {
            // skip any : colons in the hex view
            if (ch != ':')
            {
                serialnumber += ch;
            }
        }

        if (serialnumber.Length != 12)
        {
            throw new ArgumentException("Expected 12 characters as serialnumber");
        }

        for (int i = 0; i < serialnumber.Length; i += 2)
        {
            sn[i / 2] = Convert.ToByte(serialnumber.Substring(i, 2), 16);
        }

        return sn;
    }
};

public class SerialnumberArgument : Argument<Serialnumber>
{
    public byte[] StringToHex(string aSerialnumber)
    {
        return Serialnumber.FromHex(aSerialnumber);
    }

    public SerialnumberArgument(string description = "12 characters serialnumber of the device", bool aDefault = false) : base(
        name: "sn",
        description: description,
        parse: result =>
        {
            if (result.Tokens.SingleOrDefault() is Token token)
            {
                if (token.Value == string.Empty)
                {
                    return new Serialnumber(new byte[] { }, "");
                }

                return new Serialnumber(
                    aString: token.Value.ToUpper(),
                    aValue: Serialnumber.FromHex(token.Value));
            }
            return new Serialnumber(new byte[] { }, "");
        },
        isDefault: aDefault
     )
    { }
}
