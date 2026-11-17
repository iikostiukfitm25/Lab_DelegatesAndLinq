using Task3.StandardDelegates;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var orders = new List<OrderItem>
{
    new(1, "Ноутбук ProBook 15", "Електроніка", 32500m, true),
    new(2, "Бездротова миша", "Електроніка", 1200m, false),
    new(3, "Офісне крісло Ergo", "Меблі", 8400m, true),
    new(4, "Настільна лампа LED", "Освітлення", 950m, false),
    new(5, "Монітор 27\" IPS", "Електроніка", 11800m, true),
    new(6, "Клавіатура механічна", "Електроніка", 3600m, false),
    new(7, "Стіл комп'ютерний", "Меблі", 6200m, false),
    new(8, "Павербанк 20000 мАг", "Електроніка", 2100m, true)
};

Console.WriteLine("=== 1. Фільтрація через Predicate<OrderItem> (без LINQ) ===");
Predicate<OrderItem> byPriceThreshold = o => o.Price >= 5000m;
Predicate<OrderItem> byCategoryText = o => o.Category.Equals("Меблі", StringComparison.OrdinalIgnoreCase);
Predicate<OrderItem> byCombinedLogic = o => o.IsPriority && o.Price < 10000m;

Console.WriteLine($"Товари дорожчі за 5000 грн: {FilterItems(orders, byPriceThreshold).Count} шт.");
Console.WriteLine($"Товари категорії 'Меблі': {FilterItems(orders, byCategoryText).Count} шт.");
var priorityAffordable = FilterItems(orders, byCombinedLogic);
Console.WriteLine($"Пріоритетні товари дешевші за 10000 грн ({priorityAffordable.Count} шт.):");

Console.WriteLine("\n=== 2. Перетворення через Func<OrderItem, string> ===");
Func<OrderItem, string> receiptFormatter = o =>
    $"  #{o.Id}: {o.Title} [{o.Category}] — {o.Price:F2} грн (ПДВ: {o.Price * 0.2m:F2} грн)";

foreach (var item in priorityAffordable)
{
    Console.WriteLine(receiptFormatter(item));
}

Console.WriteLine("\n=== 3. Обхід колекції через Action<OrderItem> (накопичення статистики) ===");
decimal totalRevenue = 0m;
int priorityCount = 0;

ForEachItem(orders, o =>
{
    totalRevenue += o.Price;
    if (o.IsPriority) priorityCount++;
});
Console.WriteLine($"Загальна сума всіх 8 замовлень: {totalRevenue:F2} грн | Пріоритетних: {priorityCount}");

Console.WriteLine("\n=== 4. Конвеєр перетворень ApplyPipeline<decimal> ===");
var pricingSteps = new List<Func<decimal, decimal>>
{
    price => price * 0.90m,          // Крок 1: знижка 10%
    price => price - 500m,           // Крок 2: сезонний промокод -500 грн
    price => Math.Round(price, 0)    // Крок 3: округлення до цілих гривень
};

decimal initialPrice = 11800m;
decimal finalPrice = ApplyPipeline(initialPrice, pricingSteps);
Console.WriteLine($"Початкова ціна: {initialPrice:F2} грн -> Після 3 етапів конвеєра: {finalPrice:F2} грн");

static List<T> FilterItems<T>(List<T> source, Predicate<T> match)
{
    var result = new List<T>();
    foreach (var item in source)
        if (match(item)) result.Add(item);
    return result;
}

static void ForEachItem<T>(List<T> source, Action<T> action)
{
    foreach (var item in source)
        action(item);
}

static T ApplyPipeline<T>(T initialValue, IEnumerable<Func<T, T>> steps)
{
    T current = initialValue;
    foreach (var step in steps)
        current = step(current);
    return current;
}

namespace Task3.StandardDelegates
{
    public record OrderItem(int Id, string Title, string Category, decimal Price, bool IsPriority);
}