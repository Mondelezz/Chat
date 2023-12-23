using System.Buffers;

namespace Quantum.Services.WebSocketServices
{
    public class Chunk<T> : ReadOnlySequenceSegment<T> //использую для представления связанного списка блоков в памяти
    {
        // ReadOnlyMemory<T> - непрерывная область в памяти
        public Chunk(ReadOnlyMemory<T> memory)
        {
            // Содержит данные узла списка
            Memory = memory;
        }
        public Chunk<T> Add(ReadOnlyMemory<T> memory)
        {
            Chunk<T> segment = new Chunk<T>(memory)
            {
                //Индекс в конце текущего сегмента(узла) в памяти
                RunningIndex = RunningIndex + Memory.Length
            };
            // Следующему узлу присваиваю созданный сегмент данных
            Next = segment;
            // Возвращаю созданный сегмент
            return segment;
        }
    }
}
