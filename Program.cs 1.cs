// PROGRAMA 1 - CREAR CLIENTE (módulo Terceros) | Programación Estructurada, tarjeta PE-01
// Pide los datos de un cliente, los valida y lo guarda con código automático (CU2610-00104, 00105...).
// Uso: Visual Studio > proyecto nuevo de consola > pegar este archivo en Program.cs > Ctrl+F5.
//      En el menú, la opción 3 ejecuta las pruebas: todas deben salir [OK].
using System;
using System.Collections.Generic;

class Tercero { public string Codigo, Nombre, Telefono, Email, Direccion; }

class Program
{
    static List<Tercero> clientes = new List<Tercero>();
    static int siguiente = 104;                        // el próximo código será CU2610-00104

    static void Main()
    {
        System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Cargar();
        string op;
        do
        {
            Console.WriteLine("\n=== PROGRAMA 1: CREAR CLIENTE ===\n1. Crear cliente\n2. Ver clientes\n3. Pruebas\n0. Salir");
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

    // ---------- Validaciones ----------
    static bool TelefonoValido(string t)
    {
        if (t.Length < 7 || t.Length > 15) return false;
        for (int i = 0; i < t.Length; i++)
            if (!((t[i] >= '0' && t[i] <= '9') || (i == 0 && t[i] == '+'))) return false;
        return true;
    }

    static bool EmailValido(string e) { return e == "" || (e.Contains("@") && e.Contains(".")); }

    static Tercero Buscar(string nombre)
    {
        foreach (Tercero t in clientes)
            if (t.Nombre.ToLower() == nombre.ToLower()) return t;
        return null;
    }

    // ---------- Función principal: valida todo y guarda. Devuelve null (y el motivo en error) si falla ----------
    static Tercero Registrar(string nombre, string tel, string email, string dir, out string error)
    {
        error = "";
        if (nombre == "") error = "El nombre es obligatorio.";
        else if (Buscar(nombre) != null) error = "Ya existe ese cliente.";
        else if (!TelefonoValido(tel)) error = "Teléfono no válido.";
        else if (!EmailValido(email)) error = "Email no válido.";
        if (error != "") return null;

        Tercero t = new Tercero();
        t.Codigo = "CU2610-" + siguiente.ToString("00000");
        siguiente++;
        t.Nombre = nombre; t.Telefono = tel; t.Email = email; t.Direccion = dir;
        clientes.Add(t);
        return t;
    }

    // ---------- Pide los datos por teclado ----------
    static void Crear()
    {
        string nombre = Leer("Nombre");
        if (nombre == "" || Buscar(nombre) != null) { Console.WriteLine("  Nombre vacío o repetido."); return; }
        string tel = Leer("Teléfono (7 a 15 dígitos)");
        while (!TelefonoValido(tel)) tel = Leer("  Teléfono no válido, de nuevo");
        string email = Leer("Email (Enter = omitir)");
        while (!EmailValido(email)) email = Leer("  Email no válido, de nuevo");
        string dir = Leer("Dirección (Enter = omitir)");

        string error;
        Tercero t = Registrar(nombre, tel, email, dir, out error);
        Console.WriteLine(t == null ? "  Error: " + error : "  Cliente creado: " + t.Codigo + " - " + t.Nombre);
    }

    static void Ver()
    {
        foreach (Tercero t in clientes)
            Console.WriteLine("  " + t.Codigo + " | " + t.Nombre + " | " + t.Telefono);
    }

    // ---------- Datos de ejemplo (de las capturas de Dolibarr) y pruebas ----------
    static void Nuevo(string codigo, string nombre, string tel)
    {
        Tercero t = new Tercero();
        t.Codigo = codigo; t.Nombre = nombre; t.Telefono = tel; t.Email = ""; t.Direccion = "";
        clientes.Add(t);
    }

    static void Cargar()
    {
        clientes.Clear();
        siguiente = 104;
        Nuevo("CU2610-00001", "Cliente genérico TakePOS", "");
        Nuevo("CU2610-00102", "Cliente Prueba Web", "70000000");
        Nuevo("CU2610-00103", "Mark Flores", "+59161680120");
        Nuevo("CU2610-00050", "Guardería Pequeños Pasos", "");
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
        Console.WriteLine("\n--- PRUEBAS: CREAR CLIENTE ---");
        Ok("Teléfono +59161680120", "True", TelefonoValido("+59161680120").ToString());
        Ok("Teléfono con letras", "False", TelefonoValido("7000abc1").ToString());
        Ok("Teléfono muy corto", "False", TelefonoValido("123").ToString());
        Ok("Email vacío (opcional)", "True", EmailValido("").ToString());
        Ok("Email sin @", "False", EmailValido("anacorreo.com").ToString());

        Tercero t = Registrar("Ana Rojas", "70012345", "ana@correo.com", "", out e);
        Ok("Código del cliente nuevo", "CU2610-00104", t == null ? "null" : t.Codigo);
        Ok("Cantidad de clientes", "5", clientes.Count.ToString());
        Ok("Nombre repetido (otras mayúsculas)", "True", (Registrar("ana rojas", "70099999", "", "", out e) == null).ToString());
        Ok("Teléfono inválido no crea", "True", (Registrar("Luis Pérez", "12", "", "", out e) == null).ToString());
        Ok("Email inválido no crea", "True", (Registrar("Luis Pérez", "70011122", "luis.com", "", out e) == null).ToString());
        Ok("Cantidad de clientes (igual)", "5", clientes.Count.ToString());
        Tercero t2 = Registrar("Luis Pérez", "70011122", "", "", out e);
        Ok("Siguiente código", "CU2610-00105", t2 == null ? "null" : t2.Codigo);
        Cargar();
    }
}
