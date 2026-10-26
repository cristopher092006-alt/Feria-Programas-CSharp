// PROGRAMA 6 - RESERVAS DE STOCK (módulo Productos) | Programación Estructurada, tarjeta PE-01
// Reservar aparta unidades para un cliente sin venderlas. Disponible = stock - reservado.
// Cancelar una reserva devuelve las unidades al stock disponible.
// Uso: Visual Studio > proyecto nuevo de consola > pegar este archivo en Program.cs > Ctrl+F5.
//      En el menú, la opción 4 ejecuta las pruebas: todas deben salir [OK].
using System;
using System.Collections.Generic;

class Producto { public string Ref; public int Stock, Reservado; }
class Reserva { public int Id, Cant; public string Ref, Cliente; public bool Activa = true; }

class Program
{
    static List<Producto> productos = new List<Producto>();
    static List<Reserva> reservas = new List<Reserva>();
    static int siguiente = 1;

    static void Main()
    {
        System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Cargar();
        string op;
        do
        {
            Console.WriteLine("\n=== PROGRAMA 6: RESERVAS ===\n1. Crear reserva\n2. Cancelar reserva\n3. Ver stock y reservas\n4. Pruebas\n0. Salir");
            op = Leer("Opción");
            switch (op)
            {
                case "1": MenuReservar(); break;
                case "2": MenuCancelar(); break;
                case "3": Ver(); break;
                case "4": Pruebas(); break;
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

    static int Disponible(Producto p) { return p.Stock - p.Reservado; }

    // ---------- Reservar: devuelve null si pide más de lo disponible ----------
    static Reserva Reservar(Producto p, int cantidad, string cliente)
    {
        if (cantidad <= 0 || cantidad > Disponible(p)) return null;
        p.Reservado += cantidad;
        Reserva r = new Reserva();
        r.Id = siguiente; r.Ref = p.Ref; r.Cliente = cliente; r.Cant = cantidad;
        siguiente++;
        reservas.Add(r);
        return r;
    }

    // ---------- Cancelar: devuelve el stock al disponible. false si no existe o ya estaba cancelada ----------
    static bool Cancelar(int id)
    {
        foreach (Reserva r in reservas)
        {
            if (r.Id == id && r.Activa)
            {
                Buscar(r.Ref).Reservado -= r.Cant;
                r.Activa = false;
                return true;
            }
        }
        return false;
    }

    static int Activas()
    {
        int n = 0;
        foreach (Reserva r in reservas)
            if (r.Activa) n++;
        return n;
    }

    static void MenuReservar()
    {
        Producto p = Buscar(Leer("Referencia"));
        if (p == null) { Console.WriteLine("  Producto no encontrado."); return; }
        Console.WriteLine("  Disponible: " + Disponible(p));
        int cantidad = (int)LeerNum("Cantidad a reservar");
        Reserva r = Reservar(p, cantidad, Leer("Nombre del cliente"));
        Console.WriteLine(r == null ? "  No hay stock disponible suficiente." : "  Reserva N° " + r.Id + " creada. Disponible ahora: " + Disponible(p));
    }

    static void MenuCancelar()
    {
        bool cancelada = Cancelar((int)LeerNum("N° de reserva"));
        Console.WriteLine(cancelada ? "  Reserva cancelada: el stock vuelve a estar disponible." : "  Esa reserva no existe o ya estaba cancelada.");
    }

    static void Ver()
    {
        foreach (Producto p in productos)
            Console.WriteLine("  " + p.Ref + " | stock " + p.Stock + " | reservado " + p.Reservado + " | disponible " + Disponible(p));
        foreach (Reserva r in reservas)
            Console.WriteLine("  N° " + r.Id + " | " + r.Ref + " x" + r.Cant + " | " + r.Cliente + " | " + (r.Activa ? "Activa" : "Cancelada"));
    }

    // ---------- Datos de ejemplo y pruebas ----------
    static void Nuevo(string rf, int stock)
    {
        Producto p = new Producto();
        p.Ref = rf; p.Stock = stock;
        productos.Add(p);
    }

    static void Cargar()
    {
        productos.Clear();
        reservas.Clear();
        siguiente = 1;
        Nuevo("YOG-024", 28);
        Nuevo("BEB-002", 12);
    }

    static void Ok(string nombre, string esperado, string obtenido)
    {
        Console.WriteLine(esperado == obtenido ? "  [OK]    " + nombre + " -> " + obtenido
                                               : "  [FALLA] " + nombre + " -> esperado " + esperado + ", obtenido " + obtenido);
    }

    static void Pruebas()
    {
        Cargar();
        Console.WriteLine("\n--- PRUEBAS: RESERVAS ---");
        Producto y = Buscar("YOG-024");
        Ok("Disponible al inicio", "28", Disponible(y).ToString());
        Reserva r = Reservar(y, 10, "Mark Flores");
        Ok("N° de la primera reserva", "1", r == null ? "null" : r.Id.ToString());
        Ok("Disponible tras reservar 10", "18", Disponible(y).ToString());
        Ok("El stock total no cambia", "28", y.Stock.ToString());
        Ok("Cancelar la reserva", "True", Cancelar(1).ToString());
        Ok("Disponible tras cancelar", "28", Disponible(y).ToString());
        Ok("Cancelar otra vez", "False", Cancelar(1).ToString());
        Ok("Reservar más que el stock", "True", (Reservar(y, 999, "X") == null).ToString());
        Reservar(y, 28, "Y");
        Ok("Reservar todo el stock", "0", Disponible(y).ToString());
        Ok("Reservar 1 más", "True", (Reservar(y, 1, "Z") == null).ToString());
        Ok("Cancelar una reserva que no existe", "False", Cancelar(99).ToString());
        Ok("Reservas activas", "1", Activas().ToString());
        Cargar();
    }
}
