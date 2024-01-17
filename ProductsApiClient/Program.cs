using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ProductsApiClient;

using HttpClient client = new();
client.BaseAddress = new Uri("https://dummyjson.com/");
client.DefaultRequestHeaders.Accept.Clear();
client.DefaultRequestHeaders.Accept.Add(
    new MediaTypeWithQualityHeaderValue("application/json")
);

await Program(client);

static async Task Program(HttpClient client)
{

    ListOfProducts? listOfProducts = GetAllProducts(client).Result;
    if (listOfProducts != null)
    {
        await Menu(listOfProducts, client);
    }
}

static async Task Menu(ListOfProducts listOfProducts, HttpClient client)
{
    string? selection = "4";
    do
    {
        Console.Clear();
        Console.WriteLine("Bienvenido!");
        Console.WriteLine("Selecciona una de las siguientes opciones");
        Console.WriteLine("1.- Ver la lista de productos");
        Console.WriteLine("2.- Actualizar o agregar un producto");
        Console.WriteLine("3.- Eliminar un producto");
        Console.WriteLine("4.- Salir");
        selection = Console.ReadLine();
        switch (selection)
        {
            case "1":
                Console.Clear();
                PrintListOfProducts(listOfProducts);
                Console.WriteLine("Oprima una tecla para continuar");
                Console.ReadLine();
                break;
            case "2":
                var statusCode = await CreateOrUpdateAProduct(client, listOfProducts);
                Console.WriteLine("Oprima una tecla para continuar");
                Console.ReadLine();
                break;
            case "3":
                Console.Clear();
                HttpResponseMessage response = SelectAProduct(client,listOfProducts,selection);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("El producto se ha eliminado con exito");
                } else {
                    Console.WriteLine("Algo no ha salido como esperaba!");
                }
                Console.WriteLine("Oprima una tecla para continuar...");
                Console.ReadLine();
                break;
            case "4":
                Console.Clear();
                break;
            default:
                Console.WriteLine("Opcion invalida");
                break;
        }
    } while (selection != "4");
}

static void PrintListOfProducts(ListOfProducts listOfProducts){
    foreach(Products product in listOfProducts.Products)
        Console.WriteLine($"{product.Id}.- {product.Title}\nprecio ${product.Price}");
}

static async Task<ListOfProducts?> GetAllProducts(HttpClient client){
    var json = await client.GetStringAsync("products");
    ListOfProducts? listOfProducts = JsonSerializer.Deserialize<ListOfProducts>(json);
    return listOfProducts;
}

static async Task<HttpStatusCode?> CreateOrUpdateAProduct(HttpClient client, ListOfProducts listOfProducts){
    HttpResponseMessage response;
    do
    {
        Console.Clear();
        Console.WriteLine("¿Que desea hacer?\n1.-Subir un nuevo producto\n2.-Actualizar un producto");
        string? selection = Console.ReadLine();
        if (selection == "1"){
            response = await CreateANewProduct(client, listOfProducts);
            break;
        } else if (selection == "2") {
            response = SelectAProduct(client,listOfProducts,selection);
            break;
        } else {
            Console.WriteLine("opcion no valida!");
        }
    } while (true);
    
    response.EnsureSuccessStatusCode();
    if (response.StatusCode == HttpStatusCode.OK)
    {
        Console.WriteLine("Producto guardado con exito!");
    }else{
        Console.WriteLine("Algo ha salido terriblemente mal!");
    }
    return response.StatusCode;
}

static async Task<HttpResponseMessage> CreateANewProduct(HttpClient client, ListOfProducts listOfProducts){
    Console.Clear();
    string? title;
    do
    {
        Console.WriteLine("Ingrese el nombre del producto: ");
        title = Console.ReadLine();
        if (title != null)
        {
            break;
        }else{
            Console.WriteLine("Ingrese un nombre!");
        }
    } while (true);
    int price;
    do
    {
        Console.WriteLine("Ingrese el precio del producto: ");
        string? stringPrice = Console.ReadLine();
        if (stringPrice != null)
        {
            try
            {
                price = int.Parse(stringPrice);
            }
            catch (System.Exception)
            {
                Console.WriteLine("Numero invalido!");
                throw;
            }
            break;
        }else{
            Console.WriteLine("Ingrese un precio!");
        }
    } while (true);
    Products newProduct = new(){
        Id = listOfProducts.Products.Last().Id+1,
        Price = price,
        Title = title
    };
    listOfProducts.Products.Add(newProduct);
    HttpResponseMessage response = await client.PostAsJsonAsync("products/add",newProduct);
    return response;
}

static HttpResponseMessage SelectAProduct(HttpClient client, ListOfProducts listOfProducts, string PrevSelection){
    Console.Clear();
    HttpResponseMessage response;
    do
    {
        Console.WriteLine("Ingrese el numero de producto:");
        PrintListOfProducts(listOfProducts);
        string? selection = Console.ReadLine();
        if (int.Parse(selection) > 0 && int.Parse(selection) <= listOfProducts.Products.Last().Id)
        {
            if(PrevSelection == "2"){
                response = UpdateAProduct(client,listOfProducts.Products.Find(p => p.Id == int.Parse(selection))).Result;
            } else {
                response = DeleteAProduct(client,listOfProducts.Products.Find(p => p.Id == int.Parse(selection)),listOfProducts).Result;
            }
            break;
        } else {
            Console.WriteLine("Opcion invalida!");
        }
    } while (true);
    
    return response;
}

static async Task<HttpResponseMessage> UpdateAProduct(HttpClient client,Products productToEdit){
    HttpResponseMessage response;
    Console.Clear();
    Console.WriteLine("Que desea editar?");
    Console.WriteLine("1.-Nombre\n2.-Precio");
    do
    {
        string? selection = Console.ReadLine();
        if(selection == "1"){
            Console.WriteLine("Ingrese el nuevo nombre: ");
            string? name = Console.ReadLine();
            productToEdit.Title = name;
            break;
        }else if(selection == "2"){
            Console.WriteLine("Ingrese el nuevo precio:");
            string? price = Console.ReadLine();
            productToEdit.Price = int.Parse(price);
            break;
        } else{
            Console.WriteLine("Opcion invalida!");
        }
    } while (true);
    HttpContent httpContent = new StringContent(JsonSerializer.Serialize(productToEdit));
    response = await client.PutAsync($"products/{productToEdit.Id}", httpContent);

    return response;
}

static async Task<HttpResponseMessage> DeleteAProduct(HttpClient client, Products product, ListOfProducts listOfProducts){
    Console.Clear();
    HttpResponseMessage response = await client.DeleteAsync($"products/{product.Id}");
    listOfProducts.Products.Remove(product);
    return response;
}