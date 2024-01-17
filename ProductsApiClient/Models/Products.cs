using System.Text.Json.Serialization;

namespace ProductsApiClient;

public partial class Products{

    [JsonPropertyName("id")] 
    public int Id {get;set;}

    [JsonPropertyName("title")] 
    public string? Title {get;set;}

    [JsonPropertyName("price")] 
    public int Price {get;set;}
}