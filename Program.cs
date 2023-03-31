using System.Text;

class ListNode
{
    public ListNode Prev;
    public ListNode Next;
    public ListNode Rand; // произвольный элемент внутри списка
    public string Data;
}

class ListRand
{
    public ListNode Head;
    public ListNode Tail;
    public int Count;

    public void Serialize(FileStream s)
    {
        ListNode currentNode = Head;
        ListNode otherNode = Head.Next;

        int[] randIndices = new int[Count];

        // находим для каждой ноды индекс ее rand элемента,
        // а затем записываем в массив по порядку индексы этих элементов
        for (int otherIndex = 0, currentIndex = 0; 
            otherIndex < Count && currentIndex <  Count - 1;
            otherIndex++)
        {
            if(currentNode != otherNode && currentNode.Rand == otherNode )
            {
                // записываем индекс rand элемента в массив
                randIndices[otherIndex] = currentIndex;

                // меняем параметры цикла для исследования очередной ноды
                currentNode = currentNode.Next;
                currentIndex++;

                otherNode = Head;
                otherIndex = -1;
            }
            otherNode = otherNode.Next;
        }

        // начинаем запись в файл
        int MaxStringSize = 1000;
        int IntSize = 4;
        int OffsetSize = MaxStringSize + IntSize; // размер строки + размер Int32

        currentNode = Head;

        // размер листа
        byte[] countBuffer = BitConverter.GetBytes(Count);
        s.Write(countBuffer, 0, countBuffer.Length);

        for (int i = 0; i < Count; i++)
        {
            byte[] strBuffer = Encoding.ASCII.GetBytes(currentNode.Data);
            s.Write(strBuffer, (i * OffsetSize) + IntSize, MaxStringSize);

            byte[] intBuffer = BitConverter.GetBytes(randIndices[i]);
            s.Write(intBuffer, i * OffsetSize, intBuffer.Length);

            currentNode = currentNode.Next;
        }
    }

    public void Deserialize(FileStream s)
    {
        // размеры данных
        int MaxStringSize = 1000;
        int IntSize = 4;
        int OffsetSize = MaxStringSize + IntSize;

        // буферы для сохранения значений из файла
        byte[] intBuffer = new byte[IntSize];
        byte[] strBuffer = new byte[MaxStringSize];

        // считаем из файла размер списка (он записан первым)
        s.Read(intBuffer, 0, IntSize);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(intBuffer);

        Count = BitConverter.ToInt32(intBuffer, 0);

        // начнем создавать копию списка
        Head = new ListNode();
        ListNode currentNode = Head;
        int[] randIndices = new int[Count];

        for (int i = 0; i < Count; i++)
        {
            if (i == 0)
            {
                currentNode.Prev = null;
                currentNode.Next = new ListNode();
                currentNode.Next.Prev = currentNode;
            }
            else if (i == Count - 1)
            {
                currentNode.Next = null;
                Tail = currentNode;
            }
            else
            {
                currentNode.Next = new ListNode();
                currentNode.Next.Prev = currentNode;
            }

            //сразу записываем данные из файла
            s.Read(strBuffer, (i * OffsetSize) + IntSize, MaxStringSize);
            currentNode.Data = Encoding.ASCII.GetString(intBuffer);

            // записываем индексы rand в массив
            s.Read(intBuffer, i * OffsetSize, intBuffer.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBuffer);
            randIndices[i] = BitConverter.ToInt32(intBuffer, 0);

            // переходим к следующей ноде
            currentNode = currentNode.Next;
        }

        // так как при создании мы не можем сразу сослаться на все rand эл-ты,
        // то сделаем это позже
        currentNode = Head;
        ListNode otherNode = currentNode;

        for (int otherIndex = 0, i = 0; otherIndex < Count - 1; otherIndex++)
        {
            if (randIndices[i] == otherIndex)
            {
                currentNode.Rand = otherNode;
                currentNode = currentNode.Next;
                i++;
                otherNode = Head;
                otherIndex = 0;
            }
            otherNode = otherNode.Next;
        }
    }
}



