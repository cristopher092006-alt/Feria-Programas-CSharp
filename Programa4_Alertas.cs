// PROGRAMA 4 - ALERTAS DE STOCK Y VENCIMIENTO (módulo Productos) | Programación Estructurada, tarjeta PE-01
// Avisa los productos con stock de 5 o menos, y los lotes que vencen en 5 días o menos (o que ya vencieron).
// Uso: Visual Studio > proyecto nuevo de consola > pegar este archivo en Program.cs > Ctrl+F5.
//      En el menú, la opción 3 ejecuta las pruebas: todas deben salir [OK].
using System;
using System.Collections.Generic;

class Lote { public int Cantidad; public DateTime Vence; }
class Producto { public string Ref; public List<Lote> Lotes = new List<Lote>(); }

class Program
{
    const int STOCK_MIN = 5;                               // alerta si el stock es 5 o menos
    const int DIAS_ALERTA = 5;                             // alerta si vence en 5 días o menos
    static DateTime hoy = new DateTime(2026, 10, 19);      // fecha fija para que las pruebas den siempre igual
    static DateTime NUNCA = DateTime.MaxValue;             // lote que no vence
    static List<Producto> productos = new List<Producto>();

    static void Main()
    {
        System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Cargar();
        string op;
        do
        {
            Console.WriteLine("\n=== PROGRAMA 4: ALERTAS ===  (fecha del sistema: " + hoy.ToString("yyyy-MM-dd") + ")");
            Console.WriteLine("1. Ver alertas\n2. Simular que pasan días\n3. Pruebas\n0. Salir");
            op = Leer("Opción");
            switch (op)
            {
                case "1": Alertas(true); break;
                case "2": Simular(); break;
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

    static int Total(Producto p)
    {
        int suma = 0;
        foreach (Lote l in p.Lotes) suma += l.Cantidad;
        return suma;
    }

    // ---------- Reglas de alerta ----------
    static bool StockBajo(int stock) { return stock <= STOCK_MIN; }

    static bool PorVencer(DateTime vence) { return vence != NUNCA && (vence - hoy).Days <= DIAS_ALERTA; }

    // ---------- Función principal: recorre productos y lotes. Devuelve cuántas alertas hay ----------
    static int Alertas(bool imprimir)
    {
        int total = 0;
        foreach (Producto p in productos)
        {
            int stock = Total(p);
            if (StockBajo(stock))
            {
                total++;
                if (imprimir) Console.WriteLine("  [STOCK BAJO] " + p.Ref + ": " + stock + "/" + STOCK_MIN);
            }
            foreach (Lote l in p.Lotes)
            {
                if (PorVencer(l.Vence))
                {
                    total++;
                    int dias = (l.Vence - hoy).Days;
                    if (imprimir) Console.WriteLine("  [VENCIMIENTO] " + p.Ref + ", lote de " + l.Cantidad + " u.: " + (dias < 0 ? "YA VENCIÓ" : "vence en " + dias + " día(s)"));
                }
            }
        }
        if (imprimir && total == 0) Console.WriteLine("  Sin alertas.");
        return total;
    }

    static void Simular()
    {
        hoy = hoy.AddDays((int)LeerNum("¿Cuántos días pasan?"));
        Console.WriteLine("  Fecha del sistema ahora: " + hoy.ToString("yyyy-MM-dd"));
        Alertas(true);
    }

    // ---------- Datos de ejemplo y pruebas ----------
    static void Nuevo(string rf, int stock)
    {
        Producto p = new Producto();
        p.Ref = rf;
        if (stock > 0) AgregarLote(p, stock, NUNCA);
        productos.Add(p);
    }

    static void AgregarLote(Producto p, int cantidad, DateTime vence)
    {
        Lote l = new Lote();
        l.Cantidad = cantidad; l.Vence = vence;
        p.Lotes.Add(l);
    }

    static void Cargar()
    {
        hoy = new DateTime(2026, 10, 19);
        productos.Clear();
        Nuevo("BEB-001", 0);                               // dos lotes: el primero vence en 4 días
        AgregarLote(productos[0], 5, hoy.AddDays(4));
        AgregarLote(productos[0], 10, new DateTime(2026, 11, 10));
        Nuevo("BEB-002", 12);
        Nuevo("YOG-024", 28);
        Nuevo("HEL-006", 2);                               // stock bajo
        Nuevo("LEC-013", 0);                               // stock bajo (agotado)
        Nuevo("YOG-009", 2);                               // stock bajo
    }

    static void Ok(string nombre, string esperado, string obtenido)
    {
        Console.WriteLine(esperado == obtenido ? "  [OK]    " + nombre + " -> " + obtenido
                                               : "  [FALLA] " + nombre + " -> esperado " + esperado + ", obtenido " + obtenido);
    }

    static void Pruebas()
    {
        Cargar();
        Console.WriteLine("\n--- PRUEBAS: ALERTAS ---");
        Ok("Alertas con los datos de ejemplo (3 de stock + 1 de vencimiento)", "4", Alertas(true).ToString());
        Ok("Stock 5 -> alerta", "True", StockBajo(5).ToString());
        Ok("Stock 6 -> sin alerta", "False", StockBajo(6).ToString());
        Ok("Vence en 5 días -> alerta", "True", PorVencer(hoy.AddDays(5)).ToString());
        Ok("Vence en 6 días -> sin alerta", "False", PorVencer(hoy.AddDays(6)).ToString());
        Ok("Venció ayer -> alerta", "True", PorVencer(hoy.AddDays(-1)).ToString());
        Ok("No vence nunca -> sin alerta", "False", PorVencer(NUNCA).ToString());
        hoy = hoy.AddDays(18);                             // pasan 18 días: un lote vencido y otro por vencer
        Ok("Alertas 18 días después", "5", Alertas(false).ToString());
        Cargar();
    }
}
