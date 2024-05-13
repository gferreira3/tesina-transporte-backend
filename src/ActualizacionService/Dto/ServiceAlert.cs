using System.Text.Json.Serialization;

namespace ActualizacionService.Dto
{
    public class ServiceAlert
    {
        [JsonPropertyName("entity")]
        public Entity[] Entity { get; set; }
    }

    public class Entity
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("alert")]
        public Alert Alert { get; set; }
    }
}

public class Alert
{
    [JsonPropertyName("informed_entity")]
    public InformedEntity[] InformedEntity { get; set; }

    [JsonPropertyName("header_text")]
    public Detail HeaderText { get; set; }

    [JsonPropertyName("description_text")]
    public Detail DescriptionText { get; set; }
}

public class InformedEntity
{
  [JsonPropertyName("route_id")]
  public string RouteID { get; set; }
}

public class Detail
{
    [JsonPropertyName("translation")]
    public Translation[] Translation { get; set; }
}

public class Translation
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}