using System.Security.Cryptography;
using System.Text;
using GarageDoorOpener.Shared.Interfaces;

namespace GarageDoorOpener.Shared;

internal class TwoFactorAuthenticator : ITwoFactorAuthenticator
{
    private static DateTime _epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private TimeSpan DefaultClockDriftTolerance { get; set; }
    //public bool UseManagedSha1Algorithm { get; set; }
    //public bool TryUnmanagedAlgorithmOnFailure { get; set; }

    public TwoFactorAuthenticator() : this(TimeSpan.FromMinutes(5)) //: this(true, true)
    {
        
    }

    public TwoFactorAuthenticator(TimeSpan clockDrift)
    {
        DefaultClockDriftTolerance = clockDrift;
    }

    //public TwoFactorAuthenticator(bool useManagedSha1, bool useUnmanagedOnFail)
    //{
    //DefaultClockDriftTolerance = TimeSpan.FromMinutes(5);
    //UseManagedSha1Algorithm = useManagedSha1;
    //TryUnmanagedAlgorithmOnFailure = useUnmanagedOnFail;
    //}

    /// <summary>
    /// Generate a setup code for a Google Authenticator user to scan.
    /// </summary>
    /// <param name="accountTitleNoSpaces">Account Title (no spaces)</param>
    /// <param name="accountSecretKey">Account Secret Key</param>
    /// <param name="qrCodeWidth">QR Code Width</param>
    /// <param name="qrCodeHeight">QR Code Height</param>
    /// <returns>SetupCode object</returns>
    public static SetupCode GenerateSetupCode(string accountTitleNoSpaces, string accountSecretKey, int qrCodeWidth,
        int qrCodeHeight)
    {
        return GenerateSetupCode(string.Empty, accountTitleNoSpaces, accountSecretKey, qrCodeWidth, qrCodeHeight);
    }

    /// <summary>
    /// Generate a setup code for a Google Authenticator user to scan (with issuer ID).
    /// </summary>
    /// <param name="issuer">Issuer ID (the name of the system, i.e. 'MyApp')</param>
    /// <param name="accountTitleNoSpaces">Account Title (no spaces)</param>
    /// <param name="accountSecretKey">Account Secret Key</param>
    /// <param name="qrCodeWidth">QR Code Width</param>
    /// <param name="qrCodeHeight">QR Code Height</param>
    /// <returns>SetupCode object</returns>
    public static SetupCode GenerateSetupCode(string issuer, string accountTitleNoSpaces, string accountSecretKey,
        int qrCodeWidth, int qrCodeHeight)
    {
        return GenerateSetupCode(issuer, accountTitleNoSpaces, accountSecretKey, qrCodeWidth, qrCodeHeight, false);
    }

    /// <summary>
    /// Generate a setup code for a Google Authenticator user to scan (with issuer ID).
    /// </summary>
    /// <param name="issuer">Issuer ID (the name of the system, i.e. 'MyApp')</param>
    /// <param name="accountTitleNoSpaces">Account Title (no spaces)</param>
    /// <param name="accountSecretKey">Account Secret Key</param>
    /// <param name="qrCodeWidth">QR Code Width</param>
    /// <param name="qrCodeHeight">QR Code Height</param>
    /// <param name="useHttps">Use HTTPS instead of HTTP</param>
    /// <returns>SetupCode object</returns>
    public static SetupCode GenerateSetupCode(string issuer, string accountTitleNoSpaces, string accountSecretKey,
        int qrCodeWidth, int qrCodeHeight, bool useHttps)
    {
        if (accountTitleNoSpaces == null)
        {
            throw new NullReferenceException("Account Title is null");
        }

        accountTitleNoSpaces = accountTitleNoSpaces.Replace(" ", "");

        //SetupCode sc = new ();
        //sc.Account = accountTitleNoSpaces;
        //sc.AccountSecretKey = accountSecretKey;

        string encodedSecretKey = EncodeAccountSecretKey(accountSecretKey);
        //sc.ManualEntryKey = encodedSecretKey;

        string provisionUrl = string.Empty;

        if (string.IsNullOrEmpty(issuer))
        {
            provisionUrl = UrlEncode(string.Format("otpauth://totp/{0}?secret={1}", accountTitleNoSpaces,
                encodedSecretKey));
        }
        else
        {
            provisionUrl = UrlEncode(string.Format("otpauth://totp/{0}?secret={1}&issuer={2}", accountTitleNoSpaces,
                encodedSecretKey, UrlEncode(issuer)));
        }

        var protocol = useHttps ? "https" : "http";
        var url =
            $"{protocol}://chart.googleapis.com/chart?cht=qr&chs={qrCodeWidth}x{qrCodeHeight}&chl={provisionUrl}";

        //sc.QrCodeSetupImageUrl = url;

        return new(accountTitleNoSpaces, accountSecretKey, encodedSecretKey, url);
    }

    private static string UrlEncode(string value)
    {
        var result = new StringBuilder();
        var validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

        foreach (var symbol in value)
        {
            if (validChars.IndexOf(symbol) != -1)
            {
                result.Append(symbol);
            }
            else
            {
                result.Append('%' + String.Format("{0:X2}", (int)symbol));
            }
        }

        return result.ToString().Replace(" ", "%20");
    }

    private static string EncodeAccountSecretKey(string accountSecretKey)
    {
        //if (accountSecretKey.Length < 10)
        //{
        //    accountSecretKey = accountSecretKey.PadRight(10, '0');
        //}

        //if (accountSecretKey.Length > 12)
        //{
        //    accountSecretKey = accountSecretKey.Substring(0, 12);
        //}

        return Base32Encode(Encoding.UTF8.GetBytes(accountSecretKey));
    }

    private static string Base32Encode(byte[] data)
    {
        int inByteSize = 8;
        int outByteSize = 5;
        char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

        int i = 0, index = 0, digit = 0;
        int current_byte, next_byte;
        StringBuilder result = new((data.Length + 7) * inByteSize / outByteSize);

        while (i < data.Length)
        {
            current_byte = (data[i] >= 0) ? data[i] : (data[i] + 256); // Unsign

            /* Is the current digit going to span a byte boundary? */
            if (index > (inByteSize - outByteSize))
            {
                if ((i + 1) < data.Length)
                    next_byte = (data[i + 1] >= 0) ? data[i + 1] : (data[i + 1] + 256);
                else
                    next_byte = 0;

                digit = current_byte & (0xFF >> index);
                index = (index + outByteSize) % inByteSize;
                digit <<= index;
                digit |= next_byte >> (inByteSize - index);
                i++;
            }
            else
            {
                digit = (current_byte >> (inByteSize - (index + outByteSize))) & 0x1F;
                index = (index + outByteSize) % inByteSize;
                if (index == 0)
                    i++;
            }
            result.Append(alphabet[digit]);
        }

        return result.ToString();
    }

    public static string GeneratePINAtInterval(string accountSecretKey, long counter, int digits = 6)
    {
        return GenerateHashedCode(accountSecretKey, counter, digits);
    }

    internal static string GenerateHashedCode(string secret, long iterationNumber, int digits = 6)
    {
        byte[] key = Encoding.UTF8.GetBytes(secret);
        return GenerateHashedCode(key, iterationNumber, digits);
    }

    internal static string GenerateHashedCode(byte[] key, long iterationNumber, int digits = 6)
    {
        byte[] counter = BitConverter.GetBytes(iterationNumber);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(counter);
        }

        HMACSHA1 hmac = GetHMACSha1Algorithm(key);

        byte[] hash = hmac.ComputeHash(counter);

        int offset = hash[hash.Length - 1] & 0xf;

        // Convert the 4 bytes into an integer, ignoring the sign.
        int binary =
            ((hash[offset] & 0x7f) << 24)
            | (hash[offset + 1] << 16)
            | (hash[offset + 2] << 8)
            | (hash[offset + 3]);

        int password = binary % (int)Math.Pow(10, digits);
        return password.ToString(new string('0', digits));
    }

    private static long GetCurrentCounter()
    {
        return GetCurrentCounter(DateTime.UtcNow, _epoch, 30);
    }

    private static long GetCurrentCounter(DateTime now, DateTime epoch, int timeStep)
    {
        return (long)(now - epoch).TotalSeconds / timeStep;
    }

    /// <summary>
    /// Creates a HMACSHA1 algorithm to use to hash the counter bytes. By default, this will attempt to use
    /// the managed SHA1 class (SHA1Manager) and on exception (FIPS-compliant machine policy, etc) will attempt
    /// to use the unmanaged SHA1 class (SHA1CryptoServiceProvider).
    /// </summary>
    /// <param name="key">User's secret key, in bytes</param>
    /// <returns>HMACSHA1 cryptographic algorithm</returns>        
    private static HMACSHA1 GetHMACSha1Algorithm(byte[] key)
    {
        HMACSHA1 hmac;

        try
        {
            //hmac = new HMACSHA1(key, UseManagedSha1Algorithm);
            hmac = new HMACSHA1(key);
        }
        catch (InvalidOperationException ioe)
        {
            //if (UseManagedSha1Algorithm && TryUnmanagedAlgorithmOnFailure)
            //{
            //    try
            //    {
            //        hmac = new HMACSHA1(key, false);
            //    }
            //    catch (InvalidOperationException ioe2)
            //    {
            //        throw ioe2;
            //    }
            //}
            //else
            //{
            throw ioe;
            //}
        }

        return hmac;
    }

    public bool ValidateTwoFactorPIN(string accountSecretKey, string twoFactorCodeFromClient)
    {
        return ValidateTwoFactorPIN(accountSecretKey, twoFactorCodeFromClient, DefaultClockDriftTolerance);
    }

    public static bool ValidateTwoFactorPIN(string accountSecretKey, string twoFactorCodeFromClient,
        TimeSpan timeTolerance)
    {
        var codes = GetCurrentPINs(accountSecretKey, timeTolerance);
        return codes.Any(c => c == twoFactorCodeFromClient);
    }

    public string GetCurrentPIN(string accountSecretKey)
    {
        return GeneratePINAtInterval(accountSecretKey, GetCurrentCounter());
    }

    public static string GetCurrentPIN(string accountSecretKey, DateTime now)
    {
        return GeneratePINAtInterval(accountSecretKey, GetCurrentCounter(now, _epoch, 30));
    }

    public string[] GetCurrentPINs(string accountSecretKey)
    {
        return GetCurrentPINs(accountSecretKey, DefaultClockDriftTolerance);
    }

    public static string[] GetCurrentPINs(string accountSecretKey, TimeSpan timeTolerance)
    {
        List<string> codes = new();
        long iterationCounter = GetCurrentCounter();
        int iterationOffset = 0;

        if (timeTolerance.TotalSeconds > 30)
        {
            iterationOffset = Convert.ToInt32(timeTolerance.TotalSeconds / 30.00);
        }

        long iterationStart = iterationCounter - iterationOffset;
        long iterationEnd = iterationCounter + iterationOffset;

        for (long counter = iterationStart; counter <= iterationEnd; counter++)
        {
            codes.Add(GeneratePINAtInterval(accountSecretKey, counter));
        }

        return codes.ToArray();
    }
}
