using Task4.LinqQueries;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var catalog = new List<ProductOrder>
{
    new(1, "Ноутбук ProBook 15", "Електроніка", 32500m, ["робота", "потужний"]),
    new(2, "Бездротова миша", "Електроніка", 1200m, ["аксесуар", "офіс"]),
    new(3, "Офісне крісло Ergo", "Меблі", 8400m, ["офіс", "комфорт"]),
    new(4, "Настільна лампа LED", "Освітлення", 950m, ["офіс", "економія"]),
    new(5, "Монітор 27\" IPS", "Електроніка", 11800m, ["робота", "дизайн"]),
    new(6, "Клавіатура механічна", "Електроніка", 3600m, ["аксесуар", "робота"]),
    new(7, "Стіл комп'ютерний", "Меблі", 6200m, ["офіс", "меблі"]),
    new(8, "Павербанк 20000 мАг", "Електроніка", 2100m, ["мобільність", "аксесуар"])
};

Console.WriteLine("=== 1. LINQ: вбудовані лямбди vs змінні Func ===");
// Варіант А: лямбда-вирази безпосередньо у виклику
var queryLambda = catalog
    .Where(p => p.Price > 5000m)
    .OrderBy(p => p.Category)
    .ThenByDescending(p => p.Price)
    .Select(p => $"{p.Title} ({p.Category}) — {p.Price:F2} грн");

// Варіант Б: явні делегати Func у змінних
Func<ProductOrder, bool> pricePredicate = p => p.Price > 5000m;
Func<ProductOrder, string> titleSelector = p => $"{p.Title} ({p.Category}) — {p.Price:F2} грн";
var queryExplicitFuncs = catalog.Where(pricePredicate).Select(titleSelector);

Console.WriteLine($"Збіг кількості елементів (лямбди vs Func): {queryLambda.Count() == queryExplicitFuncs.Count()}");
foreach (var line in queryLambda) Console.WriteLine("  " + line);

Console.WriteLine("\n=== 2. SelectMany (унікальні теги), агрегація та GroupBy ===");
var distinctTags = catalog.SelectMany(p => p.Tags).Distinct().OrderBy(t => t);
Console.WriteLine($"Унікальні теги (SelectMany): {string.Join(", ", distinctTags)}");
Console.WriteLine($"Кількість товарів дорожчих за 3000 грн: {catalog.Count(p => p.Price > 3000m)}");
Console.WriteLine($"Середня ціна по каталогу: {catalog.Average(p => p.Price):F2} грн");

var groupedByCategory = catalog.GroupBy(p => p.Category);
foreach (var group in groupedByCategory)
{
    Console.WriteLine($"  Категорія '{group.Key}': {group.Count()} шт., сума = {group.Sum(x => x.Price):F2} грн");
}

Console.WriteLine("\n=== 3. Відкладене виконання (IEnumerable) vs матеріалізація (ToList) ===");
var deferredQuery = catalog
    .Where(p => p.Category == "Меблі")
    .Select(p => p.Title);

List<string> materializedList = deferredQuery.ToList();

// Додаємо новий елемент у вихідну колекцію ПІСЛЯ створення запиту та матеріалізації
catalog.Add(new ProductOrder(9, "Тумба офісна", "Меблі", 3100m, ["офіс"]));

Console.WriteLine($"Повторний перебір deferredQuery (IEnumerable): {string.Join(", ", deferredQuery)}");
Console.Write("Обробка materializedList через Action<string>: ");
Action<string> printItem = title => Console.Write($"[{title}] ");
materializedList.ForEach(printItem);
Console.WriteLine();

namespace Task4.LinqQueries
{
    public record ProductOrder(int Id, string Title, string Category, decimal Price, List<string> Tags);
}