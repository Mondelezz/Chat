using System.Buffers;
using System.Net.WebSockets;

namespace Quantum.Services.WebSocketServices
{
    sealed class WebSocketReceiveResultProcessor : IDisposable
    {
        //Начальные данные узла
        Chunk<byte> startChunk = null;
        //Текущие данные узла
        Chunk<byte> currentChunk = null;
        private readonly bool _isUsingArrayPool;

        public WebSocketReceiveResultProcessor(bool isUsingArrayPool)
        {
            _isUsingArrayPool = isUsingArrayPool;
        }
        public bool Receive(WebSocketReceiveResult result, ArraySegment<byte> buffer, out ReadOnlySequence<byte> frame)
        {
            if (result.EndOfMessage && result.MessageType == WebSocketMessageType.Close)
            {
                frame = default;
                return false;
            }
            //  Формируем срез из текущего буффера данных, начиная с индекса 0, до длины result
            ArraySegment<byte> slice = _isUsingArrayPool
                ? buffer.Slice(index: 0, count: result.Count) // Используем Slice, если используется пул массивов, представляет ArraySegment<byte>
                : buffer.Slice(index: 0, count: result.Count).ToArray(); // Используем ToArray, если не используется пул массивов, представляет byte[]

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
            // Сообщение завершено и начальный узел не null
            if (result.EndOfMessage && startChunk != null)
            {
                // Если следующего сегмента не существует, то:
                if (startChunk.Next == null)
                {
                    // Если наш сегмент единственный, создаем последовательность, используя данные из памяти начального узла
                    frame = new ReadOnlySequence<byte>(startChunk.Memory);
                }
                else
                {
                    // Если последний сегмент сущетсвует, то создаем последовательность начиная с первого сегмента, до последнего
                    frame = new ReadOnlySequence<byte>(
                        startSegment: startChunk,
                        startIndex: 0,
                        endSegment: currentChunk,
                        endIndex: currentChunk.Memory.Length);
                }
                // Сброс, чтобы мы могли принимать новые фрагменты с нуля
                startChunk = currentChunk = null; 
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
            throw new NotImplementedException();
        }
    }
}
