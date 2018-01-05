using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RDFCommon.Interfaces;

namespace RDFCommon.long_dictionary
{
    /// <summary>
    /// List of arrays of elements of T type in RAM.
    /// Has Stream methods.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LongStream<T> : IEnumerable<T>, IGetLongStream<T>
    {
        private uint max;// = 536870912;// 0x7fffffc7;
        public const uint MaxNotByteArraySize = 0X7FEFFFFF;
        public const uint MaxByteArraySize = 0x7fffffc7;
        private List<T[]> _buffer;
        private ulong _capacity;
        private const ulong _origin=0;
        private uint _buffersCount;
        private bool _isOpen;
        private ulong _length;
        private ulong _position;
        //private T[] _buffer;
        private bool _exposable;
        private bool _expandable;
        private bool _writable;


        public LongStream() : this(0)
        {
        }
        public LongStream(uint capacity, uint max = 0x7fffffc7) : this((ulong)capacity, max) { }

        public LongStream(ulong capacity, uint max=0x7fffffc7)
        {
            
            _buffersCount = (uint) capacity/max+1;
            _buffer = new List<T[]>((int) (capacity/max + 1));

            //if (_buffersCount > 1)

            for (uint i = 0; i < _buffersCount - 1; i++)
                _buffer.Add(new T[max]);
            _buffer.Add(new T[capacity%max]);
            //lastBuffer=new List<T>(capacity % max);
            this._capacity = capacity;
            this.max = max;
            this._expandable = true;
            this._writable = true;
            this._exposable = true;
            this._isOpen = true;
        }





        public ulong Read(T[] buffer, int offset, int  count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");//, "ArgumentNull_Buffer"));
            }
            if ((_length - (ulong)offset) < (ulong)count)
            {
                throw new ArgumentException("Argument_InvalidOffLen");
            }
            if (!this._isOpen)
            {
                throw new ArgumentException("StreamIsClosed");
                //__Error.StreamIsClosed();
            }
            uint realCount = (uint)Math.Min(this._length - this._position, (ulong)count);
            
            if (realCount <= 0)
            {
                return 0;
            }
            if (realCount <= 8)
            {
                uint num2 = realCount;
                while (--num2 > 1)
                {
                    buffer[offset + num2] = Get(this._position + num2);
                }
            this._position += realCount;
            }
            else
            {
                int localposition = (int) (this._position % max);
                int firstBufferIndex = (int)(this._position / max);
                int inFirstBuffer = (int)_buffer[firstBufferIndex].Length - localposition;
                if (inFirstBuffer >= realCount)
                {
                    Array.Copy(this._buffer[firstBufferIndex], localposition, buffer, offset, (int)realCount);
                    this._position += realCount;
                }
                else
                {
                    Array.Copy(this._buffer[firstBufferIndex], localposition, buffer, offset, inFirstBuffer);
                    this._position += (ulong) inFirstBuffer;
                    Read(buffer, offset+(int)inFirstBuffer, (int)realCount - (int)inFirstBuffer);
                }
            }
            return  realCount;
        }

        private T Get(ulong position)
        {
            if (position < max)
                return _buffer[0][position];
            int indexOfBuffer = (int) (position / max);
            int localPosition = (int) (position % max);
            return _buffer[indexOfBuffer][localPosition];
        }

        public T Read()
        {
            if (!this._isOpen)
            {
                //__Error.StreamIsClosed();
                throw new ArgumentException("StreamIsClosed");
            }
            if (this._position >= this._length)
            {
                return default(T);
            }
            ulong index = this._position;
            this._position = index + 1;
            return Get(index);
        }

        public ulong Seek(ulong offset, SeekOrigin loc)
        {
            if (!this._isOpen)
            {
                // __Error.StreamIsClosed();
                throw new ArgumentException("StreamIsClosed");
            }
            if (offset > 0x7fffffc7L * 0x7fffffc7L)
            {
                throw new ArgumentOutOfRangeException("offset", "ArgumentOutOfRange_StreamLength");
            }
            switch (loc)
            {
                case SeekOrigin.Begin:
                    {
                        this._position = _origin + offset;
                        break;
                    }
                case SeekOrigin.Current:
                    {
                        //TODO Check max
                        this._position = this._position + offset;
                        break;
                    }
                case SeekOrigin.End:
                {
                        this._position = this._length + offset;
                        break;
                    }
                default:
                    throw new ArgumentException("Argument_InvalidSeekOrigin");
            }
            return this._position;
        }


        public void SetLength(ulong value)
        {
            if ((value < 0L) || (value > 0x7fffffc7L* 0x7fffffc7L))
            {
                throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_StreamLength");
            }
            this.EnsureWriteable();
            if (value > (0x7fffffc7UL * 0x7fffffc7UL - _origin))
            {
                throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_StreamLength");
            }
            ulong num = _origin + value;
            if (!this.EnsureCapacity(num) && (num > this._length))
            {

                Clear(_length, num);

            }
            this._length = num;
            if (this._position > num)
            {
                this._position = num;
            }
        }

        public void Clear(ulong offset, ulong count)
        {
            while (true)
            {
                //TODO test parameters
                int localOffset = (int) (offset%max);
                int indexOfBuffer = (int) (offset/max);
                if (count > (ulong) _buffer[indexOfBuffer].Length - (ulong) localOffset)
                {
                    int clearedInThis = (int) (max - localOffset);
                    Array.Clear(this._buffer[indexOfBuffer], localOffset, clearedInThis);
                    offset += (ulong) clearedInThis;
                    count -= (ulong) clearedInThis;
                    continue;
                }
                else
                {
                    Array.Clear(this._buffer[indexOfBuffer], (int) (offset%max), (int) count);
                }
                break;
            }
            _length -= count;
        }


        public void Write(T[] buffer, int offset, uint count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", "ArgumentNull_Buffer");
            }
           
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException("Argument_InvalidOffLen");
            }
            if (!this._isOpen)
            {
                throw new ArgumentException("StreamIsClosed");
                // __Error.StreamIsClosed();
            }
            this.EnsureWriteable();
            ulong num = this._position + count;
           
            if (num > this._length)
            {
                bool flag = this._position > this._length;
                if ((num > this._capacity) && this.EnsureCapacity(num))
                {
                    flag = false;
                }
                if (flag)
                {
                    Clear(this._length, num - this._length);
                }
                this._length = num;
            }
            if ((count <= 8) ) //&& (buffer != this._buffer)
            {
                uint num2 = count;
                while (--num2 > 1)
                {
                    var newPosition = this._position + num2;
                    this._buffer[(int)(newPosition/max)] [(int)((this._position + num2) % max)]= buffer[offset + num2];
                }
            this._position = num;
            }
            else
            {
                int localposition = (int)(this._position % max);
                var writeBufferIndex = (int)(this._position / max);
                int inWriteBufferFree = (int)_buffer[writeBufferIndex].Length - localposition;
                if (inWriteBufferFree >= count)
                {
                    Array.Copy(buffer, offset, this._buffer[writeBufferIndex], localposition, (int) count);
                    this._position += count;
                }
                else
                {
                    Array.Copy( buffer, offset, this._buffer[writeBufferIndex], localposition, inWriteBufferFree);
                    this._position += (ulong)inWriteBufferFree;
                    Write(buffer, offset + (int)inWriteBufferFree, (uint) (count - inWriteBufferFree));
                }
            }
        }


        public void Write(T value)
        {
            if (!this._isOpen)
            {
                throw new ArgumentException("StreamIsClosed");
              //  __Error.StreamIsClosed();
            }
            this.EnsureWriteable();
            if (this._position >= this._length)
            {
                ulong num = this._position + 1;
                bool flag = this._position > this._length;
                if ((num >= this._capacity) && this.EnsureCapacity(num))
                {
                    flag = false;
                }
                if (flag)
                {
                    Clear(this._length, this._position - this._length);
                }
                this._length = num;
            }
            ulong index = this._position;
            this._position = index + 1;
           if(index < max) this._buffer[0][index] = value;
           else this._buffer[(int) (index/max)][index%max] = value;
        }

        public bool CanRead { get { return true; } }
        public bool CanSeek { get { return true; } }
        public bool CanWrite { get { return true; } }
        public ulong Length { get { return _length; } }
        public ulong Position { get { return _position; } set { //todo test parameters
                _position = value; } }

        private bool EnsureCapacity(ulong value)
        {
            if (value <= this._capacity)
            {
                return false;
            }
            ulong num = value;
            if (num < 0x100)
            {
                num = 0x100;
            }
            if (num < (this._capacity * 2))
            {
                num = this._capacity * 2;
            }
            if ((this._capacity * 2) > 0x7fffffc7L * 0x7fffffc7)
            {
                num = (value > 0x7fffffc7L*0x7fffffc7) ? value : 0x7fffffc7L*0x7fffffc7;
            }
            this.Capacity = num;
            return true;
        }

        private void EnsureWriteable()
        {
            if (!this.CanWrite)
            {
                //   __Error.WriteNotSupported();
                throw new ArgumentException("StreamIsClosed");
            }
        }
        public ulong Capacity
        {
            get
            {
                if (!this._isOpen)
                {
                    //   __Error.StreamIsClosed();
                    throw new ArgumentException("StreamIsClosed");
                }
                return (this._capacity - _origin);
           }
            set
            {
                if (value < this.Length)
                {
                    throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_SmallCapacity");
                }
                if (!this._isOpen)
                {
                    throw new ArgumentException("StreamIsClosed");
                }
                if (!this._expandable && (value != this.Capacity))
                {
                    throw new ArgumentException("MemoryStreamNotExpandable");
                }
                if (this._expandable && (value != this._capacity))
                {
                    if (value > 0)
                    {
                        int lastBufferWithData = (int) (_length / max);
                        uint newBufferCount= (uint) (value / max)+1;
                        if (lastBufferWithData == newBufferCount-1)
                        {
                            var newLastBuffer = new T[value%max];
                            var oldLastBuffer = _buffer.Last();
                            Array.Copy(oldLastBuffer, newLastBuffer, oldLastBuffer.Length);
                            _buffer.RemoveAt(_buffer.Count - 1);
                            _buffer.Add(newLastBuffer);
                        }
                        else
                        {
                            uint countOfNewBuffers = newBufferCount - _buffersCount;

                            if (_capacity%max == 0)
                            {
                                for (int i = 0; i < countOfNewBuffers - 1; i++)
                                    _buffer.Add(new T[max]);

                                if(value % max!=0)
                                    _buffer.Add(new T[value%max]);
                            }
                            else
                            {
                                for (int i = 0; i < countOfNewBuffers; i++)
                                    _buffer.Add(new T[max]);
                                if(value % max!=0)
                                _buffer.Add(new T[value % max]);

                                if (_length%max != 0 && lastBufferWithData == _buffersCount - 1)
                                {
                                    int count = (int) (_length % max);
                                    Array.Copy(_buffer[lastBufferWithData], _buffer[lastBufferWithData + 1], count);
                                }

                                _buffer.RemoveAt((int) (_buffersCount - 1));
                            }
                        }

                        _buffersCount = (uint) _buffer.Count;
                    }
                    else
                    {
                        this._buffer = null;
                    }
                    this._capacity = value;
                }
            }
        }
      

        public T this[ulong index]
        {
            get
            {
                Position = index;
                return Read();
            }
            set
            {
                Position = index;
                Write(value);
            }
        }

        public IEnumerable<T> Sort(IComparer<T> comparer=null)
        {
            if (comparer == null)
                comparer = Comparer<T>.Default;
            for (int i = 0; i < _buffersCount; i++)
            {
                Array.Sort(_buffer[i],0, _buffer[i].Length);
            }

            if (_buffersCount == 1)
                return this;
            if (_buffersCount == 2)
            {
            } //TODO

            return null;// MultiMerge.MergePartialSorted1(_buffersCount, max, GetElements , _buffer.Last().Length, l => l, comparer, _length);
        }

        private IEnumerable<T> GetElements(ulong start, ulong count)
        {
            
            for (ulong i = 0; i < count; i++)
            {
                yield return this[start];
                start++;
            }

        }

        public IEnumerable<T> Sort<Tkey>(Func<T, Tkey> keySelector, IComparer<Tkey> comparer = null)
        {
            if (comparer == null)
                comparer = Comparer<Tkey>.Default;
            if (_buffersCount == 0) return this;
            Tkey[] keys;
            if (_buffersCount == 1)
            {
                keys = _buffer[0].Select(keySelector).ToArray();
                Array.Sort(keys, _buffer[0]);
                return this;
            }
            keys=new Tkey[max];
            
            for (int i = 0; i < _buffersCount; i++)
            {
                for (int j = 0; j < max; j++)
                    keys[i] = keySelector(_buffer[i][j]);

                Array.Sort(keys, _buffer[i]);
            }

            
            if (_buffersCount == 2)
            {
            } //TODO

            uint lastBufferLength = (uint) _buffer.Last().Length;
            return MultiMerge.MergePartialSorted<Tkey, T>(_buffersCount,
                (partIndex, position) => _buffer[partIndex][position], keySelector, comparer,
                (partIndex) => partIndex == _buffersCount - 1 ? lastBufferLength : max);
        }

        class Enumerator<T> : IEnumerator<T>
        {
            private readonly LongStream<T> enumerable;

            public Enumerator(LongStream<T> enumerable)
            {
                this.enumerable = enumerable;
                Reset();
            }

            public void Dispose()
            {
         
            }

            public bool MoveNext()
            {
                if (enumerable.Position < enumerable.Length)
                {
                    Current = enumerable.Read();
                    return true;
                }
                else return false;
            }

            public void Reset()
            {
                enumerable.Position = 0;
            }

            public T Current { get; private set; }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            Clear(0, Length);
        }

        public bool Contains(T item)
        {
            return _buffer.Any(array => array.Contains(item));
        }

        public ulong IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            bool found = false;
            foreach (var array in _buffer)
            {
                for (uint i = 0; i < array.Length; i++)
                    if (Equals(array[i], item))
                    {
                        array[i] = default(T);
                        found = true;
                        Remove(i, 1);
                    }
            }
            return found;
        }

        public void Remove(ulong from, ulong count)
        {
            //TODO check
            //TODO remove arrays and use Array.Copy
            for (ulong i = from; i < Length-count; i++)
            {
                this[i] = this[i + count];
            }
            _length -= count;
        }

        public void RemoveFirstArray()
        {
            _length -= (uint) _buffer[0].Length;
            _buffer.RemoveAt(0);
            
        }
        
        public ulong Count { get { return Length; } }

        public void Add(T key)
        {
            Position = Length;
            Write(key);
        }

        public void FreeMemory()
        {
            _buffer=new List<T[]>() {};
            _length = 0;
        }

        public IGetLongStream<T> Copy()
        {
            var newStream=new LongStream<T>(Length);
            Position = 0;
            for (ulong i = 0; i < Length; i++)
            {
                newStream.Write(Read());
            }
            return newStream;
        }

        public static LongStream<T> Concat(IGetLongStream<T> other1, IGetLongStream<T> other2)
        {
            var longStream = new LongStream<T>(other2.Count + other1.Count);

            foreach (var t in other1)
            {
                longStream.Add(t);
            }
            foreach (var t in other2)
            {
                longStream.Add(t);
            }
            return longStream;
        }
    }
    
}
