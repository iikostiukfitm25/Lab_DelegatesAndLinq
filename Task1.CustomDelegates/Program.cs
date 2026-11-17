using Task1.CustomDelegates;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("=== 1. Три способи створення екземпляра власного делегата ===");
var calibrator = new SensorCalibrator(offset: 5.0);

// Спосіб 1: статичний метод класу
MetricTransformHandler staticHandler = TelemetryMath.WeightedLinear;
// Спосіб 2: екземплярний метод об'єкта
MetricTransformHandler instanceHandler = calibrator.ApplyOffsetScale;
// Спосіб 3: лямбда-вираз
MetricTransformHandler lambdaHandler = (v, w) => Math.Round(Math.Sqrt(v) * w, 2);

Console.WriteLine($"Статичний метод (16.0, 2): {staticHandler(16.0, 2):F2}");
Console.WriteLine($"Екземплярний метод (16.0, 2): {instanceHandler(16.0, 2):F2}");
Console.WriteLine($"Лямбда-вираз (16.0, 2): {lambdaHandler(16.0, 2):F2}");

Console.WriteLine("\n=== 2. Безпечний виклик непризначеного делегата ===");
AuditStepHandler<string>? auditChain = null;
auditChain?.Invoke("Старт", "Делегат порожній — NullReferenceException не виникає");
Console.WriteLine("Виклик auditChain?.Invoke(...) для null пройшов безпечно.");

Console.WriteLine("\n=== 3. Multicast-делегат типу void (додавання і вилучення) ===");
int eventCounter = 0;
var historyBuffer = new List<string>();

AuditStepHandler<string> consoleLogger = (stage, data) =>
    Console.WriteLine($"  [1. Console] Етап '{stage}': {data}");
AuditStepHandler<string> counterLogger = (stage, data) =>
{
    eventCounter++;
    Console.WriteLine($"  [2. Counter] Зафіксовано подію №{eventCounter}");
};
AuditStepHandler<string> bufferLogger = (stage, data) =>
{
    historyBuffer.Add($"{stage}:{data}");
    Console.WriteLine($"  [3. Buffer] Додано у буфер (всього записів: {historyBuffer.Count})");
};

auditChain += consoleLogger;
auditChain += counterLogger;
auditChain += bufferLogger;

Console.WriteLine("Виклик ланцюжка з 3 підписників:");
auditChain?.Invoke("Збір", "Пакет #101");

auditChain -= counterLogger;
Console.WriteLine("\nВиклик після вилучення середнього підписника (Counter):");
auditChain?.Invoke("Фініш", "Пакет #102");

Console.WriteLine("\n=== 4. Multicast зі значенням повернення (double) vs GetInvocationList() ===");
MetricTransformHandler multiCalc = staticHandler;
multiCalc += instanceHandler;
multiCalc += lambdaHandler;

double directResult = multiCalc(16.0, 2);
Console.WriteLine($"Прямий виклик multiCalc(16.0, 2) повернув лише останній результат: {directResult:F2}");

var allResults = new List<double>();
foreach (MetricTransformHandler handler in multiCalc.GetInvocationList())
{
    allResults.Add(handler(16.0, 2));
}
Console.WriteLine($"Усі результати через GetInvocationList(): [{string.Join("; ", allResults.Select(x => x.ToString("F2")))}]");

namespace Task1.CustomDelegates
{
    public delegate double MetricTransformHandler(double value, int weight);
    public delegate void AuditStepHandler<in T>(string stage, T payload);

    public static class TelemetryMath
    {
        public static double WeightedLinear(double value, int weight) => value * weight;
    }

    public class SensorCalibrator(double offset)
    {
        public double ApplyOffsetScale(double value, int weight) => (value * weight) + offset;
    }
}