using System.Text.Json.Serialization;

namespace ProductsApiClient;

public record class ListOfProducts(
    [property : JsonPropertyName("products")] List<Products> Products
);
