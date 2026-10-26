// PROGRAMA 7 - CÓDIGO DE BARRAS Y FACTURA (módulo Finanzas) | Programación Estructurada, tarjeta PE-01
// Lee códigos de barras uno por uno: si el mismo código se lee otra vez, se SUMA la cantidad en la misma línea.
// Calcula base, IVA 13% y total (con precio mayorista desde 24 unidades), el cambio y descuenta el stock (PEPS).
// Si el cliente paga de menos, la factura queda "Pendiente de pago".
// Uso: Visual Studio > proyecto nuevo de consola > pegar este archivo en Program.cs > Ctrl+F5.
//      En el menú, la opción 3 ejecuta las pruebas: todas deben salir [OK].
using System;
using System.Collections.Generic;

class Producto
{
    public string Ref, Nombre, Barras;
    public decimal Precio;                                  // sin IVA
    public List<int> Lotes = new List<int>();               // cantidad de cada lote; el primero es el más antiguo
}
class Linea { public string Ref, Descripcion; public decimal Precio, Dto, Base; public int Cant; }
class Factura { public string Ref, Cliente, Estado; public List<Linea> Lineas; public decimal Base, Iva, Total, Pagado, Resta, Cambio; }

class Program
{
    const decimal IVA = 0.13m;
    const int MIN_MAYORISTA = 24;                           // cantidad mínima para precio mayorista
    const decimal DTO_MAYORISTA = 10m;                      // % de descuento mayorista
    static List<Producto> productos = new List<Producto>();
    static List<string> clientes = new List<string>();
    static int siguiente = 6;                               // la próxima factura será FA2610-0006

    static void Main()
    {
        System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Cargar();
        string op;
        do
        {
            Console.WriteLine("\n=== PROGRAMA 7: CÓDIGO DE BARRAS Y FACTURA ===\n1. Vender con lector de código de barras\n2. Ver productos y clientes\n3. Pruebas\n0. Salir");
            op = Leer("Opción");
            switch (op)
            {
                case "1": Vender(); break;
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

    static Producto Buscar(string texto)                    // por referencia o por código de barras
    {
        foreach (Producto p in productos)
            if (p.Ref == texto.ToUpper() || p.Barras == texto) return p;
        return null;
    }

    static int Total(Producto p)
    {
        int suma = 0;
        foreach (int cantidad in p.Lotes) suma += cantidad;
        return suma;
    }

    // Descuenta del lote más antiguo (PEPS)
    static void Descontar(Producto p, int cantidad)
    {
        while (cantidad > 0)
        {
            if (p.Lotes[0] <= cantidad) { cantidad -= p.Lotes[0]; p.Lotes.RemoveAt(0); }
            else { p.Lotes[0] -= cantidad; cantidad = 0; }
        }
    }

    static decimal DescuentoPct(int cantidad, decimal manual)
    {
        if (cantidad >= MIN_MAYORISTA && manual < DTO_MAYORISTA) return DTO_MAYORISTA;
        return manual;
    }

    // Si el producto ya está en la cuenta, se suma la cantidad. Devuelve la cantidad que lleva ese producto
    static int Agregar(List<Linea> lineas, Producto p, int cantidad)
    {
        foreach (Linea l in lineas)
            if (l.Ref == p.Ref) { l.Cant += cantidad; return l.Cant; }
        Linea nueva = new Linea();
        nueva.Ref = p.Ref; nueva.Descripcion = p.Nombre; nueva.Precio = p.Precio; nueva.Cant = cantidad;
        lineas.Add(nueva);
        return cantidad;
    }

    // ---------- Función principal: valida, calcula, cobra y descuenta el stock. Devuelve null (y el motivo) si falla ----------
    static Factura Emitir(string cliente, List<Linea> lineas, decimal manual, decimal pagado, out string error)
    {
        error = "";
        if (!clientes.Contains(cliente)) error = "Cliente no encontrado.";
        else if (lineas.Count == 0) error = "No hay productos.";
        foreach (Linea l in lineas)                         // 1) se revisa el stock de TODAS las líneas antes de descontar
            if (error == "" && Total(Buscar(l.Ref)) < l.Cant) error = "Stock insuficiente de " + l.Ref + ".";
        if (error != "") return null;
        if (manual > 100) manual = 100;

        Factura f = new Factura();
        f.Ref = "FA2610-" + siguiente.ToString("0000");
        siguiente++;
        f.Cliente = cliente; f.Lineas = lineas;
        foreach (Linea l in lineas)                         // 2) líneas, base y stock
        {
            l.Dto = DescuentoPct(l.Cant, manual);
            l.Base = R(l.Precio * l.Cant * (1 - l.Dto / 100m));
            f.Base += l.Base;
            Descontar(Buscar(l.Ref), l.Cant);
        }
        f.Iva = R(f.Base * IVA);                            // 3) IVA, total, pago y cambio
        f.Total = f.Base + f.Iva;
        f.Pagado = pagado > f.Total ? f.Total : pagado;
        f.Resta = f.Total - f.Pagado;
        f.Cambio = pagado - f.Pagado;
        f.Estado = f.Resta == 0 ? "Pagada" : "Pendiente de pago";
        return f;
    }

    // ---------- Lector de código de barras ----------
    static void Vender()
    {
        string cliente = Leer("Cliente (nombre exacto)");
        List<Linea> lineas = new List<Linea>();
        Console.WriteLine("Escribe o escanea el código de barras (o la referencia). FIN para terminar.");
        while (true)
        {
            string texto = Leer("Código");
            if (texto.ToUpper() == "FIN") break;
            if (texto == "") continue;
            Producto p = Buscar(texto);
            if (p == null) { Console.WriteLine("  Producto no encontrado."); continue; }
            Console.WriteLine("  + " + p.Nombre + " (lleva " + Agregar(lineas, p, 1) + ")");
        }
        decimal manual = LeerNum("Descuento manual % (0 si no hay)");
        decimal pagado = LeerNum("Monto recibido (0 si queda pendiente)");

        string error;
        Factura f = Emitir(cliente, lineas, manual, pagado, out error);
        if (f == null) { Console.WriteLine("  No se pudo emitir: " + error); return; }
        Console.WriteLine("\n=== FACTURA " + f.Ref + " === Cliente: " + f.Cliente + " | " + f.Estado);
        foreach (Linea l in f.Lineas)
            Console.WriteLine("  " + l.Descripcion + " | " + l.Precio.ToString("0.00") + " x " + l.Cant + " | dto " + l.Dto + "% | " + l.Base.ToString("0.00"));
        Console.WriteLine("  Base: " + f.Base.ToString("0.00") + " | IVA 13%: " + f.Iva.ToString("0.00") + " | TOTAL: " + f.Total.ToString("0.00"));
        Console.WriteLine("  Pagado: " + f.Pagado.ToString("0.00") + " | Resta: " + f.Resta.ToString("0.00") + " | CAMBIO: " + f.Cambio.ToString("0.00") + " Bs");
    }

    static void Ver()
    {
        foreach (Producto p in productos)
            Console.WriteLine("  " + p.Ref + " | " + p.Nombre + " | " + p.Barras + " | " + p.Precio.ToString("0.00") + " | stock " + Total(p));
        foreach (string c in clientes) Console.WriteLine("  Cliente: " + c);
    }

    // ---------- Datos de ejemplo (de las capturas de Dolibarr) y pruebas ----------
    static void Nuevo(string rf, string nombre, decimal precio, string barras, params int[] lotes)
    {
        Producto p = new Producto();
        p.Ref = rf; p.Nombre = nombre; p.Precio = precio; p.Barras = barras;
        p.Lotes.AddRange(lotes);
        productos.Add(p);
    }

    static void Cargar()
    {
        productos.Clear();
        clientes.Clear();
        siguiente = 6;
        Nuevo("BEB-001", "Chicha de maní con leche 1 L", 8.85m, "7770000000012", 5, 10);     // dos lotes: 5 y 10
        Nuevo("BEB-002", "Api con leche 1 L", 7.96m, "7770000000029", 12);
        Nuevo("BEB-003", "Kéfir natural 1 L", 13.27m, "7770000000036", 20);
        Nuevo("YOG-024", "Yogur bebible canela 1 L", 10.62m, "7770000000821", 28);
        Nuevo("YOG-029", "Yogur bebible maracuyá 1 L", 10.62m, "7770000000838", 150);
        Nuevo("HEL-006", "Paleta de leche unidad", 2.65m, "7770000000869", 2);
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
        string e;
        Console.WriteLine("\n--- PRUEBAS: CÓDIGO DE BARRAS Y FACTURA ---");
        Ok("Código de barras 7770000000036", "BEB-003", Buscar("7770000000036").Ref);
        Ok("Código que no existe", "True", (Buscar("0000000000000") == null).ToString());

        // Factura de Mark Flores: 1 chicha + 1 kéfir, paga 30 Bs
        List<Linea> a = new List<Linea>();
        Agregar(a, Buscar("7770000000012"), 1);
        Agregar(a, Buscar("7770000000036"), 1);
        Factura f = Emitir("Mark Flores", a, 0m, 30m, out e);
        Ok("Factura: referencia", "FA2610-0006", f.Ref);
        Ok("Factura: base", "22.12", f.Base.ToString("0.00"));
        Ok("Factura: IVA", "2.88", f.Iva.ToString("0.00"));
        Ok("Factura: total", "25.00", f.Total.ToString("0.00"));
        Ok("Factura: cambio", "5.00", f.Cambio.ToString("0.00"));
        Ok("Factura: estado", "Pagada", f.Estado);
        Ok("Stock de BEB-003 tras vender 1", "19", Total(Buscar("BEB-003")).ToString());
        Ok("PEPS: el lote más antiguo de BEB-001 baja a", "4", Buscar("BEB-001").Lotes[0].ToString());

        // El mismo código leído dos veces suma la cantidad; sin pago queda pendiente
        List<Linea> b = new List<Linea>();
        Agregar(b, Buscar("7770000000821"), 1);
        Agregar(b, Buscar("7770000000821"), 1);
        Ok("Repetidos: líneas", "1", b.Count.ToString());
        Ok("Repetidos: cantidad", "2", b[0].Cant.ToString());
        Factura f2 = Emitir("Mark Flores", b, 0m, 0m, out e);
        Ok("Sin pago: estado", "Pendiente de pago", f2.Estado);
        Ok("Sin pago: total", "24.00", f2.Total.ToString("0.00"));
        Ok("Sin pago: resta por pagar", "24.00", f2.Resta.ToString("0.00"));

        // Pago de menos: 1 api = 8.99, paga 5
        List<Linea> c = new List<Linea>();
        Agregar(c, Buscar("BEB-002"), 1);
        Factura f3 = Emitir("Mark Flores", c, 0m, 5m, out e);
        Ok("Pago insuficiente: resta", "3.99", f3.Resta.ToString("0.00"));
        Ok("Pago insuficiente: cambio", "0.00", f3.Cambio.ToString("0.00"));

        // Precio mayorista: 24 yogures
        List<Linea> d = new List<Linea>();
        Agregar(d, Buscar("YOG-029"), 24);
        Factura f4 = Emitir("Guardería Pequeños Pasos", d, 0m, 300m, out e);
        Ok("Mayorista: base", "229.39", f4.Base.ToString("0.00"));
        Ok("Mayorista: total", "259.21", f4.Total.ToString("0.00"));
        Ok("Mayorista: cambio de 300", "40.79", f4.Cambio.ToString("0.00"));

        // Casos que no deben facturar
        List<Linea> g = new List<Linea>();
        Agregar(g, Buscar("HEL-006"), 3);                   // solo hay 2
        Ok("Stock insuficiente: no factura", "True", (Emitir("Mark Flores", g, 0m, 10m, out e) == null).ToString());
        Ok("Stock insuficiente: mensaje", "Stock insuficiente de HEL-006.", e);
        Ok("Cliente que no existe", "True", (Emitir("Nadie", a, 0m, 0m, out e) == null).ToString());
        Ok("Sin productos", "True", (Emitir("Mark Flores", new List<Linea>(), 0m, 0m, out e) == null).ToString());
        Cargar();
    }
}
