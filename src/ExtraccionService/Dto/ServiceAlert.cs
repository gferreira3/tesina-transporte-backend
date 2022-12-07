using ExtraccionService.Dto;
using System.Text.Json.Serialization;

namespace ExtraccionService.Dto
{
    public class ServiceAlert
    {
        [JsonPropertyName("_entity")]
        public Entity[] Entity { get; set; }
    }

    public class Entity
    {
        [JsonPropertyName("_alert")]
        public Alert Alert { get; set; }
    }
}

public class Alert
{
    [JsonPropertyName("_header_text")]
    public Detail HeaderText { get; set; }

    [JsonPropertyName("_description_text")]
    public Detail DescriptionText { get; set; }
}

public class Detail
{
    [JsonPropertyName("_translation")]
    public Translation[] Translation { get; set; }
}

public class Translation
{
    [JsonPropertyName("_text")]
    public string Text { get; set; }
}