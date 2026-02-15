public sealed class ConferenceOptions
{
    public string VenueName { get; set; } = "";     // «БелГУИР»
    public string VenueAddress { get; set; } = "";  // «Минск, П. Бровки, 6»
    public string Dates { get; set; } = "";         // «24–28 марта 2025»
    public int FeeRub { get; set; } = 0;            // напр. 1500
    public string[] Requisites { get; set; } = Array.Empty<string>();
}