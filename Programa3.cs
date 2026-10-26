// PROGRAMA 3 - ENTRADAS Y SALIDAS DE STOCK, PEPS (módulo Productos) | Programación Estructurada, tarjeta PE-01
// Entrada: cada ingreso de mercadería crea un LOTE (cantidad, fecha de ingreso, fecha de vencimiento).
// Salida (venta, pérdida o daño): se descuenta primero del lote más antiguo (PEPS = primero en entrar, primero en salir).
// Uso: Visual Studio > proyecto nuevo de consola > pegar este archivo en Program.cs > Ctrl+F5.
//      En el menú, la opción 5 ejecuta las pruebas: todas deben salir [OK].
using System;
using System.Collections.Generic;

class Lote { public int Cantidad; public DateTime Ingreso, Vence; }
class Producto { public string Ref; public List<Lote> Lotes = new List<Lote>(); }      // el lote más antiguo es el primero

class Program
{
    static DateTime hoy = new DateTime(2026, 10, 19);      // fecha fija para que las pruebas den siempre igual
    static DateTime NUNCA = DateTime.MaxValue;             // lote que no vence
    static List<Producto> productos = new List<Producto>();
    static List<string> movimientos = new List<string>();

    static void Main()
    {
        System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Cargar();
        string op;
        do
        {
            Console.WriteLine("\n=== PROGRAMA 3: ENTRADAS Y SALIDAS (PEPS) ===\n1. Entrada de stock\n2. Salida de stock\n3. Ver stock y lotes\n4. Ver movimientos\n5. Pruebas\n0. Salir");
            op = Leer("Opción");
            switch (op)
            {
                case "1": MenuEntrada(); break;
                case "2": MenuSalida(); break;
                case "3": Ver(); break;
                case "4": foreach (string m in movimientos) Console.WriteLine("  " + m); break;
                case "5": Pruebas(); break;
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

    static Producto Buscar(string rf)
    {
        foreach (Producto p in productos)
            if (p.Ref == rf.ToUpper()) return p;
        return null;
    }

    static int Total(Producto p)
    {
        int suma = 0;
        foreach (Lote l in p.Lotes) suma += l.Cantidad;
        return suma;
    }

    static void AgregarLote(Producto p, int cantidad, DateTime vence)
    {
        Lote l = new Lote();
        l.Cantidad = cantidad; l.Ingreso = hoy; l.Vence = vence;
        p.Lotes.Add(l);                                     // se agrega al final: el más antiguo queda primero
    }

    // ---------- Entrada: nuevo lote ----------
    static void Entrada(Producto p, int cantidad, DateTime vence)
    {
        if (cantidad <= 0) return;
        AgregarLote(p, cantidad, vence);
        movimientos.Add(hoy.ToString("yyyy-MM-dd") + " | " + p.Ref + " | +" + cantidad + " | Entrada");
    }

    // ---------- Función principal: salida PEPS. Devuelve false si pide más de lo que hay ----------
    static bool Salida(Producto p, int cantidad, string motivo)
    {
        if (cantidad <= 0 || cantidad > Total(p)) return false;
        int falta = cantidad;
        while (falta > 0)
        {
            Lote l = p.Lotes[0];                            // siempre el más antiguo
            if (l.Cantidad <= falta) { falta -= l.Cantidad; p.Lotes.RemoveAt(0); }     // se consume todo el lote
            else { l.Cantidad -= falta; falta = 0; }                                    // se consume una parte
        }
        movimientos.Add(hoy.ToString("yyyy-MM-dd") + " | " + p.Ref + " | -" + cantidad + " | " + motivo);
        return true;
    }

    static void MenuEntrada()
    {
        Producto p = Buscar(Leer("Referencia"));
        if (p == null) { Console.WriteLine("  Producto no encontrado."); return; }
        int cantidad = (int)LeerNum("Cantidad que ingresa");
        int dias = (int)LeerNum("Días hasta el vencimiento (0 = no vence)");
        Entrada(p, cantidad, dias == 0 ? NUNCA : hoy.AddDays(dias));
        Console.WriteLine("  Stock total: " + Total(p));
    }

    static void MenuSalida()
    {
        Producto p = Buscar(Leer("Referencia"));
        if (p == null) { Console.WriteLine("  Producto no encontrado."); return; }
        int cantidad = (int)LeerNum("Cantidad que sale");
        string motivo = Leer("Motivo (venta, pérdida, daño)");
        Console.WriteLine(Salida(p, cantidad, motivo) ? "  Salida registrada. Stock total: " + Total(p) : "  No hay stock suficiente.");
    }

    static void Ver()
    {
        foreach (Producto p in productos)
        {
            Console.WriteLine("  " + p.Ref + " | stock total: " + Total(p));
            foreach (Lote l in p.Lotes)
                Console.WriteLine("      lote de " + l.Cantidad + " u. | ingreso " + l.Ingreso.ToString("yyyy-MM-dd") + " | " + (l.Vence == NUNCA ? "no vence" : "vence " + l.Vence.ToString("yyyy-MM-dd")));
        }
    }

    // ---------- Datos de ejemplo y pruebas ----------
    static void Cargar()
    {
        productos.Clear();
        movimientos.Clear();
        Producto a = new Producto(); a.Ref = "BEB-001"; productos.Add(a);        // dos lotes: primero entró el de 5, después el de 10
        AgregarLote(a, 5, hoy.AddDays(4));
        AgregarLote(a, 10, new DateTime(2026, 11, 10));
        Producto b = new Producto(); b.Ref = "BEB-002"; productos.Add(b);
        AgregarLote(b, 12, NUNCA);
    }

    static void Ok(string nombre, string esperado, string obtenido)
    {
        Console.WriteLine(esperado == obtenido ? "  [OK]    " + nombre + " -> " + obtenido
                                               : "  [FALLA] " + nombre + " -> esperado " + esperado + ", obtenido " + obtenido);
    }

    static void Pruebas()
    {
        Cargar();
        Console.WriteLine("\n--- PRUEBAS: ENTRADAS Y SALIDAS PEPS ---");
        Producto a = Buscar("BEB-001");                     // lotes de 5 y 10
        Ok("Stock inicial de BEB-001", "15", Total(a).ToString());
        Salida(a, 7, "Prueba");
        Ok("PEPS: lotes que quedan", "1", a.Lotes.Count.ToString());
        Ok("PEPS: cantidad del lote restante", "8", a.Lotes[0].Cantidad.ToString());
        Ok("PEPS: vence el lote restante", "2026-11-10", a.Lotes[0].Vence.ToString("yyyy-MM-dd"));
        Ok("Salida de 99 (más que el stock)", "False", Salida(a, 99, "Prueba").ToString());
        Ok("El stock no cambió", "8", Total(a).ToString());

        Producto b = Buscar("BEB-002");                     // un lote de 12
        Entrada(b, 20, new DateTime(2026, 12, 1));
        Ok("Entrada: stock total de BEB-002", "32", Total(b).ToString());
        Salida(b, 12, "Daño");
        Ok("Salida de un lote completo: lotes", "1", b.Lotes.Count.ToString());
        Ok("Queda el lote nuevo de", "20", b.Lotes[0].Cantidad.ToString());
        Ok("Movimientos registrados", "3", movimientos.Count.ToString());
        Cargar();
    }
}
