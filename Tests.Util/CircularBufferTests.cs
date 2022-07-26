using ADR.Util;
using NUnit.Framework;

namespace Tests.Util
{
    public class CircularBufferTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Create()
        {
            var buffer = new CircularBuffer<int>(7);
            
            Assert.AreEqual(7, buffer.Capacity);
            Assert.AreEqual(0, buffer.Count);
        }

        [Test]
        public void Push_LessThanCapacity()
        {
            var buffer = new CircularBuffer<int>(5);

            buffer.Push(1);
            buffer.Push(2);
            buffer.Push(3);

            Assert.AreEqual(3, buffer.Count);
            Assert.AreEqual(1, buffer[0]);
            Assert.AreEqual(2, buffer[1]);
            Assert.AreEqual(3, buffer[2]);

            Assert.AreEqual(1, buffer.Peek());
        }

        [Test]
        public void Push_MoreThanCapacity()
        {
            var buffer = new CircularBuffer<int>(5);

            buffer.Push(1);
            buffer.Push(2);
            buffer.Push(3);
            buffer.Push(4);
            buffer.Push(5);
            buffer.Push(6);
            buffer.Push(7);

            Assert.AreEqual(5, buffer.Count);
            Assert.AreEqual(3, buffer[0]);
            Assert.AreEqual(4, buffer[1]);
            Assert.AreEqual(5, buffer[2]);
            Assert.AreEqual(6, buffer[3]);
            Assert.AreEqual(7, buffer[4]);

            Assert.AreEqual(3, buffer.Peek());
        }

        [Test]
        public void Push_MoreThanCapacity_CheckEnumerator()
        {
            var buffer = new CircularBuffer<int>(5);

            buffer.Push(1);
            buffer.Push(2);
            buffer.Push(3);
            buffer.Push(4);
            buffer.Push(5);
            buffer.Push(6);
            buffer.Push(7);

            var array = new int[5];
            var i = 0;
            foreach (var element in buffer)
            {
                array[i] = element;
                i++;
            }

            Assert.AreEqual(5, buffer.Count);
            Assert.AreEqual(3, array[0]);
            Assert.AreEqual(4, array[1]);
            Assert.AreEqual(5, array[2]);
            Assert.AreEqual(6, array[3]);
            Assert.AreEqual(7, array[4]);

            Assert.AreEqual(3, buffer.Peek());
        }

        [Test]
        public void Push_MoreThanCapacity_WithPop()
        {
            var buffer = new CircularBuffer<int>(5);

            buffer.Push(1);
            buffer.Push(2);
            buffer.Push(3);
            buffer.Push(4);
            buffer.Push(5);
            buffer.Push(6);
            buffer.Push(7);

            var p0 = buffer.Pop();
            var p1 = buffer.Pop();
            var p2 = buffer.Pop();

            buffer.Push(8);

            Assert.AreEqual(3, buffer.Count);
            Assert.AreEqual(3, p0);
            Assert.AreEqual(4, p1);
            Assert.AreEqual(5, p2);
            Assert.AreEqual(6, buffer[0]);
            Assert.AreEqual(7, buffer[1]);
            Assert.AreEqual(8, buffer[2]);

            Assert.AreEqual(6, buffer.Peek());
        }
    }
}