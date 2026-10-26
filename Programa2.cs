// PROGRAMA 2 - REGISTRAR PRODUCTO (módulo Productos) | Programación Estructurada, tarjeta PE-01
// Guarda un producto (referencia, nombre, precio sin IVA, código de barras) y muestra el precio con IVA 13%.
// No deja repetir la referencia ni el código de barras.
// Uso: Visual Studio > proyecto nuevo de consola > pegar este archivo en Program.cs > Ctrl+F5.
//      En el menú, la opción 3 ejecuta las pruebas: todas deben salir [OK].
using System;
using System.Collections.Generic;

class Producto { public string Ref, Nombre, Barras; public decimal Precio; }     // Precio sin IVA

class Program
{
    const decimal IVA = 0.13m;
    static List<Producto> productos = new List<Producto>();

    static void Main()
    {
        System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Cargar();
        string op;
        do
        {
            Console.WriteLine("\n=== PROGRAMA 2: REGISTRAR PRODUCTO ===\n1. Registrar producto\n2. Ver productos\n3. Pruebas\n0. Salir");
            op = Leer("Opción");
            switch (op)
            {
                case "1": Crear(); break;
                case "2": Ver(); break;
                case "3": Pruebas(); break;
            }
        } while (op != "0");
    }

    static string Leer(string mensaje) { Console.Write(mensaje + ": "); return Console.ReadLine().Trim(); }

    static decimal LeerNum(string mensaje)
    {
        decimal d;
        while (!decimal.TryParse(Leer(mensaje), out d) || d < 0) Console.WriteLine("  Número no válido.");
        return d;
    }

    static decimal ConIva(decimal precio) { return Math.Round(precio * (1 + IVA), 2, MidpointRounding.AwayFromZero); }

    static Producto Buscar(string texto)                // por referencia o por código de barras
    {
        foreach (Producto p in productos)
            if (p.Ref == texto.ToUpper() || (p.Barras != "" && p.Barras == texto)) return p;
        return null;
    }

    // ---------- Función principal: valida todo y guarda. Devuelve null (y el motivo en error) si falla ----------
    static Producto Registrar(string rf, string nombre, decimal precio, string barras, out string error)
    {
        error = "";
        rf = rf.ToUpper();
        if (rf == "") error = "La referencia es obligatoria.";
        else if (Buscar(rf) != null) error = "Esa referencia ya existe.";
        else if (nombre == "") error = "El nombre es obligatorio.";
        else if (precio <= 0) error = "El precio debe ser mayor que 0.";
        else if (barras != "" && Buscar(barras) != null) error = "Ese código de barras ya existe.";
        if (error != "") return null;

        Producto p = new Producto();
        p.Ref = rf; p.Nombre = nombre; p.Precio = precio; p.Barras = barras;
        productos.Add(p);
        return p;
    }

    static void Crear()
    {
        string rf = Leer("Referencia (ej. BEB-004)");
        string nombre = Leer("Nombre");
        decimal precio = LeerNum("Precio sin IVA");
        string barras = Leer("Código de barras (Enter = omitir)");

        string error;
        Producto p = Registrar(rf, nombre, precio, barras, out error);
        Console.WriteLine(p == null ? "  Error: " + error : "  Producto guardado. Precio con IVA: " + ConIva(p.Precio).ToString("0.00") + " Bs");
    }

    static void Ver()
    {
        foreach (Producto p in productos)
            Console.WriteLine("  " + p.Ref + " | " + p.Nombre + " | " + p.Precio.ToString("0.00") + " | con IVA " + ConIva(p.Precio).ToString("0.00") + " | " + p.Barras);
    }

    // ---------- Datos de ejemplo (de las capturas de Dolibarr) y pruebas ----------
    static void Nuevo(string rf, string nombre, decimal precio, string barras)
    {
        Producto p = new Producto();
        p.Ref = rf; p.Nombre = nombre; p.Precio = precio; p.Barras = barras;
        productos.Add(p);
    }

    static void Cargar()
    {
        productos.Clear();
        Nuevo("BEB-001", "Chicha de maní con leche 1 L", 8.85m, "7770000000012");
        Nuevo("BEB-003", "Kéfir natural 1 L", 13.27m, "7770000000036");
        Nuevo("YOG-024", "Yogur bebible canela 1 L", 10.62m, "7770000000821");
    }

    static void Ok(string nombre, string esperado, string obtenido)
    {
        Console.WriteLine(esperado == obtenido ? "  [OK]    " + nombre + " -> " + obtenido
                                               : "  [FALLA] " + nombre + " -> esperado " + esperado + ", obtenido " + obtenido);
    }

    static void Pruebas()
    {
        Cargar();
        string e;
        Console.WriteLine("\n--- PRUEBAS: REGISTRAR PRODUCTO ---");
        Ok("Precio con IVA de 8.85", "10.00", ConIva(8.85m).ToString("0.00"));
        Ok("Precio con IVA de 13.27", "15.00", ConIva(13.27m).ToString("0.00"));
        Ok("Precio con IVA de 9.00", "10.17", ConIva(9.00m).ToString("0.00"));

        Producto p = Registrar("beb-004", "Jugo de naranja 1 L", 9.00m, "7770000000890", out e);
        Ok("Referencia nueva (pasa a mayúsculas)", "BEB-004", p == null ? "null" : p.Ref);
        Ok("Cantidad de productos", "4", productos.Count.ToString());
        Ok("Referencia repetida", "True", (Registrar("beb-001", "Otro", 5m, "", out e) == null).ToString());
        Ok("Código de barras repetido", "True", (Registrar("BEB-005", "Otro", 5m, "7770000000012", out e) == null).ToString());
        Ok("Precio 0", "True", (Registrar("BEB-006", "Otro", 0m, "", out e) == null).ToString());
        Ok("Nombre vacío", "True", (Registrar("BEB-007", "", 5m, "", out e) == null).ToString());
        Ok("Cantidad de productos (igual)", "4", productos.Count.ToString());
        Cargar();
    }
}
