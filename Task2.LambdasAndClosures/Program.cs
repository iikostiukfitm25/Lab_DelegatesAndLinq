Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("=== 1. Різні форми лямбда-виразів зі стандартними делегатами ===");
// Без параметрів -> Action
Action notifyStart = () => Console.WriteLine("  [Action] Конвеєр обчислень запущено.");
// Один параметр, тіло-вираз -> Func<int, int>
Func<int, int> square = x => x * x;
// Кілька параметрів, блочне тіло -> Func<double, double, string>
Func<double, double, string> formatWithTax = (price, rate) =>
{
    double total = price * (1.0 + rate);
    return $"{total:F2} грн (з ПДВ {rate * 100:F0}%)";
};

notifyStart();
Console.WriteLine($"  [Func<int, int>] square(7) = {square(7)}");
Console.WriteLine($"  [Func<double, double, string>] formatWithTax(1000, 0.20) = {formatWithTax(1000, 0.20)}");

Console.WriteLine("\n=== 2. Фабрика вищого порядку з незалежними замиканнями ===");
Func<decimal, string> CreateTaxCalculator(string region, decimal taxRate)
{
    // Параметри region та taxRate захоплюються у власний об'єкт-замикання для кожного виклику фабрики
    return amount => $"[{region}] База: {amount:F2} -> Разом: {amount * (1 + taxRate):F2}";
}

var uaTax = CreateTaxCalculator("Україна", 0.20m);
var euTax = CreateTaxCalculator("Польща", 0.23m);
Console.WriteLine("  " + uaTax(500m));
Console.WriteLine("  " + euTax(500m));

Console.WriteLine("\n=== 3. Пастка замикання в циклі та її виправлення ===");
var buggyActions = new List<Action>();
for (int i = 0; i < 4; i++)
{
    // Помилка: усі лямбди захоплюють посилання на одну й ту саму змінну i
    buggyActions.Add(() => Console.Write($"{i} "));
}
Console.Write("  Некоректний результат (спільна змінна циклу): ");
foreach (var act in buggyActions) act();

var fixedActions = new List<Action>();
for (int i = 0; i < 4; i++)
{
    int capturedCopy = i; // Локальна копія на кожній ітерації
    fixedActions.Add(() => Console.Write($"{capturedCopy} "));
}
Console.Write("\n  Виправлений результат (копія на ітерації):   ");
foreach (var act in fixedActions) act();
Console.WriteLine();

Console.WriteLine("\n=== 4. Спостереження змінного стану між викликами ===");
Func<int> CreateStatefulCounter(int start)
{
    int current = start;
    return () => ++current;
}

var counter = CreateStatefulCounter(10);
Console.WriteLine($"  Виклик 1: {counter()}, Виклик 2: {counter()}, Виклик 3: {counter()}");