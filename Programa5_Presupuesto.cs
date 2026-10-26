// PROGRAMA 5 - DESCUENTOS, PRECIO MAYORISTA Y PRESUPUESTO (módulo Comercial) | Programación Estructurada, tarjeta PE-01
// Arma un presupuesto (cotización) con varios productos: descuento, IVA 13% y total. Vale 15 días.
// Precio mayorista: desde 24 unidades de un producto, 10% de descuento (SUPUESTO: confirmar con el equipo).
// Se aplica el mayor entre el descuento manual y el mayorista.
// Uso: Visual Studio > proyecto nuevo de consola > pegar este archivo en Program.cs > Ctrl+F5.
//      En el menú, la opción 3 ejecuta las pruebas: todas deben salir [OK].
using System;
using System.Collections.Generic;

class Producto { public string Ref, Nombre; public decimal Precio; }                 // Precio sin IVA
class Linea { public string Ref, Descripcion; public decimal Precio, Dto, Base; public int Cant; }
class Presupuesto { public string Ref, Cliente, Estado; public DateTime Validez; public List<Linea> Lineas; public decimal Base, Iva, Total; }

class Program
{
    const decimal IVA = 0.13m;
    const int MIN_MAYORISTA = 24;                          // cantidad mínima para precio mayorista
    const decimal DTO_MAYORISTA = 10m;                     // % de descuento mayorista
    static DateTime hoy = new DateTime(2026, 10, 19);      // fecha fija para que las pruebas den siempre igual
    static List<Producto> productos = new List<Producto>();
    static List<string> clientes = new List<string>();
    static int siguiente = 65;                             // el próximo presupuesto será COT-0065

    static void Main()
    {
        System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Cargar();
        string op;
        do
        {
            Console.WriteLine("\n=== PROGRAMA 5: PRESUPUESTO ===\n1. Generar presupuesto\n2. Ver productos y clientes\n3. Pruebas\n0. Salir");
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

    static decimal R(decimal valor) { return Math.Round(valor, 2, MidpointRounding.AwayFromZero); }

    static Producto Buscar(string rf)
    {
        foreach (Producto p in productos)
            if (p.Ref == rf.ToUpper()) return p;
        return null;
    }

    // ---------- Descuento: el mayor entre el manual y el mayorista ----------
    static decimal DescuentoPct(int cantidad, decimal manual)
    {
        if (cantidad >= MIN_MAYORISTA && manual < DTO_MAYORISTA) return DTO_MAYORISTA;
        return manual;
    }

    // Si el producto ya está en la lista, se suma la cantidad en vez de crear otra línea
    static void Agregar(List<Linea> lineas, Producto p, int cantidad)
    {
        foreach (Linea l in lineas)
            if (l.Ref == p.Ref) { l.Cant += cantidad; return; }
        Linea nueva = new Linea();
        nueva.Ref = p.Ref; nueva.Descripcion = p.Nombre; nueva.Precio = p.Precio; nueva.Cant = cantidad;
        lineas.Add(nueva);
    }

    // ---------- Función principal: calcula líneas, base, IVA y total. Devuelve null si falta cliente o productos ----------
    static Presupuesto Generar(string cliente, List<Linea> lineas, decimal manual)
    {
        if (!clientes.Contains(cliente) || lineas.Count == 0) return null;
        if (manual > 100) manual = 100;

        Presupuesto q = new Presupuesto();
        q.Ref = "COT-" + siguiente.ToString("0000");
        siguiente++;
        q.Cliente = cliente; q.Validez = hoy.AddDays(15); q.Lineas = lineas;
        q.Estado = "Validado (presupuesto abierto)";
        foreach (Linea l in lineas)
        {
            l.Dto = DescuentoPct(l.Cant, manual);
            l.Base = R(l.Precio * l.Cant * (1 - l.Dto / 100m));      // precio x cantidad menos el descuento
            q.Base += l.Base;
        }
        q.Iva = R(q.Base * IVA);
        q.Total = q.Base + q.Iva;
        return q;
    }

    static void Crear()
    {
        string cliente = Leer("Cliente (nombre exacto)");
        List<Linea> lineas = new List<Linea>();
        while (true)
        {
            string rf = Leer("Referencia del producto (Enter = terminar)");
            if (rf == "") break;
            Producto p = Buscar(rf);
            if (p == null) { Console.WriteLine("  No existe ese producto."); continue; }
            Agregar(lineas, p, (int)LeerNum("Cantidad"));
        }
        decimal manual = LeerNum("Descuento manual % (0 si no hay)");

        Presupuesto q = Generar(cliente, lineas, manual);
        if (q == null) { Console.WriteLine("  No se pudo generar: cliente inexistente o sin productos."); return; }
        Console.WriteLine("\n=== PRESUPUESTO " + q.Ref + " === Cliente: " + q.Cliente + " | Válido hasta: " + q.Validez.ToString("yyyy-MM-dd") + " | " + q.Estado);
        foreach (Linea l in q.Lineas)
            Console.WriteLine("  " + l.Ref + " " + l.Descripcion + " | " + l.Precio.ToString("0.00") + " x " + l.Cant + " | dto " + l.Dto + "% | " + l.Base.ToString("0.00"));
        Console.WriteLine("  Base: " + q.Base.ToString("0.00") + " | IVA 13%: " + q.Iva.ToString("0.00") + " | TOTAL: " + q.Total.ToString("0.00") + " Bs");
    }

    static void Ver()
    {
        foreach (Producto p in productos) Console.WriteLine("  " + p.Ref + " | " + p.Nombre + " | " + p.Precio.ToString("0.00"));
        foreach (string c in clientes) Console.WriteLine("  Cliente: " + c);
    }

    // ---------- Datos de ejemplo (de las capturas de Dolibarr) y pruebas ----------
    static void Nuevo(string rf, string nombre, decimal precio)
    {
        Producto p = new Producto();
        p.Ref = rf; p.Nombre = nombre; p.Precio = precio;
        productos.Add(p);
    }

    static void Cargar()
    {
        productos.Clear();
        clientes.Clear();
        siguiente = 65;
        Nuevo("BEB-003", "Kéfir natural 1 L", 13.27m);
        Nuevo("LEC-011", "Leche frutilla 1 L", 8.85m);
        Nuevo("YOG-029", "Yogur bebible maracuyá 1 L", 10.62m);
        Nuevo("LEC-002", "Leche entera 2 L", 11.95m);
        clientes.Add("Mark Flores");
        clientes.Add("Guardería Pequeños Pasos");
    }

    static void Ok(string nombre, string esperado, string obtenido)
    {
        Console.WriteLine(esperado == obtenido ? "  [OK]    " + nombre + " -> " + obtenido
                                               : "  [FALLA] " + nombre + " -> esperado " + esperado + ", obtenido " + obtenido);
    }

    static void Pruebas()
    {
        Cargar();
        Console.WriteLine("\n--- PRUEBAS: DESCUENTOS Y PRESUPUESTO ---");
        Ok("Descuento con 23 u.", "0", DescuentoPct(23, 0m).ToString("0.##"));
        Ok("Descuento con 24 u. (mayorista)", "10", DescuentoPct(24, 0m).ToString("0.##"));
        Ok("Manual 15% con 5 u.", "15", DescuentoPct(5, 15m).ToString("0.##"));
        Ok("Manual 5% con 30 u. (gana el mayorista)", "10", DescuentoPct(30, 5m).ToString("0.##"));
        Ok("Manual 15% con 30 u. (gana el manual)", "15", DescuentoPct(30, 15m).ToString("0.##"));

        // Presupuesto de la Guardería Pequeños Pasos: las 3 líneas son mayoristas
        List<Linea> lc = new List<Linea>();
        Agregar(lc, Buscar("LEC-011"), 36);
        Agregar(lc, Buscar("YOG-029"), 100);
        Agregar(lc, Buscar("LEC-002"), 24);
        Presupuesto q = Generar("Guardería Pequeños Pasos", lc, 0m);
        Ok("Presupuesto: referencia", "COT-0065", q.Ref);
        Ok("Presupuesto: base imponible", "1500.66", q.Base.ToString("0.00"));
        Ok("Presupuesto: IVA", "195.09", q.Iva.ToString("0.00"));
        Ok("Presupuesto: total", "1695.75", q.Total.ToString("0.00"));
        Ok("Presupuesto: válido hasta", "2026-11-03", q.Validez.ToString("yyyy-MM-dd"));

        // Sin descuento, y con el producto repetido (se suma en una sola línea)
        List<Linea> l2 = new List<Linea>();
        Agregar(l2, Buscar("BEB-003"), 1);
        Agregar(l2, Buscar("BEB-003"), 1);
        Presupuesto q2 = Generar("Mark Flores", l2, 0m);
        Ok("Repetidos: una sola línea", "1", q2.Lineas.Count.ToString());
        Ok("Sin descuento: base", "26.54", q2.Base.ToString("0.00"));
        Ok("Sin descuento: total", "29.99", q2.Total.ToString("0.00"));
        Ok("Cliente que no existe", "True", (Generar("Nadie", l2, 0m) == null).ToString());
        Ok("Sin productos", "True", (Generar("Mark Flores", new List<Linea>(), 0m) == null).ToString());
        Cargar();
    }
}
