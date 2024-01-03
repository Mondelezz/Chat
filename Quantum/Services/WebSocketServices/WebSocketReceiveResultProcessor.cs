using Quantum.Interfaces.WebSocketInterface;
using Quantum.Services.WebSocketServices;
using System.Buffers;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Quantum.Services.WebSocketServices
{
    public class WebSocketReceiveResultProcessor : IDisposable, IWebSocketProcessor
    {
        // Начальные данные узла
        Chunk<byte> startChunk = null;
        // Текущие данные узла
        Chunk<byte> currentChunk = null;
        // Индикатор использования пула массивов
        private bool _isUsingArrayPool;
        private readonly ILogger<WebSocketReceiveResultProcessor> _logger;

        public WebSocketReceiveResultProcessor(ILogger<WebSocketReceiveResultProcessor> logger)
        {
            _isUsingArrayPool = true;
            _logger = logger;
        }
        public bool Receive(WebSocketReceiveResult result, ArraySegment<byte> buffer, out ReadOnlySequence<byte> frame)
        {
            if (result.EndOfMessage && result.MessageType == WebSocketMessageType.Close)
            {
                frame = default;
                return false;
            }
            //  Формируем срез из текущего буффера данных, начиная с индекса 0, до длины result
            // ToArray Копирует содержимое из памяти в новый массив.
            ArraySegment<byte> slice = _isUsingArrayPool
                ? buffer.Slice(index: 0, result.Count) // Используем Slice, если используется пул массивов, представляет ArraySegment<byte>
                : buffer.Slice(index: 0, result.Count).ToArray(); // Используем ToArray, если не используется пул массивов, представляет byte[]
            _logger.Log(LogLevel.Information, $"Received frame: {Encoding.UTF8.GetString(slice.ToArray(), 0, result.Count)}");

            if (startChunk == null)
            {
                // Создаём новый объект Chunk и присваиваем этот объект начальному узлу startChunk и текущему узлу currentChunk
                startChunk = currentChunk = new Chunk<byte>(slice);
            }
            else
            {
                // Если startChunk уже инициализирован, создаем новый сегмент с данными из slice, теперь указывать будет на новый сегмент
                currentChunk = currentChunk.Add(slice);
            }
            // Сообщение не завершено и начальный узел не null
            if (!result.EndOfMessage && startChunk != null)
            {

                // Если следующего сегмента не существует, то:
                if (startChunk.Next == null)
                {
                    // Если наш сегмент единственный, создаем последовательность, используя данные из памяти начального узла
                    frame = new ReadOnlySequence<byte>(startChunk.Memory);
                }
                else
                {
                    // Если последний сегмент существует, то создаем последовательность начиная с первого сегмента, до последнего
                    frame = new ReadOnlySequence<byte>(
                        startSegment: startChunk,
                        startIndex: 0,
                        endSegment: currentChunk,
                        endIndex: currentChunk.Memory.Length);
                }
                // Сброс, чтобы мы могли принимать новые фрагменты с нуля
                return true;
            }
            else
            {
                frame = default;
                return false;
            }
        }

        public void Dispose()
        {
            if (!_isUsingArrayPool)
            {
                return;
            }
            //Если используется пул массивов
            Chunk<byte> chunk = startChunk;
            // Обходим все узлы связанного списка блоков в памяти
            while (chunk != null)
            {
                // Получаем массив байтов из памяти узла, если удается, то segment содержит эту информацию
                // Предпринимается попытка получить сегмент массива из внутреннего буфера памяти с доступом только для чтения,
                // возвращаемое значение указывает на успешное выполнение операции.
                if (MemoryMarshal.TryGetArray(chunk.Memory, out ArraySegment<byte> segment))
                {
                    // Если получилось, то массив возвращаем в пул массивов.                      
                    ArrayPool<byte>.Shared.Return(segment.Array!);
                }
                // Следующий узел в связанном списке блоков в памяти
                chunk = (Chunk<byte>)chunk.Next!;
            }
        }
    }
}
