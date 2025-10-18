using System.Globalization;
using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.Net.Http.Headers;

namespace eshop.Shared;

public static partial class ValidationExtensions
{
    public static void MustBeValidCardNumber<T>(
        this IRuleBuilder<T, string?> ruleBuilder
    ) => ruleBuilder.Must(x => x is not null && CardNumberRegex().IsMatch(x))
        .WithMessage("card_number is not valid.");

    public static void MustBeValidExpiryDate<T>(this IRuleBuilder<T, string?> ruleBuilder)
        => ruleBuilder.Must(x => x is not null && ExpiryDateRegex().IsMatch(x))
            .WithMessage("payment expiration is not valid.");

    public static void MustBeValidEtag<T>(
        this IRuleBuilder<T, string?> ruleBuilder
    ) => ruleBuilder.Must(x => x is not null && EtagRegex().IsMatch(x))
        .WithMessage($"{HeaderNames.IfMatch} header is not valid.");

    public static void MustBeValidCountryName<T>(
        this IRuleBuilder<T, string?> ruleBuilder
    ) => ruleBuilder.Must(x => x is not null && CountryNames.Contains(x))
        .WithMessage("country is not valid.");

    public static void MustBeValidUlid<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder.NotEmpty().Must(x => Ulid.TryParse(x, out _))
            .WithMessage((_, propertyValue) => $"{propertyValue} is not a valid Ulid.");

    public static IRuleBuilderOptions<T, string?> MustBeValidGuid<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder.NotEmpty().Must(x => Guid.TryParse(x, out _))
            .WithMessage((_, propertyValue) => $"{propertyValue} is not a valid UUID");

    public static void MustBeValidTimestamp<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder.NotEmpty().Must(BeValidTimestamp)
        .WithMessage((_, value) => $"{value} is not a valid timestamp");
    
    private static bool BeValidTimestamp(string? timestamp)
    {
        return DateTime.TryParseExact(
            timestamp,
            "yyyy-MM-dd'T'HH:mm:ss.fff'Z'",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out _
        );
    }
    
    
    [GeneratedRegex(
        @"^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|6(?:011|5[0-9]{2})[0-9]{12}|(?:2131|1800|35\d{3})\d{11})$")]
    private static partial Regex CardNumberRegex();

    [GeneratedRegex(@"^(0[1-9]|1[0-2])\/\d{4}$")]
    private static partial Regex ExpiryDateRegex();

    [GeneratedRegex("""^W\/"\d+"$""")]
    private static partial Regex EtagRegex();

    private static readonly HashSet<string> CountryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        // The list includes the official country names based on the ISO 3166-1 standard.
        "Afghanistan", "Albania", "Algeria", "Andorra", "Angola", "Antigua and Barbuda",
        "Argentina", "Armenia", "Australia", "Austria", "Azerbaijan", "Bahamas", "Bahrain",
        "Bangladesh", "Barbados", "Belarus", "Belgium", "Belize", "Benin", "Bhutan",
        "Bolivia", "Bosnia and Herzegovina", "Botswana", "Brazil", "Brunei", "Bulgaria",
        "Burkina Faso", "Burundi", "Cabo Verde", "Cambodia", "Cameroon", "Canada",
        "Central African Republic", "Chad", "Chile", "China", "Colombia", "Comoros",
        "Congo (Congo-Brazzaville)", "Costa Rica", "Croatia", "Cuba", "Cyprus", "Czechia (Czech Republic)",
        "Denmark", "Djibouti", "Dominica", "Dominican Republic", "Ecuador", "Egypt",
        "El Salvador", "Equatorial Guinea", "Eritrea", "Estonia", "Eswatini (fmr. Swaziland)",
        "Ethiopia", "Fiji", "Finland", "France", "Gabon", "Gambia", "Georgia", "Germany",
        "Ghana", "Greece", "Grenada", "Guatemala", "Guinea", "Guinea-Bissau", "Guyana",
        "Haiti", "Holy See", "Honduras", "Hungary", "Iceland", "India", "Indonesia", "Iran",
        "Iraq", "Ireland", "Israel", "Italy", "Jamaica", "Japan", "Jordan", "Kazakhstan",
        "Kenya", "Kiribati", "Korea (North)", "Korea (South)", "Kuwait", "Kyrgyzstan", "Laos",
        "Latvia", "Lebanon", "Lesotho", "Liberia", "Libya", "Liechtenstein", "Lithuania",
        "Luxembourg", "Madagascar", "Malawi", "Malaysia", "Maldives", "Mali", "Malta",
        "Marshall Islands", "Mauritania", "Mauritius", "Mexico", "Micronesia", "Moldova",
        "Monaco", "Mongolia", "Montenegro", "Morocco", "Mozambique", "Myanmar (formerly Burma)",
        "Namibia", "Nauru", "Nepal", "Netherlands", "New Zealand", "Nicaragua", "Niger",
        "Nigeria", "North Macedonia (formerly Macedonia)", "Norway", "Oman", "Pakistan",
        "Palau", "Palestine State", "Panama", "Papua New Guinea", "Paraguay", "Peru",
        "Philippines", "Poland", "Portugal", "Qatar", "Romania", "Russia", "Rwanda",
        "Saint Kitts and Nevis", "Saint Lucia", "Saint Vincent and the Grenadines", "Samoa",
        "San Marino", "Sao Tome and Principe", "Saudi Arabia", "Senegal", "Serbia",
        "Seychelles", "Sierra Leone", "Singapore", "Slovakia", "Slovenia", "Solomon Islands",
        "Somalia", "South Africa", "South Sudan", "Spain", "Sri Lanka", "Sudan", "Suriname",
        "Sweden", "Switzerland", "Syria", "Tajikistan", "Tanzania", "Thailand", "Timor-Leste",
        "Togo", "Tonga", "Trinidad and Tobago", "Tunisia", "Turkey", "Turkmenistan", "Tuvalu",
        "Uganda", "Ukraine", "United Arab Emirates", "United Kingdom", "United States of America",
        "Uruguay", "Uzbekistan", "Vanuatu", "Venezuela", "Vietnam", "Yemen", "Zambia", "Zimbabwe"
    };
}