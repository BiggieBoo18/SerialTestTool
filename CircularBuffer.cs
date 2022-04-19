using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialTestTool
{
    /// <summary>
    /// 循環バッファ。
    /// </summary>
    /// <typeparam name="T">要素の型</typeparam>
    public class CircularBuffer<T> : IEnumerable<T>
    {
        #region フィールド

        T[] buffer;
        int head, tail;
        int capacity;

        #endregion
        #region 初期化

        public CircularBuffer() : this(512) { }

        /// <summary>
        /// 初期最大容量を指定して初期化。
        /// </summary>
        /// <param name="capacity">初期最大容量</param>
        public CircularBuffer(int capacity)
        {
            if (!is_pow2((uint)capacity) && capacity > 1)
            {
                throw new ArgumentException("要素数は2以上の2のべき乗にしてください");
            }
            this.capacity = capacity;
            this.buffer = new T[capacity];
            this.head = this.tail = 0;
        }

        static bool is_pow2(uint n)
        {
            if (n == 0)
            {
                return false;
            }
            return (n & (n - 1)) == 0;
        }

        #endregion
        #region プロパティ

        /// <summary>
        /// 格納されている要素数。
        /// </summary>
        public int Count
        {
            get
            {
                int count = this.head - this.tail;
                if (count < 0) count += this.buffer.Length;
                return count;
            }
        }

        /// <summary>
        /// headの位置に挿入
        /// NOTE: headはtailを超えない
        /// NOTE: headが指す次のインデックスに挿入
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool put(T data)
        {
            int next = this.head + 1; // headが指す次のインデックスに挿入
            if (next == this.capacity)
            {
                next = 0;
            }
            Console.WriteLine($"put => {data}");
            if (next == this.tail) // もしheadがtailに追いついたらfalseを返す
            {
                Console.WriteLine($"Buffer Overflow");
                return false;
            }
            this.head = next;
            this.buffer[this.head] = data; // headの指すインデックスに挿入
            return true;
        }

        /// <summary>
        /// data取得
        /// NOTE: tailはheadを超えない
        /// NOTE: tailが指す次のインデックスから取得
        /// </summary>
        /// <returns></returns>
        public T get()
        {
            if (this.tail == this.head) // tailとheadが同じならheadの指すインデックスのデータを返す
            {
                return this.buffer[this.tail];
            }
            int next = this.tail + 1; // tailが指す次のインデックスを返す
            if (next == this.capacity)
            {
                next = 0;
            }
            this.tail = next;
            return this.buffer[next];
        }

        /// <summary>
        /// リングバッファのリセット
        /// </summary>
        public void reset()
        {
            this.head = 0;
            this.tail = 0;
            Array.Clear(this.buffer, 0, this.buffer.Length);
        }

        /// <summary>
        /// 全ての要素の表示
        /// </summary>
        public void show()
        {
            for (int i = 0; i < this.capacity; i++)
            {
                Console.Write($"buffer[{i}]={this.buffer[i]}");
                if (i == this.tail)
                {
                    Console.Write(" <- tail");
                }
                if (i == this.head)
                {
                    Console.Write(" <- head");
                }
                Console.WriteLine();
            }

        }

        #endregion
        #region IEnumerable<T> メンバ

        public IEnumerator<T> GetEnumerator()
        {
            if (this.head <= this.tail)
            {
                for (int i = this.head; i < this.tail; ++i)
                    yield return this.buffer[i];
            }
            else
            {
                for (int i = this.head; i < this.buffer.Length; ++i)
                    yield return this.buffer[i];
                for (int i = 0; i < this.tail; ++i)
                    yield return this.buffer[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
