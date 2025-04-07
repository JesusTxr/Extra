using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

class Producto
{
    public string Nombre { get; set; }
    public double Precio { get; set; }
    public int Cantidad { get; set; }

    public Producto() { }

    public Producto(string nombre, double precio, int cantidad)
    {
        Nombre = nombre;
        Precio = precio;
        Cantidad = cantidad;
    }

    public double CalcularTotal()
    {
        return Precio * Cantidad;
    }
}

class Inventario
{
    private string _rutaArchivo;

    public Inventario(string rutaArchivo)
    {
        _rutaArchivo = rutaArchivo;
    }

    public List<Producto> LeerProductos()
    {
        if (!File.Exists(_rutaArchivo)) return new List<Producto>();

        string json = File.ReadAllText(_rutaArchivo);
        if (string.IsNullOrWhiteSpace(json)) return new List<Producto>();

        try
        {
            return JsonSerializer.Deserialize<List<Producto>>(json);
        }
        catch
        {
            Console.WriteLine("Error al leer el archivo. Formato inválido.");
            return new List<Producto>();
        }
    }

    public void GuardarProductos(List<Producto> productos)
    {
        string json = JsonSerializer.Serialize(productos, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_rutaArchivo, json);
    }

    public void AgregarProducto(Producto nuevo)
    {
        var productos = LeerProductos();
        productos.Add(nuevo);
        GuardarProductos(productos);
        Console.WriteLine("Producto agregado al inventario.");
    }

    public bool HacerCompra(string nombre, int cantidad, Carrito carrito)
    {
        var productos = LeerProductos();
        var producto = productos.Find(p => p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));

        if (producto == null)
        {
            Console.WriteLine("Producto no encontrado en el inventario.");
            return false;
        }

        if (producto.Cantidad < cantidad)
        {
            Console.WriteLine("No hay suficiente cantidad en inventario.");
            return false;
        }

        producto.Cantidad -= cantidad;
        GuardarProductos(productos);

        carrito.AgregarAlCarrito(new Producto(producto.Nombre, producto.Precio, cantidad));
        Console.WriteLine($"Compra añadida al carrito. Subtotal: ${producto.Precio * cantidad:F2}");
        return true;
    }

    public void MostrarProductoMasCaro()
    {
        var productos = LeerProductos();
        if (productos.Count == 0)
        {
            Console.WriteLine("El inventario está vacío.");
            return;
        }

        Producto masCaro = productos[0];
        foreach (var p in productos)
        {
            if (p.Precio > masCaro.Precio)
                masCaro = p;
        }

        Console.WriteLine($"\nProducto más caro: {masCaro.Nombre} - Precio: ${masCaro.Precio:F2}");
    }
}

class Carrito
{
    private List<Producto> compras = new List<Producto>();

    public void AgregarAlCarrito(Producto p)
    {
        compras.Add(p);
    }

    public void Pagar()
    {
        if (compras.Count == 0)
        {
            Console.WriteLine("\nNo hay productos en el carrito.");
            return;
        }

        Console.WriteLine("\n--- Detalles de la compra ---");
        double total = 0;

        foreach (var p in compras)
        {
            double subtotal = p.CalcularTotal();
            Console.WriteLine($"{p.Nombre} x{p.Cantidad} = ${subtotal:F2}");
            total += subtotal;
        }

        Console.WriteLine($"\nTotal a pagar: ${total:F2}");

        Console.Write("\nIngrese el monto a pagar: ");
        if (!double.TryParse(Console.ReadLine(), out double montoPago) || montoPago <= 0)
        {
            Console.WriteLine("Monto inválido.");
            return;
        }

        if (montoPago < total)
        {
            Console.WriteLine("El monto ingresado es insuficiente para completar la compra.");
            return;
        }

        double cambio = montoPago - total;
        Console.WriteLine($"¡Gracias por su compra! Su cambio es: ${cambio:F2}");

        compras.Clear();
    }
}

class Program
{
    static void Main()
    {
        Inventario inventario = new Inventario("productos.json");
        Carrito carrito = new Carrito();

        while (true)
        {
            Console.WriteLine("\n--- Menú ---");
            Console.WriteLine("1. Agregar producto al inventario");
            Console.WriteLine("2. Comprar producto");
            Console.WriteLine("3. Ver producto más caro");
            Console.WriteLine("4. Pagar");
            Console.Write("Seleccione una opción: ");

            string opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1":
                    Console.Write("Nombre del producto: ");
                    string nombre = Console.ReadLine();

                    Console.Write("Precio unitario: ");
                    if (!double.TryParse(Console.ReadLine(), out double precio) || precio <= 0)
                    {
                        Console.WriteLine("Precio inválido.");
                        break;
                    }

                    Console.Write("Cantidad: ");
                    if (!int.TryParse(Console.ReadLine(), out int cantidad) || cantidad <= 0)
                    {
                        Console.WriteLine("Cantidad inválida.");
                        break;
                    }

                    Producto nuevo = new Producto(nombre, precio, cantidad);
                    inventario.AgregarProducto(nuevo);
                    break;

                case "2":
                    Console.Write("Ingrese el nombre del producto a comprar: ");
                    string nombreCompra = Console.ReadLine();

                    Console.Write("Cantidad a comprar: ");
                    if (!int.TryParse(Console.ReadLine(), out int cantCompra) || cantCompra <= 0)
                    {
                        Console.WriteLine("Cantidad inválida.");
                        break;
                    }

                    inventario.HacerCompra(nombreCompra, cantCompra, carrito);
                    break;

                case "3":
                    inventario.MostrarProductoMasCaro();
                    break;

                case "4":
                    carrito.Pagar();
                    break;

                default:
                    Console.WriteLine("Opción no válida.");
                    break;
            }
        }
    }
}
